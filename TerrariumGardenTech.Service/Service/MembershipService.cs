using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service
{
    public class MembershipService : IMembershipService
    {
        private readonly GenericRepository<Membership> _membershipRepository;

        public MembershipService(GenericRepository<Membership> membershipRepository)
        {
            _membershipRepository = membershipRepository;
        }

        // Create Membership
        public async Task<int> CreateMembershipAsync(int userId, string membershipType, DateTime startDate, DateTime endDate, string status)
        {
            var membership = new Membership
            {
                UserId = userId,
                MembershipType = membershipType,
                StartDate = startDate,
                EndDate = endDate,
                Status = status
            };

            await _membershipRepository.CreateAsync(membership);
            return membership.MembershipId;
        }

        // Read Membership by ID
        public async Task<Membership> GetMembershipByIdAsync(int membershipId)
        {
            return await _membershipRepository.GetByIdAsync(membershipId);
        }

        // Read all memberships for a specific user
        public async Task<List<Membership>> GetMembershipsByUserIdAsync(int userId)
        {
            return await _membershipRepository.FindAsync(m => m.UserId == userId);
        }


        // Update Membership
        public async Task<bool> UpdateMembershipAsync(int membershipId, string membershipType, DateTime startDate, DateTime endDate, string status)
        {
            var membership = await _membershipRepository.GetByIdAsync(membershipId);
            if (membership == null) return false;

            membership.MembershipType = membershipType;
            membership.StartDate = startDate;
            membership.EndDate = endDate;
            membership.Status = status;

            await _membershipRepository.UpdateAsync(membership);
            return true;
        }

        // Delete Membership
        public async Task<bool> DeleteMembershipAsync(int membershipId)
        {
            var membership = await _membershipRepository.GetByIdAsync(membershipId);
            if (membership == null) return false;

            await _membershipRepository.RemoveAsync(membership);
            return true;
        }
    }
}
