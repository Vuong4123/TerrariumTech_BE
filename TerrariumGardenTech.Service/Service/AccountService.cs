using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.UserManagement;
using TerrariumGardenTech.Repositories.Enums;

namespace TerrariumGardenTech.Service.Service
{
    public class AccountService : IAccountService
    {
        private readonly GenericRepository<User> _userRepository;

        public AccountService(GenericRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        // Tạo tài khoản mới
        public async Task<(int, string)> CreateAccountAsync(AccountCreateRequest request)
        {
            var existingUser = await _userRepository.FindOneAsync(u => u.Username == request.Username || u.Email == request.Email, false);
            if (existingUser != null)
                return (Const.FAIL_CREATE_CODE, "Username hoặc Email đã tồn tại");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = hashedPassword,
                Email = request.Email,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                RoleId = (int)RoleStatus.User, // Sử dụng RoleStatus.User thay vì số
                Status = AccountStatus.Active.ToString(), // Sử dụng AccountStatus.Active
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(newUser);
            return (Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG);
        }

        // Lấy tất cả tài khoản
        public async Task<(int, string, List<User>)> GetAllAccountsAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return (Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, users);
        }

        // Lấy tài khoản theo vai trò
        public async Task<(int, string, List<User>)> GetAccountsByRoleAsync(string role, int page, int pageSize)
        {
            var query = _userRepository.Context().Users
                .Where(u => u.Role.RoleName == role && u.Status == AccountStatus.Active.ToString()); // Sử dụng AccountStatus.Active

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, users);
        }

        // Thay đổi trạng thái tài khoản
        public async Task<(int, string)> ChangeAccountStatusAsync(int userId, string status)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return (Const.FAIL_UPDATE_CODE, "Tài khoản không tồn tại");

            // Kiểm tra trạng thái với Enum
            if (Enum.TryParse<AccountStatus>(status, out var accountStatus))
            {
                user.Status = accountStatus.ToString();
                await _userRepository.UpdateAsync(user);
                return (Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG);
            }

            return (Const.FAIL_UPDATE_CODE, "Trạng thái không hợp lệ");
        }

        // Lấy tất cả tài khoản có vai trò là User hoặc Admin
        public async Task<(int, string, List<User>)> GetAccountsAsync(int page, int pageSize)
        {
            var query = _userRepository.Context().Users
                .Where(u => u.RoleId == (int)RoleStatus.User || u.RoleId == (int)RoleStatus.Admin)  // Kiểm tra vai trò bằng Enum
                .Where(u => u.Status == AccountStatus.Active.ToString()); // Kiểm tra trạng thái bằng Enum

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, users);
        }

        // Lấy tài khoản theo ID
        public async Task<(int, string, User)> GetAccountByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || (user.RoleId != (int)RoleStatus.User && user.RoleId != (int)RoleStatus.Admin))
                return (Const.FAIL_READ_CODE, "Tài khoản không tồn tại", null);

            return (Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, user);
        }

        // Cập nhật tài khoản
        public async Task<(int, string)> UpdateAccountAsync(int userId, AccountUpdateRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || (user.RoleId != (int)RoleStatus.User && user.RoleId != (int)RoleStatus.Admin))
                return (Const.FAIL_UPDATE_CODE, "Tài khoản không tồn tại");

            user.Email = request.Email;
            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            user.DateOfBirth = request.DateOfBirth;
            user.Gender = request.Gender;
            user.RoleId = request.RoleId;

            if (!string.IsNullOrEmpty(request.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            await _userRepository.UpdateAsync(user);
            return (Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG);
        }

        // Xóa tài khoản
        public async Task<(int, string)> DeleteAccountAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || (user.RoleId != (int)RoleStatus.User && user.RoleId != (int)RoleStatus.Admin))
                return (Const.FAIL_DELETE_CODE, "Tài khoản không tồn tại");

            user.Status = AccountStatus.Suspended.ToString(); // Soft delete, sử dụng Enum AccountStatus.Inactive
            await _userRepository.UpdateAsync(user);
            return (Const.SUCCESS_DELETE_CODE, "Tài khoản đã được vô hiệu hóa");
        }
    }
}
