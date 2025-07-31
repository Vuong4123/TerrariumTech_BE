using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.Enums
{
    public enum OrderStatus
    {
        Pending,     // Mới tạo, chờ xử lý
        Taking,      // Đang chuẩn bị
        Delivering,  // Đang giao
        Delivered,   // Đã giao thành công
        Refund,      // Hoàn tiền
        Cancelled    // Đã huỷ
    }
}
