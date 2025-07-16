using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Repositories.Repositories;

namespace TerrariumGardenTech.Service.Service
{
    public class MembershipPackageService : IMembershipPackageService
    {
        private readonly UnitOfWork _unitOfWork;

        public MembershipPackageService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MembershipPackage>> GetAllAsync()
            => await _unitOfWork.MembershipPackage.GetAllAsync();

        public async Task<MembershipPackage> GetByIdAsync(int id)
            => await _unitOfWork.MembershipPackage.GetByIdAsync(id);

        public async Task<int> CreateAsync(MembershipPackage package)
        {
            await _unitOfWork.MembershipPackage.CreateAsync(package);
            return package.Id;
        }

        public async Task<bool> UpdateAsync(MembershipPackage package)
        {
            var result = await _unitOfWork.MembershipPackage.UpdateAsync(package);
            return result > 0; // Return true if the update affected rows, otherwise false
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var pkg = await _unitOfWork.MembershipPackage.GetByIdAsync(id);
            if (pkg == null) return false;
            return await _unitOfWork.MembershipPackage.RemoveAsync(pkg);
        }
    }

}
