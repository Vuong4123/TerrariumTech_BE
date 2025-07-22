namespace TerrariumGardenTech.Common.RequestModel.Role;

public class RoleUpdateRequest
{
    public int RoleId { get; set; }

    public string RoleName { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
}