using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using TerrariumGardenTech.Common.ResponseModel.Payment;

namespace TerrariumGardenTech.Common.Config
{
    /// <summary>
    /// VNPAY helper: quản lý tham số request/response, ký/kiểm HMACSHA512 theo chuẩn VNPAY.
    /// - Form-encode theo kiểu application/x-www-form-urlencoded (space -> '+')
    /// - Sắp xếp key A→Z (Ordinal)
    /// - Bỏ vnp_SecureHash/vnp_SecureHashType trước khi ký
    /// </summary>
    public class VnPayLibrary
    {
        // Lưu tham số theo thứ tự A→Z
        private readonly SortedList<string, string> _requestData = new(new VnPayCompare());
        private readonly SortedList<string, string> _responseData = new(new VnPayCompare());

        // Debug (tùy chọn)
        public string? LastRequestRaw { get; private set; }
        public string? LastRequestHash { get; private set; }
        public string? LastResponseRaw { get; private set; }
        public string? LastResponseHash { get; private set; }

        // ====== Utils ======
        // VNPAY dùng form-encoding: khoảng trắng -> '+'
        private static string EncForm(string s)
            => string.IsNullOrEmpty(s) ? string.Empty : Uri.EscapeDataString(s).Replace("%20", "+");

        private static string HmacSha512(string key, string data)
        {
            var keyBytes = Encoding.UTF8.GetBytes((key ?? string.Empty).Trim());
            var inputBytes = Encoding.UTF8.GetBytes(data ?? string.Empty);
            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(inputBytes);
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }

        // ====== Request side ======
        public void AddRequestData(string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                _requestData[key] = value!;
        }

        /// <summary>
        /// Tạo URL thanh toán theo chuẩn VNPAY (ký HMACSHA512).
        /// </summary>
        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var items = _requestData
                .Where(kv => kv.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
                .Where(kv => kv.Key is not "vnp_SecureHash" and not "vnp_SecureHashType")
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                .ToList();

            if (!items.Any())
                throw new InvalidOperationException("VNPay request is empty. AddRequestData before CreateRequestUrl.");

            var raw = string.Join("&", items.Select(kv => $"{EncForm(kv.Key)}={EncForm(kv.Value)}"));
            var secureHash = HmacSha512(vnpHashSecret, raw);

            LastRequestRaw = raw;
            LastRequestHash = secureHash;

            var query = $"{raw}&vnp_SecureHashType=HMACSHA512&vnp_SecureHash={secureHash}";
            return baseUrl.TrimEnd('?') + "?" + query;
        }

        /// <summary>
        /// Alias để tương thích với service đang gọi pay.GetPaymentUrl(...)
        /// </summary>
        public string GetPaymentUrl(string baseUrl, string vnpHashSecret)
            => CreateRequestUrl(baseUrl, vnpHashSecret);

        // ====== Response side ======
        public void AddResponseData(string key, string? value)
        {
            if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                _responseData[key] = value!;
        }

        public string GetResponseData(string key)
            => _responseData.TryGetValue(key, out var v) ? v : string.Empty;

        private string BuildResponseRaw()
        {
            // Loại bỏ 2 trường hash trước khi ký
            _responseData.Remove("vnp_SecureHashType");
            _responseData.Remove("vnp_SecureHash");

            var items = _responseData
                .Where(kv => kv.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
                .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
                .ToList();

            return string.Join("&", items.Select(kv => $"{EncForm(kv.Key)}={EncForm(kv.Value)}"));
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var raw = BuildResponseRaw();
            var myChecksum = HmacSha512(secretKey, raw);

            LastResponseRaw = raw;
            LastResponseHash = myChecksum;

            return string.Equals(myChecksum, (inputHash ?? string.Empty).ToUpperInvariant(),
                                 StringComparison.Ordinal);
        }

        /// <summary>
        /// Parse & verify toàn bộ dữ liệu callback thành <see cref="PaymentResponseModel"/>.
        /// </summary>
        public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
        {
            var vnPay = new VnPayLibrary();

            // Nạp tất cả vnp_*
            foreach (var (key, value) in collection)
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
                    vnPay.AddResponseData(key, value);

            var vnpSecureHash = collection["vnp_SecureHash"].ToString();
            if (string.IsNullOrEmpty(vnpSecureHash))
                return new PaymentResponseModel { Success = false, OrderDescription = "Missing vnp_SecureHash" };

            // Verify signature
            var ok = vnPay.ValidateSignature(vnpSecureHash, hashSecret);
            if (!ok)
                return new PaymentResponseModel { Success = false, OrderDescription = "Signature validation failed." };

            // Đọc field chuẩn
            var respCode = vnPay.GetResponseData("vnp_ResponseCode");
            var orderInfo = vnPay.GetResponseData("vnp_OrderInfo");
            var amountStr = vnPay.GetResponseData("vnp_Amount");        // VND*100
            var payDateStr = vnPay.GetResponseData("vnp_PayDate");       // yyyyMMddHHmmss
            var createStr = vnPay.GetResponseData("vnp_CreateDate");    // dự phòng
            var orderIdStr = vnPay.GetResponseData("vnp_TxnRef");
            var tranNoStr = vnPay.GetResponseData("vnp_TransactionNo");

            long.TryParse(orderIdStr, out var orderIdL);
            long.TryParse(tranNoStr, out var tranIdL);

            decimal? amount = null;
            if (decimal.TryParse(amountStr, out var rawAmt))
                amount = rawAmt / 100m;

            DateTime? payDt = null;
            var dtStr = !string.IsNullOrEmpty(payDateStr) ? payDateStr : createStr;
            if (!string.IsNullOrEmpty(dtStr) &&
                DateTime.TryParseExact(dtStr, "yyyyMMddHHmmss", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var tmp))
                payDt = tmp;

            return new PaymentResponseModel
            {
                Success = string.Equals(respCode, "00", StringComparison.Ordinal),
                PaymentMethod = "VNPAY",
                OrderDescription = orderInfo,
                OrderId = orderIdL == 0 ? null : orderIdL.ToString(),
                PaymentId = tranIdL == 0 ? null : tranIdL.ToString(),
                TransactionId = tranIdL == 0 ? null : tranIdL.ToString(),
                Token = vnpSecureHash,
                VnPayResponseCode = respCode,
                Amount = amount,
                PaymentDate = payDt
            };
        }

        /// <summary>
        /// Lấy IP client (kể cả sau reverse proxy).
        /// </summary>
        public string GetIpAddress(HttpContext context)
        {
            try
            {
                var xff = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(xff))
                    return xff.Split(',')[0].Trim();

                var ip = context.Connection.RemoteIpAddress;
                if (ip?.AddressFamily == AddressFamily.InterNetworkV6)
                    ip = Dns.GetHostEntry(ip).AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);

                return ip?.ToString() ?? "127.0.0.1";
            }
            catch { return "127.0.0.1"; }
        }
    }

    /// <summary>So sánh key theo Ordinal (A→Z) đúng yêu cầu VNPAY.</summary>
    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;
            return string.Compare(x, y, StringComparison.Ordinal);
        }
    }
}