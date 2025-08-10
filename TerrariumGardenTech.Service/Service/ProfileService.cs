using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Voucher;
using TerrariumGardenTech.Common.ResponseModel.User;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service
{
    public class ProfileService : IProfileService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(UnitOfWork unitOfWork, ICloudinaryService cloudinaryService, ILogger<ProfileService> logger)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
            _logger = logger;
        }

        public async Task<IBusinessResult> GetMyProfileAsync(int userId)
        {
            var user = await _unitOfWork.User.FindOneAsync(u => u.UserId == userId);
            if (user == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Không tìm thấy người dùng");
            var userProfile = new UserProfileResponse
            {
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                BackgroundUrl = user.BackgroundUrl
            };


            return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy profile thành công", userProfile);
        }

        public async Task<IBusinessResult> EditMyProfileAsync(int userId, EditUserProfileRequest req)
        {
            try
            {
                var user = await _unitOfWork.User.FindOneAsync(u => u.UserId == userId, true);
                if (user == null)
                    return new BusinessResult(Const.FAIL_READ_CODE, "Không tìm thấy người dùng");

                // ===== Validate cơ bản =====
                if (req.PhoneNumber is { Length: > 20 })
                    return new BusinessResult(Const.FAIL_UPDATE_CODE, "Số điện thoại quá dài");

                if (req.DateOfBirth.HasValue && req.DateOfBirth.Value > DateTime.UtcNow.Date)
                    return new BusinessResult(Const.FAIL_UPDATE_CODE, "Ngày sinh không hợp lệ");

                if (!string.IsNullOrWhiteSpace(req.Email))
                {
                    // Validate định dạng email đơn giản
                    try
                    {
                        var _ = new System.Net.Mail.MailAddress(req.Email);
                    }
                    catch
                    {
                        return new BusinessResult(Const.FAIL_UPDATE_CODE, "Email không hợp lệ");
                    }

                    // Nếu đổi email, kiểm tra trùng với user khác
                    if (!string.Equals(user.Email, req.Email, StringComparison.OrdinalIgnoreCase))
                    {
                        var emailOwner = await _unitOfWork.User.FindOneAsync(
                            u => u.Email == req.Email, false);
                        if (emailOwner != null && emailOwner.UserId != userId)
                            return new BusinessResult(Const.FAIL_UPDATE_CODE, "Email đã được sử dụng");
                    }
                }

                if (!string.IsNullOrWhiteSpace(req.Gender))
                {
                    // Chuẩn hoá và giới hạn giá trị (ví dụ)
                    var g = req.Gender.Trim().ToLowerInvariant();
                    var allowed = new[] { "male", "female", "other" };
                    if (!allowed.Contains(g))
                        return new BusinessResult(Const.FAIL_UPDATE_CODE, "Giới tính không hợp lệ (male/female/other)");
                    user.Gender = g; // lưu theo lowercase hoặc tuỳ chuẩn của bạn
                }

                // ===== Patch các field có giá trị =====
                if (!string.IsNullOrWhiteSpace(req.FullName)) user.FullName = req.FullName.Trim();
                if (!string.IsNullOrWhiteSpace(req.PhoneNumber)) user.PhoneNumber = req.PhoneNumber.Trim();
                if (req.DateOfBirth.HasValue) user.DateOfBirth = req.DateOfBirth;
                if (!string.IsNullOrWhiteSpace(req.Email)) user.Email = req.Email.Trim();

                user.UpdatedAt = DateTime.UtcNow;
                await _unitOfWork.User.UpdateAsync(user);

                return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Cập nhật profile thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi cập nhật profile");
                return new BusinessResult(Const.ERROR_EXCEPTION, "Lỗi hệ thống");
            }
        }

        public async Task<IBusinessResult> UploadAvatarAsync(int userId, IFormFile file)
        {
            var upload = await _cloudinaryService.UploadImageAsync(file, "avatars", $"avatar_{userId}");
            if (upload.Status != Const.SUCCESS_CREATE_CODE) return upload;

            var user = await _unitOfWork.User.FindOneAsync(u => u.UserId == userId, true);
            if (user == null) return new BusinessResult(Const.FAIL_READ_CODE, "Không tìm thấy người dùng");

            user.AvatarUrl = upload?.Data?.ToString();
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.User.UpdateAsync(user);

            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Cập nhật avatar thành công", user.AvatarUrl);
        }

        public async Task<IBusinessResult> UploadBackgroundAsync(int userId, IFormFile file)
        {
            var upload = await _cloudinaryService.UploadImageAsync(file, "backgrounds", $"bg_{userId}");
            if (upload.Status != Const.SUCCESS_CREATE_CODE) return upload;

            var user = await _unitOfWork.User.FindOneAsync(u => u.UserId == userId, true);
            if (user == null) return new BusinessResult(Const.FAIL_READ_CODE, "Không tìm thấy người dùng");

            user.BackgroundUrl = upload?.Data?.ToString();
            user.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.User.UpdateAsync(user);

            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Cập nhật background thành công", user.BackgroundUrl);
        }

        public async Task<IBusinessResult> AdminGetAllProfilesAsync()
        {
            var data = await _unitOfWork.User.Context().Users
                                            .AsNoTracking()
                                            .OrderByDescending(u => u.UpdatedAt ?? u.CreatedAt)
                                            .Select(u => new UserProfileDto
                                            {
                                                UserId = u.UserId,
                                                FullName = u.FullName,
                                                Gender = u.Gender,
                                                PhoneNumber = u.PhoneNumber,
                                                DateOfBirth = u.DateOfBirth,
                                                AvatarUrl = u.AvatarUrl,
                                                BackgroundUrl = u.BackgroundUrl,
                                                Email = u.Email
                                            })
                                            .ToListAsync();

            if (data == null || data.Count == 0)
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy tất cả profiles", data);
        }

        public async Task<IBusinessResult> AdminGetProfileByUserIdAsync(int userId)
        {
            var dto = await _unitOfWork.User.Context().Users
                                            .AsNoTracking()
                                            .Where(u => u.UserId == userId)
                                            .Select(u => new UserProfileDto
                                            {
                                                UserId = u.UserId,
                                                FullName = u.FullName,
                                                Gender = u.Gender,
                                                PhoneNumber = u.PhoneNumber,
                                                DateOfBirth = u.DateOfBirth,
                                                AvatarUrl = u.AvatarUrl,
                                                BackgroundUrl = u.BackgroundUrl,
                                                Email = u.Email
                                            })
                                            .FirstOrDefaultAsync();

            if (dto == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Không tìm thấy người dùng");

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy profile thành công", dto);
        }
    }
}
