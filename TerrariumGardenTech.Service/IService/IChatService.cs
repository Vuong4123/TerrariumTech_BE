using TerrariumGardenTech.Common.RequestModel.Chat;
using TerrariumGardenTech.Common.ResponseModel.Chat;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService;

public interface IChatService
{
    Task<IBusinessResult> GetAvailableUsersAsync(int currentUserId);
    Task<IBusinessResult> CreateChatAsync(int currentUserId, CreateChatRequest request);
    Task<IBusinessResult> GetUserChatsAsync(int currentUserId);
    Task<IBusinessResult> GetChatMessagesAsync(int currentUserId, int chatId, int page = 1, int pageSize = 50);
    Task<IBusinessResult> SendMessageAsync(int currentUserId, SendMessageRequest request);
    Task<IBusinessResult> MarkMessagesAsReadAsync(int currentUserId, int chatId);
}
