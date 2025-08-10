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
    private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VnPayCompare());
    private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VnPayCompare());

    public PaymentResponseModel GetFullResponseData(IQueryCollection collection, string hashSecret)
    {
        var vnPay = new VnPayLibrary();

        
        foreach (var (key, value) in collection)
        {
            if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
            {
                vnPay.AddResponseData(key, value);
            }
        }

        // --- Lấy các giá trị cần thiết từ vnPay (đã được thêm data ở bước trên) ---
        // Sử dụng TryParse để an toàn hơn thay vì Convert.ToInt64 trực tiếp
        long? orderId = null;
        if (long.TryParse(vnPay.GetResponseData("vnp_TxnRef"), out var parsedOrderId))
        {
            orderId = parsedOrderId;
        }

        long? vnPayTranId = null;
        if (long.TryParse(vnPay.GetResponseData("vnp_TransactionNo"), out var parsedTranId))
        {
            vnPayTranId = parsedTranId;
        }

        string vnpResponseCode = vnPay.GetResponseData("vnp_ResponseCode");
        string orderInfo = vnPay.GetResponseData("vnp_OrderInfo");
        string amountString = vnPay.GetResponseData("vnp_Amount"); // Lấy chuỗi số tiền
        string vnpPayDateString = vnPay.GetResponseData("vnp_PayDate"); // Lấy ngày thanh toán (ưu tiên)
        string vnpCreateDateString = vnPay.GetResponseData("vnp_CreateDate"); // Lấy ngày tạo (phòng khi không có PayDate)


        // --- Xác thực chữ ký ---
        // Lấy vnp_SecureHash từ collection (đã được parse sẵn)
        var vnpSecureHash = collection.FirstOrDefault(k => k.Key == "vnp_SecureHash").Value.ToString();
        if (string.IsNullOrEmpty(vnpSecureHash))
        {
            // Hoặc trả về lỗi, hoặc coi là không thành công
            return new PaymentResponseModel { Success = false };
        }

        var checkSignature = vnPay.ValidateSignature(vnpSecureHash, hashSecret);

        if (!checkSignature)
        {
            return new PaymentResponseModel()
            {
                Success = false,
                OrderDescription = "Signature validation failed." // Thêm thông báo lỗi rõ ràng hơn
            };
        }

        // --- Chuyển đổi giá tiền ---
        decimal? amount = null;
        if (decimal.TryParse(amountString, out decimal rawAmount))
        {
            amount = rawAmount / 100; // Chia 100 để lấy giá tiền thực tế
        }

        // --- Chuyển đổi ngày thanh toán ---
        DateTime? paymentDate = null;
        string dateToParse = vnpPayDateString; // Ưu tiên vnp_PayDate
        if (string.IsNullOrEmpty(dateToParse))
        {
            dateToParse = vnpCreateDateString; // Nếu không có, dùng vnp_CreateDate
        }

        if (!string.IsNullOrEmpty(dateToParse) &&
            DateTime.TryParseExact(dateToParse, "yyyyMMddHHmmss",
                                   System.Globalization.CultureInfo.InvariantCulture,
                                   System.Globalization.DateTimeStyles.None,
                                   out DateTime parsedDate))
        {
            paymentDate = parsedDate;
        }

        // --- Trả về PaymentResponseModel ---
        return new PaymentResponseModel()
        {
            Success = vnpResponseCode.Equals("00"), // Trạng thái thành công/thất bại
            PaymentMethod = "VnPay",
            OrderDescription = orderInfo,
            OrderId = orderId?.ToString(), // Sử dụng ?.ToString() để tránh NullReferenceException
            PaymentId = vnPayTranId?.ToString(),
            TransactionId = vnPayTranId?.ToString(),
            Token = vnpSecureHash,
            VnPayResponseCode = vnpResponseCode, // Mã phản hồi thô từ VnPay

            Amount = amount, // Gán giá tiền đã chia 100
            PaymentDate = paymentDate // Gán ngày thanh toán
        };
    }



    public string GetIpAddress(HttpContext context)
    {
        var ipAddress = string.Empty;
        try
        {
            var remoteIpAddress = context.Connection.RemoteIpAddress;
        
            if (remoteIpAddress != null)
            {
                if (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6)
                {
                    remoteIpAddress = Dns.GetHostEntry(remoteIpAddress).AddressList
                        .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
                }
        
                if (remoteIpAddress != null) ipAddress = remoteIpAddress.ToString();
        
                return ipAddress;
            }
        }
        catch (Exception ex)
        {
            return ex.Message;
        }

        return "127.0.0.1";
    }
    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _requestData.Add(key, value);
        }
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _responseData.Add(key, value);
        }
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var retValue) ? retValue : string.Empty;
    }


    public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
    {
        var data = new StringBuilder();

        foreach (var (key, value) in _requestData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
        {
            data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
        }

        var querystring = data.ToString();

        baseUrl += "?" + querystring;
        var signData = querystring;
        if (signData.Length > 0)
        {
            signData = signData.Remove(data.Length - 1, 1);
        }

        var vnpSecureHash = HmacSha512(vnpHashSecret, signData);
        baseUrl += "vnp_SecureHash=" + vnpSecureHash;

        return baseUrl;
    }

    public bool ValidateSignature(string inputHash, string secretKey)
    {
        var rspRaw = GetResponseData();
        var myChecksum = HmacSha512(secretKey, rspRaw);
        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string HmacSha512(string key, string inputData)
    {
        var hash = new StringBuilder();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            var hashValue = hmac.ComputeHash(inputBytes);
            foreach (var theByte in hashValue)
            {
                hash.Append(theByte.ToString("x2"));
            }
        }

        return hash.ToString();
    }

    private string GetResponseData()
    {
        var data = new StringBuilder();
        if (_responseData.ContainsKey("vnp_SecureHashType"))
        {
            _responseData.Remove("vnp_SecureHashType");
        }

        if (_responseData.ContainsKey("vnp_SecureHash"))
        {
            _responseData.Remove("vnp_SecureHash");
        }

        foreach (var (key, value) in _responseData.Where(kv => !string.IsNullOrEmpty(kv.Value)))
        {
            data.Append(WebUtility.UrlEncode(key) + "=" + WebUtility.UrlEncode(value) + "&");
        }

        //remove last '&'
        if (data.Length > 0)
        {
            data.Remove(data.Length - 1, 1);
        }

        return data.ToString();
    }
}

public class VnPayCompare : IComparer<string>
{
    public int Compare(string x, string y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;
        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}