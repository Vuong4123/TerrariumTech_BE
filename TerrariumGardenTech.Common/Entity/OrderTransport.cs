using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Common.Entity
{
    public class OrderTransport
    {
        public int TransportId { get; set; }
        /// <summary>
        /// Mã đơn hàng tương ứng với Order.Id
        /// </summary>
        public int OrderId { get; set; }
        /// <summary>
        /// Trạng thái vận chuyển của đơn hàng
        /// </summary>
        public TransportStatusEnum Status { get; set; }
        /// <summary>
        /// Ngày ước tính hoàn thành vận chuyển
        /// </summary>
        public DateTime EstimateCompletedDate { get; set; }
        /// <summary>
        /// Ngày thực tế hoàn thành vận chuyển
        /// </summary>
        public DateTime? CompletedDate { get; set; }
        /// <summary>
        /// Thời điểm cuối cùng xác nhận không thể giao hàng
        /// </summary>
        public DateTime? LastConfirmFailed { get; set; }
        /// <summary>
        /// Số lần liên hệ với khách hàng để giao hàng không thành công
        /// </summary>
        public int ContactFailNumber { get; set; }
        /// <summary>
        /// Ghi chú về quá trình vận chuyển
        /// </summary>
        public string? Note { get; set; }
        /// <summary>
        /// Là đơn yêu cầu thu hồi sản phẩm
        /// </summary>
        public bool IsRefund { get; set; }
        /// <summary>
        /// Mã người hiện tại đang giữ đơn vận chuyển, có thể là shipper hoặc nv kho
        /// </summary>
        public int? UserId { get; set; }
        /// <summary>
        /// Ngày tạo đơn vận chuyển - khi tạo đơn gán luôn cho 1 user là có role là nv kho
        /// </summary>
        public DateTime CreatedDate { get; set; }
        /// <summary>
        /// Người tạo đơn vận chuyển
        /// </summary>
        public string? CreatedBy { get; set; }

        public virtual Order Order { get; set; } = null!;
        public virtual IEnumerable<OrderTransportItem> Items { get; set; } = Enumerable.Empty<OrderTransportItem>();
        public virtual ICollection<TransportLog> TransportLogs { get; set; } = new List<TransportLog>();
    }

    public class TransportLog
    {
        public int LogId { get; set; }
        /// <summary>
        /// Mã đơn vận chuyển tương ứng với OrderTransport.Id
        /// </summary>
        public int OrderTransportId { get; set; }
        /// <summary>
        /// Trạng thái vận chuyển tại thời điểm ghi log
        /// </summary>
        public TransportStatusEnum LastStatus { get; set; }
        /// <summary>
        /// Trạng thái vận chuyển mới được cập nhật
        /// </summary>
        public TransportStatusEnum NewStatus { get; set; }
        /// <summary>
        /// Người giữ đơn hàng tại thời điểm tạo log, có giá trị khi:
        /// Chuyển từ kho => Shipper - lưu thông tin người giao hàng cho shipper
        /// Shipper => Khách hàng hoặc Shipper => Kho - lưu thông tin shipper
        /// </summary>
        public int? OldUser { get; set; }
        /// <summary>
        /// Người giữ đơn hàng sau khi log được tạo, có giá trị khi:
        /// Chuyển từ kho => Shipper - lưu thông tin shipper
        /// Shipper => Khách hàng - Lưu thông tin khách hàng
        /// Shipper => Kho - Lưu thông tin người nhận tại kho
        /// </summary>
        public int? CurrentUser { get; set; }
        /// <summary>
        /// Lý do cập nhật trạng thái vận chuyển: Lý do giao thất bại, lý do đổi shipper, v.v.
        /// </summary>
        public string? Reason { get; set; }
        /// <summary>
        /// Ảnh chụp xác nhận thay đổi trạng thái
        /// </summary>
        public string? CheckinImage { get; set; }
        /// <summary>
        /// Ngày tạo đơn vận chuyển
        /// </summary>
        public DateTime CreatedDate { get; set; }
        /// <summary>
        /// Người tạo đơn vận chuyển
        /// </summary>
        public string? CreatedBy { get; set; }

        public virtual OrderTransport Transport { get; set; } = null!;
    }

    public class OrderTransportItem
    {
        public int TransportItemId { get; set; }
        public int OrderItemId { get; set; }
        public int Quantity { get; set; }
        public int TransportId { get; set; }

        public virtual OrderTransport? OrderTransport { get; set; }
    }
}
