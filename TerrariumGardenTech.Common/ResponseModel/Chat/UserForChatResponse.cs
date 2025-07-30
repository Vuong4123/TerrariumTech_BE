namespace TerrariumGardenTech.Common.ResponseModel.Chat;

public class UserForChatResponse
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public int RoleId { get; set; }
    public string RoleName { get; set; }
    public string Status { get; set; }
    public bool HasExistingChat { get; set; }
    public int? ExistingChatId { get; set; }
}
