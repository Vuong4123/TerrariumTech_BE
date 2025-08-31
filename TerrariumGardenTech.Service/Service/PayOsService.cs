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

    
    public async Task<IBusinessResult> CreatePaymentLink(int orderId, string description, bool payAll = false)
    {
        var order = await _unitOfWork.Order.GetByIdWithOrderItemsAsync(orderId);
        if (order == null)
            return new BusinessResult(Const.NOT_FOUND_CODE, "No data found.");

        // 1) Tính tổng tiền từ DB
        var amountDecimal = order.TotalAmount;
        if (amountDecimal <= 0 && (order.OrderItems?.Any() ?? false))
            amountDecimal = order.OrderItems.Sum(i => i.TotalPrice ?? 0);

        if (amountDecimal <= 0)
            return new BusinessResult(Const.BAD_REQUEST_CODE, "Invalid order amount.");

        // 2) Áp dụng giảm giá nếu thanh toán toàn bộ
        if (payAll)
        {
            var discount = Math.Round(amountDecimal * 0.1m, 0, MidpointRounding.AwayFromZero);
            order.DiscountAmount = discount; // ✅ lưu vào DB để callback đọc
            amountDecimal -= discount;
        }

        // 3) Làm tròn về số nguyên VND
        var amount = (int)Math.Round(amountDecimal, 0, MidpointRounding.AwayFromZero);

        // 4) Build item data
        var itemDatas = new List<ItemData>();
        foreach (var oi in order.OrderItems ?? Enumerable.Empty<OrderItem>())
        {
            var name = oi.AccessoryId.HasValue ? (oi.Accessory?.Name ?? "Accessory") :
                       oi.TerrariumVariantId.HasValue ? (oi.TerrariumVariant?.VariantName ?? "Terrarium") :
                       "Item";
            var unit = (int)Math.Round(oi.UnitPrice ?? 0, 0, MidpointRounding.AwayFromZero);
            var qty = oi.Quantity ?? 0;
            itemDatas.Add(new ItemData(name, qty, unit));
        }

        // 5) Link callback CỨNG
        var backendBase = "https://terarium.shop/api";
        var successReturnUrl = $"{backendBase}/Payment/pay-os/callback"; // không có orderId, status
        var failReturnUrl = $"{backendBase}/Payment/pay-os/callback";

        // 6) orderCode (chứa orderId ẩn)
        var orderCode = 100000 + orderId;

        // 7) Chữ ký
        var signatureData =
            $"amount={amount}&cancelUrl={failReturnUrl}&description={description}&orderCode={orderCode}&returnUrl={successReturnUrl}";
        var signature = CreateHmacSha256(signatureData, _config["PayOS:ChecksumKey"]);

        // 8) Request data
        var paymentLinkRequest = new PaymentData(
            orderCode: orderCode,
            amount: amount,
            description: payAll ? $"{description} (Full Payment -10%)" : description,
            items: itemDatas,
            returnUrl: successReturnUrl,
            cancelUrl: failReturnUrl,
            expiredAt: (int)DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds(),
            signature: signature
        );

        // 9) Gọi PayOS API
        var response = await _payOS.createPaymentLink(paymentLinkRequest);
        if (string.IsNullOrEmpty(response.checkoutUrl))
            return new BusinessResult(Const.FAIL_CREATE_CODE, "Fail", response.checkoutUrl);

        // Lưu lại discount nếu có
        await _unitOfWork.Order.UpdateAsync(order);
        await _unitOfWork.SaveAsync();

        return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Success", response.checkoutUrl);
    }
    public async Task<IBusinessResult> ProcessPaymentCallback(PaymentReturnModel request)
    {
        try
        {
            // Ưu tiên OrderId, fallback từ OrderCode (nếu bạn dùng 100000 + orderId)
            var orderId = request.OrderId > 0
                ? request.OrderId
                : (request.OrderCode.HasValue ? (int)(request.OrderCode.Value - 100000) : 0);

            if (orderId <= 0)
                return new BusinessResult(Const.FAIL_READ_CODE, "Invalid order id");

            var order = await _unitOfWork.Order.GetOrderbyIdAsync(orderId);
            if (order == null)
                return new BusinessResult(Const.NOT_FOUND_CODE, "Order not found");

            // ===== TÍNH SỐ TIỀN =====
            decimal RoundVnd(decimal v) => Math.Round(v, 0, MidpointRounding.AwayFromZero);

            var baseAmount = order.TotalAmount > 0
                ? order.TotalAmount
                : RoundVnd(order.OrderItems?.Sum(i => i.TotalPrice ?? 0) ?? 0);

            baseAmount = RoundVnd(baseAmount);

            var fullAfter10 = RoundVnd(baseAmount * 0.9m); // thanh toán toàn bộ -10%

            var paid = RoundVnd((decimal)request.Amount);  // PayOS trả VND nguyên

            // Xác định có phải payAll không
            var isPayAll = (paid == fullAfter10);

            // expected cho lần thanh toán này
            var expected = isPayAll
                ? fullAfter10
                : RoundVnd(order.Deposit ?? baseAmount);

            if (paid != expected)
                return new BusinessResult(Const.FAIL_READ_CODE, $"Amount mismatch. paid={paid}, expected={expected}");

            // Thành công khi StatusReturn = Success hoặc Code = "00"
            bool success = request.StatusReturn == PaymentReturnModelStatus.Success
                        || string.Equals(request.Code, "00", StringComparison.OrdinalIgnoreCase);

            try
            {
                // ===== CẬP NHẬT ĐƠN =====
                order.PaymentStatus = success ? "Paid" : "Failed";
                order.TransactionId = request.TransactionId;

                // Chỉ lưu DiscountAmount khi payAll thành công
                if (success && isPayAll)
                {
                    order.DiscountAmount = baseAmount - fullAfter10; // đúng 10% đã giảm
                }

                await _unitOfWork.Order.UpdateAsync(order);

                // Thêm bản ghi Payment
                if (order.Payment == null)
                    order.Payment = new List<Payment>();

                order.Payment.Add(new Payment
                {
                    OrderId = order.OrderId,
                    PaymentMethod = "PAYOS",
                    PaymentAmount = paid,
                    Status = success ? "Paid" : "Failed",
                    PaymentDate = DateTime.UtcNow
                });

                // ✅ TRỪ STOCK KHI PAYMENT SUCCESS
                if (success)
                {
                    await ReduceStockForPaidOrder(order);
                }

                await _unitOfWork.SaveAsync();

                return new BusinessResult(
                    success ? Const.SUCCESS_UPDATE_CODE : Const.FAIL_READ_CODE,
                    success ? "Payment success" : "Payment failed"
                );
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }


    #region Helper methods
    // ✅ METHOD TRỪ STOCK CHO PAID ORDER
    private async Task ReduceStockForPaidOrder(Order order)
    {
        if (order.OrderItems == null || !order.OrderItems.Any())
            return;

        foreach (var item in order.OrderItems)
        {
            // ✅ 1. ACCESSORY RIÊNG LẺ: Luôn trừ stock
            if (item.AccessoryId.HasValue && item.AccessoryQuantity > 0)
            {
                await _unitOfWork.Accessory.ReduceStockAsync(item.AccessoryId.Value, item.AccessoryQuantity ?? 0);
            }

            // ✅ 2. TERRARIUM: CHỈ TRỪ KHI GeneratedByAI = TRUE
            if (item.TerrariumVariantId.HasValue && item.TerrariumVariantQuantity > 0)
            {
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(item.TerrariumVariantId.Value);
                if (variant != null)
                {
                    var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(variant.TerrariumId);

                    // ✅ CHỈ AI TERRARIUM mới trừ accessories, Non-AI không trừ gì
                    if (terrarium != null && terrarium.GeneratedByAI)
                    {
                        await ReduceStockForAITerrarium(terrarium.TerrariumId, item.TerrariumVariantQuantity ?? 0);
                    }
                    // ❌ Non-AI terrarium (GeneratedByAI = false): KHÔNG trừ stock
                }
            }

            // ✅ 3. COMBO: Check từng item trong combo
            if (item.ComboId.HasValue)
            {
                await ReduceStockForComboItem(item);
            }
        }
    }

    // ✅ TRỪ STOCK CHO AI TERRARIUM - trừ accessories
    private async Task ReduceStockForAITerrarium(int terrariumId, int terrariumQuantity)
    {
        var terrariumAccessories = await _unitOfWork.TerrariumAccessory.GetByTerrariumIdAsync(terrariumId);

        foreach (var ta in terrariumAccessories)
        {
            // Trừ stock accessories (nguyên liệu để tạo AI terrarium)
            await _unitOfWork.Accessory.ReduceStockAsync(ta.AccessoryId, terrariumQuantity);
        }
    }

    // ✅ TRỪ STOCK CHO COMBO
    private async Task ReduceStockForComboItem(OrderItem comboOrderItem)
    {
        if (!comboOrderItem.ComboId.HasValue) return;

        var combo = await _unitOfWork.Combo.GetByIdAsync(comboOrderItem.ComboId.Value);
        if (combo?.ComboItems == null) return;

        foreach (var comboItem in combo.ComboItems)
        {
            // Accessory trong combo: Luôn trừ
            if (comboItem.AccessoryId.HasValue)
            {
                await _unitOfWork.Accessory.ReduceStockAsync(comboItem.AccessoryId.Value, 1);
            }

            if (comboItem.TerrariumVariantId.HasValue)
            {
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(comboItem.TerrariumVariantId.Value);
                if (variant != null)
                {
                    var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(variant.TerrariumId);
                    await ReduceStockForAITerrarium(terrarium.TerrariumId, 1);
                }
            }
        }
    }
    #endregion

    #region Helper methods
    private static string CreateHmacSha256(string data, string? key)
    {
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key)))
        {
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
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