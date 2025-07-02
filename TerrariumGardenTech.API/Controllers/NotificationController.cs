using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Notification;

namespace TerrariumGardenTech.API.Controllers
{
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
        public async Task<IBusinessResult> Post([FromBody] NotificationCreateRequest notificationCreateRequest)
        {
            return await _notificationService.CreateNotificationAsync(notificationCreateRequest);
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
    }
}
