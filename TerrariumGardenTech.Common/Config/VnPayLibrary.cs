using Microsoft.AspNetCore.Http;


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using TerrariumGardenTech.Common.ResponseModel.Payment;

namespace TerrariumGardenTech.Common.Config;
public class VnPayLibrary
{
    // Lưu tham số theo thứ tự A→Z
    private readonly SortedList<string, string> _requestData = new(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new(new VnPayCompare());

    // Debug (tùy chọn): xem lại raw string & hash đã ký
    public string? LastRequestRaw { get; private set; }
    public string? LastRequestHash { get; private set; }
    public string? LastResponseRaw { get; private set; }
    public string? LastResponseHash { get; private set; }

    // ====== Utils ======
    // VNPAY yêu cầu form-encode: khoảng trắng -> '+'
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

    public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
    {
        // Lấy tất cả vnp_* (trừ 2 trường hash), sort A→Z
        var items = _requestData
            .Where(kv => kv.Key.StartsWith("vnp_", StringComparison.OrdinalIgnoreCase))
            .Where(kv => kv.Key is not "vnp_SecureHash" and not "vnp_SecureHashType")
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .ToList();

        if (!items.Any())
            throw new InvalidOperationException("VNPay request is empty. AddRequestData before CreateRequestUrl.");

        // Raw string để ký
        var raw = string.Join("&", items.Select(kv => $"{EncForm(kv.Key)}={EncForm(kv.Value)}"));
        var secureHash = HmacSha512(vnpHashSecret, raw);

        // Lưu debug
        LastRequestRaw = raw;
        LastRequestHash = secureHash;

        // Ghép URL cuối (KHÔNG re-encode toàn chuỗi lần nữa)
        var query = $"{raw}&vnp_SecureHashType=HMACSHA512&vnp_SecureHash={secureHash}";
        return baseUrl.TrimEnd('?') + "?" + query;
    }

    // ====== Response side ======
    public void AddResponseData(string key, string? value)
    {
        if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
            _responseData[key] = value!;
    }

    public string GetResponseData(string key)
        => _responseData.TryGetValue(key, out var v) ? v : string.Empty;

    // Build raw string từ response để verify
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

        // debug
        LastResponseRaw = raw;
        LastResponseHash = myChecksum;

        return string.Equals(myChecksum, (inputHash ?? string.Empty).ToUpperInvariant(),
                             StringComparison.Ordinal);
    }

