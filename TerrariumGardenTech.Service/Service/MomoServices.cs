using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Payment;
using TerrariumGardenTech.Common.ResponseModel.Payment;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service
{
    public class MomoServices : IMomoServices
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private readonly UnitOfWork _unitOfWork;

        public MomoServices(IConfiguration config, IHttpClientFactory httpClientFactory, UnitOfWork unitOfWork)
        {
            _config = config;
            _httpClient = httpClientFactory.CreateClient();
            _unitOfWork = unitOfWork;
        }

        public async Task<MomoQrResponse> CreateMomoPaymentUrl(MomoRequest req)
        {
            var momoSection = _config.GetSection("Momo");

            string endpoint = momoSection["PaymentUrl"];
            string partnerCode = momoSection["PartnerCode"];
            string accessKey = momoSection["AccessKey"];
            string secretKey = momoSection["SecretKey"];
            string returnUrl = momoSection["ReturnUrl"];
            string ipnUrl = momoSection["IpnUrl"];
            string requestType = "captureWallet";

            // ✅ Tính tiền phải trả từ Order
            var order = await _unitOfWork.Order.GetByIdWithOrderItemsAsync(req.OrderId);
            if (order is null) throw new Exception("Order not found");

            decimal baseAmount = order.TotalAmount > 0
                ? order.TotalAmount
                : order.OrderItems.Sum(i => i.TotalPrice ?? 0);

            decimal payable = req.PayAll
                ? Math.Round(baseAmount * 0.9m, 0, MidpointRounding.AwayFromZero)
                : (order.Deposit ?? baseAmount);

            long amount = (long)payable; // MoMo dùng VND (không *100)

            string orderId = $"{req.OrderId}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            string orderInfo = $"{req.OrderInfo} {(req.PayAll ? "(Full -10%)" : "(Partial)")}";
            string requestId = Guid.NewGuid().ToString();
            string extraData = Convert.ToBase64String(Encoding.UTF8.GetBytes("stk=0329126894"));

            string rawHash = $"accessKey={accessKey}&amount={amount}&extraData={extraData}" +
                             $"&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}" +
                             $"&partnerCode={partnerCode}&redirectUrl={returnUrl}" +
                             $"&requestId={requestId}&requestType={requestType}";

            string signature = CreateSignature(secretKey, rawHash);

            var payload = new
            {
                partnerCode,
                accessKey,
                requestId,
                amount = amount.ToString(),
                orderId,
                orderInfo,
                redirectUrl = returnUrl,
                ipnUrl,
                extraData,
                requestType,
                signature,
                lang = "vi"
            };

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseContent);
            if (doc.RootElement.TryGetProperty("payUrl", out var payUrlElement))
            {
                string payUrl = payUrlElement.GetString();
                return new MomoQrResponse
                {
                    PayUrl = payUrl,
                    QrImageBase64 = GenerateBase64QrCode(payUrl)
                };
            }

            var message = doc.RootElement.TryGetProperty("message", out var msg)
                ? msg.GetString()
                : "Unknown error from Momo";
            throw new Exception($"Momo error: {message}");
        }
        private string CreateSignature(string key, string rawData)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
        private string GenerateBase64QrCode(string content)
        {
            using var qrGenerator = new QRCoder.QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(content, QRCoder.QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new QRCoder.PngByteQRCode(qrCodeData);
            byte[] qrCodeBytes = qrCode.GetGraphic(20);
            return Convert.ToBase64String(qrCodeBytes);
        }


        

        // NEW: xử lý return URL (user được redirect về)
        public async Task<IBusinessResult> MomoReturnExecute(IQueryCollection query)
        {
            try
            {
                var accessKey = _config["Momo:AccessKey"];   // BẮT BUỘC có trong raw
                var secretKey = _config["Momo:SecretKey"];

                string Get(string k) => query.TryGetValue(k, out var v) ? v.ToString() : string.Empty;

                // MoMo yêu cầu thứ tự CỐ ĐỊNH (không dùng SortedDictionary)
                var raw = string.Join("&", new[]
                {
                    $"accessKey={accessKey}",
                    $"amount={Get("amount")}",
                    $"extraData={Get("extraData")}",
                    $"message={Get("message")}",
                    $"orderId={Get("orderId")}",
                    $"orderInfo={Get("orderInfo")}",
                    $"orderType={Get("orderType")}",       // có thể rỗng
                    $"partnerCode={Get("partnerCode")}",
                    $"payType={Get("payType")}",           // có thể rỗng
                    $"requestId={Get("requestId")}",
                    $"responseTime={Get("responseTime")}", // có thể rỗng
                    $"resultCode={Get("resultCode")}",
                    $"transId={Get("transId")}"
                });

                var calcSig = CreateSignature(secretKey, raw); // HMACSHA256 UTF8 -> hex lowercase
                var signature = Get("signature");

                if (!string.Equals(calcSig, signature, StringComparison.OrdinalIgnoreCase))
                    return new BusinessResult(Const.FAIL_READ_CODE, "Invalid signature");

                // ---- từ đây trở xuống giữ logic của bạn ----
                var orderIdRaw = Get("orderId");
                var idStr = orderIdRaw?.Split('_').FirstOrDefault();
                if (!int.TryParse(idStr, out var orderId))
                    return new BusinessResult(Const.FAIL_READ_CODE, "Invalid orderId");

                var order = await _unitOfWork.Order.GetOrderbyIdAsync(orderId);
                if (order == null) return new BusinessResult(Const.NOT_FOUND_CODE, "Order not found");

                decimal RoundVnd(decimal v) => Math.Round(v, 0, MidpointRounding.AwayFromZero);
                   
                var baseAmount = order.TotalAmount > 0
                    ? order.TotalAmount
                    : RoundVnd(order.OrderItems?.Sum(i => i.TotalPrice ?? 0) ?? 0);

                baseAmount = RoundVnd(baseAmount);
                var fullAfter10 = RoundVnd(baseAmount * 0.9m);

                if (!long.TryParse(Get("amount"), out var amt))
                    return new BusinessResult(Const.FAIL_READ_CODE, "Invalid amount");

                var paid = RoundVnd(amt);
                var isPayAll = (paid == fullAfter10);
                var expected = isPayAll ? fullAfter10 : RoundVnd(order.Deposit ?? baseAmount);

                if (paid != expected)
                    return new BusinessResult(Const.FAIL_READ_CODE, $"Amount mismatch. paid={paid}, expected={expected}");

                var success = Get("resultCode") == "0";
                order.PaymentStatus = success ? "Paid" : "Failed";
                order.TransactionId = Get("transId");
                // *** CÁCH 3: Chỉ áp dụng discount khi thanh toán đủ (isPayAll) ***
                if (success && isPayAll)
                {
                    order.DiscountAmount = RoundVnd(baseAmount * 0.1m); // luôn đúng 10% và làm tròn VND
                }

                await _unitOfWork.Order.UpdateAsync(order);
                order.Payment ??= new List<Payment>();
                order.Payment.Add(new Payment
                {
                    OrderId = order.OrderId,
                    PaymentMethod = "MOMO",
                    PaymentAmount = paid,
                    Status = success ? "Paid" : "Failed",
                    PaymentDate = DateTime.UtcNow
                });
                await _unitOfWork.SaveAsync();

                return new BusinessResult(
                    success ? Const.SUCCESS_UPDATE_CODE : Const.FAIL_READ_CODE,
                    success ? "Payment success" : "Payment failed"
                );
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        // NEW: xử lý IPN (server→server)
        public async Task<IBusinessResult> MomoIpnExecute(MomoIpnModel body)
        {
            try
            {
                var secretKey = _config["Momo:SecretKey"];

                var map = new Dictionary<string, string>
                {
                    ["amount"] = body.amount,
                    ["extraData"] = body.extraData,
                    ["message"] = body.message,
                    ["orderId"] = body.orderId,
                    ["orderInfo"] = body.orderInfo,
                    ["orderType"] = body.orderType,
                    ["partnerCode"] = body.partnerCode,
                    ["payType"] = body.payType,
                    ["requestId"] = body.requestId,
                    ["responseTime"] = body.responseTime,
                    ["resultCode"] = body.resultCode,
                    ["transId"] = body.transId
                };

                var raw = BuildRawSignature(map);
                var calcSig = CreateSignature(secretKey, raw);

                if (!string.Equals(calcSig, body.signature, StringComparison.OrdinalIgnoreCase))
                    return new BusinessResult(Const.FAIL_READ_CODE, "Invalid signature");

                var idStr = body.orderId?.Split('_').FirstOrDefault();
                if (!int.TryParse(idStr, out var orderId))
                    return new BusinessResult(Const.FAIL_READ_CODE, "Invalid orderId");

                var order = await _unitOfWork.Order.GetOrderbyIdAsync(orderId);
                if (order == null) return new BusinessResult(Const.NOT_FOUND_CODE, "Order not found");

                var expected = ComputeExpectedPayable(order);

                if (!long.TryParse(body.amount, out var amt))
                    return new BusinessResult(Const.FAIL_READ_CODE, "Invalid amount");
                var paid = (decimal)amt;

                if (paid != expected)
                    return new BusinessResult(Const.FAIL_READ_CODE, $"Amount mismatch. paid={paid}, expected={expected}");

                var success = body.resultCode == "0";
                order.PaymentStatus = success ? "Paid" : "Failed";
                order.TransactionId = body.transId;
                await _unitOfWork.Order.UpdateAsync(order);

                if (order.Payment == null) order.Payment = new List<Payment>();
                order.Payment.Add(new Payment
                {
                    OrderId = order.OrderId,
                    PaymentMethod = "MOMO",
                    PaymentAmount = paid,
                    Status = success ? "Paid" : "Failed",
                    PaymentDate = DateTime.UtcNow
                });

                await _unitOfWork.SaveAsync();

                return new BusinessResult(success ? Const.SUCCESS_UPDATE_CODE : Const.FAIL_READ_CODE,
                                          success ? "Payment success (IPN)" : "Payment failed (IPN)");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }
        #region Helper Methods

        // NEW: build raw data để verify chữ ký (sort key asc, bỏ "signature")
        private static string BuildRawSignature(IDictionary<string, string> data)
        {
            return string.Join("&",
                data
                  .Where(kv => !string.IsNullOrEmpty(kv.Value) && kv.Key != "signature")
                  .OrderBy(kv => kv.Key, StringComparer.Ordinal)
                  .Select(kv => $"{kv.Key}={kv.Value}")
            );
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
}
