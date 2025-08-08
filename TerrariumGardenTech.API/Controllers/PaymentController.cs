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
    private readonly IPayOsService _payOsService;
    private readonly IVnPayService _vnPayService;

    private readonly ILogger<PaymentController> _logger;

    public PaymentController(IPayOsService payOsService, IVnPayService vnPayService, ILogger<PaymentController> logger)
    {
        _payOsService = payOsService;
        _vnPayService = vnPayService;
        _logger = logger;
    }

    [HttpGet("pay-os/callback")]
    public async Task<IActionResult> ProcessPaymentFromPayOsCallback([FromQuery] PaymentReturnModel request)
    {
        //var response = await _payOsService.ProcessPaymentCallback(request);
        //if (response.Status == 200)
        //    return Redirect("https://terra-tech-garden.vercel.app/payment-success");

        //return Redirect("trang-that-bai");
        _logger.LogInformation("PayOS callback query: {@q}",
            Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()));

        var res = await _payOsService.ProcessPaymentCallback(request);

        var feUrl = res.Status == Const.SUCCESS_UPDATE_CODE
            ? $"https://terra-tech-garden.vercel.app/payment-success?orderId={request.OrderId}"
            : $"https://terra-tech-garden.vercel.app/payment-fail?orderId={request.OrderId}";

        var html = $@"<html><head><meta http-equiv='refresh' content='0;url={feUrl}'/></head>
            <body><script>window.location.replace('{feUrl}');</script>Đang chuyển hướng...</body></html>";
        return Content(html, "text/html");
    }

    [HttpPost("pay-os")]
    public async Task<IActionResult> CreatePaymentFromPayOsLink([FromBody] PaymentCreateRequest request)
    {
        var msg = await _payOsService.CreatePaymentLink(request.OrderId, request.Description);
        return Ok(msg);
    }

    [HttpGet("vn-pay/callback")]
    public async Task<IActionResult> PaymentCallback()
    {
        //var response = await _vnPayService.PaymentExecute(Request.Query);
        //if (response.Status == 200)
        //    return Redirect("https://terra-tech-garden.vercel.app/payment-success");

        //return Redirect("trang-that-bai");
        _logger.LogInformation("VNPAY callback: {@q}", Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()));

        var res = await _vnPayService.PaymentExecute(Request.Query);
        // cố lấy orderId từ vnp_TxnRef nếu có
        var orderIdStr = Request.Query["vnp_TxnRef"].ToString();
        var feUrl = res.Status == Const.SUCCESS_UPDATE_CODE
            ? $"https://terra-tech-garden.vercel.app/payment-success?orderId={orderIdStr}"
            : $"https://terra-tech-garden.vercel.app/payment-success?orderId={orderIdStr}";

        var html = $@"<html><head><meta http-equiv='refresh' content='0;url={feUrl}' /></head>
            <body><script>window.location.replace('{feUrl}');</script>Đang chuyển hướng...</body></html>";
        return Content(html, "text/html");
    }

    [HttpPost("vn-pay")]
    public async Task<IActionResult> CreatePaymentUrl(PaymentInformationModel model)
    {
        var url = await _vnPayService.CreatePaymentUrl(model, HttpContext);

        return Ok(url);
    }


//    // -------- PAYOS --------

//    [AllowAnonymous]
//    [HttpGet("pay-os/callback")]
//    public async Task<IActionResult> PayOsCallback([FromQuery] PaymentReturnModel request)
//    {
//        _logger.LogInformation("PayOS callback: {@q}", Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()));

//        var res = await _payOsService.ProcessPaymentCallback(request);

//        // FE redirect url
//        var feUrl = res.Status == Const.SUCCESS_UPDATE_CODE
//            ? $"{FE_BASE}/payment-success?orderId={request.OrderId}"
//            : $"{FE_BASE}/payment-fail?orderId={request.OrderId}";

//        // Return HTML auto redirect (fallback nếu browser chặn 3xx)
//        var html = $@"<html><head><meta http-equiv='refresh' content='0;url={feUrl}' /></head>
//<body><script>window.location.replace('{feUrl}');</script>Đang chuyển hướng...</body></html>";
//        return Content(html, "text/html");
//    }

//    [HttpPost("pay-os")]
//    public async Task<IActionResult> CreatePayOsLink([FromBody] PaymentCreateRequest request)
//    {
//        if (!ModelState.IsValid || request.OrderId <= 0)
//            return BadRequest("Invalid order id");

//        var msg = await _payOsService.CreatePaymentLink(request.OrderId, request.Description ?? string.Empty);
//        return Ok(msg); // msg.Data nên là checkoutUrl
//    }

//    // -------- VNPAY --------

//    [AllowAnonymous]
//    [HttpGet("vn-pay/callback")]
//    public async Task<IActionResult> VnPayCallback()
//    {
//        _logger.LogInformation("VNPAY callback: {@q}", Request.Query.ToDictionary(x => x.Key, x => x.Value.ToString()));

//        var res = await _vnPayService.PaymentExecute(Request.Query);
//        // cố lấy orderId từ vnp_TxnRef nếu có
//        var orderIdStr = Request.Query["vnp_TxnRef"].ToString();
//        var feUrl = res.Status == Const.SUCCESS_UPDATE_CODE
//            ? $"{FE_BASE}/payment-success?orderId={orderIdStr}"
//            : $"{FE_BASE}/payment-fail?orderId={orderIdStr}";

//        var html = $@"<html><head><meta http-equiv='refresh' content='0;url={feUrl}' /></head>
//<body><script>window.location.replace('{feUrl}');</script>Đang chuyển hướng...</body></html>";
//        return Content(html, "text/html");
//    }

//    [HttpPost("vn-pay")]
//    public async Task<IActionResult> CreateVnPayUrl([FromBody] PaymentInformationModel model)
//    {
//        if (!ModelState.IsValid || model.OrderId <= 0)
//            return BadRequest("Invalid order id");

//        var url = await _vnPayService.CreatePaymentUrl(model, HttpContext);
//        return Ok(url); // url.Data là paymentUrl
//    }
}