using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Notification;
using TerrariumGardenTech.Common.ResponseModel.Notification;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.Service;

namespace TerrariumGardenTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    // Create a new notification
    [HttpPost("create")]
    public async Task<IActionResult> CreateAndPush([FromBody] NotificationCreateRequest request)
    {
        var result = await _notificationService.CreateAndPushAsync(request);
        if (result.Status == Const.SUCCESS_CREATE_CODE)
            return Ok(result);
        return BadRequest(result);
    }

    // Get all notifications
    [HttpGet("get-all")]
    public async Task<IBusinessResult> GetAll()
    {
        return await _notificationService.GetAllNotificationAsync();
    }

    // Get notification by ID
    [HttpGet("get/{id}")]
    public async Task<IBusinessResult?> Get(int id)
    {
        return await _notificationService.GetNotificationByIdAsync(id);
    }

    // Get notifications by User ID
    [HttpGet("get-by-user/{userId}")]
    public async Task<IBusinessResult> GetByUserId(int userId)
    {
        return await _notificationService.GetNotificationByUserIdAsync(userId);
    }

    // Mark notification as read
    [HttpPut("mark-as-read/{id}")]
    public async Task<IBusinessResult> MarkAsRead(int id)
    {
        return await _notificationService.MarkNotificationAsReadAsync(id);
    }

    // Delete a notification by ID
    [HttpDelete("delete/{id}")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _notificationService.DeleteNotificationAsync(id);
    }

    /// <summary>
    /// Tạo web notification đơn giản - KHÔNG CẦN AUTHENTICATE
    /// Dùng cho system tự động tạo notification (thanh toán, order status, etc.)
    /// 
    /// // Trong PaymentService - sau khi thanh toán thành công
    //var webNotificationRequest = new WebNotificationCreateRequest
    //{
    //    UserId = order.UserId,
    //    Title = "Thanh toán thành công",
    //    Description = $"Đơn hàng #{order.OrderId} đã được thanh toán thành công. Số tiền: {paid:N0} VND"
    //};

    //await _notificationService.CreateWebNotificationAsync(webNotificationRequest);
    /// 
    /// var broadcastRequest = new BroadcastNotificationRequest
    //{
    //    Title = "Thông báo bảo trì hệ thống",
    //    Description = "Hệ thống sẽ bảo trì từ 2:00 - 4:00 sáng ngày mai",
    //    UserIds = null // null = gửi cho tất cả
    //};
    /// 
    /// </summary>
    [HttpPost("web/create")]
    public async Task<IActionResult> CreateWebNotification([FromBody] WebNotificationCreateRequest request)
    {
        var result = await _notificationService.CreateWebNotificationAsync(request);

        if (result.Status == Const.SUCCESS_CREATE_CODE)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// Broadcast notification - CHỈ ADMIN/MANAGER/STAFF
    /// </summary>
    [HttpPost("web/broadcast")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    public async Task<IActionResult> BroadcastNotification([FromBody] BroadcastNotificationRequest request)
    {
        var result = await _notificationService.BroadcastNotificationAsync(request);

        if (result.Status == Const.SUCCESS_CREATE_CODE)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// Lấy notifications của user với phân trang
    /// </summary>
    [HttpGet("web/user/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetWebNotificationsByUser(int userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _notificationService.GetNotificationsByUserIdAsync(userId, page, pageSize);

        if (result.Status == Const.SUCCESS_READ_CODE)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// Đếm số notification chưa đọc của user
    /// </summary>
    [HttpGet("web/unread-count/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUnreadCount(int userId)
    {
        // Implement this in service if needed
        var allNotifications = await _notificationService.GetNotificationByUserIdAsync(userId);

        if (allNotifications.Status == Const.SUCCESS_READ_CODE && allNotifications.Data != null)
        {
            var notifications = (List<NotificationResponse>)allNotifications.Data;
            var unreadCount = notifications.Count(n => !n.IsRead);

            return Ok(new { UnreadCount = unreadCount });
        }

        return Ok(new { UnreadCount = 0 });
    }
}