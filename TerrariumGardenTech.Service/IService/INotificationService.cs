using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.RequestModel.Notification;

namespace TerrariumGardenTech.Service.IService
{
    public interface INotificationService
    {
        Task<IBusinessResult> CreateNotificationAsync(NotificationCreateRequest notificationCreateRequest);
        Task<IBusinessResult> GetAllNotificationAsync();
        Task<IBusinessResult?> GetNotificationByIdAsync(int id);
    }
}
