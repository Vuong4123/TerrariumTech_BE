using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Web;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.ResponseModel.Payment;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Common.RequestModel.Payment;

namespace TerrariumGardenTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[AllowAnonymous]
public class PaymentController : ControllerBase
{
    // FE domain của bạn
    private const string FE_BASE = "https://terra-tech-garden.vercel.app";
    private const string FE_SUCCESS_PATH = "/payment-success";
    private const string FE_FAIL_PATH = "/payment-fail"; // nếu muốn tách trang fail

    private const string FE_BASE_MEMBERSHIP = "https://terra-tech-garden.vercel.app"; // dev local
    private const string FE_SUCCESS_MEMBERSHIP_PATH = "/membership-success";
    private const string FE_FAIL_MEMBERSHIP_PATH = "/membership-fali"; // nếu muốn tách trang fail

    private readonly IPayOsService _payOsService;
    private readonly IVnPayService _vnPayService;
    private readonly IMomoServices _momoServices;
    private readonly ILogger<PaymentController> _logger;
    public PaymentController(IPayOsService payOsService, IVnPayService vnPayService, IMomoServices momoServices, ILogger<PaymentController> logger)
    {
        _payOsService = payOsService;
        _vnPayService = vnPayService;
        _momoServices = momoServices;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet("momo/membership/callback")]
    public async Task<IActionResult> MomoMembershipCallback()
    {
        var result = await _momoServices.MomoMembershipReturnExecute(Request.Query);
        var isSuccess = result.Status == Const.SUCCESS_UPDATE_CODE;

        var baseUrl = $"{FE_BASE_MEMBERSHIP}{(isSuccess ? FE_SUCCESS_MEMBERSHIP_PATH : FE_FAIL_MEMBERSHIP_PATH)}";
        var feUrl = $"{baseUrl}?status={(isSuccess ? "success" : "fail")}&type=membership";
        return Content(BuildRedirectHtml(feUrl), "text/html");
    }

    [AllowAnonymous]
    [HttpPost("momo/membership/ipn")]
    public async Task<IActionResult> MomoMembershipIpn([FromBody] MomoIpnModel body)
    {
        var rs = await _momoServices.MomoMembershipIpnExecute(body);
        if (rs.Status == Const.SUCCESS_UPDATE_CODE)
            return Ok(new { resultCode = 0, message = "success" });
        return Ok(new { resultCode = 5, message = "fail" });
    }


    // ====================== PAYOS ======================

    //[HttpPost("pay-os")]
    //public async Task<IActionResult> CreatePayOsLink([FromBody] PaymentCreateRequest request)
    //{
    //    if (!ModelState.IsValid || request.OrderId <= 0)
    //        return BadRequest("Invalid order id");

    //    var result = await _payOsService.CreatePaymentLink(
    //        request.OrderId,
    //        request.Description ?? string.Empty,
    //        request.PayAll                       // ✅ truyền cờ xuống service
    //    );
    //    return Ok(result);
    //}


    //[AllowAnonymous]
    //[HttpGet("pay-os/callback")]
    //public async Task<IActionResult> PayOsCallback([FromQuery] PaymentReturnModel request)

    //{
    //    _logger.LogInformation("PAYOS callback: {@q}",
    //        Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()));

    //    var result = await _payOsService.ProcessPaymentCallback(request);

    //    // Giữ nguyên query khi đẩy về FE (nếu PayOS có trả các tham số cần)
    //    var feBase = $"{FE_BASE}{FE_SUCCESS_PATH}";
    //    var qs = Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty;
    //    var feUrl = feBase + qs;

    //    // (hoặc) nếu FE chỉ cần alias:
    //    var orderId = request.OrderId;
    //    var status = result.Status == Const.SUCCESS_UPDATE_CODE ? "success" : "fail";
    //    feUrl = $"{FE_BASE}{FE_SUCCESS_PATH}?orderId={orderId}&status={status}";

    //    var html = $@"<html><head><meta http-equiv='refresh' content='0;url={feUrl}'/></head>
    //    <body><script>window.location.replace('{feUrl}');</script>Đang chuyển hướng…</body></html>";
    //    return Content(html, "text/html");
    //}

