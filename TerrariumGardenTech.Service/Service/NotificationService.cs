using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Notification;
using TerrariumGardenTech.Service.ResponseModel.Notification;

namespace TerrariumGardenTech.Service.Service
{
    public class NotificationService : INotificationService
    {
        private readonly UnitOfWork _unitOfWork;

        public NotificationService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IBusinessResult> CreateNotificationAsync(NotificationCreateRequest notificationCreateRequest)
        {
            try
            {
                var user = await _unitOfWork.User.GetByIdAsync(notificationCreateRequest.UserId);
                if (user == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "User not found.");
                }

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
                {
                    return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, notification);
                }
                return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
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
            }).ToList();

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
    }
}
