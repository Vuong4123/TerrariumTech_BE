using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Repositories.Enums;
using TerrariumGardenTech.Service.Base;
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

        public async Task<IBusinessResult> CreateAccountAsync(AccountCreateRequest request)
        {
            var existingUser = await _userRepository.FindOneAsync(u => u.Username == request.Username || u.Email == request.Email);
            if (existingUser != null)
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Username hoặc Email đã tồn tại");

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
                RoleId = (int)RoleStatus.User,
                Status = AccountStatus.Active.ToString(),
                CreatedAt = DateTime.UtcNow
            };

            await _userRepository.CreateAsync(newUser);
            return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, newUser);
        }

        public async Task<IBusinessResult> GetAllAccountsAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, users);
        }

        public async Task<IBusinessResult> GetAccountsByRoleAsync(string roleName, int page, int pageSize)
        {
            if (!Enum.TryParse<RoleStatus>(roleName, out var roleEnum))
                return new BusinessResult(Const.FAIL_READ_CODE, "Vai trò không hợp lệ");

            var query = _userRepository.Context().Users
                .Where(u => u.RoleId == (int)roleEnum && u.Status == AccountStatus.Active.ToString());

            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, users);
        }

        public async Task<IBusinessResult> ChangeAccountStatusAsync(int userId, string status)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Tài khoản không tồn tại");

            if (!Enum.TryParse<AccountStatus>(status, out var parsedStatus))
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Trạng thái không hợp lệ");

            user.Status = parsedStatus.ToString();
            await _userRepository.UpdateAsync(user);
            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, user);
        }

        public async Task<IBusinessResult> GetAccountsAsync(int page, int pageSize)
        {
            var users = await _userRepository.Context().Users
                .Where(u => (u.RoleId == (int)RoleStatus.User || u.RoleId == (int)RoleStatus.Admin) &&
                            u.Status == AccountStatus.Active.ToString())
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, users);
        }

        public async Task<IBusinessResult> GetAccountByIdAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Tài khoản không tồn tại");

            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, user);
        }

        public async Task<IBusinessResult> UpdateAccountAsync(int userId, AccountUpdateRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Tài khoản không tồn tại");

            if (!Enum.IsDefined(typeof(RoleStatus), request.RoleId))
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Vai trò không hợp lệ");

            user.Email = request.Email;
            user.FullName = request.FullName;
            user.PhoneNumber = request.PhoneNumber;
            user.DateOfBirth = request.DateOfBirth;
            user.Gender = request.Gender;
            user.RoleId = request.RoleId;

            if (!string.IsNullOrWhiteSpace(request.Password))
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            await _userRepository.UpdateAsync(user);
            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, user);
        }

        public async Task<IBusinessResult> DeleteAccountAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return new BusinessResult(Const.FAIL_DELETE_CODE, "Tài khoản không tồn tại");

            user.Status = AccountStatus.Suspended.ToString(); // Soft delete
            await _userRepository.UpdateAsync(user);
            return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG, user);
        }
    }
}
