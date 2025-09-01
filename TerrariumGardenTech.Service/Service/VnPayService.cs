using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Sockets;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Config;
using TerrariumGardenTech.Common.Entity;
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

            try
            {
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

                // ✅ TRỪ STOCK KHI PAYMENT SUCCESS
                await ReduceStockForPaidOrder(order);

                await _unitOfWork.SaveAsync();

                return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Payment success");
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }


    #region Helper methods

    // ✅ METHOD TRỪ STOCK CHO PAID ORDER - CHỈ CHO AI TERRARIUM
    private async Task ReduceStockForPaidOrder(Order order)
    {
        if (order.OrderItems == null || !order.OrderItems.Any())
            return;

        foreach (var item in order.OrderItems)
        {
            // ✅ 1. ACCESSORY RIÊNG LẺ: CHỈ TRỪ KHI THUỘC VỀ AI TERRARIUM
            if (item.AccessoryId.HasValue && item.AccessoryQuantity > 0)
            {
                // ✅ Check xem accessory này có thuộc về AI terrarium không
                bool shouldReduceAccessoryStock = await ShouldReduceStockForAccessory(item);
                if (shouldReduceAccessoryStock)
                {
                    await _unitOfWork.Accessory.ReduceStockAsync(item.AccessoryId.Value, item.AccessoryQuantity ?? 0);
                }
            }

            // ✅ 2. TERRARIUM VARIANT: CHỈ TRỪ KHI LÀ AI TERRARIUM
            if (item.TerrariumVariantId.HasValue && item.TerrariumVariantQuantity > 0)
            {
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(item.TerrariumVariantId.Value);
                if (variant != null)
                {
                    var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(variant.TerrariumId);
                    if (terrarium != null && terrarium.GeneratedByAI)
                    {
                        // ✅ CHỈ AI TERRARIUM MỚI TRỪ STOCK ACCESSORIES
                        await ReduceStockForAITerrariumVariant(variant.TerrariumVariantId, item.TerrariumVariantQuantity ?? 0);
                    }
                    // ❌ NON-AI TERRARIUM: KHÔNG TRỪ GÌ CẢ
                }
            }

            // ✅ 3. COMBO: CHỈ TRỪ KHI CÓ AI TERRARIUM TRONG COMBO
            if (item.ComboId.HasValue && item.ItemType == "Combo")
            {
                await ReduceStockForComboItem(item);
            }
        }
    }

    // ✅ KIỂM TRA XEM ACCESSORY CÓ NÊN TRỪ STOCK KHÔNG
    private async Task<bool> ShouldReduceStockForAccessory(OrderItem item)
    {
        // Nếu accessory đi kèm với terrarium variant
        if (item.TerrariumVariantId.HasValue)
        {
            var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(item.TerrariumVariantId.Value);
            if (variant != null)
            {
                var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(variant.TerrariumId);
                // CHỈ TRỪ KHI LÀ AI TERRARIUM
                return terrarium != null && terrarium.GeneratedByAI;
            }
        }

        // Nếu là accessory standalone (không thuộc terrarium nào)
        if (!item.TerrariumVariantId.HasValue && !item.ComboId.HasValue)
        {
            // ❌ ACCESSORY STANDALONE: KHÔNG TRỪ
            return false;
        }

        // Nếu thuộc combo
        if (item.ComboId.HasValue)
        {
            var combo = await _unitOfWork.Combo.GetByIdAsync(item.ComboId.Value);
            if (combo != null)
            {
                return await ComboContainsAITerrarium(combo);
            }
        }

        return false;
    }

    // ✅ TRỪ STOCK CHO AI TERRARIUM VARIANT - chỉ trừ accessories của variant
    private async Task ReduceStockForAITerrariumVariant(int terrariumVariantId, int variantQuantity)
    {
        // ✅ BÂY GIỜ SẼ RETURN List<TerrariumVariantAccessory> THAY VÌ List<TerrariumVariant>
        var variantAccessories = await _unitOfWork.TerrariumVariantAccessory
            .GetByTerrariumVariantId(terrariumVariantId);

        foreach (var va in variantAccessories) // va là TerrariumVariantAccessory
        {
            // ✅ BÂY GIỜ va.Quantity VÀ va.AccessoryId SẼ WORK
            int accessoryQtyToReduce = va.Quantity * variantQuantity;
            await _unitOfWork.Accessory.ReduceStockAsync(va.AccessoryId, accessoryQtyToReduce);
        }
    }


    // ✅ TRỪ STOCK CHO COMBO - CHỈ TRỪ KHI CÓ AI TERRARIUM
    private async Task ReduceStockForComboItem(OrderItem comboOrderItem)
    {
        if (!comboOrderItem.ComboId.HasValue) return;

        var combo = await _unitOfWork.Combo.GetByIdAsync(comboOrderItem.ComboId.Value);
        if (combo?.ComboItems == null) return;

        foreach (var comboItem in combo.ComboItems)
        {
            // ✅ ACCESSORY trong combo: CHỈ TRỪ KHI COMBO CÓ AI TERRARIUM
            if (comboItem.AccessoryId.HasValue)
            {
                bool comboHasAITerrarium = await ComboContainsAITerrarium(combo);
                if (comboHasAITerrarium)
                {
                    await _unitOfWork.Accessory.ReduceStockAsync(comboItem.AccessoryId.Value, comboItem.Quantity);
                }
            }

            // ✅ TERRARIUM VARIANT trong combo: CHỈ TRỪ KHI LÀ AI
            if (comboItem.TerrariumVariantId.HasValue)
            {
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(comboItem.TerrariumVariantId.Value);
                if (variant != null)
                {
                    var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(variant.TerrariumId);
                    if (terrarium != null && terrarium.GeneratedByAI)
                    {
                        // ✅ CHỈ AI TERRARIUM MỚI TRỪ ACCESSORIES
                        await ReduceStockForAITerrariumVariant(variant.TerrariumVariantId, comboItem.Quantity);
                    }
                    // ❌ NON-AI TERRARIUM: KHÔNG TRỪ GÌ CẢ
                }
            }
        }
    }

    // ✅ KIỂM TRA COMBO CÓ CHỨA AI TERRARIUM KHÔNG
    private async Task<bool> ComboContainsAITerrarium(Combo combo)
    {
        foreach (var comboItem in combo.ComboItems)
        {
            if (comboItem.TerrariumVariantId.HasValue)
            {
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(comboItem.TerrariumVariantId.Value);
                if (variant != null)
                {
                    var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(variant.TerrariumId);
                    if (terrarium != null && terrarium.GeneratedByAI)
                    {
                        return true; // Combo có chứa AI terrarium
                    }
                }
            }
        }
        return false; // Combo không có AI terrarium
    }

    #endregion




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