using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Payment;
using TerrariumGardenTech.Service.IService;

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
    private readonly IPayOsService _payOsService;
    private readonly IVnPayService _vnPayService;

    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPayOsService payOsService, IVnPayService vnPayService, ILogger<PaymentController> logger)
    {
        _payOsService = payOsService;
        _vnPayService = vnPayService;
        _logger = logger;
    }

    // ====================== PAYOS ======================

    [HttpPost("pay-os")]
    public async Task<IActionResult> CreatePayOsLink([FromBody] PaymentCreateRequest request)
    {
        if (!ModelState.IsValid || request.OrderId <= 0)
            return BadRequest("Invalid order id");

        var result = await _payOsService.CreatePaymentLink(request.OrderId, request.Description ?? string.Empty);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("pay-os/callback")]
    public async Task<IActionResult> PayOsCallback([FromQuery] PaymentReturnModel request)
    {
        _logger.LogInformation("PAYOS callback: {@q}",
            Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()));

        var result = await _payOsService.ProcessPaymentCallback(request);

        // Giữ nguyên query khi đẩy về FE (nếu PayOS có trả các tham số cần)
        var feBase = $"{FE_BASE}{FE_SUCCESS_PATH}";
        var qs = Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty;
        var feUrl = feBase + qs;

        // (hoặc) nếu FE chỉ cần alias:
        var orderId = request.OrderId;
        var status = result.Status == Const.SUCCESS_UPDATE_CODE ? "success" : "fail";
        feUrl = $"{FE_BASE}{FE_SUCCESS_PATH}?orderId={orderId}&status={status}";

        var html = $@"<html><head><meta http-equiv='refresh' content='0;url={feUrl}'/></head>
<body><script>window.location.replace('{feUrl}');</script>Đang chuyển hướng…</body></html>";
        return Content(html, "text/html");
    }

    // ====================== VNPAY ======================

    [HttpPost("vn-pay")]
    public async Task<IActionResult> CreateVnPayUrl([FromBody] PaymentInformationModel model)
    {
        if (!ModelState.IsValid || model.OrderId <= 0)
            return BadRequest("Invalid order id");

        var result = await _vnPayService.CreatePaymentUrl(model, HttpContext);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpGet("vn-pay/callback")]
    public async Task<IActionResult> VnPayCallback()
    {
        // Log toàn bộ params VNPAY trả về
        _logger.LogInformation("VNPAY callback: {@q}",
            Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()));

        // 1) xử lý & lưu DB
        var result = await _vnPayService.PaymentExecute(Request.Query);

        // 2) redirect về FE, giữ nguyên toàn bộ vnp_* đúng như VNPAY trả
        var feBase = $"{FE_BASE}{FE_SUCCESS_PATH}";
        var qs = Request.QueryString.HasValue ? Request.QueryString.Value : string.Empty;
        var feUrl = feBase + qs;

        // (tuỳ chọn) thêm alias cho FE:
        var orderId = Request.Query["vnp_TxnRef"].ToString();
        var amountRaw = Request.Query["vnp_Amount"].ToString();
        var amountVnd = int.TryParse(amountRaw, out var a) ? a / 100 : 0;
        var success = result.Status == Const.SUCCESS_UPDATE_CODE;
        feUrl += $"&orderId={orderId}&amount={amountVnd}&status={(success ? "success" : "fail")}";

        var html = $@"<html><head><meta http-equiv='refresh' content='0;url={feUrl}'/></head>
<body><script>window.location.replace('{feUrl}');</script>Đang chuyển hướng…</body></html>";
        return Content(html, "text/html");
    }
}