    // ====================== VNPAY ======================

    //[HttpPost("vn-pay")]
    //public async Task<IActionResult> CreateVnPayUrl([FromBody] PaymentInformationModel model)
    //{
    //    if (!ModelState.IsValid || model.OrderId <= 0)
    //        return BadRequest("Invalid order id");

    //    var result = await _vnPayService.CreatePaymentUrl(model, HttpContext);
    //    return StatusCode(result.Status, result);
    //}



    //[AllowAnonymous]
    //[HttpGet("vn-pay/callback")]
    //public async Task<IActionResult> VnPayCallback()
    //{
    //    // Log toàn bộ params VNPAY trả về
    //    _logger.LogInformation("VNPAY callback: {@q}",
    //        Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()));

    //    // 1) xử lý & lưu DB
    //    var result = await _vnPayService.PaymentExecute(Request.Query);

    //    // 2) redirect về FE, giữ nguyên toàn bộ vnp_* đúng như VNPAY trả
    //    var feBase = $"{FE_BASE}{FE_SUCCESS_PATH}";
    //    var qs = Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty;
    //    var feUrl = feBase + qs;

    //    // (tuỳ chọn) thêm alias cho FE:
    //    var orderId = Request.Query["vnp_TxnRef"].ToString();
    //    var amountRaw = Request.Query["vnp_Amount"].ToString();
    //    var amountVnd = int.TryParse(amountRaw, out var a) ? a / 100 : 0;
    //    var success = result.Status == Const.SUCCESS_UPDATE_CODE;
    //    feUrl += $"&orderId={orderId}&amount={amountVnd}&status={(success ? "success" : "fail")}";

    //    var html = $@"<html><head><meta http-equiv='refresh' content='0;url={feUrl}'/></head>
    //    <body><script>window.location.replace('{feUrl}');</script>Đang chuyển hướng…</body></html>";
    //    return Content(html, "text/html");
    //}

    // Tạo link thanh toán MoMo (trả URL + QR base64)
    [HttpPost("momo/create")]
    public async Task<IActionResult> CreateMomoLink([FromBody] MomoRequest request)
    {
        if (!ModelState.IsValid || request.OrderId <= 0)
            return BadRequest("Invalid order id");

        // ĐẢM BẢO trong service CreateMomoPaymentUrl bạn set đúng:
        // ReturnUrl = https://terarium.shop/api/Payment/momo/callback
        // IpnUrl    = https://terarium.shop/api/Payment/momo/ipn
        var rsp = await _momoServices.CreateMomoPaymentUrl(request);
        return Ok(rsp);
    }

    // NEW: Tạo link nạp ví MoMo (sử dụng amount + userId trong extraData)
    [HttpPost("momo/wallet/create")]
    public async Task<IActionResult> CreateMomoWalletTopup([FromBody] MomoWalletTopupRequest request)
    {
        if (!ModelState.IsValid || request.UserId <= 0 || request.Amount <= 0)
            return BadRequest("Invalid payload");

        var rsp = await _momoServices.CreateMomoWalletTopupUrl(request);
        return Ok(rsp);
    }

