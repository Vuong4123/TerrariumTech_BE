namespace TerrariumGardenTech.API.Hubs;

/// <summary>
/// Interface định nghĩa các methods mà SignalR clients có thể nhận
/// 
/// Được sử dụng để strongly-typed SignalR Hub
/// Đảm bảo type safety khi gửi messages từ server đến client
/// </summary>
public interface IChatHubClient
{
    /// <summary>
    /// Nhận tin nhắn mới
    /// </summary>
    /// <param name="message">Thông tin tin nhắn</param>
    Task NewMessage(object message);

    /// <summary>
    /// Thông báo user online
    /// </summary>
    /// <param name="userId">ID của user online</param>
    Task UserOnline(int userId);

    /// <summary>
    /// Thông báo user offline
    /// </summary>
    /// <param name="userId">ID của user offline</param>
    Task UserOffline(int userId);

    /// <summary>
    /// Thông báo user join chat
    /// </summary>
    /// <param name="data">Thông tin join chat</param>
    Task UserJoinedChat(object data);

    /// <summary>
    /// Thông báo user leave chat
    /// </summary>
    /// <param name="data">Thông tin leave chat</param>
    Task UserLeftChat(object data);

    /// <summary>
    /// Thông báo messages đã được đọc
    /// </summary>
    /// <param name="data">Thông tin đã đọc</param>
    Task MessagesMarkedAsRead(object data);

    /// <summary>
    /// Thông báo typing indicator
    /// </summary>
    /// <param name="data">Thông tin typing</param>
    Task TypingIndicator(object data);

    /// <summary>
    /// Thông báo lỗi
    /// </summary>
    /// <param name="message">Thông báo lỗi</param>
    Task Error(string message);
}
