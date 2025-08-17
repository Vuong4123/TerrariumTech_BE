using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Reneuve;
using TerrariumGardenTech.Common.ResponseModel.Revenue;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AnalyticsController : ControllerBase
    {
        private readonly IRevenueService _analyticsService;

        public AnalyticsController(IRevenueService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        #region Revenue Endpoints

        /// <summary>
        /// Tổng quan doanh thu - Dashboard chính
        /// </summary>
        [HttpGet("revenue/overview")]
        public async Task<IActionResult> GetRevenueOverview([FromQuery] AnalyticsBaseRequest request)
        {
            var result = await _analyticsService.GetRevenueOverviewAsync(request.From, request.To);

            if (result.Status == Const.SUCCESS_READ_CODE)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        /// <summary>
        /// Doanh thu theo kỳ (daily, weekly, monthly, yearly)
        /// </summary>
        [HttpGet("revenue/period/{period}")]
        public async Task<IActionResult> GetRevenueByPeriod(
            string period,
            [FromQuery] AnalyticsBaseRequest request)
        {
            var result = await _analyticsService.GetRevenueByPeriodAsync(period, request.From, request.To);

            if (result.Status == Const.SUCCESS_READ_CODE)
            {
                return Ok(result);
            }

            if (result.Status == Const.BAD_REQUEST_CODE)
            {
                return BadRequest(result);
            }

            return StatusCode(500, result);
        }

        /// <summary>
        /// Doanh thu theo sản phẩm (bể vs phụ kiện)
        /// </summary>
        [HttpGet("revenue/products")]
        public async Task<IActionResult> GetRevenueByProduct([FromQuery] AnalyticsBaseRequest request)
        {
            var result = await _analyticsService.GetRevenueByProductAsync(request.From, request.To);

            if (result.Status == Const.SUCCESS_READ_CODE)
            {
                return Ok(result);
            }

            return StatusCode(500, result);
        }

        #endregion

        #region Order Endpoints

        /// <summary>
        /// Thống kê tổng quan đơn hàng
        /// </summary>
        [HttpGet("orders/stats")]
        public async Task<IActionResult> GetOrderStats([FromQuery] AnalyticsBaseRequest request)
        {
            var result = await _analyticsService.GetOrderStatsAsync(request.From, request.To);

            if (result.Status == Const.SUCCESS_READ_CODE)
            {
                return Ok(result);
            }

            return StatusCode(500, result);
        }

        /// <summary>
        /// Xu hướng đơn hàng theo thời gian
        /// </summary>
        [HttpGet("orders/trends/{period}")]
        public async Task<IActionResult> GetOrderTrends(
            string period,
            [FromQuery] AnalyticsBaseRequest request)
        {
            var result = await _analyticsService.GetOrderTrendsAsync(period, request.From, request.To);

            if (result.Status == Const.SUCCESS_READ_CODE)
            {
                return Ok(result);
            }

            if (result.Status == Const.BAD_REQUEST_CODE)
            {
                return BadRequest(result);
            }

            return StatusCode(500, result);
        }

        /// <summary>
        /// Đơn hàng theo trạng thái
        /// </summary>
        [HttpGet("orders/status")]
        public async Task<IActionResult> GetOrdersByStatus([FromQuery] AnalyticsBaseRequest request)
        {
            var result = await _analyticsService.GetOrdersByStatusAsync(request.From, request.To);

            if (result.Status == Const.SUCCESS_READ_CODE)
            {
                return Ok(result);
            }

            if (result.Status == Const.WARNING_NO_DATA_CODE)
            {
                return Ok(result); // No data is still OK, just empty result
            }

            if (result.Status == Const.FAIL_READ_CODE)
            {
                return StatusCode(500, result);
            }

            return BadRequest(result);
        }

        #endregion

        #region Product Endpoints

        /// <summary>
        /// Top sản phẩm bán chạy
        /// </summary>
        [HttpGet("products/top-selling")]
        public async Task<IActionResult> GetTopSellingProducts([FromQuery] TopProductsRequest request)
        {
            var result = await _analyticsService.GetTopSellingProductsAsync(request.Top, request.From, request.To);

            if (result.Status == Const.SUCCESS_READ_CODE)
            {
                return Ok(result);
            }

            return StatusCode(500, result);
        }

        /// <summary>
        /// Phân tích Bundle (combo bể + phụ kiện)
        /// </summary>
        [HttpGet("products/bundles")]
        public async Task<IActionResult> GetBundleAnalytics([FromQuery] AnalyticsBaseRequest request)
        {
            var result = await _analyticsService.GetBundleAnalyticsAsync(request.From, request.To);

            if (result.Status == Const.SUCCESS_READ_CODE)
            {
                return Ok(result);
            }

            return StatusCode(500, result);
        }

        #endregion

        #region Conversion Endpoints

        /// <summary>
        /// Tỷ lệ chuyển đổi và funnel analysis
        /// </summary>
        [HttpGet("conversion/rates")]
        public async Task<IActionResult> GetConversionRates([FromQuery] AnalyticsBaseRequest request)
        {
            var result = await _analyticsService.GetConversionRatesAsync(request.From, request.To);

            if (result.Status == Const.SUCCESS_READ_CODE)
            {
                return Ok(result);
            }

            return StatusCode(500, result);
        }

        #endregion
    }
}