    // ====== Convenience: parse + verify trả về model gọn ======
    public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
    {
        var vnPay = new VnPayLibrary();

        // Nạp tất cả vnp_*
        foreach (var (key, value) in collection)
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                vnPay.AddResponseData(key, value);

        // Lấy hash từ VNPAY
        var vnpSecureHash = collection["vnp_SecureHash"].ToString();
        if (string.IsNullOrEmpty(vnpSecureHash))
            return new PaymentResponseModel { Success = false, OrderDescription = "Missing vnp_SecureHash" };

        // Verify
        var ok = vnPay.ValidateSignature(vnpSecureHash, hashSecret);
        if (!ok)
            return new PaymentResponseModel { Success = false, OrderDescription = "Signature validation failed." };

        // Đọc field
        var respCode = vnPay.GetResponseData("vnp_ResponseCode");
        var orderInfo = vnPay.GetResponseData("vnp_OrderInfo");
        var amountStr = vnPay.GetResponseData("vnp_Amount");         // VND * 100
        var payDateStr = vnPay.GetResponseData("vnp_PayDate");        // yyyyMMddHHmmss
        var createStr = vnPay.GetResponseData("vnp_CreateDate");     // fallback
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

    // IP helper (sau proxy)
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

// So sánh key theo Ordinal
public class VnPayCompare : IComparer<string>
{
    public int Compare(string x, string y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var ci = CompareInfo.GetCompareInfo("en-US");
        return ci.Compare(x, y, CompareOptions.Ordinal);
    }
}


//public class VnPayLibrary
//{
//    private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
//    private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

//    public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
//    {
//        var vnPay = new VnPayLibrary();


//        foreach (var (key, value) in collection)
//        {
//            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
//            {
//                vnPay.AddResponseData(key, value);
//            }
//        }

//        // --- Lấy các giá trị cần thiết từ vnPay (đã được thêm data ở bước trên) ---
//        // Sử dụng TryParse để an toàn hơn thay vì Convert.ToInt64 trực tiếp
//        long? orderId = null;
//        if (long.TryParse(vnPay.GetResponseData("vnp_TxnRef"), out var parsedOrderId))
//        {
//            orderId = parsedOrderId;
//        }

//        long? vnPayTranId = null;
//        if (long.TryParse(vnPay.GetResponseData("vnp_TransactionNo"), out var parsedTranId))
//        {
//            vnPayTranId = parsedTranId;
//        }

//        string vnpResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
//        string orderInfo = vnPay.GetResponseData("vnp_OrderInfo");
//        string amountString = vnPay.GetResponseData("vnp_Amount"); // Lấy chuỗi số tiền
//        string vnpPayDateString = vnPay.GetResponseData("vnp_PayDate"); // Lấy ngày thanh toán (ưu tiên)
//        string vnpCreateDateString = vnPay.GetResponseData("vnp_CreateDate"); // Lấy ngày tạo (phòng khi không có PayDate)


//        // --- Xác thực chữ ký ---
//        // Lấy vnp_SecureHash từ collection (đã được parse sẵn)
//        var vnpSecureHash = collection.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value.ToString();
//        if (string.IsNullOrEmpty(vnpSecureHash))
//        {
//            // Hoặc trả về lỗi, hoặc coi là không thành công
//            return new PaymentResponseModel { Success = false };
//        }

//        var checkSignature = vnPay.ValidateSignature(vnpSecureHash, hashSecret);

//        if (!checkSignature)
//        {
//            return new PaymentResponseModel()
//            {
//                Success = false,
//                OrderDescription = "Signature validation failed." // Thêm thông báo lỗi rõ ràng hơn
//            };
//        }

//        // --- Chuyển đổi giá tiền ---
//        decimal? amount = null;
//        if (decimal.TryParse(amountString, out decimal rawAmount))
//        {
//            amount = rawAmount / 100; // Chia 100 để lấy giá tiền thực tế
//        }

//        // --- Chuyển đổi ngày thanh toán ---
//        DateTime? paymentDate = null;
//        string dateToParse = vnpPayDateString; // Ưu tiên vnp_PayDate
//        if (string.IsNullOrEmpty(dateToParse))
//        {
//            dateToParse = vnpCreateDateString; // Nếu không có, dùng vnp_CreateDate
//        }

//        if (!string.IsNullOrEmpty(dateToParse) &&
//            DateTime.TryParseExact(dateToParse, "yyyyMMddHHmmss",
//                                   System.Globalization.CultureInfo.InvariantCulture,
//                                   System.Globalization.DateTimeStyles.None,
//                                   out DateTime parsedDate))
//        {
//            paymentDate = parsedDate;
//        }

//        // --- Trả về PaymentResponseModel ---
//        return new PaymentResponseModel()
//        {
//            Success = vnpResponseCode.Equals("00"), // Trạng thái thành công/thất bại
//            PaymentMethod = "VnPay",
//            OrderDescription = orderInfo,
//            OrderId = orderId?.ToString(), // Sử dụng ?.ToString() để tránh NullReferenceException
//            PaymentId = vnPayTranId?.ToString(),
//            TransactionId = vnPayTranId?.ToString(),
//            Token = vnpSecureHash,
//            VnPayResponseCode = vnpResponseCode, // Mã phản hồi thô từ VnPay

//            Amount = amount, // Gán giá tiền đã chia 100
//            PaymentDate = paymentDate // Gán ngày thanh toán
//        };
//    }



//    public string GetIpAddress(HttpContext context)
//    {
//        var ipAddress = string.Empty;
//        try
//        {
//            var remoteIpAddress = context.Connection.RemoteIpAddress;

//            if (remoteIpAddress != null)
//            {
//                if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
//                {
//                    remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
//                        .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
//                }

//                if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();

//                return ipAddress;
//            }
//        }
//        catch (Exception ex)
//        {
//            return ex.Message;
//        }

//        return "127.0.0.1";
//    }
//    public void AddRequestData(string key, string value)
//    {
//        if (!string.IsNullOrEmpty(value))
//        {
//            _requestData.Add(key, value);
//        }
//    }

//    public void AddResponseData(string key, string value)
//    {
//        if (!string.IsNullOrEmpty(value))
//        {
//            _responseData.Add(key, value);
//        }
//    }

//    public string GetResponseData(string key)
//    {
//        return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
//    }


//    public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
//    {
//        var data = new StringBuilder();

//        foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
//        {
//            data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
//        }

//        var querystring = data.ToString();

//        baseUrl += "?" + querystring;
//        var signData = querystring;
//        if (signData.Length > 0)
//        {
//            signData = signData.Remove(data.Length - 1, 1);
//        }

//        var vnpSecureHash = HmacSha512(vnpHashSecret, signData);
//        baseUrl += "vnp_SecureHash=" + vnpSecureHash;

//        return baseUrl;
//    }

//    public bool ValidateSignature(string inputHash, string secretKey)
//    {
//        var rspRaw = GetResponseData();
//        var myChecksum = HmacSha512(secretKey, rspRaw);
//        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
//    }

//    private string HmacSha512(string key, string inputData)
//    {
//        var hash = new StringBuilder();
//        var keyBytes = Encoding.UTF8.GetBytes(key);
//        var inputBytes = Encoding.UTF8.GetBytes(inputData);
//        using (var hmac = new HMACSHA512(keyBytes))
//        {
//            var hashValue = hmac.ComputeHash(inputBytes);
//            foreach (var theByte in hashValue)
//            {
//                hash.Append(theByte.ToString("x2"));
//            }
//        }

//        return hash.ToString();
//    }

//    private string GetResponseData()
//    {
//        var data = new StringBuilder();
//        if (_responseData.ContainsKey("vnp_SecureHashType"))
//        {
//            _responseData.Remove("vnp_SecureHashType");
//        }

//        if (_responseData.ContainsKey("vnp_SecureHash"))
//        {
//            _responseData.Remove("vnp_SecureHash");
//        }

//        foreach (var (key, value) in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
//        {
//            data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
//        }

//        //remove last '&'
//        if (data.Length > 0)
//        {
//            data.Remove(data.Length - 1, 1);
//        }

//        return data.ToString();
//    }
//}

//public class VnPayCompare : IComparer<string>
//{
//    public int Compare(string x, string y)
//    {
//        if (x == y) return 0;
//        if (x == null) return -1;
//        if (y == null) return 1;
//        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
//        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
//    }
//}