using System.ComponentModel.DataAnnotations;

namespace TerrariumGardenTech.Repositories.Entity;

public partial class ChatMessage
{
    public int MessageId { get; set; }
    
    public int ChatId { get; set; }
    
    public int SenderId { get; set; }
    
    public string Content { get; set; }
    
    public DateTime SentAt { get; set; }
    
    public bool IsRead { get; set; } = false;
    
    public bool IsDeleted { get; set; } = false;
    
    public virtual Chat Chat { get; set; }
    
    public virtual User Sender { get; set; }
}
