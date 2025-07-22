using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Accessory;
using TerrariumGardenTech.Service.ResponseModel.Accessory;

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
    public async Task<IBusinessResult> Get()
    {
        return await _accessoryService.GetAll();
    }

    [HttpGet("get-details")]
    public async Task<IBusinessResult> GetDetail()
    {
        return await _accessoryService.GetAllDetail();
    }

    [HttpGet("filter")]
    public async Task<IBusinessResult> FilterTerrariums([FromQuery] int categoryId)
    {
        return await _accessoryService.FilterAccessoryAsync(categoryId);
    }

    // GET api/<AccessoryController>/5
    [HttpGet("get-{id}")]
    public async Task<IBusinessResult> Get(int id)
    {
        return await _accessoryService.GetById(id);
    }

    // POST api/<AccessoryController>
    [HttpPost("add-accessory")]
    public async Task<IBusinessResult> Post(AccessoryCreateRequest accessoryCreateRequest)
    {
        if (accessoryCreateRequest == null || !ModelState.IsValid)
            return new BusinessResult(Const.FAIL_CREATE_CODE, "Invalid request data.");
        return await _accessoryService.CreateAccessory(accessoryCreateRequest);
    }

    // PUT api/<AccessoryController>/5
    [HttpPut("update-accessory{id}")]
    public async Task<IBusinessResult> Put(AccessoryUpdateRequest accessoryUpdateRequest)
    {
        if (accessoryUpdateRequest == null || !ModelState.IsValid)
            return new BusinessResult(Const.FAIL_UPDATE_CODE, "Invalid request data.");
        return await _accessoryService.UpdateAccessory(accessoryUpdateRequest);
    }

    // DELETE api/<AccessoryController>/5
    [HttpDelete("delete-accessory{id}")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _accessoryService.DeleteById(id);
    }
}