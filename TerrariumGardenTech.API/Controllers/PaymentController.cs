using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Payment;

namespace TerrariumGardenTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IPayOsService _payOsService;

    public PaymentController(IPayOsService payOsService)
    {
        _payOsService = payOsService;
    }

    [HttpGet("callback")]
    public async Task<IActionResult> ProcessPaymentCallback([FromQuery] PaymentReturnModel response)
    {
        await _payOsService.ProcessPaymentCallback(response);
        return Redirect("trang-cam-on");
    }

    [HttpPost]
    public async Task<IActionResult> TestPayOs([FromBody] PaymentCreateRequest request)
    {
        var msg = await _payOsService.CreatePaymentLink(request.OrderId, request.Description);
        return Ok(msg);
    }
}