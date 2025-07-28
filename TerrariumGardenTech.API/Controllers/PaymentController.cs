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

    public PaymentController(IPayOsService payOsService, IVnPayService vnPayService)
    {
        _payOsService = payOsService;
        _vnPayService = vnPayService;
    }

    [HttpGet("pay-os/callback")]
    public async Task<IActionResult> ProcessPaymentFromPayOsCallback([FromQuery] PaymentReturnModel request)
    {
        var response = await _payOsService.ProcessPaymentCallback(request);
        if (response.Status == 200)
            return Redirect("trang-cam-on");

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
            return Redirect("trang-cam-on");

        return Redirect("trang-that-bai");
    }

    [HttpPost("vn-pay")]
    public async Task<IActionResult> CreatePaymentUrl(PaymentInformationModel model)
    {
        var url = await _vnPayService.CreatePaymentUrl(model, HttpContext);

        return Ok(url);
    }
}