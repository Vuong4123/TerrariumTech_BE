using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Common.Entity;

public class Cart
{
    public int CartId { get; set; } // Khóa chính
    public int UserId { get; set; } // Khóa ngoại đến User
    public User User { get; set; } // Liên kết đến User (mỗi người dùng có 1 giỏ)

    public List<CartItem> CartItems { get; set; } // Danh sách các sản phẩm trong giỏ

    public DateTime CreatedAt { get; set; } // Thời gian tạo giỏ
    public DateTime UpdatedAt { get; set; } // Thời gian cập nhật giỏ
}