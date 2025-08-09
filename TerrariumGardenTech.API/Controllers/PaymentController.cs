using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private readonly IMomoServices _momoServices;
    public PaymentController(IPayOsService payOsService, IVnPayService vnPayService,IMomoServices momoServices)
    {
        _payOsService = payOsService;
        _vnPayService = vnPayService;
        _momoServices = momoServices;
    }

    [HttpGet("pay-os/callback")]
    public async Task<IActionResult> ProcessPaymentFromPayOsCallback([FromQuery] PaymentReturnModel request)
    {
        var response = await _payOsService.ProcessPaymentCallback(request);
        if (response.Status == 200)
            return Redirect("https://terra-tech-garden.vercel.app/payment-success");

        return Redirect("trang-that-bai");
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
        var response = await _vnPayService.PaymentExecute(Request.Query);
        if (response.Status == 200)
            return Redirect("https://terra-tech-garden.vercel.app/payment-success");

        return Redirect("trang-that-bai");
    }

    [HttpPost("vn-pay")]
    public async Task<IActionResult> CreatePaymentUrl(PaymentInformationModel model)
    {
        var url = await _vnPayService.CreatePaymentUrl(model, HttpContext);

        return Ok(url);
    }
    [HttpPost("momo/create")]
    public async Task<IActionResult> CreatePaymentLink([FromBody] MomoRequest request)
    {
        var payUrl = await _momoServices.CreateMomoPaymentUrl(request);
        return Ok(new { PayUrl = payUrl });
    }



    //    [HttpGet("vn-pay/callback")]
    //    public async Task<IActionResult> PaymentCallback()
    //    {
    //        var response = await _vnPayService.PaymentExecute(Request.Query);
    //        if (response.Status == 200)
    //        {
    //            var html = @"<html>
    //<head>
    //    <meta http-equiv='refresh' content='0;url=https://terra-tech-garden.vercel.app/payment-success' />
    //</head>
    //<body>
    //    <script>
    //        window.location.href = 'https://terra-tech-garden.vercel.app/payment-success';
    //    </script>
    //    <p>Đang chuyển hướng về trang xác nhận thành công...</p>
    //</body>
    //</html>";
    //            return Content(html, "text/html");
    //        }

    //        // Thanh toán thất bại
    //        var failHtml = @"<html>
    //<head>
    //    <meta http-equiv='refresh' content='0;url=https://terra-tech-garden.vercel.app/payment-fail' />
    //</head>
    //<body>
    //    <script>
    //        window.location.href = 'https://terra-tech-garden.vercel.app/payment-fail';
    //    </script>
    //    <p>Thanh toán thất bại! Đang chuyển về trang xác nhận...</p>
    //</body>
    //</html>";
    //        return Content(failHtml, "text/html");
    //    }
}