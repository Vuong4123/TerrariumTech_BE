using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrariumGardenTech.Common.Enums
{
    public enum OrderStatusEnum
    {
        /// <summary>
        /// Trạng thái đơn hàng bị giao thất bại do hàng bị hỏng, thất lạc hoặc không thể giao hàng đến khách hàng
        /// </summary>
        Failed = -2,
        /// <summary>
        /// Trạng thái đơn hàng đã hủy
        /// </summary>
        Cancle = -1,
        /// <summary>
        /// Trạng thái đơn hàng đang chờ xử lý
        /// </summary>
        Pending = 1,
        /// <summary>
        /// Trạng thái đơn hàng đã xác nhận
        /// </summary>
        Confirmed = 2,
        /// <summary>
        /// Trạng thái đơn hàng đang xử lý
        /// </summary>
        Processing = 3,
        /// <summary>
        /// Trạng thái đơn hàng đang vận chuyển
        /// </summary>
        Shipping = 4,
        /// <summary>
        /// Trạng thái đơn hàng đã hoàn thành
        /// </summary>
        Completed = 5,
        /// <summary>
        /// Trạng thái đơn hàng yêu cầu hoàn tiền
        /// </summary>
        RequestRefund = 6,
        /// <summary>
        /// Trạng thái đơn hàng đang trong quá trình xử lý hoàn tiền
        /// </summary>
        Refuning = 7,
        /// <summary>
        /// Trạng thái đơn hàng đã hoàn tiền
        /// </summary>
        Refunded = 8
    }

    public enum TransportStatusEnum
    {
        /// <summary>
        /// Trạng thái hàng hóa đã bị mất trong kho
        /// </summary>
        LostInWarehouse = -2,
        /// <summary>
        /// Trạng thái hàng hóa đã bị mất khi vận chuyển đến khách hàng
        /// </summary>
        LostShipping = -1,
        /// <summary>
        /// Trạng thái hàng hóa đang ở kho, chưa vận chuyển
        /// </summary>
        InWarehouse = 0,
        /// <summary>
        /// Trạng thái hàng hóa đang vận chuyển đến khách hàng
        /// </summary>
        Shipping = 1,
        /// <summary>
        /// Trạng thái hàng hóa đã được giao đến khách hàng
        /// </summary>
        Completed = 2,
        /// <summary>
        /// Trạng thái hàng hóa vận chuyển không thành công
        /// </summary>
        Failed = 3,
        /// <summary>
        /// Trạng thái hàng hóa đang ở trong kho của khách hàng
        /// </summary>
        InCustomer = 4,
        /// <summary>
        /// Trạng thái hàng hóa đang ở kho của khách hàng nhưng không thể liên lạc với khách hàng để lấy hàng
        /// </summary>
        GetFromCustomerFail = 5,
        /// <summary>
        /// Trạng thái hàng hóa đang được vận chuyển về kho
        /// </summary>
        ShippingToWareHouse = 6,
        /// <summary>
        /// Trạng thái hàng hóa đã được vận chuyển về kho thành công
        /// </summary>
        CompletedToWareHouse = 7,
        /// <summary>
        /// Trạng thái hàng hóa vận chuyển về kho không thành công
        /// </summary>
        FailedToWareHouse = 8
    }

    public enum RequestRefundStatusEnum
    {
        /// <summary>
        /// Trạng thái yêu cầu hoàn tiền đã bị hủy - do người mua tự hủy
        /// </summary>
        Cancle = -1,
        /// <summary>
        /// Trạng thái yêu cầu hoàn tiền đang chờ xử lý
        /// </summary>
        Waiting = 0,
        /// <summary>
        /// Trạng thái yêu cầu hoàn tiền đã được phê duyệt
        /// </summary>
        Approved = 1,
        /// <summary>
        /// Trạng thái yêu cầu hoàn tiền đã bị từ chối
        /// </summary>
        Rejected = 2
    }
}
