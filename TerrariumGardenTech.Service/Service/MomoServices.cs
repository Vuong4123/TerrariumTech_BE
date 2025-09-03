using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.Enums;
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

        public async Task<MomoQrResponse> CreateMomoMembershipDirectPaymentUrl(DirectPaymentRequest req)
        {
            var momo = _config.GetSection("Momo");
            string endpoint = momo["PaymentUrl"]!;
            string partnerCode = momo["PartnerCode"]!;
            string accessKey = momo["AccessKey"]!;
            string secretKey = momo["SecretKey"]!;
            string returnUrl = momo["MembershipReturnUrl"] ?? momo["ReturnUrl"]!;
            string ipnUrl = momo["MembershipIpnUrl"] ?? momo["IpnUrl"]!;
            string requestType = "captureWallet";

            // Lấy gói và tiền phải trả (VND, làm tròn)
            var package = await _unitOfWork.MembershipPackageRepository.GetByIdAsync(req.PackageId)
                          ?? throw new Exception("Membership package not found");
            long amount = (long)Math.Round(package.Price, 0, MidpointRounding.AwayFromZero);

            // orderId/requestId độc lập hệ Order
            string orderId = $"MB_{req.UserId}_{req.PackageId}_{Guid.NewGuid():N}";
            string requestId = Guid.NewGuid().ToString("N");
            string orderInfo = SanitizeOrderInfo($"Membership {package.Type} ({package.Id})");

            // extraData: đủ thông tin để tạo Membership khi callback
            string extraData = Convert.ToBase64String(Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(new
                {
                    userId = req.UserId,
                    packageId = req.PackageId,
                    startDate = req.StartDate.ToString("o")
                })
            ));

            // Raw signature đúng thứ tự theo MoMo v2
            string rawHash =
                $"accessKey={accessKey}&amount={amount}&extraData={extraData}" +
                $"&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}" +
                $"&partnerCode={partnerCode}&redirectUrl={returnUrl}" +
                $"&requestId={requestId}&requestType={requestType}";

            string signature = CreateSignature(secretKey, rawHash);

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
            { Content = JsonContent.Create(payload) };

            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            var httpRes = await _httpClient.SendAsync(reqMsg);
            var body = await httpRes.Content.ReadAsStringAsync();

            if (!httpRes.IsSuccessStatusCode)
                throw new Exception($"MoMo HTTP {(int)httpRes.StatusCode}: {body}");

            var momoRes = JsonSerializer.Deserialize<MomoCreateResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                          ?? throw new Exception("Cannot parse MoMo response");

            if (momoRes.ResultCode != 0)
                throw new Exception($"MoMo error {momoRes.ResultCode}: {momoRes.Message ?? "Unknown"}");

            var payUrl = momoRes.PayUrl ?? momoRes.Deeplink ?? momoRes.QrCodeUrl;
            if (string.IsNullOrWhiteSpace(payUrl))
                throw new Exception("MoMo response missing payUrl");

            return new MomoQrResponse
            {
                PayUrl = payUrl!,
                QrImageBase64 = GenerateBase64QrCode(payUrl!)
            };
        }

        // 2.2) ReturnUrl cho Membership: verify & tạo Membership (idempotent)
        public async Task<IBusinessResult> MomoMembershipReturnExecute(IQueryCollection query)
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
                if (!string.Equals(calcSig, Get("signature"), StringComparison.OrdinalIgnoreCase))
                    return new BusinessResult(Const.FAIL_READ_CODE, "Invalid signature");

                if (Get("resultCode") != "0")
                    return new BusinessResult(Const.FAIL_READ_CODE, "Payment failed");

                if (!long.TryParse(Get("amount"), out var amt) || amt <= 0)
                    return new BusinessResult(Const.FAIL_READ_CODE, "Invalid amount");

                // Giải mã metadata
                int userId, packageId; DateTime startDate;
                try
                {
                    var json = Encoding.UTF8.GetString(Convert.FromBase64String(Get("extraData")));
                    using var doc = JsonDocument.Parse(json);
                    userId = doc.RootElement.GetProperty("userId").GetInt32();
                    packageId = doc.RootElement.GetProperty("packageId").GetInt32();
                    startDate = DateTime.Parse(
                        doc.RootElement.GetProperty("startDate").GetString()!,
                        null, DateTimeStyles.RoundtripKind
                    );
                }
                catch
                {
                    return new BusinessResult(Const.FAIL_READ_CODE, "Invalid extraData");
                }

                // Đối chiếu số tiền với giá gói
                var pkg = await _unitOfWork.MembershipPackageRepository.GetByIdAsync(packageId);
                if (pkg == null) return new BusinessResult(Const.NOT_FOUND_CODE, "Package not found");
                long expected = (long)Math.Round(pkg.Price, 0, MidpointRounding.AwayFromZero);
                if (amt != expected)
                    return new BusinessResult(Const.FAIL_READ_CODE, $"Amount mismatch. paid={amt}, expected={expected}");

                // Idempotent: nếu đã có Membership Active tương ứng thì coi như thành công
                var existed = await _unitOfWork.MemberShip.FindAsync(m =>
                    m.UserId == userId && m.PackageId == packageId &&
                    m.StartDate == startDate && m.Status == CommonData.AccountStatus.Active);

                int membershipId;
                if (existed.Any())
                {
                    membershipId = existed.First().MembershipId;
                }
                else
                {
                    var membership = new Membership
                    {
                        UserId = userId,
                        PackageId = pkg.Id,
                        Price = pkg.Price,
                        StartDate = startDate,
                        EndDate = startDate.AddDays(pkg.DurationDays),
                        StatusEnum = MembershipStatus.Active
                    };
                    await _unitOfWork.MemberShip.CreateAsync(membership);
                    await _unitOfWork.SaveAsync();
                    membershipId = membership.MembershipId;
                }

                // Không tạo/ghi Order, chỉ trả SUCCESS
                return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Membership payment success",
                                          new { MembershipId = membershipId });
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        // 2.3) IPN cho Membership: chỉ verify; không ghi DB để tránh double-processing
        public async Task<IBusinessResult> MomoMembershipIpnExecute(MomoIpnModel body)
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
                var raw = BuildRawSignature(map); // helper đã có sẵn
                var calcSig = CreateSignature(secretKey, raw);
                if (!string.Equals(calcSig, body.signature, StringComparison.OrdinalIgnoreCase))
                    return new BusinessResult(Const.FAIL_READ_CODE, "Invalid signature");

                // (tuỳ chọn) kiểm tra amount khớp giá gói từ extraData
                try
                {
                    if (!string.IsNullOrEmpty(body.extraData))
                    {
                        var json = Encoding.UTF8.GetString(Convert.FromBase64String(body.extraData));
                        using var doc = JsonDocument.Parse(json);
                        var packageId = doc.RootElement.GetProperty("packageId").GetInt32();
                        var pkg = await _unitOfWork.MembershipPackageRepository.GetByIdAsync(packageId);
                        if (pkg != null && long.TryParse(body.amount, out var amt))
                        {
                            var expected = (long)Math.Round(pkg.Price, 0, MidpointRounding.AwayFromZero);
                            if (amt != expected)
                                return new BusinessResult(Const.FAIL_READ_CODE, "Amount mismatch");
                        }
                    }
                }
                catch { /* ignore */ }

                return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Verified");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
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

            // 1) Lấy đơn hàng
            var order = await _unitOfWork.Order.GetByIdWithOrderItemsAsync(req.OrderId)
                         ?? throw new Exception("Order not found");

            // Base (phòng khi FE không gửi FinalAmount)
            decimal baseAmount = order.TotalAmount > 0
                ? order.TotalAmount
                : order.OrderItems.Sum(i => i.TotalPrice ?? 0m);

            // 2) Số tiền thanh toán: ưu tiên dùng số FE đã tính (FinalAmount)
            decimal RoundVnd(decimal v) => Math.Round(v, 0, MidpointRounding.AwayFromZero);

            decimal fallback = req.PayAll ? baseAmount : (order.Deposit ?? baseAmount);
            decimal payableDec = RoundVnd(req.FinalAmount.HasValue && req.FinalAmount.Value > 0
                                          ? req.FinalAmount.Value
                                          : fallback);

            if (payableDec <= 0) throw new Exception("Invalid amount");

            // 3) amount phải là số nguyên (VND)
            long amount = (long)payableDec;

            // 4) orderId/requestId duy nhất
            string orderId = $"{req.OrderId}-{Guid.NewGuid():N}";
            string requestId = Guid.NewGuid().ToString("N");

            // 5) orderInfo gọn, không đề cập 10% ở BE
            string orderInfoRaw = $"{SanitizeOrderInfo(req.OrderInfo)} {(req.PayAll ? "(Full)" : "(Partial)")}";
            string orderInfo = orderInfoRaw;

            // 6) extraData: nhúng thông tin để BE đối soát (voucherId, finalAmount từ FE)
            // -> BE sẽ đọc ở return và kiểm tra khớp với policy.
            var extraObj = new
            {
                voucherId = req.VoucherId,
                finalAmount = payableDec, // FE đã tính
                client = "web",      // tuỳ bạn thích log
                ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            string extraJson = JsonSerializer.Serialize(extraObj);
            string extraData = Convert.ToBase64String(Encoding.UTF8.GetBytes(extraJson));

            // 7) raw signature – đúng thứ tự Momo (tuỳ phiên bản API của bạn)
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

            // 8) Payload
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

            _httpClient.Timeout = TimeSpan.FromSeconds(30);

            var httpRes = await _httpClient.SendAsync(reqMsg);
            var body = await httpRes.Content.ReadAsStringAsync();

            if (!httpRes.IsSuccessStatusCode)
                throw new Exception($"MoMo HTTP {(int)httpRes.StatusCode}: {body}");

            var momo = JsonSerializer.Deserialize<MomoCreateResponse>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("Cannot parse MoMo response");

            if (momo.ResultCode != 0)
                throw new Exception($"MoMo error {momo.ResultCode}: {momo.Message ?? "Unknown"}");

            var payUrl = momo.PayUrl ?? momo.Deeplink ?? momo.QrCodeUrl
                         ?? throw new Exception("MoMo response missing payUrl");

            return new MomoQrResponse
            {
                PayUrl = payUrl,
                QrImageBase64 = GenerateBase64QrCode(payUrl)
            };
        }

        public async Task<IBusinessResult> MomoReturnExecute(IQueryCollection query)
        {
            try
            {
                var accessKey = _config["Momo:AccessKey"];
                var secretKey = _config["Momo:SecretKey"];

                string Get(string k) => query.TryGetValue(k, out var v) ? v.ToString() : string.Empty;

                // MoMo yêu cầu thứ tự CỐ ĐỊNH
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

                var orderIdRaw = Get("orderId");
                var idStr = orderIdRaw?.Split('-').FirstOrDefault();
                if (!int.TryParse(idStr, out var orderId))
                    return new BusinessResult(Const.FAIL_READ_CODE, "Invalid orderId");

                var order = await _unitOfWork.Order.GetOrderbyIdAsync(orderId);
                if (order == null)
                    return new BusinessResult(Const.NOT_FOUND_CODE, "Order not found");

                // ===== Helpers =====
                decimal RoundVnd(decimal v) => Math.Round(v, 0, MidpointRounding.AwayFromZero);
                DateTime UtcNow() => DateTime.UtcNow;

                // Base gốc để tính giảm (ưu tiên OriginalAmount)
                decimal original =
                    (order.OriginalAmount.HasValue && order.OriginalAmount.Value > 0)
                        ? RoundVnd(order.OriginalAmount.Value)
                        : RoundVnd(order.TotalAmount > 0
                            ? order.TotalAmount
                            : (order.OrderItems?.Sum(i => i.TotalPrice ?? 0m) ?? 0m));

                // MoMo amount
                if (!long.TryParse(Get("amount"), out var amt) || amt <= 0)
                    return new BusinessResult(Const.FAIL_READ_CODE, "Invalid amount");
                var paid = RoundVnd(amt);

                // Partial vs Full
                decimal depositAmount = RoundVnd(order.Deposit ?? 0m);
                bool isPartial = (depositAmount > 0) && (paid == depositAmount);
                bool isPayAll = !isPartial;

                // Lấy voucherId (ưu tiên từ order; nếu không có, thử query)
                int? voucherId = order.VoucherId;
                if (!voucherId.HasValue || voucherId.Value <= 0)
                {
                    var vStr = Get("voucherId");
                    if (int.TryParse(vStr, out var vid) && vid > 0) voucherId = vid;
                }

                // Validate & tính giảm từ voucher (chỉ áp khi FULL)
                async Task<(decimal discount, string reason, bool ok, Voucher voucher)> GetVoucherDiscountAndValidateAsync(
                    int? vid, decimal baseForCalc, int userId)
                {
                    if (!vid.HasValue || vid.Value <= 0)
                        return (0m, "No voucher", true, null);

                    var voucher = await _unitOfWork.Voucher.GetByIdAsync(vid.Value);
                    if (voucher == null)
                        return (0m, "Voucher not found", false, null);

                    var now = UtcNow();

                    // Trạng thái
                    if (!string.Equals(voucher.Status, "Active", StringComparison.OrdinalIgnoreCase))
                        return (0m, "Voucher inactive", false, voucher);

                    // Thời gian hiệu lực
                    if (voucher.ValidFrom.HasValue && now < voucher.ValidFrom.Value.ToUniversalTime())
                        return (0m, "Voucher not started", false, voucher);
                    if (voucher.ValidTo.HasValue && now > voucher.ValidTo.Value.ToUniversalTime())
                        return (0m, "Voucher expired", false, voucher);

                    // Hạn mức usage tổng
                    if (voucher.TotalUsage > 0 && voucher.RemainingUsage <= 0)
                        return (0m, "Voucher out of stock", false, voucher);

                    // Min order (theo original)
                    if (voucher.MinOrderAmount.HasValue && baseForCalc < voucher.MinOrderAmount.Value)
                        return (0m, "Order below min amount", false, voucher);

                    // Voucher cá nhân
                    if (voucher.IsPersonal)
                    {
                        if (string.IsNullOrWhiteSpace(voucher.TargetUserId) || voucher.TargetUserId != userId.ToString())
                            return (0m, "Voucher not for this user", false, voucher);
                    }

                    // Giảm = Percent + Amount (nullable-safe)
                    decimal percent = voucher.DiscountPercent.GetValueOrDefault(0m);
                    decimal amount = voucher.DiscountAmount.GetValueOrDefault(0m);

                    decimal discount = 0m;
                    if (percent > 0) discount += RoundVnd(baseForCalc * (percent / 100m));
                    if (amount > 0) discount += RoundVnd(amount);

                    if (discount > baseForCalc) discount = baseForCalc;
                    if (discount <= 0) return (0m, "Voucher discount is zero", false, voucher);

                    return (discount, null, true, voucher);
                }

                var (voucherDiscount, _, voucherOk, voucherEntity) =
                    await GetVoucherDiscountAndValidateAsync(voucherId, original, order.UserId);

                // Áp voucher chỉ khi FULL (nếu muốn áp cả PARTIAL, đổi điều kiện tại đây)
                decimal voucherApplied = (isPayAll && voucherOk) ? RoundVnd(voucherDiscount) : 0m;

                // === 10% tính trên (Original - Voucher), CHỈ khi FULL ===
                decimal percentBase = original - voucherApplied;
                if (percentBase < 0) percentBase = 0m;
                decimal discountTenPercent = isPayAll ? RoundVnd(percentBase * 0.10m) : 0m;

                // EXPECTED
                decimal expectedFull = RoundVnd(original - voucherApplied - discountTenPercent);
                decimal expectedPartial = depositAmount > 0 ? depositAmount : RoundVnd(original);

                // Cho phép tolerance 1 VND
                decimal expected = isPayAll ? expectedFull : expectedPartial;
                bool amountOk = Math.Abs(paid - expected) <= 1m;

                // Kết quả trả về từ MoMo
                bool momoOk = Get("resultCode") == "0";

                // Chỉ success khi cả MoMo ok và số tiền hợp lệ
                bool success = momoOk && amountOk;

                // Lý do thất bại
                string failReason = null;
                if (!amountOk)
                {
                    failReason = $"Amount mismatch. paid={paid}, expected={expected}";
                }

                // ===== Ghi nhận =====
                if (success)
                {
                    order.PaymentStatus = "Paid";
                    order.TransactionId = Get("transId");

                    if (isPayAll)
                    {
                        // Lưu DiscountAmount = 10% sau voucher (không gồm voucher)
                        order.DiscountAmount = discountTenPercent;

                        // Phản ánh đúng số KH đã trả
                        order.TotalAmount = paid;

                        // Nếu có voucher dùng được -> trừ RemainingUsage
                        if (voucherEntity != null && voucherId.HasValue && voucherId.Value > 0 && voucherApplied > 0)
                        {
                            if (voucherEntity.TotalUsage > 0 && voucherEntity.RemainingUsage > 0)
                            {
                                voucherEntity.RemainingUsage -= 1;
                                await _unitOfWork.Voucher.UpdateAsync(voucherEntity);
                            }
                        }
                    }

                    await _unitOfWork.Order.UpdateAsync(order);

                    order.Payment ??= new List<Payment>();
                    order.Payment.Add(new Payment
                    {
                        OrderId = order.OrderId,
                        PaymentMethod = "MOMO",
                        PaymentAmount = paid,
                        Status = "Paid",
                        PaymentDate = UtcNow()
                    });
                    await _unitOfWork.SaveAsync();

                    return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Payment success");
                }
                else
                {
                    order.PaymentStatus = "Failed";
                    order.TransactionId = Get("transId");
                    await _unitOfWork.Order.UpdateAsync(order);

                    order.Payment ??= new List<Payment>();
                    order.Payment.Add(new Payment
                    {
                        OrderId = order.OrderId,
                        PaymentMethod = "MOMO",
                        PaymentAmount = paid,
                        Status = "Failed",
                        PaymentDate = UtcNow()
                    });

                    await _unitOfWork.SaveAsync();

                    if (!amountOk && momoOk)
                        return new BusinessResult(Const.FAIL_READ_CODE, failReason);

                    return new BusinessResult(Const.FAIL_READ_CODE, "Payment failed");
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }
        #region backup

        //    public async Task<MomoQrResponse> CreateMomoPaymentUrl(MomoRequest req)
        //    {
        //        var momoSection = _config.GetSection("Momo");

        //        string endpoint = momoSection["PaymentUrl"]!;
        //        string partnerCode = momoSection["PartnerCode"]!;
        //        string accessKey = momoSection["AccessKey"]!;
        //        string secretKey = momoSection["SecretKey"]!;
        //        string returnUrl = momoSection["ReturnUrl"]!;
        //        string ipnUrl = momoSection["IpnUrl"]!;
        //        string requestType = "captureWallet";

        //        // 1) Tính tiền phải trả
        //        var order = await _unitOfWork.Order.GetByIdWithOrderItemsAsync(req.OrderId)
        //                     ?? throw new Exception("Order not found");

        //        decimal baseAmount = order.TotalAmount > 0
        //            ? order.TotalAmount
        //            : order.OrderItems.Sum(i => i.TotalPrice ?? 0);

        //        //decimal payable = req.PayAll
        //        //    ? Math.Round(baseAmount * 0.9m, 0, MidpointRounding.AwayFromZero)
        //        //    : (order.Deposit ?? baseAmount);
        //        decimal payable = req.PayAll
        //? baseAmount
        //: (order.Deposit ?? baseAmount);

        //        if (payable <= 0) throw new Exception("Invalid amount");

        //        // 2) amount phải là số nguyên (VND) – KHÔNG ToString theo culture
        //        long amount = (long)payable;

        //        // 3) orderId/requestId tuyệt đối duy nhất
        //        string orderId = $"{req.OrderId}-{Guid.NewGuid():N}";
        //        string requestId = Guid.NewGuid().ToString("N");

        //        // 4) orderInfo an toàn – loại bỏ ký tự phá vỡ chuỗi ký
        //        string orderInfoRaw = $"{req.OrderInfo} {(req.PayAll ? "(Full -10%)" : "(Partial)")}";
        //        string orderInfo = SanitizeOrderInfo(orderInfoRaw); // bỏ &, =, \r\n,…

        //        // 5) extraData base64 (có thể để rỗng)
        //        string extraData = Convert.ToBase64String(Encoding.UTF8.GetBytes("stk=0329126894"));

        //        // 6) raw signature – đúng thứ tự và giá trị
        //        var fields = new (string k, string v)[]
        //        {
        //            ("accessKey",   accessKey),
        //            ("amount",      amount.ToString(CultureInfo.InvariantCulture)),
        //            ("extraData",   extraData),
        //            ("ipnUrl",      ipnUrl),
        //            ("orderId",     orderId),
        //            ("orderInfo",   orderInfo),
        //            ("partnerCode", partnerCode),
        //            ("redirectUrl", returnUrl),
        //            ("requestId",   requestId),
        //            ("requestType", requestType),
        //        };
        //        string rawHash = string.Join("&", fields.Select(p => $"{p.k}={p.v}"));

        //        string signature = HmacSha256(secretKey, rawHash);

        //        // 7) Gửi payload – để amount là số, không phải string
        //        var payload = new
        //        {
        //            partnerCode,
        //            accessKey,
        //            requestId,
        //            amount,
        //            orderId,
        //            orderInfo,
        //            redirectUrl = returnUrl,
        //            ipnUrl,
        //            extraData,
        //            requestType,
        //            signature,
        //            lang = "vi"
        //        };

        //        using var reqMsg = new HttpRequestMessage(HttpMethod.Post, endpoint)
        //        {
        //            Content = JsonContent.Create(payload)
        //        };

        //        // timeout phòng kẹt
        //        _httpClient.Timeout = TimeSpan.FromSeconds(30);

        //        var httpRes = await _httpClient.SendAsync(reqMsg);
        //        var body = await httpRes.Content.ReadAsStringAsync();

        //        if (!httpRes.IsSuccessStatusCode)
        //            throw new Exception($"MoMo HTTP {(int)httpRes.StatusCode}: {body}");

        //        var momo = JsonSerializer.Deserialize<MomoCreateResponse>(body, new JsonSerializerOptions
        //        {
        //            PropertyNameCaseInsensitive = true
        //        }) ?? throw new Exception("Cannot parse MoMo response");

        //        // 8) Kiểm tra resultCode
        //        if (momo.ResultCode != 0)
        //            throw new Exception($"MoMo error {momo.ResultCode}: {momo.Message ?? "Unknown"}");

        //        var payUrl = momo.PayUrl ?? momo.Deeplink ?? momo.QrCodeUrl;
        //        if (string.IsNullOrWhiteSpace(payUrl))
        //            throw new Exception("MoMo response missing payUrl");

        //        return new MomoQrResponse
        //        {
        //            PayUrl = payUrl!,
        //            QrImageBase64 = GenerateBase64QrCode(payUrl!)
        //        };
        //    }

        //// NEW: xử lý return URL (user được redirect về)
        //public async Task<IBusinessResult> MomoReturnExecute(IQueryCollection query)
        //{
        //    try
        //    {
        //        var accessKey = _config["Momo:AccessKey"];
        //        var secretKey = _config["Momo:SecretKey"];

        //        string Get(string k) => query.TryGetValue(k, out var v) ? v.ToString() : string.Empty;

        //        // MoMo yêu cầu thứ tự CỐ ĐỊNH
        //        var raw = string.Join("&", new[]
        //        {
        //    $"accessKey={accessKey}",
        //    $"amount={Get("amount")}",
        //    $"extraData={Get("extraData")}",
        //    $"message={Get("message")}",
        //    $"orderId={Get("orderId")}",
        //    $"orderInfo={Get("orderInfo")}",
        //    $"orderType={Get("orderType")}",
        //    $"partnerCode={Get("partnerCode")}",
        //    $"payType={Get("payType")}",
        //    $"requestId={Get("requestId")}",
        //    $"responseTime={Get("responseTime")}",
        //    $"resultCode={Get("resultCode")}",
        //    $"transId={Get("transId")}"
        //});

        //        var calcSig = CreateSignature(secretKey, raw);
        //        var signature = Get("signature");

        //        if (!string.Equals(calcSig, signature, StringComparison.OrdinalIgnoreCase))
        //            return new BusinessResult(Const.FAIL_READ_CODE, "Invalid signature");

        //        var orderIdRaw = Get("orderId");
        //        var idStr = orderIdRaw?.Split('-').FirstOrDefault();
        //        if (!int.TryParse(idStr, out var orderId))
        //            return new BusinessResult(Const.FAIL_READ_CODE, "Invalid orderId");

        //        var order = await _unitOfWork.Order.GetOrderbyIdAsync(orderId);
        //        if (order == null)
        //            return new BusinessResult(Const.NOT_FOUND_CODE, "Order not found");

        //        decimal RoundVnd(decimal v) => Math.Round(v, 0, MidpointRounding.AwayFromZero);

        //        var baseAmount = order.TotalAmount > 0
        //            ? order.TotalAmount
        //            : RoundVnd(order.OrderItems?.Sum(i => i.TotalPrice ?? 0) ?? 0);

        //        baseAmount = RoundVnd(baseAmount);
        //        var fullAfter10 = RoundVnd(baseAmount);

        //        if (!long.TryParse(Get("amount"), out var amt) || amt <= 0)
        //            return new BusinessResult(Const.FAIL_READ_CODE, "Invalid amount");

        //        var paid = RoundVnd(amt);
        //        var isPayAll = (paid == fullAfter10);
        //        var expected = isPayAll ? fullAfter10 : RoundVnd(order.Deposit ?? baseAmount);

        //        if (paid != expected)
        //            return new BusinessResult(Const.FAIL_READ_CODE, $"Amount mismatch. paid={paid}, expected={expected}");

        //        var success = Get("resultCode") == "0";

        //        // ✅ CHỈ XỬ LÝ KHI PAYMENT SUCCESS
        //        if (success)
        //        {
        //            order.PaymentStatus = "Paid";
        //            order.TransactionId = Get("transId");

        //            if (isPayAll)
        //            {
        //                order.DiscountAmount = RoundVnd(baseAmount * 0.1m);
        //            }

        //            await _unitOfWork.Order.UpdateAsync(order);

        //            order.Payment ??= new List<Payment>();
        //            order.Payment.Add(new Payment
        //            {
        //                OrderId = order.OrderId,
        //                PaymentMethod = "MOMO",
        //                PaymentAmount = paid,
        //                Status = "Paid",
        //                PaymentDate = DateTime.UtcNow
        //            });

        //            // ✅ TRỪ STOCK KHI MOMO PAYMENT SUCCESS - CHỈ CHO AI TERRARIUM
        //            await ReduceStockForPaidOrder(order);

        //            await _unitOfWork.SaveAsync();
        //        }
        //        else
        //        {
        //            order.PaymentStatus = "Failed";
        //            order.TransactionId = Get("transId");
        //            await _unitOfWork.Order.UpdateAsync(order);

        //            order.Payment ??= new List<Payment>();
        //            order.Payment.Add(new Payment
        //            {
        //                OrderId = order.OrderId,
        //                PaymentMethod = "MOMO",
        //                PaymentAmount = paid,
        //                Status = "Failed",
        //                PaymentDate = DateTime.UtcNow
        //            });

        //            await _unitOfWork.SaveAsync();
        //        }

        //        return new BusinessResult(
        //            success ? Const.SUCCESS_UPDATE_CODE : Const.FAIL_READ_CODE,
        //            success ? "Payment success" : "Payment failed"
        //        );
        //    }
        //    catch (Exception ex)
        //    {
        //        return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        //    }
        //}
        #endregion

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

                var idStr = body.orderId?.Split('-').FirstOrDefault();
                if (!int.TryParse(idStr, out var orderId))
                    return new BusinessResult(Const.FAIL_READ_CODE, "Invalid orderId");

                var order = await _unitOfWork.Order.GetOrderbyIdAsync(orderId);
                if (order == null)
                    return new BusinessResult(Const.NOT_FOUND_CODE, "Order not found");

                var expected = ComputeExpectedPayable(order);

                if (!long.TryParse(body.amount, out var amt))
                    return new BusinessResult(Const.FAIL_READ_CODE, "Invalid amount");
                var paid = (decimal)amt;

                if (paid != expected)
                    return new BusinessResult(Const.FAIL_READ_CODE, $"Amount mismatch. paid={paid}, expected={expected}");

                var success = body.resultCode == "0";

                // ✅ PREVENT DOUBLE PROCESSING - CHỈ XỬ LÝ NẾU CHƯA PAID
                if (order.PaymentStatus != "Paid" && success)
                {
                    order.PaymentStatus = "Paid";
                    order.TransactionId = body.transId;
                    await _unitOfWork.Order.UpdateAsync(order);

                    if (order.Payment == null) order.Payment = new List<Payment>();
                    order.Payment.Add(new Payment
                    {
                        OrderId = order.OrderId,
                        PaymentMethod = "MOMO",
                        PaymentAmount = paid,
                        Status = "Paid",
                        PaymentDate = DateTime.UtcNow
                    });

                    // ✅ TRỪ STOCK KHI IPN SUCCESS - CHỈ CHO AI TERRARIUM
                    await ReduceStockForPaidOrder(order);

                    await _unitOfWork.SaveAsync();
                }
                else if (!success)
                {
                    order.PaymentStatus = "Failed";
                    order.TransactionId = body.transId;
                    await _unitOfWork.Order.UpdateAsync(order);
                    await _unitOfWork.SaveAsync();
                }

                return new BusinessResult(
                    success ? Const.SUCCESS_UPDATE_CODE : Const.FAIL_READ_CODE,
                    success ? "Payment success (IPN)" : "Payment failed (IPN)"
                );
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
        #region Stock Reduction Helper Methods

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
                            await ReduceStockForAITerrariumVariant(variant.TerrariumVariantId, item.TerrariumVariantQuantity ?? 0);
                        }
                    }
                }

                // ✅ 3. COMBO: CHỈ TRỪ KHI CÓ AI TERRARIUM TRONG COMBO
                if (item.ComboId.HasValue && item.ItemType == "Combo")
                {
                    await ReduceStockForComboItem(item);
                }
            }
        }

        private async Task<bool> ShouldReduceStockForAccessory(OrderItem item)
        {
            if (item.TerrariumVariantId.HasValue)
            {
                var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(item.TerrariumVariantId.Value);
                if (variant != null)
                {
                    var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(variant.TerrariumId);
                    return terrarium != null && terrarium.GeneratedByAI;
                }
            }

            if (!item.TerrariumVariantId.HasValue && !item.ComboId.HasValue)
            {
                return false;
            }

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

        private async Task ReduceStockForAITerrariumVariant(int terrariumVariantId, int variantQuantity)
        {
            var variantAccessories = await _unitOfWork.TerrariumVariantAccessory
                .GetByTerrariumVariantId(terrariumVariantId);

            foreach (var va in variantAccessories)
            {
                int accessoryQtyToReduce = va.Quantity * variantQuantity;
                await _unitOfWork.Accessory.ReduceStockAsync(va.AccessoryId, accessoryQtyToReduce);
            }
        }

        private async Task ReduceStockForComboItem(OrderItem comboOrderItem)
        {
            if (!comboOrderItem.ComboId.HasValue) return;

            var combo = await _unitOfWork.Combo.GetByIdAsync(comboOrderItem.ComboId.Value);
            if (combo?.ComboItems == null) return;

            foreach (var comboItem in combo.ComboItems)
            {
                if (comboItem.AccessoryId.HasValue)
                {
                    bool comboHasAITerrarium = await ComboContainsAITerrarium(combo);
                    if (comboHasAITerrarium)
                    {
                        await _unitOfWork.Accessory.ReduceStockAsync(comboItem.AccessoryId.Value, comboItem.Quantity);
                    }
                }

                if (comboItem.TerrariumVariantId.HasValue)
                {
                    var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(comboItem.TerrariumVariantId.Value);
                    if (variant != null)
                    {
                        var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(variant.TerrariumId);
                        if (terrarium != null && terrarium.GeneratedByAI)
                        {
                            await ReduceStockForAITerrariumVariant(variant.TerrariumVariantId, comboItem.Quantity);
                        }
                    }
                }
            }
        }

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
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        #endregion

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
