using TerrariumGardenTech.Common.ResponseModel.Revenue;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface IRevenueService
{
    // Doanh thu
    Task<IBusinessResult> GetRevenueOverviewAsync(DateTime? from = null, DateTime? to = null);
    Task<IBusinessResult> GetRevenueByPeriodAsync(string period, DateTime? from = null, DateTime? to = null);
    Task<IBusinessResult> GetRevenueByProductAsync(DateTime? from = null, DateTime? to = null);

    // Đơn hàng
    Task<IBusinessResult> GetOrderStatsAsync(DateTime? from = null, DateTime? to = null);
    Task<IBusinessResult> GetOrdersByStatusAsync(DateTime? from = null, DateTime? to = null);
    Task<IBusinessResult> GetOrderTrendsAsync(string period, DateTime? from = null, DateTime? to = null);

    // Sản phẩm
    Task<IBusinessResult> GetTopSellingProductsAsync(int top = 10, DateTime? from = null, DateTime? to = null);
    Task<IBusinessResult> GetBundleAnalyticsAsync(DateTime? from = null, DateTime? to = null);

    // Conversion & Funnel
    Task<IBusinessResult> GetConversionRatesAsync(DateTime? from = null, DateTime? to = null);
    Task<AdminMembershipStatisticsDto> GetComprehensiveStatisticsAsync();
}