namespace TerrariumGardenTech.Common.ResponseModel.Revenue;

#region Revenue Models

public class RevenueOverviewResponse
{
    public decimal TotalRevenue { get; set; } // Tổng doanh thu kỳ hiện tại
    public decimal AdjustedRevenue { get; set; } // Doanh thu đã điều chỉnh sau khi trừ hoàn tiền
    public decimal PreviousPeriodRevenue { get; set; } // Tổng doanh thu kỳ trước
    public decimal RevenueGrowthPercent { get; set; } // Tỷ lệ tăng trưởng
    public int TotalOrders { get; set; } // Số lượng đơn hàng hoàn thành
    public decimal AverageOrderValue { get; set; } // Giá trị trung bình mỗi đơn hàng
    public int TotalCustomers { get; set; } // Tổng số khách hàng
    public List<RevenueByMonth> RevenueByMonth { get; set; } = new(); // Doanh thu theo tháng
    public List<RevenueByCategory> RevenueByPaymentStatus { get; set; } = new(); // Doanh thu theo trạng thái thanh toán
}


public class RevenueByPeriodResponse
{
    public string Period { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<RevenueByPeriod> Data { get; set; } = new();
}

public class RevenueByPeriod
{
    public string Period { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public DateTime Date { get; set; }
}
public class RevenueByMonth
{
    public string Period { get; set; }           // "2024-01", "2024-02", etc.
    public decimal Revenue { get; set; }         // Doanh thu tháng đó
    public int OrderCount { get; set; }          // Số đơn hàng tháng đó
    public DateTime Date { get; set; }           // Ngày đầu tháng
    public decimal AverageOrderValue { get; set; } // Giá trị TB mỗi đơn
}

public class ProductRevenueResponse
{
    public decimal TotalRevenue { get; set; }
    public decimal TerrariumRevenue { get; set; }
    public decimal TerrariumPercentage { get; set; }
    public decimal AccessoryRevenue { get; set; }
    public decimal AccessoryPercentage { get; set; }
    public List<TopProduct> TopTerrariums { get; set; } = new();
    public List<TopProduct> TopAccessories { get; set; } = new();
}

#endregion

#region Order Models

public class OrderStatsResponse
{
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int PendingOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public double CompletionRate { get; set; }
    public double CancellationRate { get; set; }
    public decimal AverageOrderValue { get; set; }
    public double AverageItemsPerOrder { get; set; }
    public List<OrderStatusStat> OrdersByStatus { get; set; } = new();
}

public class OrderTrendsResponse
{
    public string Period { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalOrders { get; set; }
    public List<OrderTrendData> Data { get; set; } = new();
}

public class OrderTrendData
{
    public string Period { get; set; }
    public DateTime Date { get; set; }
    public int TotalOrders { get; set; }
    public int CompletedOrders { get; set; }
    public int CancelledOrders { get; set; }
    public decimal Revenue { get; set; }
}

#endregion

#region Customer Models

public class CustomerStatsResponse
{
    public int TotalCustomers { get; set; }
    public int NewCustomers { get; set; }
    public int ReturningCustomers { get; set; }
    public int OneTimeCustomers { get; set; }
    public double AverageOrdersPerCustomer { get; set; }
    public decimal AverageSpentPerCustomer { get; set; }
    public List<CustomerSegment> CustomerSegmentation { get; set; } = new();
}

public class TopCustomer
{
    public int UserId { get; set; }
    public string UserEmail { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal AverageOrderValue { get; set; }
    public DateTime LastOrderDate { get; set; }
}

public class RetentionByPeriod
{
    public string Period { get; set; }
    public DateTime Date { get; set; }
    public double RetentionRate { get; set; }
    public int ActiveCustomers { get; set; }
    public int NewCustomers { get; set; }
}

#endregion

#region Product Models

public class TopSellingProductsResponse
{
    public List<TopProduct> TopTerrariums { get; set; } = new();
    public List<TopProduct> TopAccessories { get; set; } = new();
    public string Period { get; set; }
}

public class TopProduct
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int TotalQuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
}

public class ProductPerformanceResponse
{
    public int TotalProducts { get; set; }
    public int ActiveProducts { get; set; }           // Có bán trong kỳ
    public int TopPerformers { get; set; }            // Top 20% sản phẩm
    public decimal TotalRevenue { get; set; }
    public List<ProductPerformance> TerrariumPerformance { get; set; } = new();
    public List<ProductPerformance> AccessoryPerformance { get; set; } = new();
    public List<string> LowPerformingProducts { get; set; } = new(); // Sản phẩm bán ít
}

public class ProductPerformance
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
    public double MarketSharePercent { get; set; }    // Thị phần %
    public string PerformanceLevel { get; set; }      // "High", "Medium", "Low"
}

public class BundleAnalyticsResponse
{
    public int TotalOrders { get; set; }
    public int BundleOrders { get; set; }
    public int SingleProductOrders { get; set; }
    public double BundleOrdersPercentage { get; set; }
    public decimal BundleRevenue { get; set; }
    public decimal SingleProductRevenue { get; set; }
    public decimal BundleRevenuePercentage { get; set; }
    public decimal AverageBundleValue { get; set; }
    public decimal AverageSingleValue { get; set; }
    public List<BundleCombination> PopularBundleCombinations { get; set; } = new();
}

#endregion

#region Conversion Models

public class ConversionRatesResponse
{
    public int TotalVisits { get; set; }
    public int CartsCreated { get; set; }
    public int OrdersCreated { get; set; }
    public int CompletedOrders { get; set; }
    public double VisitToCartRate { get; set; }
    public double CartToOrderRate { get; set; }
    public double OrderCompletionRate { get; set; }
    public double OverallConversionRate { get; set; }
}

public class CartAbandonmentTrend
{
    public string Period { get; set; }
    public DateTime Date { get; set; }
    public double AbandonmentRate { get; set; }
    public int TotalCarts { get; set; }
    public int AbandonedCarts { get; set; }
}

#endregion

#region Common Models

public class RevenueByCategory
{
    public string Category { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public decimal Percentage { get; set; }
}

public class OrderStatusStat
{
    public string Status { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class CustomerSegment
{
    public string Segment { get; set; }
    public int Count { get; set; }
}

public class BundleCombination
{
    public string TerrariumName { get; set; }
    public List<string> AccessoryNames { get; set; } = new();
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
}
public class PeriodComparison
{
    public string Period1Label { get; set; }
    public string Period2Label { get; set; }
    public RevenueComparison Revenue { get; set; }
    public OrderComparison Orders { get; set; }
    public CustomerComparison Customers { get; set; }
}

public class RevenueComparison
{
    public decimal Period1Revenue { get; set; }
    public decimal Period2Revenue { get; set; }
    public decimal Difference { get; set; }
    public double GrowthPercent { get; set; }
    public string Trend { get; set; } // "Increasing", "Decreasing", "Stable"
}

public class OrderComparison
{
    public int Period1Orders { get; set; }
    public int Period2Orders { get; set; }
    public int Difference { get; set; }
    public double GrowthPercent { get; set; }
    public string Trend { get; set; }
}

public class CustomerComparison
{
    public int Period1Customers { get; set; }
    public int Period2Customers { get; set; }
    public int Difference { get; set; }
    public double GrowthPercent { get; set; }
    public string Trend { get; set; }
}

#endregion
public class OrdersByStatusResponse
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public int TotalOrders { get; set; }
    public List<OrderStatusDetail> StatusBreakdown { get; set; } = new();
    public List<StatusTrend> StatusTrends { get; set; } = new();
    public OrderStatusSummary Summary { get; set; }
}

public class OrderStatusDetail
{
    public string Status { get; set; }
    public int Count { get; set; }
    public double Percentage { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int OrderCount { get; set; }
    public List<RecentOrderSummary> RecentOrders { get; set; } = new();
}

public class StatusTrend
{
    public string Status { get; set; }
    public int CurrentPeriodCount { get; set; }
    public int PreviousPeriodCount { get; set; }
    public double GrowthPercent { get; set; }
    public string Trend { get; set; }
}
public class OrderStatusSummary
{
    public string MostCommonStatus { get; set; }
    public double CompletionRate { get; set; }
    public double CancellationRate { get; set; }
    public decimal PendingOrdersValue { get; set; }
}
public class RecentOrderSummary
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
}
public class DashboardSummary
{
    public RevenueOverviewResponse Revenue { get; set; }
    public OrderStatsResponse Orders { get; set; }
    public CustomerStatsResponse Customers { get; set; }
    public TopSellingProductsResponse TopProducts { get; set; }
    public ConversionRatesResponse Conversion { get; set; }
    public BundleAnalyticsResponse Bundles { get; set; }
    public DateTime LastUpdated { get; set; }
    public string Period { get; set; }
}

/// <summary>
/// Base response cho tất cả analytics endpoints
/// </summary>
public class AnalyticsBaseResponse
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string DataSource { get; set; } = "TerrariumGardenTech";
}
/// <summary>
/// Dashboard tổng hợp response
/// </summary>
public class DashboardResponse : AnalyticsBaseResponse
{
    /// <summary>
    /// Thông tin kỳ báo cáo
    /// </summary>
    public PeriodInfo Period { get; set; } = new();

