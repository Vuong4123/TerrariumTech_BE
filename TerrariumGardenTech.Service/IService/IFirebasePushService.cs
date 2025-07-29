using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Service.IService
{
    public interface IFirebasePushService
    {
        Task<string> SendNotificationAsync(
            string fcmToken,
            string title,
            string body,
            IReadOnlyDictionary<string, string>? data = null);
    }
}
