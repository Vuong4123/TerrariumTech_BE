using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class MembershipService : IMembershipService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly IUserContextService userContextService;

    public MembershipService(IUserContextService userContextService, UnitOfWork unitOfWork)
    {
        this.userContextService = userContextService;
        _unitOfWork = unitOfWork;
    }

    public async Task<int> CreateMembershipAsync(int packageId, DateTime startDate)
    {
        var currentUserId = userContextService.GetCurrentUser();

        // Lấy gói membership từ PackageId
        var package = await _unitOfWork.MembershipPackageRepository.GetByIdAsync(packageId);
        if (package == null)
            throw new ArgumentException("Gói membership không tồn tại");

        // Kiểm tra nếu người dùng đã có membership active
        var activeMemberships = await _unitOfWork.MemberShipRepository.FindAsync(m =>
            m.UserId == currentUserId && m.Status == MembershipStatus.Active.ToString());

        if (activeMemberships.Any())
            throw new ArgumentException("Bạn đã có gói membership đang hoạt động.");

        // Tính ngày kết thúc
        var endDate = startDate.AddDays(package.DurationDays);

        var membership = new Membership
        {
            UserId = currentUserId,
            PackageId = package.Id, // Lưu PackageId thay vì MembershipType
            Price = package.Price, // Gán giá từ gói
            StartDate = startDate,
            EndDate = endDate,
            StatusEnum = MembershipStatus.Active
        };

        await _unitOfWork.MemberShipRepository.CreateAsync(membership);
        return membership.MembershipId;
    }

    public async Task<int> CreateMembershipForUserAsync(int userId, int packageId, DateTime startDate)
    {
        var package = await _unitOfWork.MembershipPackageRepository.GetByIdAsync(packageId);
        if (package == null)
            throw new ArgumentException("Gói membership không tồn tại.");

        var membership = new Membership
        {
            UserId = userId,
            PackageId = packageId,
            Price = package.Price,
            StartDate = startDate,
            EndDate = startDate.AddDays(package.DurationDays),
            StatusEnum = MembershipStatus.Active
        };

        await _unitOfWork.MemberShipRepository.CreateAsync(membership);
        return membership.MembershipId;
    }

    public async Task<List<Membership>> GetAllMembershipsAsync()
    {
        var role = userContextService.GetCurrentUserRole();
        if (role is not RoleStatus.Admin and not RoleStatus.Manager)
            throw new UnauthorizedAccessException("Bạn không có quyền xem tất cả memberships.");

        return await _unitOfWork.MemberShipRepository.GetAllAsync();
    }

    public async Task<Membership> GetMembershipByIdAsync(int membershipId)
    {
        var membership = await _unitOfWork.MemberShipRepository.GetByIdAsync(membershipId);
        if (membership == null) return null;

        if (!CanAccessUser(membership.UserId))
            throw new UnauthorizedAccessException("Bạn không có quyền truy cập membership này.");

        return membership;
    }

    public async Task<List<Membership>> GetMembershipsByUserIdAsync(int userId)
    {
        if (!CanAccessUser(userId))
            throw new UnauthorizedAccessException("Bạn không có quyền xem memberships của người dùng này.");

        var memberships = await _unitOfWork.MemberShipRepository.FindAsync(m => m.UserId == userId);
        var now = DateTime.UtcNow;

        foreach (var membership in memberships)
            if (membership.EndDate < now && membership.StatusEnum != MembershipStatus.Expired)
            {
                membership.StatusEnum = MembershipStatus.Expired;
                await _unitOfWork.MemberShipRepository.UpdateAsync(membership);
            }

        return memberships;
    }

    public async Task<bool> UpdateMembershipAsync(int membershipId, int packageId, DateTime startDate)
    {
        var membership = await _unitOfWork.MemberShipRepository.GetByIdAsync(membershipId);
        if (membership == null) return false;

        var package = await _unitOfWork.MembershipPackageRepository.GetByIdAsync(packageId);
        if (package == null)
            throw new ArgumentException("Gói membership không tồn tại.");

        membership.PackageId = package.Id;
        membership.Price = package.Price; // Cập nhật giá từ package
        membership.StartDate = startDate;
        membership.EndDate = startDate.AddDays(package.DurationDays); // Tính lại ngày kết thúc từ gói
        membership.StatusEnum = MembershipStatus.Active;

        await _unitOfWork.MemberShipRepository.UpdateAsync(membership);
        return true;
    }

    public async Task<bool> DeleteMembershipAsync(int membershipId)
    {
        var membership = await _unitOfWork.MemberShipRepository.GetByIdAsync(membershipId);
        if (membership == null) return false;

        if (!CanAccessUser(membership.UserId))
            throw new UnauthorizedAccessException("Bạn không có quyền xoá membership này.");

        await _unitOfWork.MemberShipRepository.RemoveAsync(membership);
        return true;
    }

    public async Task<int> UpdateAllExpiredMembershipsAsync()
    {
        var role = userContextService.GetCurrentUserRole();
        if (role is not RoleStatus.Admin and not RoleStatus.Manager)
            throw new UnauthorizedAccessException("Bạn không có quyền cập nhật tất cả memberships.");

        var allMemberships = await _unitOfWork.MemberShipRepository.GetAllAsync();
        var updatedCount = 0;
        var now = DateTime.UtcNow;

        foreach (var membership in allMemberships)
            if (membership.EndDate.HasValue && membership.EndDate.Value < now &&
                membership.StatusEnum != MembershipStatus.Expired)
            {
                membership.StatusEnum = MembershipStatus.Expired;
                await _unitOfWork.MemberShipRepository.UpdateAsync(membership);
                updatedCount++;
            }

        return updatedCount;
    }

    public async Task<int> UpdateExpiredMembershipsByUserIdAsync(int userId)
    {
        if (!CanAccessUser(userId))
            throw new UnauthorizedAccessException("Bạn không có quyền cập nhật memberships của người dùng này.");

        var memberships = await _unitOfWork.MemberShipRepository.FindAsync(m => m.UserId == userId);
        var updatedCount = 0;
        var now = DateTime.UtcNow;

        foreach (var membership in memberships)
            if (membership.EndDate.HasValue && membership.EndDate.Value < now &&
                membership.StatusEnum != MembershipStatus.Expired)
            {
                membership.StatusEnum = MembershipStatus.Expired;
                await _unitOfWork.MemberShipRepository.UpdateAsync(membership);
                updatedCount++;
            }

        return updatedCount;
    }

    public bool IsMembershipExpired(Membership membership)
    {
        return membership.EndDate.HasValue && membership.EndDate.Value < DateTime.UtcNow;
    }

    private bool CanAccessUser(int targetUserId)
    {
        var currentUserId = userContextService.GetCurrentUser();
        var role = userContextService.GetCurrentUserRole();
        return role is RoleStatus.Admin or RoleStatus.Manager or RoleStatus.Staff || currentUserId == targetUserId;
    }
}