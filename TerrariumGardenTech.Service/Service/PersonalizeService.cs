using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Personalize;

namespace TerrariumGardenTech.Service.Service
{
    public class PersonalizeService : IPersonalizeService
    {
        private readonly UnitOfWork _unitOfWork;
        public PersonalizeService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<IBusinessResult> GetAllPersonalize()
        {
            throw new NotImplementedException();
        }

        public Task<IBusinessResult> GetPersonalizeById(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IBusinessResult> SavePersonalize(Personalize personalize)
        {
            throw new NotImplementedException();
        }
        public Task<IBusinessResult> CreatePersonalize(PersonalizeCreateRequest personalizeCreateRequest)
        {
            throw new NotImplementedException();
        }

        public Task<IBusinessResult> UpdatePersonalize(PersonalizeUpdateRequest personalizeUpdateRequest)
        {
            throw new NotImplementedException();
        }

        public Task<IBusinessResult> DeletePersonalizeById(int id)
        {
            throw new NotImplementedException();
        }
    }
}
