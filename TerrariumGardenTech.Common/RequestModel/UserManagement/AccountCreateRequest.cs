namespace TerrariumGardenTech.Common.RequestModel.UserManagement;

public class AccountCreateRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty; // Mật khẩu
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public int RoleId { get; set; } // 2: Staff, 3: Manager
}