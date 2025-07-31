using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Chat;
using TerrariumGardenTech.Common.ResponseModel.Chat;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

/// <summary>
/// Đây là service chính xử lý tất cả logic nghiệp vụ cho hệ thống chat:
/// 1. Kiểm tra quyền user theo vai trò (Admin > Manager > Staff > User)
/// 2. Tạo chat mới hoặc lấy chat đã có
/// 3. Gửi/nhận tin nhắn với validation
/// 4. Quản lý trạng thái đã đọc/chưa đọc
/// </summary>
public class ChatService : IChatService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly ILogger<ChatService> _logger;
    public ChatService(UnitOfWork unitOfWork, ILogger<ChatService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Lấy danh sách user có thể chat dựa trên vai trò của user hiện tại
    ///
    /// Logic lọc theo vai trò:
    /// - Admin (4): Có thể thấy Manager(3), Staff(2), User(1)
    /// - Manager (3): Có thể thấy Staff(2), User(1)
    /// - Staff (2): Có thể thấy User(1), Staff(2) khác
    /// - User (1): Có thể thấy Staff(2), Manager(3), Admin(4)
    ///
    /// Với mỗi user có thể chat, kiểm tra xem đã có chat chưa và bao gồm thông tin đó
    ///
    /// CÁCH HOẠT ĐỘNG:
    /// 1. Lấy thông tin user hiện tại và vai trò
    /// 2. Dựa vào vai trò để lọc danh sách user có thể chat
    /// 3. Kiểm tra từng user xem đã có chat chưa
    /// 4. Trả về danh sách kèm trạng thái chat
    /// </summary>
    /// <param name="currentUserId">ID của user đang đăng nhập</param>
    /// <returns>
    /// Thành công: Danh sách UserForChatResponse với thông tin user và trạng thái chat
    /// Lỗi: Business result với thông báo lỗi
    /// </returns>
    public async Task<IBusinessResult> GetAvailableUsersAsync(int currentUserId)
    {
        try
        {
            var currentUser = await _unitOfWork.User.GetByIdAsync(currentUserId);
            if (currentUser == null)
            {
                return new BusinessResult(Const.FAIL_READ_CODE, "User not found");
            }

            if (!Enum.TryParse<RoleStatus>(currentUser.RoleId.ToString(), out var currentUserRole))
            {
                return new BusinessResult(Const.FAIL_READ_CODE, "Invalid user role");
            }
            var availableUsers = await _unitOfWork.User.GetUsersForChatAsync(currentUserId, currentUserRole);

            var response = new List<UserForChatResponse>();

            foreach (var user in availableUsers)
            {
                try
                {
                    var existingChat = await _unitOfWork.Chat.GetChatBetweenUsersAsync(currentUserId, user.UserId);

                    response.Add(new UserForChatResponse
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        FullName = user.FullName,
                        Email = user.Email,
                        RoleId = user.RoleId ?? 0,
                        RoleName = user.Role?.RoleName ?? "Unknown",
                        Status = user.Status,
                        HasExistingChat = existingChat != null,
                        ExistingChatId = existingChat?.ChatId
                    });
                }
                catch (Exception userEx)
                {
                    _logger.LogError(userEx, "Error processing user {UserId} for available users list", user.UserId);
                }
            }

            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, response);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, "An error occurred while getting available users");
        }
    }

    /// <summary>
    /// Creates a new chat between current user and target user
    ///
    /// Process:
    /// 1. Validates target user exists
    /// 2. Checks if chat already exists (prevents duplicates)
    /// 3. Creates new chat if none exists
    /// 4. Returns chat details with both users' information
    public async Task<IBusinessResult> CreateChatAsync(int currentUserId, CreateChatRequest request)
    {
        try
        {
            var targetUser = await _unitOfWork.User.GetByIdAsync(request.TargetUserId);
            if (targetUser == null)
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Target user not found");

            var existingChat = await _unitOfWork.Chat.GetChatBetweenUsersAsync(currentUserId, request.TargetUserId);
            if (existingChat != null)
            {
                var existingChatResponse = MapChatToResponse(existingChat, currentUserId);
                return new BusinessResult(Const.SUCCESS_READ_CODE, "Chat already exists", existingChatResponse);
            }

            var newChat = new Chat
            {
                User1Id = currentUserId,        
                User2Id = request.TargetUserId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _unitOfWork.Chat.CreateAsync(newChat);

            var createdChat = await _unitOfWork.Chat.GetChatBetweenUsersAsync(currentUserId, request.TargetUserId);
            var response = MapChatToResponse(createdChat, currentUserId);

            return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, response);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, "An error occurred while creating chat");
        }
    }

    /// <summary>
    /// Gets all chats for the current user, ordered by most recent activity
    ///
    /// Returns chats where user is either User1 or User2, including:
    /// - Other participant's details (name, role)
    /// - Last message preview
    /// - Chat metadata (created/updated timestamps)
    public async Task<IBusinessResult> GetUserChatsAsync(int currentUserId)
    {
        try
        {
            var chats = await _unitOfWork.Chat.GetUserChatsAsync(currentUserId);

            var response = chats.Select(chat => MapChatToResponse(chat, currentUserId)).ToList();

            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, response);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, "An error occurred while getting chats");
        }
    }

    /// <summary>
    /// Gets paginated messages for a specific chat
    ///
    /// Security: User must be a participant in the chat to access messages
    ///
    /// Pagination:
    /// - Messages are retrieved in descending order (newest first from DB)
    /// - Then reversed to show chronological order (oldest first) in response
    /// - Default page size: 50, maximum: 100 (enforced in controller)
    public async Task<IBusinessResult> GetChatMessagesAsync(int currentUserId, int chatId, int page = 1, int pageSize = 50)
    {
        try
        {
            var chat = await _unitOfWork.Chat.GetChatWithMessagesAsync(chatId, currentUserId);
            if (chat == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Chat not found or access denied");

            var messages = await _unitOfWork.ChatMessage.GetChatMessagesAsync(chatId, page, pageSize);

            var response = messages.Select(MapMessageToResponse).Reverse().ToList();

            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, response);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, "An error occurred while getting messages");
        }
    }

    /// <summary>
    /// Sends a new message in an existing chat
    ///
    /// Security: User must be a participant in the chat to send messages
    ///
    /// Process:
    /// 1. Validates user access to chat
    /// 2. Creates message with current timestamp
    /// 3. Updates chat's UpdatedAt timestamp (for sorting in chat list)
    /// 4. Returns created message details
    public async Task<IBusinessResult> SendMessageAsync(int currentUserId, SendMessageRequest request)
    {
        try
        {
            var chat = await _unitOfWork.Chat.GetChatWithMessagesAsync(request.ChatId, currentUserId);
            if (chat == null)
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Chat not found or access denied");

            var message = new ChatMessage
            {
                ChatId = request.ChatId,
                SenderId = currentUserId,
                Content = request.Content,
                SentAt = DateTime.UtcNow,
                IsRead = false, 
                IsDeleted = false  
            };

            await _unitOfWork.ChatMessage.CreateAsync(message);

            chat.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Chat.UpdateAsync(chat);

            var createdMessage = await _unitOfWork.ChatMessage.GetByIdAsync(message.MessageId);
            var response = MapMessageToResponse(createdMessage);

            return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, response);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, "An error occurred while sending message");
        }
    }

    /// <summary>
    /// Marks all unread messages in a chat as read for the current user
    ///
    /// Security: User must be a participant in the chat
    ///
    /// Process:
    /// 1. Validates user access to chat
    /// 2. Finds all unread messages from other users in this chat
    /// 3. Updates IsRead flag to true for those messages
    public async Task<IBusinessResult> MarkMessagesAsReadAsync(int currentUserId, int chatId)
    {
        try
        {
            var chat = await _unitOfWork.Chat.GetChatWithMessagesAsync(chatId, currentUserId);
            if (chat == null)
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Chat not found or access denied");

            await _unitOfWork.ChatMessage.MarkMessagesAsReadAsync(chatId, currentUserId);

            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Messages marked as read");
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, "An error occurred while marking messages as read");
        }
    }

    /// <summary>
    /// Maps Chat entity to ChatResponse DTO for API response
    ///
    /// Includes:
    /// - Both users' details (names and roles)
    /// - Chat metadata (timestamps, active status)
    /// - Last message preview (if any)
    private ChatResponse MapChatToResponse(Chat chat, int currentUserId)
    {
        var otherUser = chat.User1Id == currentUserId ? chat.User2 : chat.User1;
        var currentUser = chat.User1Id == currentUserId ? chat.User1 : chat.User2;

        return new ChatResponse
        {
            ChatId = chat.ChatId,
            User1Id = chat.User1Id,
            User2Id = chat.User2Id,
            User1Name = chat.User1?.FullName ?? "Unknown",
            User2Name = chat.User2?.FullName ?? "Unknown",
            User1Role = chat.User1?.Role?.RoleName ?? "Unknown",
            User2Role = chat.User2?.Role?.RoleName ?? "Unknown",
            CreatedAt = chat.CreatedAt,
            UpdatedAt = chat.UpdatedAt,
            IsActive = chat.IsActive,
            LastMessage = chat.ChatMessages?.FirstOrDefault() != null ? MapMessageToResponse(chat.ChatMessages.First()) : null
        };
    }

    /// <summary>
    /// Maps ChatMessage entity to ChatMessageResponse DTO for API response
    ///
    /// Includes:
    /// - Message content and metadata
    /// - Sender details (name and role)
    /// - Read and delete status
    private ChatMessageResponse MapMessageToResponse(ChatMessage message)
    {
        return new ChatMessageResponse
        {
            MessageId = message.MessageId,
            ChatId = message.ChatId,
            SenderId = message.SenderId,
            SenderName = message.Sender?.FullName ?? "Unknown",
            SenderRole = message.Sender?.Role?.RoleName ?? "Unknown",
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = message.IsRead,
            IsDeleted = message.IsDeleted
        };
    }
}
