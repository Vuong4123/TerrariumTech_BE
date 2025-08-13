using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.RequestModel.Notification
{

    public class WebNotificationCreateRequest
    {
        public int UserId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Để gửi cho tất cả user trong hệ thống
        public bool BroadcastToAll { get; set; } = false;
    }
    public class BroadcastNotificationRequest
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<int>? UserIds { get; set; } // Null = gửi cho tất cả
    }
}
