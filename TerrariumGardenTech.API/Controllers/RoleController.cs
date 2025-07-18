using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Role;
using TerrariumGardenTech.Service.ResponseModel.Role;

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
    public async Task<IBusinessResult> Get()
    {
        var result = await _roleService.GetAll();
        if (result == null || result.Data == null) return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
        var role = (result.Data as IEnumerable<Role>)?.Select(r => new RoleResponse
        {
            RoleId = r.RoleId,
            RoleName = r.RoleName,
            Description = r.Description
        }).ToList();
        if (role == null) return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
        return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", role);
    }

    // GET api/<RoleController>/5
    [HttpGet("get-{id}")]
    public async Task<IBusinessResult> Get(int id)
    {
        var result = await _roleService.GetById(id);
        if (result == null || result.Data == null) return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
        // Kiểm tra kiểu dữ liệu của result.Data (đảm bảo nó là Category, không phải IEnumerable)
        if (result.Data is Role role)
        {
            // Ánh xạ dữ liệu từ Category sang CategoryRequest
            var roleResponse = new RoleResponse
            {
                RoleId = role.RoleId,
                RoleName = role.RoleName,
                Description = role.Description
            };

            // Trả về BusinessResult với dữ liệu đã ánh xạ
            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", role);
        }

        // Trả về lỗi nếu không thể ánh xạ
        return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
    }


    // POST api/<RoleController>
    [HttpPost]
    public async Task<IBusinessResult> Post([FromBody] RoleCreateRequest roleCreateRequest)
    {
        if (roleCreateRequest == null) return new BusinessResult(Const.ERROR_EXCEPTION, "Invalid request data.");
        return await _roleService.CreateRole(roleCreateRequest);
    }

    // PUT api/<RoleController>/5
    [HttpPut("{id}")]
    public async Task<IBusinessResult> Put([FromBody] RoleUpdateRequest roleUpdateRequest)
    {
        if (roleUpdateRequest == null || !ModelState.IsValid)
            return new BusinessResult(Const.FAIL_CREATE_CODE, "Invalid request data.");
        return await _roleService.UpdateRole(roleUpdateRequest);
    }

    // DELETE api/<RoleController>/5
    [HttpDelete("{id}")]
    public async Task<IBusinessResult> Delete(int id)
    {
        var result = await _roleService.DeleteById(id);
        if (result == null || result.Data == null) return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
        if (result.Data is bool isDeleted)
            return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Role deleted successfully.");
        return new BusinessResult(Const.FAIL_DELETE_CODE, "Failed to delete role.");
    }
}