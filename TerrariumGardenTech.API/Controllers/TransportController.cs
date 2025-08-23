using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;
using TerrariumGardenTech.API.Extensions;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Enums;
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

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? orderId = null, [FromQuery] int? shipperId = null, [FromQuery] TransportStatusEnum? status = null, bool? isRefund = null, [FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
    {
        var (total, datas) = await _transportService.Paging(orderId, shipperId, status, isRefund, pageIndex, pageSize);
        return Ok(new
        {
            Total = total,
            Data = datas
        });
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
        var response = await _transportService.CreateTransport(request, currentUserId);
        if (response.Status != Const.SUCCESS_CREATE_CODE)
            return BadRequest(response);
        return Ok(response);
    }

    /// <summary>
    /// Cập nhật thông tin đơn vận chuyển
    /// </summary>
    [HttpPut("{transportId:int}")]
    public async Task<IActionResult> Update(int transportId, [FromForm] UpdateTransportModel request)
    {
        var currentUserId = User.GetUserId();
        request.TransportId = transportId;
        var response = await _transportService.UpdateTransport(request, currentUserId);
        if (response.Status != Const.SUCCESS_CREATE_CODE)
            return BadRequest(response);
        return Ok(response);
    }

    [HttpDelete("{transportId:int}")]
    public async Task<IActionResult> Remove(int transportId)
    {
        var response = await _transportService.DeleteTransport(transportId);
        if (response.Status != Const.SUCCESS_DELETE_CODE)
            return BadRequest(response);
        return Ok(response);
    }
}
