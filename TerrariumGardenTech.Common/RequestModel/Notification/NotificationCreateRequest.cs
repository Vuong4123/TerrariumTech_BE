namespace TerrariumGardenTech.Common.RequestModel.Notification;

public class NotificationCreateRequest
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}