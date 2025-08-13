using TerrariumGardenTech.Common.RequestModel.Notification;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface INotificationService
{
    Task<IBusinessResult> CreateNotificationAsync(NotificationCreateRequest notificationCreateRequest);
    Task<IBusinessResult> CreateAndPushAsync(NotificationCreateRequest req);
    Task<IBusinessResult> GetAllNotificationAsync();
    Task<IBusinessResult?> GetNotificationByIdAsync(int id);
    Task<IBusinessResult> GetNotificationByUserIdAsync(int userId);
    Task<IBusinessResult> MarkNotificationAsReadAsync(int id);
    Task<IBusinessResult> DeleteNotificationAsync(int id);

    Task<IBusinessResult> CreateWebNotificationAsync(WebNotificationCreateRequest request);
    Task<IBusinessResult> BroadcastNotificationAsync(BroadcastNotificationRequest request);
    Task<IBusinessResult> GetNotificationsByUserIdAsync(int userId, int page = 1, int pageSize = 20);
}