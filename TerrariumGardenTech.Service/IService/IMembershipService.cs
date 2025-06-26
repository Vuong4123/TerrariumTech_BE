using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Service.IService
{
    public interface IMembershipService
    {
        Task<int> CreateMembershipAsync(int userId, string membershipType, DateTime startDate, DateTime endDate, string status);
        Task<Membership> GetMembershipByIdAsync(int membershipId);
        Task<List<Membership>> GetMembershipsByUserIdAsync(int userId);
        Task<bool> UpdateMembershipAsync(int membershipId, string membershipType, DateTime startDate, DateTime endDate, string status);
        Task<bool> DeleteMembershipAsync(int membershipId);
    }
}
