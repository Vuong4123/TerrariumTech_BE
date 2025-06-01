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
        private CategoryRepository _categoryRepository;
        private TerrariumCategoryRepository _terrariumCategoryRepository;
        private AccessoryRepository _accessoryRepository;


        public UnitOfWork()
        {
            _unitOfWorkContext = new TerrariumGardenTechDBContext();
        }


        public TerrariumRepository Terrarium {  get { return _terrariumRepository ??= new TerrariumRepository(_unitOfWorkContext); } }
        public CategoryRepository Category { get { return _categoryRepository ??= new CategoryRepository(_unitOfWorkContext); } }
        public TerrariumCategoryRepository TerrariumCategory { get { return _terrariumCategoryRepository ??= new TerrariumCategoryRepository(_unitOfWorkContext); } }
        public AccessoryRepository Accessory { get { return _accessoryRepository ??= new AccessoryRepository(_unitOfWorkContext); } }

    }
}
