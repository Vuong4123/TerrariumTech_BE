using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.Enums
{
    public enum OrderStatus
    {
        Pending = 1,     // Mới tạo, chờ xử lý
        Taking = 2,      // Đang chuẩn bị
        Delivering = 3,  // Đang giao
        Delivered = 4,   // Đã giao thành công
        Refund = 5,      // Hoàn tiền
        Cancelled = 6    // Đã huỷ
    }
}
