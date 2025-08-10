using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Role;
using TerrariumGardenTech.Common.ResponseModel.Role;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoleController : ControllerBase
{
    public IRoleService _roleService;

    public RoleController(IRoleService roleService)
    {
        _roleService = roleService ?? throw new ArgumentNullException(nameof(roleService));
    }

    // GET: api/<RoleController>
    [HttpGet("get-all")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Get()
    {
        return await _roleService.GetAll();
    }

    // GET api/<RoleController>/5
    [HttpGet("get/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Get(int id)
    {
        return await _roleService.GetById(id);
    }


    // POST api/<RoleController>
    [HttpPost("add-role")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Post([FromBody] RoleCreateRequest roleCreateRequest)
    {
        return await _roleService.CreateRole(roleCreateRequest);
    }

    // PUT api/<RoleController>/5
    [HttpPut("update-role/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Put([FromBody] RoleUpdateRequest roleUpdateRequest)
    {
        return await _roleService.UpdateRole(roleUpdateRequest);
    }

    // DELETE api/<RoleController>/5
    [HttpDelete("delete-role/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _roleService.DeleteById(id);
    }
}