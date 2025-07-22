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

    public PaymentController(IPayOsService payOsService)
    {
        _payOsService = payOsService;
    }

    [HttpGet("callback")]
    public async Task<IActionResult> ProcessPaymentFromPayOsCallback([FromQuery] PaymentReturnModel response)
    {
        await _payOsService.ProcessPaymentCallback(response);
        return Redirect("trang-cam-on");
    }

    [HttpPost]
    public async Task<IActionResult> CreatePaymentFromPayOsLink([FromBody] PaymentCreateRequest request)
    {
        var msg = await _payOsService.CreatePaymentLink(request.OrderId, request.Description);
        return Ok(msg);
    }
}