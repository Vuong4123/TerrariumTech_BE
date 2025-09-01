using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.API.Extensions;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WalletController : ControllerBase
    {
        private readonly IWalletServices _walletService;

        public WalletController(IWalletServices walletService)
        {
            _walletService = walletService;
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit(int userId, decimal amount, string method)
        {
            var currentUserId = User.GetUserId();
            var result = await _walletService.DepositAsync(currentUserId, amount, method);
            return Ok(result);
        }

        [HttpPost("pay")]
        public async Task<IActionResult> Pay(int userId, decimal amount, int orderId)
        {
            var currentUserId = User.GetUserId();
            var result = await _walletService.PayAsync(currentUserId, amount, orderId);
            return Ok(result);
        }

        [HttpPost("refund")]
        public async Task<IActionResult> Refund(int userId, decimal amount, int orderId)
        {
            var result = await _walletService.RefundAsync(userId, amount, orderId);
            return Ok(result);
        }

        [HttpGet("balance")]
        public async Task<IActionResult> GetBalance(int userId)
        {
            var currentUserId = User.GetUserId();
            var balance = await _walletService.GetBalanceAsync(currentUserId);
            return Ok(balance);
        }
        /// <summary>
        /// Lấy biến động số dư ví của người dùng
        /// </summary>
        [HttpGet("balance-history/{userId}")]
        public async Task<IActionResult> GetWalletBalanceHistory(
            int userId,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var result = await _walletService.GetWalletBalanceHistoryAsync(userId, fromDate, toDate);

                return Ok(new BusinessResult(
                    Const.SUCCESS_READ_CODE,
                    "Lấy biến động số dư ví thành công",
                    result
                ));
            }
            catch (Exception ex)
            {
                return NotFound(new BusinessResult(Const.ERROR_EXCEPTION, ex.Message));
            }
        }
        [HttpGet("admin/all-history")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAllWalletHistoryForAdmin(
       [FromQuery] DateTime? fromDate = null,
       [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var result = await _walletService.GetAllWalletHistoryForAdminAsync(fromDate, toDate);
                return Ok(new
                {
                    success = true,
                    message = $"Lấy được {result.TotalTransactions} giao dịch từ {result.TotalWallets} ví",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
