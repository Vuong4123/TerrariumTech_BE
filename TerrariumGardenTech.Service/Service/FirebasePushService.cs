using FirebaseAdmin.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service
{
    public class FirebasePushService : IFirebasePushService
    {
        public async Task<string> SendNotificationAsync(
            string fcmToken,
            string title,
            string body,
            IReadOnlyDictionary<string, string>? data = null)
        {
            var message = new Message
            {
                Token = fcmToken,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                },
                Data = data
            };

            try
            {
                var messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                return messageId;
            }
            catch (FirebaseAdmin.Messaging.FirebaseMessagingException ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }
    }
}
