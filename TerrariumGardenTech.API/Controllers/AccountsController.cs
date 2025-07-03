using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.UserManagement;
using TerrariumGardenTech.Repositories.Enums;

namespace TerrariumGardenTech.API.Controllers
{
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

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] AccountCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.RoleId != (int)RoleStatus.Staff && request.RoleId != (int)RoleStatus.Manager)
                return BadRequest(new { message = "Role không hợp lệ, chỉ được tạo Staff hoặc Manager" });

            var (code, message) = await _accountService.CreateAccountAsync(request);
            if (code != Const.SUCCESS_CREATE_CODE)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        // Lấy tất cả tài khoản
        [HttpGet]
        public async Task<IActionResult> GetAllAccounts()
        {
            var (code, message, accounts) = await _accountService.GetAllAccountsAsync();
            if (code != Const.SUCCESS_READ_CODE)
                return BadRequest(new { message });

            return Ok(accounts);
        }

        // Lấy tài khoản theo vai trò
        [HttpGet("role/{role}")]
        public async Task<IActionResult> GetAccountsByRole(string role, int page = 1, int pageSize = 20)
        {
            var (code, message, accounts) = await _accountService.GetAccountsByRoleAsync(role, page, pageSize);
            if (code != Const.SUCCESS_READ_CODE)
                return BadRequest(new { message });

            return Ok(accounts);
        }

        // Thay đổi trạng thái tài khoản
        [HttpPut("status/{userId}")]
        public async Task<IActionResult> ChangeAccountStatus(int userId, [FromBody] string status)
        {
            if (string.IsNullOrEmpty(status) || !Enum.IsDefined(typeof(AccountStatus), status))
                return BadRequest(new { message = "Trạng thái không hợp lệ" });

            var (code, message) = await _accountService.ChangeAccountStatusAsync(userId, status);
            if (code != Const.SUCCESS_UPDATE_CODE)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        // Lấy tài khoản theo ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(int id)
        {
            var (code, message, account) = await _accountService.GetAccountByIdAsync(id);
            if (code != Const.SUCCESS_READ_CODE)
                return NotFound(new { message });

            return Ok(account);
        }

        // Cập nhật tài khoản
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] AccountUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var (code, message) = await _accountService.UpdateAccountAsync(id, request);
            if (code != Const.SUCCESS_UPDATE_CODE)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        // Xóa tài khoản
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var (code, message) = await _accountService.DeleteAccountAsync(id);
            if (code != Const.SUCCESS_DELETE_CODE)
                return BadRequest(new { message });

            return Ok(new { message });
        }
    }
}
