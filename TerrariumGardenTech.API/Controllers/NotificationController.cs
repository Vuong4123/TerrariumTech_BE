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
    }
}
