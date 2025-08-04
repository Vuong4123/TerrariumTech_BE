using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Common.Entity
{
    public class OrderRequestRefund
    {
        public int RequestRefundId { get; set; }
        public int OrderId { get; set; }
        /// <summary>
        /// Lý do yêu cầu hoàn tiền
        /// </summary>
        public string Reason { get; set; } = "";
        /// <summary>
        /// Ngày tạo yêu cầu hoàn tiền
        /// </summary>
        public DateTime RequestDate { get; set; }
        /// <summary>
        /// Trạng thái của yêu cầu hoàn tiền
        /// </summary>
        public RequestRefundStatusEnum Status { get; set; }
        /// <summary>
        /// Lý do sửa đổi yêu cầu hoàn tiền - quản lý điền lý do
        /// </summary>
        public string? ReasonModified { get; set; }
        /// <summary>
        /// Người sửa đổi yêu cầu hoàn tiền
        /// </summary>
        public int UserModified { get; set; }
        /// <summary>
        /// Số tiền/điểm hoàn lại cho yêu cầu hoàn tiền, nếu có
        /// </summary>
        public decimal? RefundAmount { get; set; }
        /// <summary>
        /// Đánh dấu không hoàn tiền mà chuyển sang điểm
        /// </summary>
        public bool IsPoint { get; set; }
        /// <summary>
        /// Thông tin vận chuyển liên quan đến yêu cầu hoàn tiền
        /// </summary>
        public int? TransportId { get; set; }
        /// <summary>
        /// Thời gian sửa đổi yêu cầu hoàn tiền
        /// </summary>
        public DateTime LastModifiedDate { get; set; }

        public virtual Order Order { get; set; } = null!;
    }
}