    // MoMo redirect về (ReturnUrl) — chỉ điều hướng UI
    [HttpGet("momo/callback")]
    public async Task<IActionResult> MomoCallback()
    {
        string Get(string k) => Request.Query.TryGetValue(k, out var v) ? v.ToString() : string.Empty;

        var resultCode = Get("resultCode");
        var isSuccess = resultCode == "0";
        var orderIdRaw = Get("orderId");                  // "{orderId}_{ticks}"
        var internalId = (orderIdRaw ?? "").Split('_').FirstOrDefault() ?? orderIdRaw;

        // Gọi service để log/khớp nhẹ, nhưng đừng quyết định UI theo DB ở đây
        try { await _momoServices.MomoReturnExecute(Request.Query); }
        catch (Exception ex) { _logger.LogWarning(ex, "ReturnExecute warn"); }

        // Các field MoMo V2 thường có trên return
        var amount = Get("amount");       // VND, KHÔNG *100
        var transId = Get("transId");
        var payType = Get("payType");      // ATM|QR|NAPAS|CREDIT...
        var bankCode = Get("bankCode");     // có thể rỗng
        var responseTime = Get("responseTime");
        var message = Get("message");
        var orderInfo = Get("orderInfo");

        // TUYỆT ĐỐI không đính kèm chữ ký
        // var signature = Get("signature");  // ❌ KHÔNG gửi về FE

        var baseUrl = $"{FE_BASE}{(isSuccess ? FE_SUCCESS_PATH : FE_FAIL_PATH)}";
        var feUrl =
            $"{baseUrl}?orderId={Uri.EscapeDataString(internalId ?? "")}" +
            $"&status={(isSuccess ? "success" : "fail")}" +
            $"&amount={Uri.EscapeDataString(amount)}" +
            $"&transId={Uri.EscapeDataString(transId)}" +
            $"&payType={Uri.EscapeDataString(payType)}" +
            $"&bank={Uri.EscapeDataString(bankCode)}" +
            $"&message={Uri.EscapeDataString(message)}" +
            $"&orderInfo={Uri.EscapeDataString(orderInfo)}" +
            $"&resultCode={Uri.EscapeDataString(resultCode)}" +
            $"&responseTime={Uri.EscapeDataString(responseTime)}";

        return Content(BuildRedirectHtml(feUrl), "text/html");
    }

    // NEW: Redirect callback for Wallet topup. Only verify signature, then redirect to FE with status & amount.
    [HttpGet("momo/wallet/callback")]
    public async Task<IActionResult> MomoWalletCallback()
    {
        string Get(string k) => Request.Query.TryGetValue(k, out var v) ? v.ToString() : string.Empty;

        var resultCode = Get("resultCode");
        var isSuccess = resultCode == "0";
        var amount = Get("amount");
        try { await _momoServices.MomoWalletReturnExecute(Request.Query); } catch { }

        var baseUrl = $"{FE_BASE}{(isSuccess ? FE_SUCCESS_PATH : FE_FAIL_PATH)}";
        var feUrl = $"{baseUrl}?status={(isSuccess ? "success" : "fail")}&amount={Uri.EscapeDataString(amount)}";
        return Content(BuildRedirectHtml(feUrl), "text/html");
    }

    // MoMo IPN (server-to-server) — xác thực & cập nhật đơn hàng
    [HttpPost("momo/ipn")]
    public async Task<IActionResult> MomoIpn([FromBody] MomoIpnModel body)
    {
        _logger.LogInformation("MOMO IPN: {@body}", body);

        // Lưu ý: Ở đây bạn PHẢI verify chữ ký HMAC đúng theo spec MoMo v2 trong service.
        var rs = await _momoServices.MomoIpnExecute(body);

        // MoMo mong nhận 200 OK với body JSON có resultCode/message
        // 0 = success; khác 0 = fail
        if (rs.Status == Const.SUCCESS_UPDATE_CODE)
            return Ok(new { resultCode = 0, message = "success" });

        return Ok(new { resultCode = 5, message = "fail" });
    }

    // NEW: IPN cho nạp ví MoMo
    [HttpPost("momo/wallet/ipn")]
    public async Task<IActionResult> MomoWalletIpn([FromBody] MomoIpnModel body)
    {
        _logger.LogInformation("MOMO WALLET IPN: {@body}", body);

        var rs = await _momoServices.MomoWalletIpnExecute(body);

        if (rs.Status == Const.SUCCESS_UPDATE_CODE)
            return Ok(new { resultCode = 0, message = "success" });

        return Ok(new { resultCode = 5, message = "fail" });
    }

    // helper render HTML redirect thân thiện
    private static string BuildRedirectHtml(string url) =>
        $@"<html><head><meta http-equiv='refresh' content='0;url={url}'/></head>
<body><script>window.location.replace('{url}');</script>Đang chuyển hướng…</body></html>";

}