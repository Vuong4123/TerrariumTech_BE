using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Sockets;
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
            //// ===== 0) TẮT VNPAY BẰNG CONFIG =====
            //var enabled = _configuration.GetValue<bool?>("Vnpay:Enabled") ?? false;
            //if (!enabled)
            //    return new BusinessResult(Const.FAIL_CREATE_CODE, "VNPAY đang được tắt qua cấu hình.");

            // ===== 1) ĐỌC VÀ KIỂM TRA CẤU HÌNH =====
            var tmnCode = _configuration["Vnpay:TmnCode"];
            var secret = _configuration["Vnpay:HashSecret"];
            var baseUrl = _configuration["Vnpay:BaseUrl"];
            var version = _configuration["Vnpay:Version"] ?? "2.1.0";
            var currCode = _configuration["Vnpay:CurrCode"] ?? "VND";
            var locale = _configuration["Vnpay:Locale"] ?? "vn";
            var command = _configuration["Vnpay:Command"] ?? "pay";
            var returnUrl = _configuration["PaymentCallBack:ReturnUrl"];

            if (string.IsNullOrWhiteSpace(tmnCode) ||
                string.IsNullOrWhiteSpace(secret) ||
                string.IsNullOrWhiteSpace(baseUrl) ||
                string.IsNullOrWhiteSpace(returnUrl))
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE,
                    "Thiếu cấu hình VNPAY (TmnCode/HashSecret/BaseUrl/ReturnUrl).");
            }

            // ===== 2) LẤY ĐƠN HÀNG & TÍNH TIỀN =====
            var order = await _unitOfWork.Order.GetByIdWithOrderItemsAsync(model.OrderId);
            if (order == null) return new BusinessResult(Const.NOT_FOUND_CODE, "No data found.");

            if (order.OrderItems == null || !order.OrderItems.Any())
                return new BusinessResult(Const.NOT_FOUND_CODE, "No items found in order.");

            decimal baseAmount = order.TotalAmount > 0
                ? order.TotalAmount
                : order.OrderItems.Sum(i => i.TotalPrice ?? 0);

            if (baseAmount <= 0)
                return new BusinessResult(Const.BAD_REQUEST_CODE, "Invalid order amount.");

            decimal payable = model.PayAll
                ? Math.Round(baseAmount * 0.9m, 0, MidpointRounding.AwayFromZero)
                : (order.Deposit ?? baseAmount);

            var amountVnd = (int)Math.Round(payable, 0);

            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

            // ===== 3) TẠO URL THANH TOÁN =====
            var pay = new VnPayLibrary();
            pay.AddRequestData("vnp_Version", version);
            pay.AddRequestData("vnp_Command", command);
            pay.AddRequestData("vnp_TmnCode", tmnCode);
            pay.AddRequestData("vnp_Amount", (amountVnd * 100).ToString());  // VND * 100
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", currCode);
            pay.AddRequestData("vnp_IpAddr", GetClientIp(context));
            pay.AddRequestData("vnp_Locale", locale);
            pay.AddRequestData("vnp_OrderInfo",
                $"{model.Name} {model.OrderDescription} {(model.PayAll ? "(Full)" : "(Partial/Deposit)")}");
            pay.AddRequestData("vnp_OrderType", model.OrderType ?? "other");
            pay.AddRequestData("vnp_ReturnUrl", returnUrl);
            pay.AddRequestData("vnp_TxnRef", model.OrderId.ToString());

            var paymentUrl = pay.GetPaymentUrl(baseUrl, secret); // alias của CreateRequestUrl
            if (string.IsNullOrEmpty(paymentUrl))
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Fail tạo URL VNPAY.");

            // (Tuỳ chọn) log chuỗi ký để so khớp khi cần
            // _logger.LogInformation("VNPAY RAW:{raw} HASH:{hash}", pay.LastRequestRaw, pay.LastRequestHash);

            return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Success", paymentUrl);
        }
        catch (Exception e)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, "Fail", e.ToString());
        }
    }

    public async Task<IBusinessResult> PaymentExecute(IQueryCollection collections)
    {
        try
        {
            //// ===== 0) TẮT VNPAY BẰNG CONFIG =====
            //var enabled = _configuration.GetValue<bool?>("Vnpay:Enabled") ?? false;
            //if (!enabled)
            //    return new BusinessResult(Const.FAIL_READ_CODE, "VNPAY đang được tắt qua cấu hình.");

            // ===== 1) ĐỌC & KIỂM TRA CẤU HÌNH =====
            var secret = _configuration["Vnpay:HashSecret"];
            if (string.IsNullOrWhiteSpace(secret))
                return new BusinessResult(Const.FAIL_READ_CODE, "Thiếu HashSecret.");

            // ===== 2) XÁC THỰC CALLBACK =====
            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, secret);
            if (!response.Success)
                return new BusinessResult(Const.FAIL_READ_CODE, "VNPAY xác thực thất bại!");

            // vnp_TxnRef = OrderId
            if (!int.TryParse(response.OrderId, out var orderId))
                return new BusinessResult(Const.FAIL_READ_CODE, "OrderId không hợp lệ!");

            var order = await _unitOfWork.Order.GetOrderbyIdAsync(orderId);
            if (order == null)
                return new BusinessResult(Const.NOT_FOUND_CODE, $"Không tìm thấy đơn hàng: {orderId}");

            // ===== 3) TÍNH SỐ TIỀN & ĐỐI CHIẾU =====
            decimal RoundVnd(decimal v) => Math.Round(v, 0, MidpointRounding.AwayFromZero);

            var baseAmount = order.TotalAmount > 0
                ? order.TotalAmount
                : RoundVnd(order.OrderItems?.Sum(i => i.TotalPrice ?? 0) ?? 0);

            baseAmount = RoundVnd(baseAmount);
            var fullAfter10 = RoundVnd(baseAmount * 0.9m);

            var amountMinor = response.Amount ?? 0m;   // vnp_Amount = VND * 100
            var paid = RoundVnd(amountMinor / 100m);

            var isPayAll = (paid == fullAfter10);
            var expected = isPayAll
                ? fullAfter10
                : RoundVnd(order.Deposit ?? baseAmount);

            if (paid != expected)
                return new BusinessResult(Const.FAIL_READ_CODE, $"Amount mismatch. paid={paid}, expected={expected}");

            // ===== 4) CẬP NHẬT ĐƠN =====
            order.PaymentStatus = "Paid";
            order.TransactionId = response.TransactionId;

            if (isPayAll)
                order.DiscountAmount = baseAmount - fullAfter10;

            await _unitOfWork.Order.UpdateAsync(order);

            order.Payment ??= new System.Collections.Generic.List<Payment>();
            order.Payment.Add(new Payment
            {
                OrderId = order.OrderId,
                PaymentMethod = "VNPAY",
                PaymentAmount = paid,
                Status = "Paid",
                PaymentDate = response.PaymentDate ?? DateTime.UtcNow
            });

            await _unitOfWork.SaveAsync();

            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Payment success");
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
    // ===== Helper: lấy IP client (kể cả sau proxy) =====
    private static string GetClientIp(HttpContext context)
    {
        try
        {
            var xff = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(xff))
                return xff.Split(',')[0].Trim();

            var ip = context.Connection.RemoteIpAddress;
            if (ip?.AddressFamily == AddressFamily.InterNetworkV6)
                ip = Dns.GetHostEntry(ip)
                        .AddressList
                        .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

            return ip?.ToString() ?? "127.0.0.1";
        }
        catch { return "127.0.0.1"; }
    }


    #endregion


}