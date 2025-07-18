using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Service.IService;

public interface IMembershipPackageService
{
    Task<List<MembershipPackage>> GetAllAsync();
    Task<MembershipPackage> GetByIdAsync(int id);
    Task<int> CreateAsync(MembershipPackage package);
    Task<bool> UpdateAsync(MembershipPackage package);
    Task<bool> DeleteAsync(int id);
}