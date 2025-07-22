using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.UserManagement;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AccountsController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    // Tạo tài khoản mới
    [HttpPost]
    public async Task<IBusinessResult> CreateAccount([FromBody] AccountCreateRequest request)
    {
        if (!ModelState.IsValid) return new BusinessResult(Const.FAIL_CREATE_CODE, "Dữ liệu không hợp lệ.");

        if (request.RoleId != (int)RoleStatus.Staff && request.RoleId != (int)RoleStatus.Manager)
            return new BusinessResult(Const.FAIL_CREATE_CODE, "Role không hợp lệ, chỉ được tạo Staff hoặc Manager.");

        return await _accountService.CreateAccountAsync(request);
    }

    // Lấy tất cả tài khoản
    [HttpGet]
    public async Task<IBusinessResult> GetAllAccounts()
    {
        return await _accountService.GetAllAccountsAsync();
    }

    // Lấy tài khoản theo vai trò
    [HttpGet("role/{role}")]
    public async Task<IBusinessResult> GetAccountsByRole(string role, int page = 1, int pageSize = 20)
    {
        return await _accountService.GetAccountsByRoleAsync(role, page, pageSize);
    }

    // Thay đổi trạng thái tài khoản
    [HttpPut("status/{userId}")]
    public async Task<IBusinessResult> ChangeAccountStatus(int userId, [FromBody] string status)
    {
        if (string.IsNullOrEmpty(status) || !Enum.IsDefined(typeof(AccountStatus), status))
            return new BusinessResult(Const.FAIL_UPDATE_CODE, "Trạng thái không hợp lệ");

        return await _accountService.ChangeAccountStatusAsync(userId, status);
    }

    // Lấy tài khoản theo ID
    [HttpGet("{id}")]
    public async Task<IBusinessResult> GetAccountById(int id)
    {
        return await _accountService.GetAccountByIdAsync(id);
    }

    // Cập nhật tài khoản
    [HttpPut("{id}")]
    public async Task<IBusinessResult> UpdateAccount(int id, [FromBody] AccountUpdateRequest request)
    {
        if (!ModelState.IsValid) return new BusinessResult(Const.FAIL_UPDATE_CODE, "Dữ liệu không hợp lệ.");

        return await _accountService.UpdateAccountAsync(id, request);
    }

    // Xóa tài khoản
    [HttpDelete("{id}")]
    public async Task<IBusinessResult> DeleteAccount(int id)
    {
        return await _accountService.DeleteAccountAsync(id);
    }
}