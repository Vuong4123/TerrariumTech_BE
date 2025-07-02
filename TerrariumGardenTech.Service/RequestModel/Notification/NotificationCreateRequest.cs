namespace TerrariumGardenTech.Service.RequestModel.Notification
{
    public class NotificationCreateRequest
    {
        public int UserId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
    }
}
