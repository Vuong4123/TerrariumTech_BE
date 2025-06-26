using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Repositories.Enums;

namespace TerrariumGardenTech.Service.IService
{
    public interface IMembershipService
    {
        //Task<int> CreateMembershipAsync(int userId, MembetShipType membershipType, DateTime startDate, DateTime endDate, string status);
        Task<Membership> GetMembershipByIdAsync(int membershipId);
        //Task<List<Membership>> GetMembershipsByUserIdAsync(int userId);
        //Task<bool> UpdateMembershipAsync(int membershipId, MembetShipType membershipType, DateTime startDate, DateTime endDate, string status);
        Task<bool> DeleteMembershipAsync(int membershipId);
    }
}
