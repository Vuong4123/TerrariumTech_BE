using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
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

            string endpoint = momoSection["PaymentUrl"]!;
            string partnerCode = momoSection["PartnerCode"]!;
            string accessKey = momoSection["AccessKey"]!;
            string secretKey = momoSection["SecretKey"]!;
            string returnUrl = momoSection["ReturnUrl"]!;
            string ipnUrl = momoSection["IpnUrl"]!;
            string requestType = "captureWallet";

            // 1) Tính tiền phải trả
            var order = await _unitOfWork.Order.GetByIdWithOrderItemsAsync(req.OrderId)
                         ?? throw new Exception("Order not found");

            decimal baseAmount = order.TotalAmount > 0
                ? order.TotalAmount
                : order.OrderItems.Sum(i => i.TotalPrice ?? 0);

            decimal payable = req.PayAll
                ? Math.Round(baseAmount * 0.9m, 0, MidpointRounding.AwayFromZero)
                : (order.Deposit ?? baseAmount);

            if (payable <= 0) throw new Exception("Invalid amount");

            // 2) amount phải là số nguyên (VND) – KHÔNG ToString theo culture
            long amount = (long)payable;

            // 3) orderId/requestId tuyệt đối duy nhất
            string orderId = $"{req.OrderId}-{Guid.NewGuid():N}";
            string requestId = Guid.NewGuid().ToString("N");

            // 4) orderInfo an toàn – loại bỏ ký tự phá vỡ chuỗi ký
            string orderInfoRaw = $"{req.OrderInfo} {(req.PayAll ? "(Full -10%)" : "(Partial)")}";
            string orderInfo = SanitizeOrderInfo(orderInfoRaw); // bỏ &, =, \r\n,…

            // 5) extraData base64 (có thể để rỗng)
            string extraData = Convert.ToBase64String(Encoding.UTF8.GetBytes("stk=0329126894"));

            // 6) raw signature – đúng thứ tự và giá trị
            var fields = new (string k, string v)[]
            {
                ("accessKey",   accessKey),
                ("amount",      amount.ToString(CultureInfo.InvariantCulture)),
                ("extraData",   extraData),
                ("ipnUrl",      ipnUrl),
                ("orderId",     orderId),
                ("orderInfo",   orderInfo),
                ("partnerCode", partnerCode),
                ("redirectUrl", returnUrl),
                ("requestId",   requestId),
                ("requestType", requestType),
            };
            string rawHash = string.Join("&", fields.Select(p => $"{p.k}={p.v}"));

            string signature = HmacSha256(secretKey, rawHash);

            // 7) Gửi payload – để amount là số, không phải string
            var payload = new
            {
                partnerCode,
                accessKey,
                requestId,
                amount,
                orderId,
                orderInfo,
                redirectUrl = returnUrl,
                ipnUrl,
                extraData,
                requestType,
                signature,
                lang = "vi"
            };

            using var reqMsg = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(payload)
            };

            // timeout phòng kẹt
            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            var httpRes = await _httpClient.SendAsync(reqMsg);
            var body = await httpRes.Content.ReadAsStringAsync();

            if (!httpRes.IsSuccessStatusCode)
                throw new Exception($"MoMo HTTP {(int)httpRes.StatusCode}: {body}");

            var momo = JsonSerializer.Deserialize<MomoCreateResponse>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("Cannot parse MoMo response");

            // 8) Kiểm tra resultCode
            if (momo.ResultCode != 0)
                throw new Exception($"MoMo error {momo.ResultCode}: {momo.Message ?? "Unknown"}");

            var payUrl = momo.PayUrl ?? momo.Deeplink ?? momo.QrCodeUrl;
            if (string.IsNullOrWhiteSpace(payUrl))
                throw new Exception("MoMo response missing payUrl");

            return new MomoQrResponse
            {
                PayUrl = payUrl!,
                QrImageBase64 = GenerateBase64QrCode(payUrl!)
            };
        }

        

        // NEW: Create MoMo link for Wallet Top-up with metadata (userId + random orderId)
        public async Task<MomoQrResponse> CreateMomoWalletTopupUrl(MomoWalletTopupRequest req)
        {
            var momoSection = _config.GetSection("Momo");

            string endpoint = momoSection["PaymentUrl"];
            string partnerCode = momoSection["PartnerCode"];
            string accessKey = momoSection["AccessKey"];
            string secretKey = momoSection["SecretKey"];
            string returnUrl = momoSection["WalletReturnUrl"] ?? momoSection["ReturnUrl"];
            string ipnUrl = momoSection["WalletIpnUrl"] ?? momoSection["IpnUrl"];
            string requestType = "captureWallet";

            long amount = req.Amount; // VND

            string randomId = Guid.NewGuid().ToString("N").Substring(0, 12);
            string orderId = $"WL_{randomId}"; // independent from Order table
            string orderInfo = string.IsNullOrWhiteSpace(req.Description) ? "Wallet Top-up" : req.Description!;
            string requestId = Guid.NewGuid().ToString();

            var meta = new { userId = req.UserId };
            string extraData = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(meta)));

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

            var json = JsonSerializer.Serialize(payload);
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
                var idStr = orderIdRaw?.Split('-').FirstOrDefault();
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

                if (!long.TryParse(Get("amount"), out var amt) || amt <= 0)
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

        // NEW: IPN for Wallet top-up. Verify signature; DO NOT credit to avoid double-processing; rely on Return callback per requirement.
        public async Task<IBusinessResult> MomoWalletIpnExecute(MomoIpnModel body)
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

                // If needed, we could parse userId/amount here, but to avoid double-crediting we simply acknowledge.
                return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Verified");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        // NEW: Return for Wallet top-up — verify signature, parse userId and amount, then credit wallet points (1000 VND => 1 point)
        public async Task<IBusinessResult> MomoWalletReturnExecute(IQueryCollection query)
        {
            try
            {
                var accessKey = _config["Momo:AccessKey"];
                var secretKey = _config["Momo:SecretKey"];

                string Get(string k) => query.TryGetValue(k, out var v) ? v.ToString() : string.Empty;

                var raw = string.Join("&", new[]
                {
                    $"accessKey={accessKey}",
                    $"amount={Get("amount")}",
                    $"extraData={Get("extraData")}",
                    $"message={Get("message")}",
                    $"orderId={Get("orderId")}",
                    $"orderInfo={Get("orderInfo")}",
                    $"orderType={Get("orderType")}",
                    $"partnerCode={Get("partnerCode")}",
                    $"payType={Get("payType")}",
                    $"requestId={Get("requestId")}",
                    $"responseTime={Get("responseTime")}",
                    $"resultCode={Get("resultCode")}",
                    $"transId={Get("transId")}"
                });

                var calcSig = CreateSignature(secretKey, raw);
                var signature = Get("signature");
                if (!string.Equals(calcSig, signature, StringComparison.OrdinalIgnoreCase))
                    return new BusinessResult(Const.FAIL_READ_CODE, "Invalid signature");

                if (!long.TryParse(Get("amount"), out var amt))
                    return new BusinessResult(Const.FAIL_READ_CODE, "Invalid amount");

                int userId = 0;
                var extra = Get("extraData");
                if (!string.IsNullOrEmpty(extra))
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(Convert.FromBase64String(extra));
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("userId", out var userIdEl))
                            userId = userIdEl.GetInt32();
                    }
                    catch { }
                }

                if (userId <= 0)
                    return new BusinessResult(Const.FAIL_READ_CODE, "Invalid userId in extraData");

                var success = Get("resultCode") == "0";
                if (!success)
                    return new BusinessResult(Const.FAIL_READ_CODE, "Payment failed");

                var wallet = await _unitOfWork.Wallet.FindOneAsync(w => w.UserId == userId && w.WalletType == "User");
                if (wallet == null)
                {
                    wallet = new Wallet { UserId = userId, WalletType = "User", Balance = 0 };
                    await _unitOfWork.Wallet.CreateAsync(wallet);
                }

                var points = (decimal)amt / 1000m;
                wallet.Balance += points;

                await _unitOfWork.WalletTransactionRepository.CreateAsync(new WalletTransaction
                {
                    WalletId = wallet.WalletId,
                    Amount = points,
                    Type = "Deposit",
                    CreatedDate = DateTime.UtcNow,
                    OrderId = null
                });

                await _unitOfWork.SaveAsync();

                return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Wallet top-up success");
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

        // === Helpers ===
        private static string HmacSha256(string key, string raw)
        {
            using var h = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hash = h.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        private static string SanitizeOrderInfo(string s)
        {
            if (string.IsNullOrEmpty(s)) return "Thanh toan don hang";
            // Loại bỏ ký tự có thể phá format rawHash (&, =)
            s = s.Replace("&", " ")
                 .Replace("=", " ")
                 .Replace("\r", " ")
                 .Replace("\n", " ");
            // Giới hạn theo spec (thường 255)
            return s.Length > 250 ? s[..250] : s;
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


        #endregion
    }

    // Model nhỏ để đọc phản hồi MoMo
    public sealed class MomoCreateResponse
    {
        public int? ResultCode { get; set; }     // 0 = success
        public string? Message { get; set; }
        public string? PayUrl { get; set; }
        public string? Deeplink { get; set; }
        public string? QrCodeUrl { get; set; }
    }

}
