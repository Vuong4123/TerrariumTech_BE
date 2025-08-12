using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
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

            // 1) Tính tổng gốc
            decimal baseAmount = order.TotalAmount > 0
                ? order.TotalAmount
                : order.OrderItems.Sum(i => i.TotalPrice ?? 0);

            if (baseAmount <= 0)
                return new BusinessResult(Const.BAD_REQUEST_CODE, "Invalid order amount.");

            // 2) Tính số tiền phải trả
            //    - Trả toàn bộ => giảm 10%
            //    - Không => lấy cọc (nếu có) hoặc tổng gốc
            decimal payable = model.PayAll
                ? Math.Round(baseAmount * 0.9m, 0, MidpointRounding.AwayFromZero)
                : (order.Deposit ?? baseAmount);

            var amountVnd = (int)payable;                  // VND
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

            var pay = new VnPayLibrary();

            var urlCallBack = 
                "https://localhost:7072/api/Payment/vn-pay/callback";
            //"http://terarium.shop/api/Payment/vn-pay/callback";

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", (amountVnd * 100).ToString()); // ✅ VNPAY = VND*100
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            pay.AddRequestData("vnp_OrderInfo",
                $"{model.Name} {model.OrderDescription} {(model.PayAll ? "(Full)" : "(Partial/Deposit)")}");
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
            pay.AddRequestData("vnp_TxnRef", model.OrderId.ToString());

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
            var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);
            if (!response.Success)
                return new BusinessResult(Const.FAIL_READ_CODE, "VNPAY xác thực thất bại!");

            // vnp_TxnRef = OrderId
            if (!int.TryParse(response.OrderId, out var orderId))
                return new BusinessResult(Const.FAIL_READ_CODE, "OrderId không hợp lệ!");

            var order = await _unitOfWork.Order.GetOrderbyIdAsync(orderId);
            if (order == null)
                return new BusinessResult(Const.NOT_FOUND_CODE, $"Không tìm thấy đơn hàng: {orderId}");

            // ===== Tính số tiền =====
            decimal RoundVnd(decimal v) => Math.Round(v, 0, MidpointRounding.AwayFromZero);

            var baseAmount = order.TotalAmount > 0
                ? order.TotalAmount
                : RoundVnd(order.OrderItems?.Sum(i => i.TotalPrice ?? 0) ?? 0);

            baseAmount = RoundVnd(baseAmount);
            var fullAfter10 = RoundVnd(baseAmount * 0.9m);          // payAll -10%

            var amountMinor = response.Amount ?? 0m;                 // vnp_Amount = VND * 100
            var paid = RoundVnd(amountMinor / 100m);                 // về VND nguyên

            // Xác định có phải payAll không
            var isPayAll = (paid == fullAfter10);

            // expected cho lần thanh toán này
            var expected = isPayAll
                ? fullAfter10
                : RoundVnd(order.Deposit ?? baseAmount);

            if (paid != expected)
                return new BusinessResult(Const.FAIL_READ_CODE, $"Amount mismatch. paid={paid}, expected={expected}");

            // ===== Cập nhật đơn =====
            var success = response.Success;
            order.PaymentStatus = success ? "Paid" : "Failed";
            order.TransactionId = response.TransactionId;

            // Chỉ lưu DiscountAmount khi payAll thành công
            if (success && isPayAll)
            {
                order.DiscountAmount = baseAmount - fullAfter10;     // đúng 10% đã giảm
            }

            await _unitOfWork.Order.UpdateAsync(order);

            if (order.Payment == null) order.Payment = new List<Payment>();
            order.Payment.Add(new Payment
            {
                OrderId = order.OrderId,
                PaymentMethod =  "VNPAY" ,
                PaymentAmount = paid,
                Status = success ? "Paid" : "Failed",
                PaymentDate = response.PaymentDate ?? DateTime.UtcNow,
            });

            await _unitOfWork.SaveAsync();

            return new BusinessResult(
                success ? Const.SUCCESS_UPDATE_CODE : Const.FAIL_READ_CODE,
                success ? "Payment success" : "Payment failed"
            );
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }


    #region Helper Methods
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

    private static decimal RoundVnd(decimal v)
        => Math.Round(v, 0, MidpointRounding.AwayFromZero);

    private static decimal ComputeExpectedPayable(Order order)
    {
        decimal gross = order.TotalAmount > 0
            ? order.TotalAmount
            : (order.OrderItems?.Sum(i => i.TotalPrice ?? 0) ?? 0);

        decimal discount = order.DiscountAmount ?? 0;
        decimal basePay = order.Deposit ?? gross;

        var expected = basePay - discount;
        if (expected < 0) expected = 0;
        return RoundVnd(expected);
    }

    #endregion


}