    /// <summary>
    /// Metrics doanh thu
    /// </summary>
    public DashboardRevenueMetrics Revenue { get; set; } = new();

    /// <summary>
    /// Metrics đơn hàng
    /// </summary>
    public DashboardOrderMetrics Orders { get; set; } = new();

    /// <summary>
    /// Metrics khách hàng
    /// </summary>
    public DashboardCustomerMetrics Customers { get; set; } = new();

    /// <summary>
    /// Top sản phẩm
    /// </summary>
    public DashboardProductMetrics Products { get; set; } = new();

    /// <summary>
    /// Conversion metrics
    /// </summary>
    public DashboardConversionMetrics Conversion { get; set; } = new();

    /// <summary>
    /// Key insights và alerts
    /// </summary>
    public List<BusinessInsight> Insights { get; set; } = new();
}/// <summary>
/// Period comparison response
/// </summary>
public class PeriodComparisonResponse
{
    public ComparisonPeriod Period1 { get; set; } = new();
    public ComparisonPeriod Period2 { get; set; } = new();
    public ComparisonSummary Comparison { get; set; } = new();
    public List<string> KeyFindings { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Enhanced revenue overview với more insights
/// </summary>
public class EnhancedRevenueOverviewResponse : RevenueOverviewResponse
{
    /// <summary>
    /// Revenue breakdown by product categories
    /// </summary>
    public List<CategoryRevenue> RevenueByCategory { get; set; } = new();

    /// <summary>
    /// Revenue trends and forecasting
    /// </summary>
    public RevenueForecast Forecast { get; set; } = new();

    /// <summary>
    /// Performance vs targets
    /// </summary>
    public RevenueTargetAnalysis TargetAnalysis { get; set; } = new();
}

/// <summary>
/// Enhanced customer analytics response
/// </summary>
public class EnhancedCustomerAnalyticsResponse : CustomerStatsResponse
{
    /// <summary>
    /// Customer lifetime value analysis
    /// </summary>
    public CustomerLifetimeValueAnalysis LifetimeValue { get; set; } = new();

    /// <summary>
    /// Customer journey analysis
    /// </summary>
    public CustomerJourneyAnalysis Journey { get; set; } = new();

    /// <summary>
    /// Churn prediction và risk analysis
    /// </summary>
    public ChurnRiskAnalysis ChurnRisk { get; set; } = new();
}
#region Dashboard Component Models

public class PeriodInfo
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public int TotalDays { get; set; }
    public string PeriodType { get; set; } // "Last 30 days", "This month", etc.
}

public class DashboardRevenueMetrics
{
    public decimal TotalRevenue { get; set; }
    public decimal PreviousPeriodRevenue { get; set; }
    public double GrowthPercent { get; set; }
    public string GrowthTrend { get; set; } // "Up", "Down", "Stable"
    public decimal TargetRevenue { get; set; }
    public double TargetAchievement { get; set; }
    public List<RevenueByDay> DailyTrend { get; set; } = new();
    public RevenueBreakdown Breakdown { get; set; } = new();
}

public class DashboardOrderMetrics
{
    public int TotalOrders { get; set; }
    public int PreviousPeriodOrders { get; set; }
    public double GrowthPercent { get; set; }
    public decimal AverageOrderValue { get; set; }
    public double CompletionRate { get; set; }
    public OrderStatusDistribution StatusDistribution { get; set; } = new();
    public List<OrderTrendPoint> DailyTrend { get; set; } = new();
}

public class DashboardCustomerMetrics
{
    public int TotalCustomers { get; set; }
    public int NewCustomers { get; set; }
    public int ReturningCustomers { get; set; }
    public double RetentionRate { get; set; }
    public double ChurnRate { get; set; }
    public decimal AverageCustomerValue { get; set; }
    public CustomerSegmentDistribution Segments { get; set; } = new();
}

public class DashboardProductMetrics
{
    public List<TopProduct> TopTerrariums { get; set; } = new();
    public List<TopProduct> TopAccessories { get; set; } = new();
    public int TotalActiveProducts { get; set; }
    public decimal BundleRevenue { get; set; }
    public double BundlePenetration { get; set; }
    public List<string> TrendingProducts { get; set; } = new();
}

public class DashboardConversionMetrics
{
    public double OverallConversionRate { get; set; }
    public double CartToOrderRate { get; set; }
    public double AbandonmentRate { get; set; }
    public int ConvertedCustomers { get; set; }
    public decimal ConversionRevenue { get; set; }
    public List<ConversionFunnelStep> Funnel { get; set; } = new();
}

public class BusinessInsight
{
    public string Type { get; set; } // "Success", "Warning", "Info", "Alert"
    public string Title { get; set; }
    public string Description { get; set; }
    public string Metric { get; set; }
    public string Value { get; set; }
    public string Recommendation { get; set; }
    public int Priority { get; set; } // 1-5
}

#endregion

#region Comparison Models

public class ComparisonPeriod
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public string Label { get; set; }
    public PeriodMetrics Metrics { get; set; } = new();
}

public class PeriodMetrics
{
    public decimal Revenue { get; set; }
    public int Orders { get; set; }
    public int Customers { get; set; }
    public decimal AverageOrderValue { get; set; }
    public double ConversionRate { get; set; }
}

public class ComparisonSummary
{
    public MetricComparison Revenue { get; set; } = new();
    public MetricComparison Orders { get; set; } = new();
    public MetricComparison Customers { get; set; } = new();
    public MetricComparison ConversionRate { get; set; } = new();
    public string OverallTrend { get; set; }
    public double OverallGrowth { get; set; }
}

public class MetricComparison
{
    public decimal Period1Value { get; set; }
    public decimal Period2Value { get; set; }
    public decimal Difference { get; set; }
    public double GrowthPercent { get; set; }
    public string Trend { get; set; }
    public string Significance { get; set; } // "High", "Medium", "Low", "Not Significant"
}

#endregion

#region Advanced Analytics Models

public class CategoryRevenue
{
    public string CategoryName { get; set; }
    public decimal Revenue { get; set; }
    public double Percentage { get; set; }
    public int OrderCount { get; set; }
    public string Trend { get; set; }
}

public class RevenueForecast
{
    public decimal PredictedRevenue { get; set; }
    public string ForecastPeriod { get; set; }
    public double Confidence { get; set; }
    public List<ForecastPoint> ForecastData { get; set; } = new();
}

public class RevenueTargetAnalysis
{
    public decimal MonthlyTarget { get; set; }
    public decimal ActualRevenue { get; set; }
    public double AchievementRate { get; set; }
    public decimal ProjectedMonthEnd { get; set; }
    public string Status { get; set; } // "On Track", "Behind", "Ahead"
}

public class CustomerLifetimeValueAnalysis
{
    public decimal AverageLTV { get; set; }
    public int AverageLifespanDays { get; set; }
    public decimal AverageOrderFrequency { get; set; }
    public List<LTVSegment> Segments { get; set; } = new();
}

public class CustomerJourneyAnalysis
{
    public double AverageTimeToFirstPurchase { get; set; }
    public double AverageTimeBetweenPurchases { get; set; }
    public List<JourneyStage> Stages { get; set; } = new();
}

public class ChurnRiskAnalysis
{
    public int HighRiskCustomers { get; set; }
    public int MediumRiskCustomers { get; set; }
    public int LowRiskCustomers { get; set; }
    public List<ChurnRiskFactor> RiskFactors { get; set; } = new();
}

#endregion

#region Supporting Models

public class RevenueByDay
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
}

public class RevenueBreakdown
{
    public decimal TerrariumRevenue { get; set; }
    public decimal AccessoryRevenue { get; set; }
    public decimal BundleRevenue { get; set; }
    public decimal ShippingRevenue { get; set; }
}

public class OrderStatusDistribution
{
    public int Completed { get; set; }
    public int Pending { get; set; }
    public int Processing { get; set; }
    public int Cancelled { get; set; }
}

public class OrderTrendPoint
{
    public DateTime Date { get; set; }
    public int Orders { get; set; }
    public decimal Revenue { get; set; }
}

public class CustomerSegmentDistribution
{
    public int VIPCustomers { get; set; }
    public int LoyalCustomers { get; set; }
    public int RegularCustomers { get; set; }
    public int NewCustomers { get; set; }
}

public class ConversionFunnelStep
{
    public string StepName { get; set; }
    public int Count { get; set; }
    public double ConversionRate { get; set; }
    public double DropOffRate { get; set; }
}

public class ForecastPoint
{
    public DateTime Date { get; set; }
    public decimal PredictedRevenue { get; set; }
    public decimal LowerBound { get; set; }
    public decimal UpperBound { get; set; }
}

public class LTVSegment
{
    public string SegmentName { get; set; }
    public int CustomerCount { get; set; }
    public decimal AverageLTV { get; set; }
    public string Characteristics { get; set; }
}

public class JourneyStage
{
    public string StageName { get; set; }
    public int CustomerCount { get; set; }
    public double AverageDuration { get; set; }
    public double ConversionRate { get; set; }
}

public class ChurnRiskFactor
{
    public string Factor { get; set; }
    public double Impact { get; set; }
    public string Description { get; set; }
}

#endregion

public class AdminMembershipStatisticsDto
{
    // Thống kê tổng quan
    public int TotalMemberships { get; set; }
    public int ActiveMemberships { get; set; }
    public int ExpiredMemberships { get; set; }
    public int CancelledMemberships { get; set; }

    // Thống kê doanh thu
    public decimal TotalRevenue { get; set; }
    public decimal CurrentMonthRevenue { get; set; }
    public decimal LastMonthRevenue { get; set; }
    public double RevenueGrowthPercent { get; set; }

    // Thống kê theo gói
    public List<PackageSummaryDto> PackageSummary { get; set; }

    // Thống kê theo thời gian
    public List<MonthlyStatDto> Last12MonthsStats { get; set; }

    // Top users
    public List<TopUserDto> TopUsers { get; set; }
}

public class PackageSummaryDto
{
    public int PackageId { get; set; }
    public string PackageType { get; set; }
    public int DurationDays { get; set; }
    public decimal Price { get; set; }
    public int TotalSold { get; set; }
    public decimal Revenue { get; set; }
    public double MarketSharePercent { get; set; }
}

public class MonthlyStatDto
{
    public string Month { get; set; } // "2024-01"
    public int NewMemberships { get; set; }
    public decimal Revenue { get; set; }
}

public class TopUserDto
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public int TotalPurchases { get; set; }
    public decimal TotalSpent { get; set; }
    public string CurrentPackage { get; set; }
}