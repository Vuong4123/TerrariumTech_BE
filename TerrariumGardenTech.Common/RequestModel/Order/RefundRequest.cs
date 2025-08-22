using TerrariumGardenTech.Common.Enums;

namespace TerrariumGardenTech.Common.RequestModel.Order
{
    public class CreateRefundRequest
    {
        public int OrderId { get; set; }
        public IEnumerable<RefundItem> RefundItems { get; set; } = Enumerable.Empty<RefundItem>();
    }

    public class RefundItem
    {
        public int OrderItemId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = "";
    }

    public class UpdateRefundRequest
    {
        public int RefundId { get; set; }
        public RequestRefundStatusEnum Status { get; set; }
        public IEnumerable<UpdateRefundItem> Items { get; set; } = Enumerable.Empty<UpdateRefundItem>();

        /// <summary>
        /// Nếu = true thì sẽ tiến hành tạo đơn vận chuyển hoàn tiền
        /// </summary>
        public bool CreateTransport { get; set; }
        /// <summary>
        /// Ngày ước tính sẽ đến nhận hàng hoàn tiền
        /// </summary>
        public DateTime? EstimateCompletedDate { get; set; }
        /// <summary>
        /// Lưu ý nhận hàng
        /// </summary>
        public string? Note { get; set; }
        /// <summary>
        /// Giao cho shipper nào lấy
        /// </summary>
        public int? UserId { get; set; }
    }

    public class UpdateRefundItem
    {
        public int OrderRefundItemId { get; set; }
        public bool IsApproved { get; set; }
        public string Reason { get; set; } = string.Empty;
        public int Point { get; set; }
    }
}