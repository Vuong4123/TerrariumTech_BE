using System.ComponentModel.DataAnnotations;

namespace TerrariumGardenTech.Repositories.Entity;

public partial class Chat
{
    public int ChatId { get; set; }
    
    public int User1Id { get; set; }
    
    public int User2Id { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public virtual User User1 { get; set; }
    
    public virtual User User2 { get; set; }
    
    public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
}
