using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Chat;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers;

/// <summary>
/// 1. GET /available-users - Lấy danh sách user có thể chat
/// 2. POST /create - Tạo chat mới
/// 3. GET /my-chats - Lấy danh sách chat của user
/// 4. GET /{chatId}/messages - Lấy tin nhắn trong chat
/// 5. POST /send-message - Gửi tin nhắn
/// 6. PUT /{chatId}/mark-read - Đánh dấu đã đọc
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    /// <summary>
    /// GET /api/chat/available-users
    /// Authorization: Bearer {jwt-token}
    ///
    /// Response:
    /// {
    ///   "status": 200,
    ///   "message": "Get data success",
    ///   "data": [
    ///     {
    ///       "userId": 5,
    ///       "username": "john_staff",
    ///       "fullName": "John Smith",
    ///       "hasExistingChat": false,
    ///       "existingChatId": null
    ///     }
    ///   ]
    /// }
    /// </example>
    [HttpGet("available-users")]
    public async Task<IActionResult> GetAvailableUsers()
    {
        // Extract user ID from JWT token
        var currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user token");

        // Get available users based on role hierarchy
        var result = await _chatService.GetAvailableUsersAsync(currentUserId);

        if (result.Status == Const.SUCCESS_READ_CODE)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// <example>
    /// POST /api/chat/create
    /// Authorization: Bearer {jwt-token}
    /// Content-Type: application/json
    ///
    /// {
    ///   "targetUserId": 5
    /// }
    ///
    /// Response:
    /// {
    ///   "status": 201,
    ///   "message": "Save data success",
    ///   "data": {
    ///     "chatId": 1,
    ///     "user1Name": "Current User",
    ///     "user2Name": "Target User",
    ///     "createdAt": "2024-01-15T10:30:00Z"
    ///   }
    /// }
    /// </example>
    [HttpPost("create")]
    public async Task<IActionResult> CreateChat([FromBody] CreateChatRequest request)
    {
        // Validate request model
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user token");

        var result = await _chatService.CreateChatAsync(currentUserId, request);

        if (result.Status == Const.SUCCESS_CREATE_CODE || result.Status == Const.SUCCESS_READ_CODE)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// <example>
    /// GET /api/chat/my-chats
    /// Authorization: Bearer {jwt-token}
    ///
    /// Response:
    /// {
    ///   "status": 200,
    ///   "message": "Get data success",
    ///   "data": [
    ///     {
    ///       "chatId": 1,
    ///       "user1Name": "Alice Manager",
    ///       "user2Name": "John Smith",
    ///       "lastMessage": {
    ///         "content": "Thanks for the update!",
    ///         "sentAt": "2024-01-15T14:25:00Z",
    ///         "isRead": false
    ///       }
    ///     }
    ///   ]
    /// }
    /// </example>
    [HttpGet("my-chats")]
    public async Task<IActionResult> GetMyChats()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user token");

        var result = await _chatService.GetUserChatsAsync(currentUserId);

        if (result.Status == Const.SUCCESS_READ_CODE)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// <example>
    /// GET /api/chat/1/messages?page=1&pageSize=20
    /// Authorization: Bearer {jwt-token}
    ///
    /// Response:
    /// {
    ///   "status": 200,
    ///   "message": "Get data success",
    ///   "data": [
    ///     {
    ///       "messageId": 14,
    ///       "senderId": 3,
    ///       "senderName": "Alice Manager",
    ///       "content": "Hello John, how's the project going?",
    ///       "sentAt": "2024-01-15T14:20:00Z",
    ///       "isRead": true
    ///     }
    ///   ]
    /// }
    /// </example>
    [HttpGet("{chatId}/messages")]
    public async Task<IActionResult> GetChatMessages(int chatId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user token");

        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 50;

        var result = await _chatService.GetChatMessagesAsync(currentUserId, chatId, page, pageSize);

        if (result.Status == Const.SUCCESS_READ_CODE)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// <example>
    /// POST /api/chat/send-message
    /// Authorization: Bearer {jwt-token}
    /// Content-Type: application/json
    ///
    /// {
    ///   "chatId": 1,
    ///   "content": "Hello! How can I help you today?"
    /// }
    ///
    /// Response:
    /// {
    ///   "status": 201,
    ///   "message": "Save data success",
    ///   "data": {
    ///     "messageId": 16,
    ///     "chatId": 1,
    ///     "senderId": 3,
    ///     "senderName": "Alice Manager",
    ///     "content": "Hello! How can I help you today?",
    ///     "sentAt": "2024-01-15T14:30:00Z",
    ///     "isRead": false
    ///   }
    /// }
    /// </example>
    [HttpPost("send-message")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user token");

        var result = await _chatService.SendMessageAsync(currentUserId, request);

        if (result.Status == Const.SUCCESS_CREATE_CODE)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// <returns>
    /// 200 OK: Messages marked as read successfully
    /// 400 Bad Request: Chat not found or access denied
    /// 401 Unauthorized: Invalid or missing JWT token
    /// </returns>
    /// <example>
    /// PUT /api/chat/1/mark-read
    /// Authorization: Bearer {jwt-token}
    ///
    /// Response:
    /// {
    ///   "status": 200,
    ///   "message": "Messages marked as read"
    /// }
    /// </example>
    [HttpPut("{chatId}/mark-read")]
    public async Task<IActionResult> MarkMessagesAsRead(int chatId)
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user token");

        var result = await _chatService.MarkMessagesAsReadAsync(currentUserId, chatId);

        if (result.Status == Const.SUCCESS_UPDATE_CODE)
            return Ok(result);

        return BadRequest(result);
    }

    /// <summary>
    /// DEBUG ENDPOINT - Remove this in production
    /// GET /api/chat/debug/user-info
    ///
    /// Helps debug user role and status issues by showing current user details
    /// </summary>
    [HttpGet("debug/user-info")]
    public async Task<IActionResult> GetCurrentUserDebugInfo()
    {
        var currentUserId = GetCurrentUserId();
        if (currentUserId == 0)
            return Unauthorized("Invalid user token");

        try
        {
            var result = await _chatService.GetAvailableUsersAsync(currentUserId);

            return Ok(new
            {
                CurrentUserId = currentUserId,
                AvailableUsersResult = result,
                JwtClaims = User.Claims.Select(c => new { c.Type, c.Value }).ToList(),
                Message = "Debug info for troubleshooting available users issue"
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"Debug error: {ex.Message}");
        }
    }

    /// <summary>
    /// Helper method to extract current user ID from JWT token
    ///
    /// Looks for NameIdentifier claim in the JWT token which contains the user ID
    /// This is set during authentication when the JWT token is created
    /// </summary>
    /// <returns>
    /// User ID if found and valid, 0 if not found or invalid
    /// </returns>
    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
            return userId;

        return 0;
    }
}
