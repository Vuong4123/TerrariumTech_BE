namespace TerrariumGardenTech.Common.RequestModel.Reneuve;

/// <summary>
/// Base request model cho các analytics queries
/// </summary>
public class AnalyticsBaseRequest
{
    /// <summary>
    /// Ngày bắt đầu (optional, mặc định -30 ngày)
    /// </summary>
    public DateTime? From { get; set; }

    /// <summary>
    /// Ngày kết thúc (optional, mặc định hôm nay)
    /// </summary>
    public DateTime? To { get; set; }
}

/// <summary>
/// Request cho revenue by period
/// </summary>
public class RevenueByPeriodRequest : AnalyticsBaseRequest
{
    /// <summary>
    /// Kỳ báo cáo: daily, weekly, monthly, yearly
    /// </summary>
    public string Period { get; set; } = "monthly";
}

/// <summary>
/// Request cho top selling products
/// </summary>
public class TopProductsRequest : AnalyticsBaseRequest
{
    /// <summary>
    /// Số lượng top products muốn lấy (mặc định 10)
    /// </summary>
    public int Top { get; set; } = 10;

    /// <summary>
    /// Loại sản phẩm: all, terrarium, accessory
    /// </summary>
    public string ProductType { get; set; } = "all";
}

/// <summary>
/// Request cho order trends
/// </summary>
public class OrderTrendsRequest : AnalyticsBaseRequest
{
    /// <summary>
    /// Kỳ báo cáo: daily, weekly, monthly
    /// </summary>
    public string Period { get; set; } = "daily";

    /// <summary>
    /// Filter theo status cụ thể (optional)
    /// </summary>
    public string? StatusFilter { get; set; }
}

/// <summary>
/// Request so sánh 2 kỳ
/// </summary>
public class PeriodComparisonRequest
{
    /// <summary>
    /// Kỳ 1 - Ngày bắt đầu
    /// </summary>
    public DateTime Period1From { get; set; }

    /// <summary>
    /// Kỳ 1 - Ngày kết thúc
    /// </summary>
    public DateTime Period1To { get; set; }

    /// <summary>
    /// Kỳ 2 - Ngày bắt đầu
    /// </summary>
    public DateTime Period2From { get; set; }

    /// <summary>
    /// Kỳ 2 - Ngày kết thúc
    /// </summary>
    public DateTime Period2To { get; set; }

    /// <summary>
    /// Metrics muốn so sánh: revenue, orders, customers, all
    /// </summary>
    public string ComparisonType { get; set; } = "all";
}

/// <summary>
/// Request cho dashboard
/// </summary>
public class DashboardRequest : AnalyticsBaseRequest
{
    /// <summary>
    /// Các metrics muốn include trong dashboard
    /// </summary>
    public List<string> IncludeMetrics { get; set; } = new()
    {
        "revenue", "orders", "customers", "products", "conversion"
    };

    /// <summary>
    /// Top N cho các rankings (mặc định 5)
    /// </summary>
    public int TopCount { get; set; } = 5;

    /// <summary>
    /// Có include trends không
    /// </summary>
    public bool IncludeTrends { get; set; } = true;
}

/// <summary>
/// Request cho customer retention analysis
/// </summary>
public class CustomerRetentionRequest : AnalyticsBaseRequest
{
    /// <summary>
    /// Khoảng thời gian định nghĩa churn (ngày)
    /// </summary>
    public int ChurnPeriodDays { get; set; } = 90;

    /// <summary>
    /// Có phân tích cohort không
    /// </summary>
    public bool IncludeCohortAnalysis { get; set; } = false;
}

/// <summary>
/// Request cho product performance
/// </summary>
public class ProductPerformanceRequest : AnalyticsBaseRequest
{
    /// <summary>
    /// Threshold cho low-performing products (% market share)
    /// </summary>
    public double LowPerformanceThreshold { get; set; } = 1.0;

    /// <summary>
    /// Có include category comparison không
    /// </summary>
    public bool IncludeCategoryComparison { get; set; } = true;

    /// <summary>
    /// Có include recommendations không
    /// </summary>
    public bool IncludeRecommendations { get; set; } = true;
}

/// <summary>
/// Request cho cart abandonment analysis
/// </summary>
public class CartAbandonmentRequest : AnalyticsBaseRequest
{
    /// <summary>
    /// Khoảng thời gian coi cart là abandoned (giờ)
    /// </summary>
    public int AbandonmentThresholdHours { get; set; } = 24;

    /// <summary>
    /// Có include abandonment reasons analysis không
    /// </summary>
    public bool IncludeReasonAnalysis { get; set; } = true;

    /// <summary>
    /// Có include recovery recommendations không
    /// </summary>
    public bool IncludeRecoveryRecommendations { get; set; } = true;
}