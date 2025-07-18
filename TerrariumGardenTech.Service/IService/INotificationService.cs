using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Notification;

namespace TerrariumGardenTech.Service.IService;

public interface INotificationService
{
    Task<IBusinessResult> CreateNotificationAsync(NotificationCreateRequest notificationCreateRequest);
    Task<IBusinessResult> GetAllNotificationAsync();
    Task<IBusinessResult?> GetNotificationByIdAsync(int id);
    Task<IBusinessResult> GetNotificationByUserIdAsync(int userId);
    Task<IBusinessResult> MarkNotificationAsReadAsync(int id);
    Task<IBusinessResult> DeleteNotificationAsync(int id);
}