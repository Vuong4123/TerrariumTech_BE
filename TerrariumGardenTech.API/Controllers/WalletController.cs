using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            var result = await _walletService.DepositAsync(userId, amount, method);
            return Ok(result);
        }

        [HttpPost("pay")]
        public async Task<IActionResult> Pay(int userId, decimal amount, int orderId)
        {
            var result = await _walletService.PayAsync(userId, amount, orderId);
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
            var balance = await _walletService.GetBalanceAsync(userId);
            return Ok(balance);
        }
    }
}
