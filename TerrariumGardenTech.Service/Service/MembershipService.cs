using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service
{
    public class MembershipService(IUserContextService userContextService,
    UnitOfWork _unitOfWork)
     : IMembershipService
    {

        // Create Membership
        public async Task<int> CreateMembershipAsync( string membershipType, DateTime startDate, DateTime endDate, string status)
        {
            var GetCurrentUser = userContextService.GetCurrentUser();
            var membership = new Membership
            {
                UserId = GetCurrentUser,
                MembershipType = membershipType,
                StartDate = startDate,
                EndDate = endDate,
                Status = status
            };

            await _unitOfWork.MemberShipRepository.CreateAsync(membership);
            return membership.MembershipId;
        }
        public async Task<List<Membership>> GetAllMembershipsAsync()
        {
            // Trả về tất cả các memberships trong bảng
            return await _unitOfWork.MemberShipRepository.GetAllAsync();
        }

        // Read Membership by ID
        public async Task<Membership> GetMembershipByIdAsync(int membershipId)
        {
            return await _unitOfWork.MemberShipRepository.GetByIdAsync(membershipId);
        }

        // Read all memberships for a specific user
        public async Task<List<Membership>> GetMembershipsByUserIdAsync(int userId)
        {
            await UpdateExpiredMembershipsByUserIdAsync(userId);
            return await _unitOfWork.MemberShipRepository.FindAsync(m => m.UserId == userId);
        }

        // Update Membership
        public async Task<bool> UpdateMembershipAsync(int membershipId, string membershipType, DateTime startDate, DateTime endDate, string status)
        {
            var membership = await _unitOfWork.MemberShipRepository.GetByIdAsync(membershipId);
            if (membership == null) return false;

            membership.MembershipType = membershipType;
            membership.StartDate = startDate;
            membership.EndDate = endDate;
            membership.Status = status;

            await _unitOfWork.MemberShipRepository.UpdateAsync(membership);
            return true;
        }
        //Test git
        // Delete Membership
        public async Task<bool> DeleteMembershipAsync(int membershipId)
        {
            var membership = await _unitOfWork.MemberShipRepository.GetByIdAsync(membershipId);
            if (membership == null) return false;

            await _unitOfWork.MemberShipRepository.RemoveAsync(membership);
            return true;
        }

        // Check and update expired memberships for all users
        public async Task<int> UpdateAllExpiredMembershipsAsync()
        {
            var allMemberships = await _unitOfWork.MemberShipRepository.GetAllAsync();
            int updatedCount = 0;
            var now = DateTime.UtcNow;

            foreach (var membership in allMemberships)
            {
                if (membership.EndDate.HasValue && membership.EndDate.Value < now && membership.Status != "Expired")
                {
                    membership.Status = "Expired";
                    await _unitOfWork.MemberShipRepository.UpdateAsync(membership);
                    updatedCount++;
                }
            }
            return updatedCount;
        }

        // Check and update expired memberships for a specific user
        public async Task<int> UpdateExpiredMembershipsByUserIdAsync(int userId)
        {
            var memberships = await _unitOfWork.MemberShipRepository.FindAsync(m => m.UserId == userId);
            int updatedCount = 0;
            var now = DateTime.UtcNow;

            foreach (var membership in memberships)
            {
                if (membership.EndDate.HasValue && membership.EndDate.Value < now && membership.Status != "Expired")
                {
                    membership.Status = "Expired";
                    await _unitOfWork.MemberShipRepository.UpdateAsync(membership);
                    updatedCount++;
                }
            }
            return updatedCount;
        }

        // Check if a membership is expired
        public bool IsMembershipExpired(Membership membership)
        {
            if (membership.EndDate.HasValue)
            {
                return membership.EndDate.Value < DateTime.UtcNow;
            }
            return false;
        }
    }
}
