using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common.RequestModel.TankMethod;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TankMethodController : ControllerBase
{
    private readonly ITankMethodService _tankMethodService;

    public TankMethodController(ITankMethodService tankMethodService)
    {
        _tankMethodService = tankMethodService;
    }

    // GET: api/<TankMethodController>
    [HttpGet("get-all")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Get()
    {
        return await _tankMethodService.GetAllTankMethodAsync();
    }

    // GET api/<TankMethodController>/5
    [HttpGet("get/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Get(int id)
    {
        return await _tankMethodService.GetTankMethodByIdAsync(id);
    }

    // POST api/<TankMethodController>
    [HttpPost("add-tankmethod")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Post([FromBody] TankMethodCreateRequest tankMethodCreateRequest)
    {
        return await _tankMethodService.CreateTankMethodAsync(tankMethodCreateRequest);
    }

    // PUT api/<TankMethodController>/5
    [HttpPut("update-tankmethod/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Put([FromBody] TankMethodUpdateRequest tankMethodUpdateRequest)
    {
        return await _tankMethodService.UpdateTankMethodAsync(tankMethodUpdateRequest);
    }

    // DELETE api/<TankMethodController>/5
    [HttpDelete("delete-tankmethod/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _tankMethodService.DeleteTankMethodAsync(id);
    }
}