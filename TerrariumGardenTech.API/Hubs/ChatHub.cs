using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using TerrariumGardenTech.Common.RequestModel.Chat;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Hubs;

[Authorize]
public class ChatHub : Hub<IChatHubClient>
{
    private readonly IChatService _chatService;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IChatService chatService, ILogger<ChatHub> logger)
    {
        _chatService = chatService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        if (userId > 0)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            await Clients.Others.UserOnline(userId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = GetCurrentUserId();
        if (userId > 0)
        {
            await Clients.Others.UserOffline(userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task JoinChatRoom(int chatId)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            await Clients.Caller.Error("Unauthorized access");
            return;
        }

        try
        {
            var chatMessages = await _chatService.GetChatMessagesAsync(userId, chatId, 1, 1);

            if (chatMessages.Status == 200)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
                await Clients.Group($"Chat_{chatId}").UserJoinedChat(new
                {
                    UserId = userId,
                    ChatId = chatId,
                    Timestamp = DateTime.UtcNow
                });
            }
            else
            {
                await Clients.Caller.Error("Access denied to this chat");
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.Error("Failed to join chat room");
        }
    }

    public async Task LeaveChatRoom(int chatId)
    {
        var userId = GetCurrentUserId();

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Chat_{chatId}");
        await Clients.Group($"Chat_{chatId}").UserLeftChat(new
        {
            UserId = userId,
            ChatId = chatId,
            Timestamp = DateTime.UtcNow
        });
    }

    public async Task SendMessage(int chatId, string content)
    {
        var userId = GetCurrentUserId();
        if (userId == 0)
        {
            await Clients.Caller.Error("Unauthorized access");
            return;
        }

        if (string.IsNullOrWhiteSpace(content) || content.Length > 1000)
        {
            await Clients.Caller.Error("Invalid message content");
            return;
        }

        try
        {
            var request = new SendMessageRequest
            {
                ChatId = chatId,
                Content = content.Trim()
            };

            var result = await _chatService.SendMessageAsync(userId, request);

            if (result.Status == 201)
            {
                await Clients.Group($"Chat_{chatId}").NewMessage(result.Data);
            }
            else
            {
                await Clients.Caller.Error(result.Message);
            }
        }
        catch (Exception ex)
        {
            await Clients.Caller.Error("Failed to send message");
        }
    }

    public async Task MarkMessagesAsRead(int chatId)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return;

            var result = await _chatService.MarkMessagesAsReadAsync(userId, chatId);

            if (result.Status == 200)
            {
                await Clients.Group($"Chat_{chatId}").MessagesMarkedAsRead(new
                {
                    ChatId = chatId,
                    ReadByUserId = userId,
                    Timestamp = DateTime.UtcNow
                });
            }
    }

    public async Task SendTypingIndicator(int chatId, bool isTyping)
    {
        var userId = GetCurrentUserId();
        if (userId == 0) return;

        await Clients.GroupExcept($"Chat_{chatId}", Context.ConnectionId).TypingIndicator(new
        {
            ChatId = chatId,
            UserId = userId,
            IsTyping = isTyping,
            Timestamp = DateTime.UtcNow
        });
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
            return userId;

        return 0;
    }
}
