using Microsoft.Extensions.Configuration;
using MimeKit.Encodings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

using System.Threading.Tasks;
using TerrariumGardenTech.Common.RequestModel.Payment;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service
{
    public class MomoServices : IMomoServices
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public MomoServices(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _config = config;
            _httpClient = httpClientFactory.CreateClient();
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
            string orderId = $"{req.OrderId}_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
            string orderInfo = req.OrderInfo.ToString();
            string amount = ((long)req.Amount).ToString();
            string requestId = Guid.NewGuid().ToString();
            string userStk = "0329126894";
            string extraData = Convert.ToBase64String(
                       Encoding.UTF8.GetBytes($"stk={userStk}"));

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

            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseContent = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseContent);
            if (doc.RootElement.TryGetProperty("payUrl", out var payUrlElement))
            {
                string payUrl = payUrlElement.GetString();
                string qrBase64 = GenerateBase64QrCode(payUrl);
                return new MomoQrResponse
                {
                    PayUrl = payUrl,
                    QrImageBase64 = GenerateBase64QrCode(payUrl)  // Tạo mã QR luôn
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
    }
}
