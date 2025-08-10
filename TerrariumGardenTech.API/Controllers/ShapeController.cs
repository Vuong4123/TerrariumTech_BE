using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Shape;
using TerrariumGardenTech.Common.ResponseModel.Shape;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShapeController : ControllerBase
{
    public IShapeService _shapeService;

    public ShapeController(IShapeService shapeService)
    {
        _shapeService = shapeService ?? throw new ArgumentNullException(nameof(shapeService));
    }

    // GET: api/<RoleController>
    [HttpGet("get-all")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Get()
    {
        return await _shapeService.GetAllShapesAsync();
    }

    // GET api/<RoleController>/5
    [HttpGet("get-{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Get(int id)
    {
        return await _shapeService.GetShapeByIdAsync(id);
    }


    // POST api/<RoleController>
    [HttpPost("add-shape")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Post([FromBody] ShapeCreateRequest shapeCreateRequest)
    {
        return await _shapeService.CreateShapeAsync(shapeCreateRequest);
    }

    // PUT api/<RoleController>/5
    [HttpPut("update-shape-{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Put([FromBody] ShapeUpdateRequest shapeUpdateRequest)
    {
        return await _shapeService.UpdateShapeAsync(shapeUpdateRequest);
    }

    // DELETE api/<RoleController>/5
    [HttpDelete("delete-shape-{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _shapeService.DeleteShapeAsync(id);   
    }
}