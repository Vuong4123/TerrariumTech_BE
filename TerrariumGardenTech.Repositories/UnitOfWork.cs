using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Repositories.Repositories;

namespace TerrariumGardenTech.Repositories
{
    public class UnitOfWork
    {
        private TerrariumGardenTechDBContext _unitOfWorkContext;
        private TerrariumRepository _terrariumRepository;
        


        public UnitOfWork()
        {
            _unitOfWorkContext = new TerrariumGardenTechDBContext();
        }


        public TerrariumRepository Terrarium {  get { return _terrariumRepository ??= new TerrariumRepository(_unitOfWorkContext); } }

    }
}
