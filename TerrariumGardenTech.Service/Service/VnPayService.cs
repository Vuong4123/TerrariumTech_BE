using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Net.payOS.Types;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Config;
using TerrariumGardenTech.Common.RequestModel.Payment;
using TerrariumGardenTech.Common.ResponseModel.Payment;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class VnPayService : IVnPayService
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UnitOfWork _unitOfWork;
    protected readonly IMapper _mapper;

    public VnPayService(IHttpContextAccessor httpContextAccessor,
        UnitOfWork unitOfWork, IConfiguration config, IMapper mapper)
    {
        _configuration = config;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _mapper = mapper;
    }

    public async Task<IBusinessResult> CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
    {
        
        try
        {
            var order = await _unitOfWork.Order.GetByIdWithOrderItemsAsync(model.OrderId);
            if (order == null) return new BusinessResult(Const.NOT_FOUND_CODE, "No data found.");

            if (order.OrderItems == null || !order.OrderItems.Any())
                return new BusinessResult(Const.NOT_FOUND_CODE, "No items found in order.");

            // Prefer tổng đơn đã tính sẵn; fallback tính từ chi tiết
            decimal amountDec = order.TotalAmount;
            if (amountDec <= 0)
                amountDec = order.OrderItems.Sum(i => i.TotalPrice ?? 0);

            if (amountDec <= 0)
                return new BusinessResult(Const.BAD_REQUEST_CODE, "Invalid order amount.");

            // VNPAY yêu cầu vnp_Amount = VND * 100 (đơn vị tối thiểu)
            var amountVnd = (int)Math.Round(amountDec, 0, MidpointRounding.AwayFromZero);

            // (Optional) Liệt kê items cho OrderInfo / debug
            var itemDatas = new List<ItemData>();
            foreach (var oi in order.OrderItems)
            {
                if (oi.AccessoryId.HasValue)
                    itemDatas.Add(new ItemData(oi.Accessory?.Name ?? "Accessory", oi.Quantity ?? 0, (int)Math.Round(oi.UnitPrice ?? 0)));
                if (oi.TerrariumVariantId.HasValue)
                    itemDatas.Add(new ItemData(oi.TerrariumVariant?.VariantName ?? "Terrarium", oi.Quantity ?? 0, (int)Math.Round(oi.UnitPrice ?? 0)));
            }

            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);

            var pay = new VnPayLibrary();

            // Khuyến nghị dùng domain public cố định
            var urlCallBack = 
                //"https://terarium.shop/api/Payment/vn-pay/callback";
            "https://localhost:7072/api/Payment/vn-pay/callback"; // local test

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", (amountVnd * 100).ToString()); // VNPAY = VND*100
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {amountVnd}đ");
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", model.OrderId.ToString()); // dùng vnp_TxnRef = orderId

            var paymentUrl = pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);
            if (string.IsNullOrEmpty(paymentUrl))
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Fail", paymentUrl);

            return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Success", paymentUrl);
        }
        catch (Exception e)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, "Fail", e.Message);
        }
    }

    public async Task<IBusinessResult> PaymentExecute(IQueryCollection collections)
    {
        try
        {
            var pay = new VnPayLibrary();
            var resp = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);
            if (!resp.Success)
                return new BusinessResult(Const.FAIL_READ_CODE, "VNPAY xác thực thất bại!");

            if (!int.TryParse(resp.OrderId, out var orderId))
                return new BusinessResult(Const.FAIL_READ_CODE, "OrderId không hợp lệ! Giá trị: " + resp.OrderId);

            var order = await _unitOfWork.Order.GetOrderbyIdAsync(orderId);
            if (order == null)
                return new BusinessResult(Const.NOT_FOUND_CODE, $"Không tìm thấy đơn hàng: {orderId}");

            // Idempotent: nếu callback cũ
            if (!string.IsNullOrEmpty(resp.TransactionId) &&
                string.Equals(order.TransactionId, resp.TransactionId, StringComparison.OrdinalIgnoreCase))
            {
                var dtoAlready = BuildDtoFromQuery(collections, orderId, resp.Success);
                return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Callback already processed", dtoAlready);
            }

            // Cập nhật đơn + ghi payment
            order.PaymentStatus = resp.Success ? "Paid" : "Failed";
            order.TransactionId = resp.TransactionId;
            await _unitOfWork.Order.UpdateAsync(order);

            if (order.Payment == null) order.Payment = new List<Payment>();
            order.Payment.Add(new Payment
            {
                OrderId = order.OrderId,
                PaymentMethod = resp.PaymentMethod,
                PaymentAmount = resp.Amount / 100,             // VND
                Status = resp.Success ? "Paid" : "Failed",
                PaymentDate = resp.PaymentDate ?? DateTime.UtcNow,
            });

            await _unitOfWork.SaveAsync();

            // Trả DTO để controller gắn vào URL redirect
            var dto = BuildDtoFromQuery(collections, orderId, resp.Success);
            return new BusinessResult(resp.Success ? Const.SUCCESS_UPDATE_CODE : Const.FAIL_READ_CODE, null, dto);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }

    private static VnPayResultDto BuildDtoFromQuery(IQueryCollection q, int orderId, bool success)
    {
        // các key chuẩn của VNPAY
        var amountStr = q["vnp_Amount"].ToString();           // VND*100
        var transNo = q["vnp_TransactionNo"].ToString();    // mã GD
        var bankCode = q["vnp_BankCode"].ToString();
        var cardType = q["vnp_CardType"].ToString();
        var payDate = q["vnp_PayDate"].ToString();          // yyyyMMddHHmmss
        var respCode = q["vnp_ResponseCode"].ToString();

        var amountVnd = 0;
        if (int.TryParse(amountStr, out var a)) amountVnd = a / 100;

        return new VnPayResultDto
        {
            OrderId = orderId,
            Success = success,
            AmountVnd = amountVnd,
            TransactionId = transNo,
            BankCode = bankCode,
            CardType = cardType,
            PayDateRaw = payDate,
            ResponseCode = respCode
        };
    }


}