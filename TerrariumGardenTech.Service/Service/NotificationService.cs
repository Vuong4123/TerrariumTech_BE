using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Notification;
using TerrariumGardenTech.Common.ResponseModel.Notification;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class NotificationService : INotificationService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly IFirebasePushService _pushService;
    private readonly IFirebaseStorageService _storageService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(UnitOfWork unitOfWork, IFirebasePushService pushService, IFirebaseStorageService storageService, ILogger<NotificationService> logger)
    {
        _unitOfWork = unitOfWork;
        _pushService = pushService;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<IBusinessResult> CreateNotificationAsync(NotificationCreateRequest notificationCreateRequest)
    {
        try
        {
            var user = await _unitOfWork.User.GetByIdAsync(notificationCreateRequest.UserId);
            if (user == null) return new BusinessResult(Const.FAIL_CREATE_CODE, "User not found.");

            var notification = new Notification
            {
                UserId = notificationCreateRequest.UserId,
                Title = notificationCreateRequest.Title,
                Message = notificationCreateRequest.Message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _unitOfWork.Notification.CreateAsync(notification);
            if (result > 0)
                return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, notification);
            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> CreateAndPushAsync(NotificationCreateRequest req)
    {
        _logger.LogInformation("Start sending notification...");

        var dbResult = await CreateNotificationAsync(req);
        if (dbResult.Status != Const.SUCCESS_CREATE_CODE)
        {
            _logger.LogError("Notification creation failed.");
            return dbResult;
        }

        var tokens = await _storageService.GetUserFcmTokensAsync(req.UserId.ToString());
        if (tokens == null || !tokens.Any())
        {
            _logger.LogWarning("No FCM tokens found.");
            return new BusinessResult(Const.FAIL_READ_CODE, "No FCM tokens found.");
        }

        _logger.LogInformation($"Found {tokens.Count} FCM tokens.");

        foreach (var token in tokens)
        {
            _logger.LogInformation($"Sending notification to token: {token}");

            var payload = new Dictionary<string, string>
            {
                ["notificationId"] = ((Notification)dbResult.Data!).NotificationId.ToString()
            };
            var readOnlyPayload = new ReadOnlyDictionary<string, string>(payload);

            _logger.LogInformation($"Notification Title: {req.Title}, Body: {req.Message}");

            var messageId = await _pushService.SendNotificationAsync(
                token,
                req.Title,
                req.Message,
                readOnlyPayload);

            _logger.LogInformation($"Notification sent, Message ID: {messageId}");
        }

        return dbResult;
    }

    public async Task<IBusinessResult> GetAllNotificationAsync()
    {
        var notifications = await _unitOfWork.Notification.GetAllAsync();
        if (notifications == null || !notifications.Any())
            return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);

        var notificationResponses = notifications.Select(n => new NotificationResponse
        {
            NotificationId = n.NotificationId,
            UserId = n.UserId,
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).OrderByDescending(c => c.CreatedAt).ToList();

        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, notificationResponses);
    }

    public async Task<IBusinessResult?> GetNotificationByIdAsync(int id)
    {
        var notification = await _unitOfWork.Notification.GetByIdAsync(id);
        if (notification == null)
            return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);

        var notificationResponse = new NotificationResponse
        {
            NotificationId = notification.NotificationId,
            UserId = notification.UserId,
            Title = notification.Title,
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };

        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, notificationResponse);
    }

    public async Task<IBusinessResult> GetNotificationByUserIdAsync(int userId)
    {
        var notifications = await _unitOfWork.Notification.FindAsync(n => n.UserId == userId);
        if (notifications == null || !notifications.Any())
            return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);

        var notificationResponses = notifications.Select(n => new NotificationResponse
        {
            NotificationId = n.NotificationId,
            UserId = n.UserId,
            Title = n.Title,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).OrderByDescending(c => c.CreatedAt).ToList();

        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, notificationResponses);
    }

    public async Task<IBusinessResult> MarkNotificationAsReadAsync(int id)
    {
        var notification = await _unitOfWork.Notification.GetByIdAsync(id);
        if (notification == null)
            return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG, "Notification not found.");

        notification.IsRead = true;
        var result = await _unitOfWork.Notification.UpdateAsync(notification);

        if (result > 0)
            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Notification marked as read successfully.");

        return new BusinessResult(Const.FAIL_UPDATE_CODE, "Failed to mark notification as read.");
    }

    public async Task<IBusinessResult> DeleteNotificationAsync(int id)
    {
        var notification = await _unitOfWork.Notification.GetByIdAsync(id);
        if (notification == null)
            return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG, "Notification not found.");

        var deleteResult = await _unitOfWork.Notification.RemoveAsync(notification);
        if (deleteResult)
            return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG);
        return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);
    }

    #region web notification
    public async Task<IBusinessResult> CreateWebNotificationAsync(WebNotificationCreateRequest request)
    {
        try
        {
            if (request.BroadcastToAll)
            {
                // Gửi cho tất cả user
                var allUsers = await _unitOfWork.User.GetAllAsync();
                var notifications = new List<Notification>();

                foreach (var user in allUsers)
                {
                    var notification = new Notification
                    {
                        UserId = user.UserId,
                        Title = request.Title,
                        Message = request.Description,
                        IsRead = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    notifications.Add(notification);
                }

                await _unitOfWork.Notification.CreateRangeAsync(notifications);
                await _unitOfWork.SaveAsync();

                return new BusinessResult(Const.SUCCESS_CREATE_CODE,
                    $"Broadcast notification sent to {notifications.Count} users",
                    notifications.Count);
            }
            else
            {
                // Gửi cho user cụ thể
                var user = await _unitOfWork.User.GetByIdAsync(request.UserId);
                if (user == null)
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "User not found.");

                var notification = new Notification
                {
                    UserId = request.UserId,
                    Title = request.Title,
                    Message = request.Description,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _unitOfWork.Notification.CreateAsync(notification);
                await _unitOfWork.SaveAsync();

                if (result > 0)
                    return new BusinessResult(Const.SUCCESS_CREATE_CODE,
                        "Web notification created successfully", notification);

                return new BusinessResult(Const.FAIL_CREATE_CODE, "Failed to create notification");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating web notification");
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> BroadcastNotificationAsync(BroadcastNotificationRequest request)
    {
        try
        {
            List<User> targetUsers;

            if (request.UserIds != null && request.UserIds.Any())
            {
                // Gửi cho danh sách user cụ thể
                targetUsers = await _unitOfWork.User.FindAsync(u => request.UserIds.Contains(u.UserId));
            }
            else
            {
                // Gửi cho tất cả user
                targetUsers = await _unitOfWork.User.GetAllAsync();
            }

            if (!targetUsers.Any())
                return new BusinessResult(Const.FAIL_CREATE_CODE, "No target users found");

            var notifications = targetUsers.Select(user => new Notification
            {
                UserId = user.UserId,
                Title = request.Title,
                Message = request.Description,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            }).ToList();

            await _unitOfWork.Notification.CreateRangeAsync(notifications);
            await _unitOfWork.SaveAsync();

            return new BusinessResult(Const.SUCCESS_CREATE_CODE,
                $"Broadcast notification sent to {notifications.Count} users",
                notifications.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting notification");
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> GetNotificationsByUserIdAsync(int userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var notifications = await _unitOfWork.Notification
                .FindAsync(n => n.UserId == userId);

            var totalCount = notifications.Count();
            var pagedNotifications = notifications
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationResponse
                {
                    NotificationId = n.NotificationId,
                    UserId = n.UserId,
                    Title = n.Title,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt
                }).ToList();

            var result = new
            {
                Notifications = pagedNotifications,
                TotalCount = totalCount,
                CurrentPage = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
            };

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Get notifications successfully", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting notifications by user ID");
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }
    #endregion
}