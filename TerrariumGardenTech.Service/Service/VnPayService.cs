using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Net.payOS.Types;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Config;
using TerrariumGardenTech.Common.RequestModel.Payment;
using TerrariumGardenTech.Common.ResponseModel;
using TerrariumGardenTech.Common.ResponseModel.Order;
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
        #region code để dành
        //try
        //{
        //    var order = await _unitOfWork.Order.GetByIdWithOrderItemsAsync(model.OrderId);
        //    if (order == null) return new BusinessResult(Const.NOT_FOUND_CODE, "No data found.");

        //    // Kiểm tra nếu không có order items
        //    if (order.OrderItems == null || !order.OrderItems.Any())
        //    {
        //        return new BusinessResult(Const.NOT_FOUND_CODE, "No items found in order.");
        //    }

        //    var itemDatas = new List<ItemData>();
        //    var amount = 0;

        //    foreach (var orderItem in order.OrderItems)
        //    {
        //        // Kiểm tra và xử lý Accessory
        //        if (orderItem.AccessoryId.HasValue)
        //        {
        //            itemDatas.Add(new ItemData(orderItem.Accessory.Name, orderItem.Quantity ?? 0,
        //                (int)(orderItem.UnitPrice ?? 0)));
        //            amount += (int)(orderItem.TotalPrice ?? 0);
        //        }

        //        // Kiểm tra và xử lý TerrariumVariant
        //        if (orderItem.TerrariumVariantId.HasValue)
        //        {
        //            itemDatas.Add(new ItemData(orderItem.TerrariumVariant.VariantName, orderItem.Quantity ?? 0,
        //                (int)(orderItem.UnitPrice ?? 0)));
        //            amount += (int)(orderItem.TotalPrice ?? 0);
        //        }
        //    }


        //    var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        //    var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
        //    var tick = DateTime.Now.Ticks.ToString();
        //    var pay = new VnPayLibrary();
        //    var urlCallBack =
        //    //"https://terarium.shop/api/Payment/vn-pay/callback";
        //    $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}/api/Payment/vn-pay/callback";

        //    pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
        //    pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
        //    pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
        //    pay.AddRequestData("vnp_Amount", ((int)amount * 100).ToString());
        //    pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
        //    pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
        //    pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
        //    pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
        //    pay.AddRequestData("vnp_OrderInfo", $"{model.Name} {model.OrderDescription} {amount}");
        //    pay.AddRequestData("vnp_OrderType", model.OrderType);
        //    pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
        //    pay.AddRequestData("vnp_TxnRef", model.OrderId.ToString());
        //    // pay.AddRequestData("vnp_OrderId", model.OrderId.ToString());

        //    var paymentUrl =
        //        pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);

        //    if (string.IsNullOrEmpty(paymentUrl))
        //    {
        //        return new BusinessResult(Const.FAIL_CREATE_CODE, "Fail", paymentUrl);
        //    }

        //    return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Success", paymentUrl);
        //}
        //catch (Exception e)
        //{
        //    return new BusinessResult(Const.ERROR_EXCEPTION, "Fail", e.Message);

        //}
        #endregion
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
            var urlCallBack = "https://terarium.shop/api/Payment/vn-pay/callback";

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
        #region code để dành
        //try
        //{
        //    var pay = new VnPayLibrary();
        //    var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

        //    if (!response.Success)
        //        return new BusinessResult(Const.FAIL_READ_CODE, "VNPAY xác thực thất bại!");

        //    // Nên lấy OrderId từ vnp_TxnRef nếu cần
        //    if (!int.TryParse(response.OrderId, out var orderId))
        //        return new BusinessResult(Const.FAIL_READ_CODE, "OrderId không hợp lệ! Giá trị: " + response.OrderId);

        //    var order = await _unitOfWork.Order.GetOrderbyIdAsync(orderId);
        //    if (order == null)
        //        return new BusinessResult(Const.NOT_FOUND_CODE, $"Không tìm thấy đơn hàng: {orderId}");

        //    order.PaymentStatus = response.Success ? "Paid" : "Failed";
        //    order.TransactionId = response.TransactionId;
        //    await _unitOfWork.Order.UpdateAsync(order);

        //    if (order.Payment == null || !order.Payment.Any())
        //        order.Payment = new List<Payment>();

        //    order.Payment.Add(new Payment
        //    {
        //        OrderId = order.OrderId,
        //        PaymentMethod = response.PaymentMethod,
        //        PaymentAmount = response.Amount / 100,
        //        Status = response.Success ? "Paid" : "Failed",
        //        PaymentDate = response.PaymentDate ?? DateTime.UtcNow,
        //    });
        //    await _unitOfWork.SaveAsync();

        //    var orderResponse = _mapper.Map<OrderResponse>(order);
        //    return new BusinessResult(
        //        response.Success ? Const.SUCCESS_UPDATE_CODE : Const.FAIL_READ_CODE,
        //        null,
        //        orderResponse
        //    );
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine("[VNPAY CALLBACK ERROR]: " + ex.ToString());
        //    return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        //}
        #endregion
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

            // Idempotent bằng Order.TransactionId
            if (!string.IsNullOrEmpty(resp.TransactionId) &&
                string.Equals(order.TransactionId, resp.TransactionId, StringComparison.OrdinalIgnoreCase))
            {
                var already = _mapper.Map<OrderResponse>(order);
                return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Callback already processed", already);
            }

            order.PaymentStatus = resp.Success ? "Paid" : "Failed";
            order.TransactionId = resp.TransactionId; // lưu ở Order
            await _unitOfWork.Order.UpdateAsync(order);

            if (order.Payment == null) order.Payment = new List<Payment>();
            order.Payment.Add(new Payment
            {
                OrderId = order.OrderId,
                PaymentMethod = resp.PaymentMethod,
                PaymentAmount = resp.Amount / 100,
                Status = resp.Success ? "Paid" : "Failed",
                PaymentDate = resp.PaymentDate ?? DateTime.UtcNow,
                // KHÔNG gán TransactionId ở đây nữa
            });

            await _unitOfWork.SaveAsync();

            var orderResponse = _mapper.Map<OrderResponse>(order);
            return new BusinessResult(resp.Success ? Const.SUCCESS_UPDATE_CODE : Const.FAIL_READ_CODE, null, orderResponse);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }


}