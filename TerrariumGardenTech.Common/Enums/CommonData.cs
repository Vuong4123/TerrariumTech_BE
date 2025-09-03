namespace TerrariumGardenTech.Common.Enums;

public static class CommonData
{
    public struct AccountStatus
    {
        public const string Active = "Active";
        public const string Inactive = "Inactive";
    }
    public struct CartItemType
    {
        public const string SINGLE = "SINGLE";              // Mua lẻ
        public const string MAIN_ITEM = "MAIN_ITEM";        // Bể chính  
        public const string BUNDLE_ACCESSORY = "BUNDLE_ACCESSORY"; // Phụ kiện combo
        public const string COMBO = "COMBO"; // Phụ kiện combo
    }
    public struct LayoutStatus
    {
        public const string Draft = "Draft"; // Mới tạo, chưa gửi duyệt
        public const string Pending = "Pending"; // Chờ duyệt
        public const string Approved = "Approved";
        public const string Rejected = "Rejected";
        public const string Ordered = "Ordered";
    }
    public struct OrderStatusData
    {
        public const string Pending = "Pending";         // Chờ duyệt
        public const string Approved = "Approved";       // Đã duyệt
        public const string Rejected = "Rejected";       // Bị từ chối
        public const string Cancel = "Cancel";           // Đã hủy
        public const string Confirmed = "Confirmed";     // Đã xác nhận

        public const string AdminReject = "AdminReject"; // Bị từ chối bởi Admin

        public const string Processing = "Processing";   // Đang xử lý
        public const string Shipping = "Shipping";       // Đang giao hàng
        public const string Completed = "Completed";     // Hoàn thành
        public const string Failed = "Failed";           // Giao hàng thất bại

        public const string RequestRefund = "RequestRefund"; // Yêu cầu hoàn tiền
        public const string Refuning = "Refuning";           // Đang hoàn tiền
        public const string Refunded = "Refunded";           // Đã hoàn tiền
    }

}