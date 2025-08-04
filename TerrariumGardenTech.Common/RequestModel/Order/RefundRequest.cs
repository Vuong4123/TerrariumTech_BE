using TerrariumGardenTech.Common.Enums;

namespace TerrariumGardenTech.Common.RequestModel.Order
{
    public class CreateRefundRequest
    {
        public int OrderId { get; set; }
        public string Reason { get; set; } = "";
    }

    public class UpdateRefundRequest
    {
        public int RefundId { get; set; }
        public RequestRefundStatusEnum Status { get; set; }
        public string? Reason { get; set; }
        public decimal? RefundAmount { get; set; }
        public bool IsPoint { get; set; }

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
}