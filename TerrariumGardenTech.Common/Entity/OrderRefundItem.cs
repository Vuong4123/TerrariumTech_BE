namespace TerrariumGardenTech.Common.Entity
{
    public class OrderRefundItem
    {
        public int OrderRefundItemId { get; set; }

        /// <summary>
        /// Id sản phẩm cần hoàn tiền
        /// </summary>
        public int OrderItemId { get; set; }
        /// <summary>
        /// Số lượng sản phẩm cần hoàn tiền
        /// </summary>
        public int Quantity { get; set; }
        /// <summary>
        /// Số điểm hoàn lại cho {Quantity} sản phẩm
        /// </summary>
        public int RefundPoint { get; set; }
        /// <summary>
        /// Lý do yêu cầu hoàn tiền
        /// </summary>
        public string? Reason { get; set; }
        /// <summary>
        /// Lý do sửa đổi yêu cầu hoàn tiền - quản lý điền lý do
        /// </summary>
        public string? ReasonModified { get; set; }
        /// <summary>
        /// Người sửa đổi yêu cầu hoàn tiền
        /// </summary>
        public int UserModified { get; set; }
        /// <summary>
        /// Ok, cho refund sản phẩm này
        /// </summary>
        public bool? IsApproved { get; set; }

        public int OrderRefundId { get; set; }
        public virtual OrderRequestRefund? OrderRefund { get; set; }
    }
}
