using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.UserManagement;

namespace TerrariumGardenTech.Service.Service
{
    public class AccountService : IAccountService
    {
        private readonly GenericRepository<User> _userRepository;

        public AccountService(GenericRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

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
                RoleId = request.RoleId,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(newUser);
            return (Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG);
        }

        public async Task<(int, string, List<User>)> GetAccountsAsync(int page, int pageSize)
        {
            var query = _userRepository.Context().Users
                .Where(u => u.RoleId == 2 || u.RoleId == 3)
                .Where(u => u.Status == "Active");

            var totalCount = await query.CountAsync();
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, users);
        }

        public async Task<(int, string, User)> GetAccountByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || (user.RoleId != 2 && user.RoleId != 3))
                return (Const.FAIL_READ_CODE, "Tài khoản không tồn tại", null);

            return (Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, user);
        }

        public async Task<(int, string)> UpdateAccountAsync(int userId, AccountUpdateRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || (user.RoleId != 2 && user.RoleId != 3))
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

        public async Task<(int, string)> DeleteAccountAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || (user.RoleId != 2 && user.RoleId != 3))
                return (Const.FAIL_DELETE_CODE, "Tài khoản không tồn tại");

            user.Status = "Inactive"; // Soft delete
            await _userRepository.UpdateAsync(user);
            return (Const.SUCCESS_DELETE_CODE, "Tài khoản đã được vô hiệu hóa");
        }
    }

}
