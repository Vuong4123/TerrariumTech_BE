using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.API.Extensions;
using TerrariumGardenTech.Common.RequestModel.Transports;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransportController : ControllerBase
{
    private readonly ITransportService _transportService;

    public TransportController(ITransportService transportService)
    {
        _transportService = transportService;
    }

    /// <summary>
    /// Lấy thông tin đơn vận chuyển theo mã
    /// </summary>
    [HttpGet("{transportId:int}")]
    public async Task<IActionResult> GetById(int transportId)
    {
        var transport = await _transportService.GetById(transportId);
        if (transport == null)
        {
            return NotFound("Không tìm thấy đơn vận chuyển yêu cầu!");
        }
        return Ok(transport);
    }

    /// <summary>
    /// Tạo đơn vận chuyển
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransportModel request)
    {
        var currentUserId = User.GetUserId();
        var (success, message) = await _transportService.CreateTransport(request, currentUserId);
        if (!success)
            return BadRequest(message);
        return Ok(message);
    }

    /// <summary>
    /// Cập nhật thông tin đơn vận chuyển
    /// </summary>
    [HttpPut("{transportId:int}")]
    public async Task<IActionResult> Update(int transportId, [FromForm] UpdateTransportModel request)
    {
        var currentUserId = User.GetUserId();
        request.TransportId = transportId;
        var (success, message) = await _transportService.UpdateTransport(request, currentUserId);
        if (!success)
            return BadRequest(message);
        return Ok(message);
    }

    [HttpDelete("{transportId:int}")]
    public async Task<IActionResult> Remove(int transportId)
    {
        var (success, message) = await _transportService.DeleteTransport(transportId);
        if (!success)
            return BadRequest(message);
        return Ok(message);
    }
}
