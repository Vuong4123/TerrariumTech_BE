using Microsoft.Extensions.Logging;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.ResponseModel.Revenue;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class RevenueService : IRevenueService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly ILogger<RevenueService> _logger;

    public RevenueService(UnitOfWork unitOfWork, ILogger<RevenueService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    #region Revenue Analytics

    /// <summary>
    /// Tổng quan doanh thu - Dashboard chính
    /// </summary>
    public async Task<IBusinessResult> GetRevenueOverviewAsync(DateTime? from = null, DateTime? to = null)
    {
        var fromDate = from ?? DateTime.Now.AddMonths(-12);
        var toDate = to ?? DateTime.Now;

        var orders = await _unitOfWork.Order.GetAllAsync();
        var completedOrders = orders.Where(o =>
            o.Status == OrderStatusEnum.Confirmed &&
            o.OrderDate >= fromDate &&
            o.OrderDate <= toDate).ToList();

        var previousPeriodFrom = fromDate.AddDays(-(toDate - fromDate).Days);
        var previousPeriodOrders = orders.Where(o =>
            o.Status == OrderStatusEnum.Confirmed &&
            o.OrderDate >= previousPeriodFrom &&
            o.OrderDate < fromDate).ToList();

        var currentRevenue = completedOrders.Sum(o => o.TotalAmount);
        var previousRevenue = previousPeriodOrders.Sum(o => o.TotalAmount);
        var revenueGrowth = previousRevenue > 0 ? ((currentRevenue - previousRevenue) / previousRevenue) * 100 : 0;

        var result = new RevenueOverviewResponse
        {
            TotalRevenue = currentRevenue,
            PreviousPeriodRevenue = previousRevenue,
            RevenueGrowthPercent = Math.Round(revenueGrowth, 2),
            TotalOrders = completedOrders.Count,
            AverageOrderValue = completedOrders.Count > 0 ? currentRevenue / completedOrders.Count : 0,
            TotalCustomers = completedOrders.Select(o => o.UserId).Distinct().Count(),
            RevenueByMonth = GetRevenueByMonth(completedOrders, fromDate, toDate),
            RevenueByPaymentStatus = completedOrders
                .GroupBy(o => o.PaymentStatus ?? "Unknown")
                .Select(g => new RevenueByCategory
                {
                    Category = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count(),
                    Percentage = Math.Round((g.Sum(o => o.TotalAmount) / currentRevenue) * 100, 2)
                }).ToList()
        };

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy tổng quan doanh thu thành công", result);
    }

    /// <summary>
    /// Doanh thu theo kỳ (ngày, tuần, tháng, năm)
    /// </summary>
    public async Task<IBusinessResult> GetRevenueByPeriodAsync(string period, DateTime? from = null, DateTime? to = null)
    {
        var fromDate = from ?? DateTime.Now.AddMonths(-3);
        var toDate = to ?? DateTime.Now;

        var orders = await _unitOfWork.Order.GetAllAsync();
        var completedOrders = orders.Where(o =>
            o.Status == OrderStatusEnum.Confirmed &&
            o.OrderDate >= fromDate &&
            o.OrderDate <= toDate).ToList();

        List<RevenueByPeriod> revenueData = new List<RevenueByPeriod>();

        switch (period.ToLower())
        {
            case "daily":
                revenueData = GetDailyRevenue(completedOrders, fromDate, toDate);
                break;

            case "weekly":
                revenueData = GetWeeklyRevenue(completedOrders, fromDate, toDate);
                break;

            case "monthly":
                revenueData = GetMonthlyRevenue(completedOrders, fromDate, toDate);
                break;

            case "yearly":
                revenueData = GetYearlyRevenue(completedOrders, fromDate, toDate);
                break;

            default:
                return new BusinessResult(Const.BAD_REQUEST_CODE, "Period không hợp lệ. Sử dụng: daily, weekly, monthly, yearly");
        }

        var result = new RevenueByPeriodResponse
        {
            Period = period,
            FromDate = fromDate,
            ToDate = toDate,
            TotalRevenue = revenueData.Sum(r => r.Revenue),
            Data = revenueData
        };

        return new BusinessResult(Const.SUCCESS_READ_CODE, $"Lấy doanh thu theo {period} thành công", result);
    }

    /// <summary>
    /// Doanh thu theo sản phẩm (Bể vs Phụ kiện)
    /// </summary>
    public async Task<IBusinessResult> GetRevenueByProductAsync(DateTime? from = null, DateTime? to = null)
    {
        var fromDate = from ?? DateTime.Now;
        var toDate = to ?? DateTime.Now;

        var orders = await _unitOfWork.Order.GetAllAsync();
        var completedOrders = orders.Where(o =>
            o.Status == OrderStatusEnum.Confirmed &&
            o.OrderDate >= fromDate &&
            o.OrderDate <= toDate).ToList();

        var orderItems = completedOrders.SelectMany(o => o.OrderItems).ToList();

        var terrariumRevenue = orderItems
            .Where(item => item.TerrariumVariantId.HasValue)
            .Sum(item => item.TotalPrice ?? 0);

        var accessoryRevenue = orderItems
            .Where(item => item.AccessoryId.HasValue)
            .Sum(item => item.TotalPrice ?? 0);

        var totalRevenue = terrariumRevenue + accessoryRevenue;

        var result = new ProductRevenueResponse
        {
            TotalRevenue = totalRevenue,
            TerrariumRevenue = terrariumRevenue,
            TerrariumPercentage = totalRevenue > 0 ? Math.Round((terrariumRevenue / totalRevenue) * 100, 2) : 0,
            AccessoryRevenue = accessoryRevenue,
            AccessoryPercentage = totalRevenue > 0 ? Math.Round((accessoryRevenue / totalRevenue) * 100, 2) : 0,

            TopTerrariums = await GetTopTerrariums(orderItems, 5),
            TopAccessories = await GetTopAccessories(orderItems, 5)
        };

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy doanh thu theo sản phẩm thành công", result);
    }

    #endregion Revenue Analytics

    #region Order Analytics

    /// <summary>
    /// Thống kê tổng quan đơn hàng
    /// </summary>
    public async Task<IBusinessResult> GetOrderStatsAsync(DateTime? from = null, DateTime? to = null)
    {
        var fromDate = from ?? DateTime.Now.AddMonths(-1);
        var toDate = to ?? DateTime.Now;

        var allOrders = await _unitOfWork.Order.GetAllAsync();
        var orders = allOrders.Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate).ToList();

        var result = new OrderStatsResponse
        {
            TotalOrders = orders.Count,
            CompletedOrders = orders.Count(o => o.Status == OrderStatusEnum.Confirmed),
            PendingOrders = orders.Count(o => o.Status == OrderStatusEnum.Pending),
            CancelledOrders = orders.Count(o => o.Status == OrderStatusEnum.Cancle),
            ProcessingOrders = orders.Count(o => o.Status == OrderStatusEnum.Processing),

            CompletionRate = orders.Count > 0 ? Math.Round(((double)orders.Count(o => o.Status == OrderStatusEnum.Confirmed) / orders.Count) * 100, 2) : 0,
            CancellationRate = orders.Count > 0 ? Math.Round(((double)orders.Count(o => o.Status == OrderStatusEnum.Cancle) / orders.Count) * 100, 2) : 0,

            AverageOrderValue = orders.Count > 0 ? orders.Average(o => o.TotalAmount) : 0,
            AverageItemsPerOrder = orders.Count > 0 ? orders.SelectMany(o => o.OrderItems).Count() / (double)orders.Count : 0,

            OrdersByStatus = Enum.GetValues<OrderStatusEnum>()
                .Select(status => new OrderStatusStat
                {
                    Status = status.ToString(),
                    Count = orders.Count(o => o.Status == status),
                    Percentage = orders.Count > 0 ? Math.Round(((double)orders.Count(o => o.Status == status) / orders.Count) * 100, 2) : 0
                }).ToList()
        };

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy thống kê đơn hàng thành công", result);
    }

    /// <summary>
    /// Xu hướng đơn hàng theo thời gian
    /// </summary>
    public async Task<IBusinessResult> GetOrderTrendsAsync(string period, DateTime? from = null, DateTime? to = null)
    {
        var fromDate = from ?? DateTime.Now.AddMonths(-3);
        var toDate = to ?? DateTime.Now;

        var orders = await _unitOfWork.Order.GetAllAsync();
        var filteredOrders = orders.Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate).ToList();

        List<OrderTrendData> trendData = new List<OrderTrendData>();

        switch (period.ToLower())
        {
            case "daily":
                trendData = GetDailyOrderTrends(filteredOrders, fromDate, toDate);
                break;

            case "weekly":
                trendData = GetWeeklyOrderTrends(filteredOrders, fromDate, toDate);
                break;

            case "monthly":
                trendData = GetMonthlyOrderTrends(filteredOrders, fromDate, toDate);
                break;

            default:
                return new BusinessResult(Const.BAD_REQUEST_CODE, "Period không hợp lệ");
        }

        var result = new OrderTrendsResponse
        {
            Period = period,
            FromDate = fromDate,
            ToDate = toDate,
            TotalOrders = filteredOrders.Count,
            Data = trendData
        };

        return new BusinessResult(Const.SUCCESS_READ_CODE, $"Lấy xu hướng đơn hàng theo {period} thành công", result);
    }

    #endregion Order Analytics

    #region Customer Analytics

    /// <summary>
    /// Thống kê khách hàng
    /// </summary>
    public async Task<IBusinessResult> GetCustomerStatsAsync(DateTime? from = null, DateTime? to = null)
    {
        var fromDate = from ?? DateTime.Now.AddMonths(-12);
        var toDate = to ?? DateTime.Now;

        var orders = await _unitOfWork.Order.GetAllAsync();
        var filteredOrders = orders.Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate).ToList();

        var customerOrders = filteredOrders
            .GroupBy(o => o.UserId)
            .Select(g => new
            {
                UserId = g.Key,
                OrderCount = g.Count(),
                TotalSpent = g.Sum(o => o.TotalAmount),
                FirstOrderDate = g.Min(o => o.OrderDate),
                LastOrderDate = g.Max(o => o.OrderDate)
            }).ToList();

        var result = new CustomerStatsResponse
        {
            TotalCustomers = customerOrders.Count,
            NewCustomers = customerOrders.Count(c => c.FirstOrderDate >= fromDate),
            ReturningCustomers = customerOrders.Count(c => c.OrderCount > 1),
            OneTimeCustomers = customerOrders.Count(c => c.OrderCount == 1),

            AverageOrdersPerCustomer = customerOrders.Count > 0 ? Math.Round(customerOrders.Average(c => c.OrderCount), 2) : 0,
            AverageSpentPerCustomer = customerOrders.Count > 0 ? Math.Round(customerOrders.Average(c => c.TotalSpent), 2) : 0,

            CustomerSegmentation = new List<CustomerSegment>
                {
                    new CustomerSegment { Segment = "VIP (>5 đơn)", Count = customerOrders.Count(c => c.OrderCount > 5) },
                    new CustomerSegment { Segment = "Loyal (3-5 đơn)", Count = customerOrders.Count(c => c.OrderCount >= 3 && c.OrderCount <= 5) },
                    new CustomerSegment { Segment = "Regular (2 đơn)", Count = customerOrders.Count(c => c.OrderCount == 2) },
                    new CustomerSegment { Segment = "New (1 đơn)", Count = customerOrders.Count(c => c.OrderCount == 1) }
                }
        };

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy thống kê khách hàng thành công", result);
    }

    /// <summary>
    /// Top khách hàng VIP
    /// </summary>
    public async Task<IBusinessResult> GetTopCustomersAsync(int top = 10, DateTime? from = null, DateTime? to = null)
    {
        var fromDate = from ?? DateTime.Now.AddMonths(-12);
        var toDate = to ?? DateTime.Now;

        var orders = await _unitOfWork.Order.GetAllAsync();
        var users = await _unitOfWork.User.GetAllAsync();

        var customerData = orders
            .Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate && o.Status == OrderStatusEnum.Confirmed)
            .GroupBy(o => o.UserId)
            .Select(g => new TopCustomer
            {
                UserId = g.Key,
                UserEmail = users.FirstOrDefault(u => u.UserId == g.Key)?.Email ?? "Unknown",
                TotalOrders = g.Count(),
                TotalSpent = g.Sum(o => o.TotalAmount),
                AverageOrderValue = g.Average(o => o.TotalAmount),
                LastOrderDate = g.Max(o => o.OrderDate) ?? DateTime.MinValue
            })
            .OrderByDescending(c => c.TotalSpent)
            .Take(top)
            .ToList();

        return new BusinessResult(Const.SUCCESS_READ_CODE, $"Lấy top {top} khách hàng VIP thành công", customerData);
    }

    #endregion Customer Analytics

    #region Product Analytics

    /// <summary>
    /// Top sản phẩm bán chạy
    /// </summary>
    public async Task<IBusinessResult> GetTopSellingProductsAsync(int top = 10, DateTime? from = null, DateTime? to = null)
    {
        var fromDate = from ?? DateTime.Now;
        var toDate = to ?? DateTime.Now;

        var orders = await _unitOfWork.Order.GetAllAsync();
        var completedOrders = orders.Where(o =>
            o.Status == OrderStatusEnum.Confirmed &&
            o.OrderDate >= fromDate &&
            o.OrderDate <= toDate).ToList();

        var orderItems = completedOrders.SelectMany(o => o.OrderItems).ToList();

        // Top bể thủy sinh
        var topTerrariums = await GetTopTerrariums(orderItems, top);

        // Top phụ kiện
        var topAccessories = await GetTopAccessories(orderItems, top);

        var result = new TopSellingProductsResponse
        {
            TopTerrariums = topTerrariums,
            TopAccessories = topAccessories,
            Period = $"{fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}"
        };

        return new BusinessResult(Const.SUCCESS_READ_CODE, $"Lấy top {top} sản phẩm bán chạy thành công", result);
    }

    /// <summary>
    /// Phân tích Bundle (Combo bể + phụ kiện)
    /// </summary>
    public async Task<IBusinessResult> GetBundleAnalyticsAsync(DateTime? from = null, DateTime? to = null)
    {
        var fromDate = from ?? DateTime.Now.AddMonths(-3);
        var toDate = to ?? DateTime.Now;

        var orders = await _unitOfWork.Order.GetAllAsync();
        var completedOrders = orders.Where(o =>
            o.Status == OrderStatusEnum.Confirmed &&
            o.OrderDate >= fromDate &&
            o.OrderDate <= toDate).ToList();

        var bundleOrders = completedOrders
            .Where(o => o.OrderItems.Any(item => item.TerrariumVariantId.HasValue) &&
                       o.OrderItems.Any(item => item.AccessoryId.HasValue))
            .ToList();

        var singleProductOrders = completedOrders.Except(bundleOrders).ToList();

        var bundleRevenue = bundleOrders.Sum(o => o.TotalAmount);
        var singleRevenue = singleProductOrders.Sum(o => o.TotalAmount);
        var totalRevenue = bundleRevenue + singleRevenue;

        var result = new BundleAnalyticsResponse
        {
            TotalOrders = completedOrders.Count,
            BundleOrders = bundleOrders.Count,
            SingleProductOrders = singleProductOrders.Count,

            BundleOrdersPercentage = completedOrders.Count > 0 ? Math.Round(((double)bundleOrders.Count / completedOrders.Count) * 100, 2) : 0,

            BundleRevenue = bundleRevenue,
            SingleProductRevenue = singleRevenue,
            BundleRevenuePercentage = totalRevenue > 0 ? Math.Round((bundleRevenue / totalRevenue) * 100, 2) : 0,

            AverageBundleValue = bundleOrders.Count > 0 ? bundleRevenue / bundleOrders.Count : 0,
            AverageSingleValue = singleProductOrders.Count > 0 ? singleRevenue / singleProductOrders.Count : 0,

            PopularBundleCombinations = GetPopularBundleCombinations(bundleOrders)
        };

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy phân tích Bundle thành công", result);
    }

    #endregion Product Analytics

    #region Conversion Analytics

    /// <summary>
    /// Tỷ lệ chuyển đổi và funnel analysis
    /// </summary>
    public async Task<IBusinessResult> GetConversionRatesAsync(DateTime? from = null, DateTime? to = null)
    {
        var fromDate = from ?? DateTime.Now.AddMonths(-1);
        var toDate = to ?? DateTime.Now;

        // Giả sử bạn có bảng tracking visits/sessions
        // Ở đây tôi sẽ dùng dữ liệu có sẵn để ước tính

        var orders = await _unitOfWork.Order.GetAllAsync();
        var carts = await _unitOfWork.Cart.GetAllAsync();

        var periodOrders = orders.Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate).ToList();
        var periodCarts = carts.Where(c => c.CreatedAt >= fromDate && c.CreatedAt <= toDate).ToList();

        var totalVisits = periodCarts.Count * 3; // Ước tính mỗi cart có 3 visits
        var cartsCreated = periodCarts.Count;
        var ordersCreated = periodOrders.Count;
        var completedOrders = periodOrders.Count(o => o.Status == OrderStatusEnum.Confirmed);

        var result = new ConversionRatesResponse
        {
            TotalVisits = totalVisits,
            CartsCreated = cartsCreated,
            OrdersCreated = ordersCreated,
            CompletedOrders = completedOrders,

            VisitToCartRate = totalVisits > 0 ? Math.Round(((double)cartsCreated / totalVisits) * 100, 2) : 0,
            CartToOrderRate = cartsCreated > 0 ? Math.Round(((double)ordersCreated / cartsCreated) * 100, 2) : 0,
            OrderCompletionRate = ordersCreated > 0 ? Math.Round(((double)completedOrders / ordersCreated) * 100, 2) : 0,
            OverallConversionRate = totalVisits > 0 ? Math.Round(((double)completedOrders / totalVisits) * 100, 2) : 0
        };

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy tỷ lệ chuyển đổi thành công", result);
    }

    #endregion Conversion Analytics

    #region Helper Methods
    private List<RevenueByPeriod> GetMonthlyRevenue(List<Order> orders, DateTime fromDate, DateTime toDate)
    {
        return orders
            .GroupBy(o => new { Year = o.OrderDate?.Year, Month = o.OrderDate?.Month })
            .Select(g => new RevenueByPeriod
            {
                Period = $"{g.Key.Year}-{g.Key.Month:00}",
                Revenue = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count(),
                Date = new DateTime(g.Key.Year ?? DateTime.Now.Year, g.Key.Month ?? DateTime.Now.Month, 1)
            })
            .OrderBy(r => r.Date)
            .ToList();
    }

    private async Task<List<TopProduct>> GetTopTerrariums(List<OrderItem> orderItems, int top)
    {
        var terrariumItems = orderItems.Where(item => item.TerrariumVariantId.HasValue).ToList();
        var terrariumVariants = await _unitOfWork.TerrariumVariant.GetAllAsync();

        return terrariumItems
            .GroupBy(item => item.TerrariumVariantId.Value)
            .Select(g => new TopProduct
            {
                ProductId = g.Key,
                ProductName = terrariumVariants.FirstOrDefault(tv => tv.TerrariumVariantId == g.Key)?.VariantName ?? "Unknown",
                TotalQuantitySold = g.Sum(item => item.TerrariumVariantQuantity ?? 0),
                TotalRevenue = g.Sum(item => item.TotalPrice ?? 0),
                OrderCount = g.Count()
            })
            .OrderByDescending(p => p.TotalQuantitySold)
            .Take(top)
            .ToList();
    }

    private async Task<List<TopProduct>> GetTopAccessories(List<OrderItem> orderItems, int top)
    {
        var accessoryItems = orderItems.Where(item => item.AccessoryId.HasValue).ToList();
        var accessories = await _unitOfWork.Accessory.GetAllAsync();

        return accessoryItems
            .GroupBy(item => item.AccessoryId.Value)
            .Select(g => new TopProduct
            {
                ProductId = g.Key,
                ProductName = accessories.FirstOrDefault(a => a.AccessoryId == g.Key)?.Name ?? "Unknown",
                TotalQuantitySold = g.Sum(item => item.AccessoryQuantity ?? 0),
                TotalRevenue = g.Sum(item => item.TotalPrice ?? 0),
                OrderCount = g.Count()
            })
            .OrderByDescending(p => p.TotalQuantitySold)
            .Take(top)
            .ToList();
    }


    /// <summary>
    /// Generate RevenueByMonth data từ orders
    /// </summary>
    private List<RevenueByMonth> GetRevenueByMonth(List<Order> orders, DateTime fromDate, DateTime toDate)
    {
        return orders
            .GroupBy(o => new { Year = o.OrderDate?.Year, Month = o.OrderDate?.Month })
            .Select(g => new RevenueByMonth
            {
                Period = $"{g.Key.Year}-{g.Key.Month:00}",
                Revenue = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count(),
                Date = new DateTime(g.Key.Year ?? DateTime.Now.Year, g.Key.Month ?? DateTime.Now.Month, 1),
                AverageOrderValue = g.Count() > 0 ? g.Sum(o => o.TotalAmount) / g.Count() : 0
            })
            .OrderBy(r => r.Date)
            .ToList();
    }

    /// <summary>
    /// Generate daily revenue data
    /// </summary>
    private List<RevenueByPeriod> GetDailyRevenue(List<Order> orders, DateTime fromDate, DateTime toDate)
    {
        var result = new List<RevenueByPeriod>();

        for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
        {
            var dayOrders = orders.Where(o => o.OrderDate?.Date == date).ToList();
            result.Add(new RevenueByPeriod
            {
                Period = date.ToString("yyyy-MM-dd"),
                Revenue = dayOrders.Sum(o => o.TotalAmount),
                OrderCount = dayOrders.Count,
                Date = date
            });
        }

        return result;
    }

    /// <summary>
    /// Generate weekly revenue data
    /// </summary>
    private List<RevenueByPeriod> GetWeeklyRevenue(List<Order> orders, DateTime fromDate, DateTime toDate)
    {
        var result = new List<RevenueByPeriod>();

        // Tìm thứ 2 đầu tiên
        var startOfWeek = fromDate.AddDays(-(int)fromDate.DayOfWeek + (int)DayOfWeek.Monday);

        for (var weekStart = startOfWeek; weekStart <= toDate; weekStart = weekStart.AddDays(7))
        {
            var weekEnd = weekStart.AddDays(6);
            var weekOrders = orders.Where(o =>
                o.OrderDate >= weekStart &&
                o.OrderDate <= weekEnd).ToList();

            result.Add(new RevenueByPeriod
            {
                Period = $"Week {weekStart:yyyy-MM-dd}",
                Revenue = weekOrders.Sum(o => o.TotalAmount),
                OrderCount = weekOrders.Count,
                Date = weekStart
            });
        }

        return result;
    }

    /// <summary>
    /// Generate yearly revenue data
    /// </summary>
    private List<RevenueByPeriod> GetYearlyRevenue(List<Order> orders, DateTime fromDate, DateTime toDate)
    {
        return orders
            .GroupBy(o => o.OrderDate?.Year)
            .Select(g => new RevenueByPeriod
            {
                Period = g.Key.ToString(),
                Revenue = g.Sum(o => o.TotalAmount),
                OrderCount = g.Count(),
                Date = new DateTime(g.Key ?? DateTime.Now.Year, 1, 1)
            })
            .OrderBy(r => r.Date)
            .ToList();
    }

    /// <summary>
    /// Generate daily order trends
    /// </summary>
    private List<OrderTrendData> GetDailyOrderTrends(List<Order> orders, DateTime fromDate, DateTime toDate)
    {
        var result = new List<OrderTrendData>();

        for (var date = fromDate.Date; date <= toDate.Date; date = date.AddDays(1))
        {
            var dayOrders = orders.Where(o => o.OrderDate?.Date == date).ToList();

            result.Add(new OrderTrendData
            {
                Period = date.ToString("yyyy-MM-dd"),
                Date = date,
                TotalOrders = dayOrders.Count,
                CompletedOrders = dayOrders.Count(o => o.Status == OrderStatusEnum.Confirmed),
                CancelledOrders = dayOrders.Count(o => o.Status == OrderStatusEnum.Cancle),
                Revenue = dayOrders.Where(o => o.Status == OrderStatusEnum.Confirmed).Sum(o => o.TotalAmount)
            });
        }

        return result;
    }

    /// <summary>
    /// Generate weekly order trends
    /// </summary>
    private List<OrderTrendData> GetWeeklyOrderTrends(List<Order> orders, DateTime fromDate, DateTime toDate)
    {
        var result = new List<OrderTrendData>();
        var startOfWeek = fromDate.AddDays(-(int)fromDate.DayOfWeek + (int)DayOfWeek.Monday);

        for (var weekStart = startOfWeek; weekStart <= toDate; weekStart = weekStart.AddDays(7))
        {
            var weekEnd = weekStart.AddDays(6);
            var weekOrders = orders.Where(o =>
                o.OrderDate >= weekStart &&
                o.OrderDate <= weekEnd).ToList();

            result.Add(new OrderTrendData
            {
                Period = $"Week {weekStart:yyyy-MM-dd}",
                Date = weekStart,
                TotalOrders = weekOrders.Count,
                CompletedOrders = weekOrders.Count(o => o.Status == OrderStatusEnum.Confirmed),
                CancelledOrders = weekOrders.Count(o => o.Status == OrderStatusEnum.Cancle),
                Revenue = weekOrders.Where(o => o.Status == OrderStatusEnum.Confirmed).Sum(o => o.TotalAmount)
            });
        }

        return result;
    }

    /// <summary>
    /// Generate monthly order trends
    /// </summary>
    private List<OrderTrendData> GetMonthlyOrderTrends(List<Order> orders, DateTime fromDate, DateTime toDate)
    {
        return orders
            .GroupBy(o => new { Year = o.OrderDate?.Year, Month = o.OrderDate?.Month })
            .Select(g => new OrderTrendData
            {
                Period = $"{g.Key.Year}-{g.Key.Month:00}",
                Date = new DateTime(g.Key.Year ?? DateTime.Now.Year, g.Key.Month ?? DateTime.Now.Month, 1),
                TotalOrders = g.Count(),
                CompletedOrders = g.Count(o => o.Status == OrderStatusEnum.Confirmed),
                CancelledOrders = g.Count(o => o.Status == OrderStatusEnum.Cancle),
                Revenue = g.Where(o => o.Status == OrderStatusEnum.Confirmed).Sum(o => o.TotalAmount)
            })
            .OrderBy(r => r.Date)
            .ToList();
    }

    /// <summary>
    /// Lấy item phổ biến nhất từ các đơn hàng bundle
    /// </summary>
    private List<BundleCombination> GetPopularBundleCombinations(List<Order> bundleOrders)
    {
        var combinations = new List<BundleCombination>();

        foreach (var order in bundleOrders.Take(10)) // Top 10 bundle combinations
        {
            var terrariumItems = order.OrderItems.Where(item => item.TerrariumVariantId.HasValue).ToList();
            var accessoryItems = order.OrderItems.Where(item => item.AccessoryId.HasValue).ToList();
        }

        return combinations
            .GroupBy(c => c.TerrariumName)
            .Select(g => new BundleCombination
            {
                TerrariumName = g.Key,
                AccessoryNames = g.SelectMany(c => c.AccessoryNames).Distinct().ToList(),
                OrderCount = g.Sum(c => c.OrderCount),
                TotalRevenue = g.Sum(c => c.TotalRevenue)
            })
            .OrderByDescending(c => c.OrderCount)
            .ToList();
    }

    /// <summary>
    /// Lấy thống kê đơn hàng theo trạng thái chi tiết
    /// </summary>
    public async Task<IBusinessResult> GetOrdersByStatusAsync(DateTime? from = null, DateTime? to = null)
    {
        try
        {
            var fromDate = from ?? DateTime.Now.AddMonths(-3);
            var toDate = to ?? DateTime.Now;

            var orders = await _unitOfWork.Order.GetAllAsync();
            var filteredOrders = orders.Where(o => o.OrderDate >= fromDate && o.OrderDate <= toDate).ToList();

            if (!filteredOrders.Any())
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "Không có đơn hàng nào trong khoảng thời gian này");
            }

            var totalOrders = filteredOrders.Count;
            var statusStats = new List<OrderStatusDetail>();

            // Thống kê theo từng status
            foreach (OrderStatusEnum status in Enum.GetValues<OrderStatusEnum>())
            {
                var statusOrders = filteredOrders.Where(o => o.Status == status).ToList();
                var statusCount = statusOrders.Count;

                if (statusCount > 0) // Chỉ thêm status có đơn hàng
                {
                    statusStats.Add(new OrderStatusDetail
                    {
                        Status = status.ToString(),
                        Count = statusCount,
                        Percentage = Math.Round(((double)statusCount / totalOrders) * 100, 2),
                        TotalRevenue = statusOrders.Sum(o => o.TotalAmount),
                        AverageOrderValue = statusCount > 0 ? statusOrders.Average(o => o.TotalAmount) : 0,
                        RecentOrders = statusOrders
                            .OrderByDescending(o => o.OrderDate)
                            .Take(5)
                            .Select(o => new RecentOrderSummary
                            {
                                OrderId = o.OrderId,
                                UserId = o.UserId,
                                TotalAmount = o.TotalAmount,
                                OrderDate = o.OrderDate ?? DateTime.MinValue
                            }).ToList()
                    });
                }
            }

            // Tính toán trends (so với kỳ trước)
            var previousPeriodFrom = fromDate.AddDays(-(toDate - fromDate).Days);
            var previousOrders = orders.Where(o =>
                o.OrderDate >= previousPeriodFrom &&
                o.OrderDate < fromDate).ToList();

            var trends = new List<StatusTrend>();
            foreach (var currentStatus in statusStats)
            {
                var previousCount = previousOrders.Count(o => o.Status.ToString() == currentStatus.Status);
                var growthPercent = previousCount > 0
                    ? Math.Round(((double)(currentStatus.Count - previousCount) / previousCount) * 100, 2)
                    : 100; // Nếu trước đó = 0 thì tăng 100%

                trends.Add(new StatusTrend
                {
                    Status = currentStatus.Status,
                    CurrentPeriodCount = currentStatus.Count,
                    PreviousPeriodCount = previousCount,
                    GrowthPercent = growthPercent,
                    Trend = growthPercent > 5 ? "Increasing" :
                           growthPercent < -5 ? "Decreasing" : "Stable"
                });
            }

            // Sửa lỗi ở đây - thay đổi cách tính completion rate và cancellation rate
            var confirmedStatus = statusStats.FirstOrDefault(s => s.Status == "Confirmed");
            var cancelledStatus = statusStats.FirstOrDefault(s => s.Status == "Cancelled");
            var pendingStatus = statusStats.FirstOrDefault(s => s.Status == "Pending");

            var result = new OrdersByStatusResponse
            {
                FromDate = fromDate,
                ToDate = toDate,
                TotalOrders = totalOrders,
                StatusBreakdown = statusStats.OrderByDescending(s => s.Count).ToList(),
                StatusTrends = trends,
                Summary = new OrderStatusSummary
                {
                    MostCommonStatus = statusStats.OrderByDescending(s => s.Count).First().Status,
                    CompletionRate = totalOrders > 0 ? Math.Round(((double)(confirmedStatus?.Count ?? 0) / totalOrders) * 100, 2) : 0,
                    CancellationRate = totalOrders > 0 ? Math.Round(((double)(cancelledStatus?.Count ?? 0) / totalOrders) * 100, 2) : 0,
                    PendingOrdersValue = pendingStatus?.TotalRevenue ?? 0
                }
            };

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy thống kê đơn hàng theo trạng thái thành công", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting orders by status");
            return new BusinessResult(Const.FAIL_READ_CODE, $"Lỗi khi lấy thống kê: {ex.Message}");
        }
    }
    #endregion Helper Methods
}