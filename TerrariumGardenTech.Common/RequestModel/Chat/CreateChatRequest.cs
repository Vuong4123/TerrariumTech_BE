using System.ComponentModel.DataAnnotations;

namespace TerrariumGardenTech.Common.RequestModel.Chat;

public class CreateChatRequest
{
    [Required]
    public int TargetUserId { get; set; }
}
