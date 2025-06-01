using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.UserManagement;

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

            if (request.RoleId != 2 && request.RoleId != 3)
                return BadRequest(new { message = "Role không hợp lệ, chỉ được tạo Staff hoặc Manager" });

            var (code, message) = await _accountService.CreateAccountAsync(request);
            if (code != Const.SUCCESS_CREATE_CODE)
                return BadRequest(new { message });

            return Ok(new { message });
        }

        [HttpGet]
        public async Task<IActionResult> GetAccounts(int page = 1, int pageSize = 20)
        {
            var (code, message, accounts) = await _accountService.GetAccountsAsync(page, pageSize);
            if (code != Const.SUCCESS_READ_CODE)
                return BadRequest(new { message });

            return Ok(accounts);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(int id)
        {
            var (code, message, account) = await _accountService.GetAccountByIdAsync(id);
            if (code != Const.SUCCESS_READ_CODE)
                return NotFound(new { message });

            return Ok(account);
        }

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
