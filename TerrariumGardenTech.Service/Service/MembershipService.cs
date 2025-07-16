//using TerrariumGardenTech.Repositories;
//using TerrariumGardenTech.Repositories.Base;
//using TerrariumGardenTech.Repositories.Entity;
//using TerrariumGardenTech.Repositories.Enums;
//using TerrariumGardenTech.Service.IService;

//namespace TerrariumGardenTech.Service.Service
//{
//    public class MembershipService(IUserContextService userContextService, UnitOfWork _unitOfWork)
//        : IMembershipService
//    {
//        private static readonly Dictionary<string, int> MembershipDurations = new()
//        {
//            {"1Month", 30},
//            {"3Months", 90},
//            {"1Year", 365}
//        };

//        private int GetDurationDays(string membershipType)
//        {
//            if (!MembershipDurations.TryGetValue(membershipType, out int days))
//                throw new ArgumentException("Loại gói Membership không hợp lệ");
//            return days;
//        }

//        private bool CanAccessUser(int targetUserId)
//        {
//            var currentUserId = userContextService.GetCurrentUser();
//            var role = userContextService.GetCurrentUserRole();
//            return role is RoleStatus.Admin or RoleStatus.Manager or RoleStatus.Staff || currentUserId == targetUserId;
//        }

//        public async Task<int> CreateMembershipAsync(int packageId, DateTime startDate)
//        {
//            try
//            {
//                var currentUserId = userContextService.GetCurrentUser();

//                // Lấy gói membership từ packageId
//                var package = await _unitOfWork.MembershipPackageRepository.GetByIdAsync(packageId);
//                if (package == null)
//                    throw new ArgumentException("Gói membership không tồn tại");

//                // Kiểm tra nếu người dùng đã có gói membership active
//                var activeMemberships = await _unitOfWork.MemberShipRepository.FindAsync(m =>
//                    m.UserId == currentUserId && m.Status == MembershipStatus.Active.ToString());

//                if (activeMemberships.Any())
//                    throw new ArgumentException("Bạn đã có gói membership đang hoạt động.");

//                // Tính ngày kết thúc
//                var endDate = startDate.AddDays(package.DurationDays);

//                var membership = new Membership
//                {
//                    UserId = currentUserId,
//                    PackageId = package.Id,
//                    Price = package.Price,  // Gán giá từ gói
//                    StartDate = startDate,
//                    EndDate = endDate,
//                    StatusEnum = MembershipStatus.Active
//                };

//                // Lưu membership vào database
//                await _unitOfWork.MemberShipRepository.CreateAsync(membership);
//                return membership.MembershipId;
//            }
//            catch (ArgumentException ex)
//            {
//                // Log lỗi và ném lại exception
//                Console.WriteLine($"[CreateMembershipAsync] Validation error: {ex.Message}");
//                throw;
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"[CreateMembershipAsync] Exception: {ex.Message}");
//                throw new Exception("Đã xảy ra lỗi khi tạo membership", ex);
//            }
//        }





//        public async Task<List<Membership>> GetAllMembershipsAsync()
//        {
//            var role = userContextService.GetCurrentUserRole();
//            if (role is not RoleStatus.Admin and not RoleStatus.Manager)
//                throw new UnauthorizedAccessException("Bạn không có quyền xem tất cả memberships.");

//            return await _unitOfWork.MemberShipRepository.GetAllAsync();
//        }

//        public async Task<Membership> GetMembershipByIdAsync(int membershipId)
//        {
//            var membership = await _unitOfWork.MemberShipRepository.GetByIdAsync(membershipId);
//            if (membership == null) return null;

//            if (!CanAccessUser(membership.UserId))
//                throw new UnauthorizedAccessException("Bạn không có quyền truy cập membership này.");

//            return membership;
//        }

//        public async Task<List<Membership>> GetMembershipsByUserIdAsync(int userId)
//        {
//            if (!CanAccessUser(userId))
//                throw new UnauthorizedAccessException("Bạn không có quyền xem memberships của người dùng này.");

//            var memberships = await _unitOfWork.MemberShipRepository.FindAsync(m => m.UserId == userId);
//            var now = DateTime.UtcNow;

//            foreach (var membership in memberships)
//            {
//                if (membership.EndDate < now && membership.StatusEnum != MembershipStatus.Expired)
//                {
//                    membership.StatusEnum = MembershipStatus.Expired;
//                    await _unitOfWork.MemberShipRepository.UpdateAsync(membership);
//                }
//            }

//            return memberships;
//        }

//        public async Task<bool> UpdateMembershipAsync(int membershipId, string membershipType, DateTime startDate)
//        {
//            var membership = await _unitOfWork.MemberShipRepository.GetByIdAsync(membershipId);
//            if (membership == null) return false;

//            if (!CanAccessUser(membership.UserId))
//                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật membership này.");

//            membership.Price = 0; // Giá sẽ được cập nhật sau khi thanh toán
//            membership.StartDate = startDate;
//            membership.EndDate = startDate.AddDays(GetDurationDays(membershipType));
//            membership.StatusEnum = MembershipStatus.Active;

//            await _unitOfWork.MemberShipRepository.UpdateAsync(membership);
//            return true;
//        }

//        public async Task<bool> DeleteMembershipAsync(int membershipId)
//        {
//            var membership = await _unitOfWork.MemberShipRepository.GetByIdAsync(membershipId);
//            if (membership == null) return false;

//            if (!CanAccessUser(membership.UserId))
//                throw new UnauthorizedAccessException("Bạn không có quyền xoá membership này.");

//            await _unitOfWork.MemberShipRepository.RemoveAsync(membership);
//            return true;
//        }

//        public async Task<int> UpdateAllExpiredMembershipsAsync()
//        {
//            var role = userContextService.GetCurrentUserRole();
//            if (role is not RoleStatus.Admin and not RoleStatus.Manager)
//                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật tất cả memberships.");

//            var allMemberships = await _unitOfWork.MemberShipRepository.GetAllAsync();
//            int updatedCount = 0;
//            var now = DateTime.UtcNow;

//            foreach (var membership in allMemberships)
//            {
//                if (membership.EndDate.HasValue && membership.EndDate.Value < now && membership.StatusEnum != MembershipStatus.Expired)
//                {
//                    membership.StatusEnum = MembershipStatus.Expired;
//                    await _unitOfWork.MemberShipRepository.UpdateAsync(membership);
//                    updatedCount++;
//                }
//            }

//            return updatedCount;
//        }

//        public async Task<int> UpdateExpiredMembershipsByUserIdAsync(int userId)
//        {
//            if (!CanAccessUser(userId))
//                throw new UnauthorizedAccessException("Bạn không có quyền cập nhật memberships của người dùng này.");

//            var memberships = await _unitOfWork.MemberShipRepository.FindAsync(m => m.UserId == userId);
//            int updatedCount = 0;
//            var now = DateTime.UtcNow;

//            foreach (var membership in memberships)
//            {
//                if (membership.EndDate.HasValue && membership.EndDate.Value < now && membership.StatusEnum != MembershipStatus.Expired)
//                {
//                    membership.StatusEnum = MembershipStatus.Expired;
//                    await _unitOfWork.MemberShipRepository.UpdateAsync(membership);
//                    updatedCount++;
//                }
//            }

//            return updatedCount;
//        }

//        public bool IsMembershipExpired(Membership membership)
//        {
//            return membership.EndDate.HasValue && membership.EndDate.Value < DateTime.UtcNow;
//        }
//    }
//}
