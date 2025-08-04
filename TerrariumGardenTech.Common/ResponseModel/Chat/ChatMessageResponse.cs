namespace TerrariumGardenTech.Common.ResponseModel.Chat;

public class ChatMessageResponse
{
    public int MessageId { get; set; }
    public int ChatId { get; set; }
    public int SenderId { get; set; }
    public string SenderName { get; set; }
    public string SenderRole { get; set; }
    public string Content { get; set; }
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
    public bool IsDeleted { get; set; }
}
