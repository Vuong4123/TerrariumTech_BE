namespace TerrariumGardenTech.Common.ResponseModel.Chat;

public class ChatResponse
{
    public int ChatId { get; set; }
    public int User1Id { get; set; }
    public int User2Id { get; set; }
    public string User1Name { get; set; }
    public string User2Name { get; set; }
    public string User1Role { get; set; }
    public string User2Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public ChatMessageResponse LastMessage { get; set; }
}
