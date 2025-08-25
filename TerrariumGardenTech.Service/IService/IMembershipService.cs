using TerrariumGardenTech.Common.ResponseModel.Order;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Service.IService;

public interface IMembershipService
{
    Task<int> CreateMembershipAsync(int packageId, DateTime startDate);
    Task<List<Membership>> GetAllMembershipsAsync();
    Task<Membership> GetMembershipByIdAsync(int membershipId);
    Task<List<Membership>> GetMembershipsByUserIdAsync(int userId);
    Task<bool> UpdateMembershipAsync(int membershipId, int packageId, DateTime startDate);
    Task<bool> DeleteMembershipAsync(int membershipId);
    Task<int> UpdateAllExpiredMembershipsAsync(); // Updates all expired memberships
    Task<int> UpdateExpiredMembershipsByUserIdAsync(int userId); // Updates expired memberships for a user
    bool IsMembershipExpired(Membership membership); // Checks if a membership is expired
    Task<MembershipCreationResult> CreateMembershipForUserAsync(int userId, int packageId, DateTime startDate);
}