using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Accessory;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = "Admin,Staff")]
public class AccessoryController : ControllerBase
{
    private readonly IAccessoryService _accessoryService;

    public AccessoryController(IAccessoryService accessoryService)
    {
        _accessoryService = accessoryService ?? throw new ArgumentNullException(nameof(accessoryService));
    }

    // GET: api/<AccessoryController>
    [HttpGet("get-all")]
    [Authorize(Roles = "Admin,Staff,Manager,User")]

    public async Task<IBusinessResult> Get([FromQuery] AccessoryGetAllRequest request)
    {
        return await _accessoryService.GetAll(request);
    }

    [HttpGet("get-by-name/{name}")]
    [Authorize(Roles = "Admin,Staff,Manager,User")]
    public async Task<IBusinessResult> GetByAccesname(string name)
    {
        return await _accessoryService.GetByAccesname(name);
    }

    [HttpGet("filter")]
    [Authorize(Roles = "Admin,Staff,Manager,User")]
    public async Task<IBusinessResult> FilterByCategory([FromQuery] int categoryId)
    {
        return await _accessoryService.FilterAccessoryAsync(categoryId);
    }

    // GET api/<AccessoryController>/5
    [HttpGet("get/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Get(int id)
    {
        return await _accessoryService.GetById(id);
    }

    // POST api/<AccessoryController>
    [HttpPost("add-accessory")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Post(AccessoryCreateRequest accessoryCreateRequest)
    {
        return await _accessoryService.CreateAccessory(accessoryCreateRequest);
    }

    // PUT api/<AccessoryController>/5
    [HttpPut("update-accessory/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Put(AccessoryUpdateRequest accessoryUpdateRequest)
    {
        return await _accessoryService.UpdateAccessory(accessoryUpdateRequest);
    }

    // DELETE api/<AccessoryController>/5
    [HttpDelete("delete-accessory/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _accessoryService.DeleteById(id);
    }
}