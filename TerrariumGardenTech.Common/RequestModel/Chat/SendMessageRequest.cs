using System.ComponentModel.DataAnnotations;

namespace TerrariumGardenTech.Common.RequestModel.Chat;

public class SendMessageRequest
{
    [Required]
    public int ChatId { get; set; }
    
    [Required]
    [StringLength(1000, ErrorMessage = "Message content cannot exceed 1000 characters")]
    public string Content { get; set; }
}
