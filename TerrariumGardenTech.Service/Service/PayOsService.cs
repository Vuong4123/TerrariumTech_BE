using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using System.Security.Cryptography;
using System.Text;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Payment;
using TerrariumGardenTech.Common.ResponseModel.Order;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class PayOsService : IPayOsService
{
    private readonly IConfiguration _config;
    private readonly IHttpContextAccessor _httpContextAccessor;
    protected readonly IMapper _mapper;
    private readonly PayOS _payOS;
    private readonly UnitOfWork _unitOfWork;

    public PayOsService(IHttpContextAccessor httpContextAccessor,
        UnitOfWork unitOfWork, IConfiguration config, IMapper mapper)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _payOS = new PayOS(
            config["PayOS:ClientId"],
            config["PayOS:ApiKey"],
            config["PayOS:ChecksumKey"]
        );
        _config = config;
        _mapper = mapper;
    }

    //public async Task<IBusinessResult> CreatePaymentLink(int orderId, string description)
    //{
    //    var order = await _unitOfWork.Order.GetByIdWithOrderItemsAsync(orderId);
    //    if (order == null) return new BusinessResult(Const.NOT_FOUND_CODE, "No data found.");

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

    //    var domain =
    //        $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}/api";
    //    var successReturnUrl =
    //        $"{domain}/Payment/pay-os/callback?orderId={orderId}&statusReturn={PaymentReturnModelStatus.Success}";
    //    var failReturnUrl = $"{domain}/Payment/pay-os/callback?orderId={orderId}&statusReturn={PaymentReturnModelStatus.Fail}";
    //    var orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));

    //    var signatureData =
    //        $"amount={order.TotalAmount}&cancelUrl={failReturnUrl}&description={description}&orderCode={orderCode}&returnUrl={successReturnUrl}";
    //    var signature = CreateHmacSha256(signatureData, _config["PayOS:ChecksumKey"]);

    //    var paymentLinkRequest = new PaymentData(
    //        orderCode,
    //        amount,
    //        description,
    //        itemDatas,
    //        returnUrl: successReturnUrl,
    //        cancelUrl: failReturnUrl,
    //        expiredAt: (int)DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds(),
    //        signature: signature
    //    );
    //    var response = await _payOS.createPaymentLink(paymentLinkRequest);

    //    if (string.IsNullOrEmpty(response.checkoutUrl))
    //    {
    //        return new BusinessResult(Const.FAIL_CREATE_CODE, "Fail", response.checkoutUrl);
    //    }

    //    return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Paid", response.checkoutUrl);
    //}
    public async Task<IBusinessResult> CreatePaymentLink(int orderId, string description)
    {
        var order = await _unitOfWork.Order.GetByIdWithOrderItemsAsync(orderId);
        if (order == null) return new BusinessResult(Const.NOT_FOUND_CODE, "No data found.");

        // Tính amount từ DB, ưu tiên TotalAmount nếu đã chuẩn
        var amountDecimal = order.TotalAmount;
        if (amountDecimal <= 0 && (order.OrderItems?.Any() ?? false))
            amountDecimal = order.OrderItems.Sum(i => i.TotalPrice ?? 0);

        if (amountDecimal <= 0)
            return new BusinessResult(Const.BAD_REQUEST_CODE, "Invalid order amount.");

        // VND integer
        var amount = (int)Math.Round(amountDecimal, 0, MidpointRounding.AwayFromZero);

        // Build item data (an toàn null)
        var itemDatas = new List<ItemData>();
        foreach (var oi in order.OrderItems ?? Enumerable.Empty<OrderItem>())
        {
            if (oi.AccessoryId.HasValue)
            {
                var name = oi.Accessory?.Name ?? "Accessory";
                var unit = (int)Math.Round(oi.UnitPrice ?? 0, 0, MidpointRounding.AwayFromZero);
                var qty = oi.Quantity ?? 0;
                itemDatas.Add(new ItemData(name, qty, unit));
            }
            if (oi.TerrariumVariantId.HasValue)
            {
                var name = oi.TerrariumVariant?.VariantName ?? "Terrarium";
                var unit = (int)Math.Round(oi.UnitPrice ?? 0, 0, MidpointRounding.AwayFromZero);
                var qty = oi.Quantity ?? 0;
                itemDatas.Add(new ItemData(name, qty, unit));
            }
        }

        // Nên dùng domain cố định để PayOS gọi được ở prod
        var backendBase = "https://terarium.shop/api";
        var successReturnUrl = $"{backendBase}/Payment/pay-os/callback?orderId={orderId}&status=success";
        var failReturnUrl = $"{backendBase}/Payment/pay-os/callback?orderId={orderId}&status=fail";

        // orderCode unique & có thể suy ra orderId
        var orderCode = 100000 + orderId;

        // CHỮ KÝ: dùng đúng 'amount' bạn sẽ gửi
        var signatureData =
            $"amount={amount}&cancelUrl={failReturnUrl}&description={description}&orderCode={orderCode}&returnUrl={successReturnUrl}";
        var signature = CreateHmacSha256(signatureData, _config["PayOS:ChecksumKey"]);

        var paymentLinkRequest = new PaymentData(
            orderCode: orderCode,
            amount: amount,
            description: description,
            items: itemDatas,
            returnUrl: successReturnUrl,
            cancelUrl: failReturnUrl,
            expiredAt: (int)DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds(),
            signature: signature
        );

        var response = await _payOS.createPaymentLink(paymentLinkRequest);
        if (string.IsNullOrEmpty(response.checkoutUrl))
            return new BusinessResult(Const.FAIL_CREATE_CODE, "Fail", response.checkoutUrl);

        return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Success", response.checkoutUrl);
    }
    public async Task<IBusinessResult> ProcessPaymentCallback(PaymentReturnModel returnModel)
    {
        try
        {
            // Lấy orderId từ query (bạn đã đưa vào returnUrl ở bước tạo link)
            var order = await _unitOfWork.Order.GetByIdAsync(returnModel.OrderId);
            if (order == null) return new BusinessResult(Const.NOT_FOUND_CODE, "No data found.");

            // Map status PayOS -> status nội bộ
            var isSuccess = string.Equals(returnModel.Status, "success", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(returnModel.Status, "PAID", StringComparison.OrdinalIgnoreCase);

            order.PaymentStatus = isSuccess ? "Paid" : "Failed";
            order.TransactionId = returnModel.TransactionId; // nếu PayOS callback có trả
            await _unitOfWork.Order.UpdateAsync(order);
            if (order.Payment == null) order.Payment = new List<Payment>();
            order.Payment.Add(new Payment
            {
                OrderId = order.OrderId,
                PaymentMethod = "payOS",
                PaymentAmount = order.TotalAmount - (order.Deposit ?? 0),
                Status = isSuccess ? "Paid" : "Failed",
                PaymentDate = DateTime.UtcNow
            });

            await _unitOfWork.SaveAsync();
            var orderResponse = _mapper.Map<OrderResponse>(order);
            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, orderResponse);
        }
        catch (Exception e)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, e.Message);
        }
    }

    private static string CreateHmacSha256(string data, string? key)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}