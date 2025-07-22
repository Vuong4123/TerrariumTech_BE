using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class MembershipPackageService : IMembershipPackageService
{
    private readonly UnitOfWork _unitOfWork;

    public MembershipPackageService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<MembershipPackage>> GetAllAsync()
    {
        return await _unitOfWork.MembershipPackageRepository.GetAllAsync();
    }

    public async Task<MembershipPackage> GetByIdAsync(int id)
    {
        return await _unitOfWork.MembershipPackageRepository.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(MembershipPackage package)
    {
        await _unitOfWork.MembershipPackageRepository.CreateAsync(package);
        return package.Id;
    }

    public async Task<bool> UpdateAsync(MembershipPackage package)
    {
        var result = await _unitOfWork.MembershipPackageRepository.UpdateAsync(package);
        return result > 0; // Return true if the update affected rows, otherwise false
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var pkg = await _unitOfWork.MembershipPackageRepository.GetByIdAsync(id);
        if (pkg == null) return false;
        return await _unitOfWork.MembershipPackageRepository.RemoveAsync(pkg);
    }
}