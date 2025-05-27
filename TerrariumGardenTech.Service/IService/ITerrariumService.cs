using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService
{
    public interface ITerrariumService
    {
        Task<IBusinessResult> GetAll();
        Task<IBusinessResult> GetById(int id);
        Task<IBusinessResult> CreateTerrarium();
        Task<IBusinessResult> UpdateTerrarium();
        Task<IBusinessResult> Save(Terrarium terrarium);
        Task<IBusinessResult> DeleteById(int id);
    }
}
