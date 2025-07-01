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
        private UserRepository _userRepository;
        private TerrariumRepository _terrariumRepository;
        private CategoryRepository _categoryRepository;
        private AccessoryRepository _accessoryRepository;
        private BlogRepository _blogRepository;
        private BlogCategoryRepository _blogCategoryRepository;
        private RoleRepository _roleRepository;
        private PersonalizeRepository _personalizeRepository;
        private AddressRepository _addressRepository;
        private ShapeRepository _shapeRepository;
        private TankMethodRepository _tankMethodRepository;
        private EnvironmentRepository _environmentRepository;
        private TerrariumShapeRepository _terrariumShapeRepository;
        private TerrariumTankMethodRepository _terrariumTankMethodRepository;
        private TerrariumEnvironmentRepository _terrariumEnvironmentRepository;
        private VoucherRepository _voucherRepository;
        private TerrariumImageRepository _terrariumImageRepository;


        public UnitOfWork()
        {
            _unitOfWorkContext = new TerrariumGardenTechDBContext();
        }


        public TerrariumRepository Terrarium { get { return _terrariumRepository ??= new TerrariumRepository(_unitOfWorkContext); } }
        public CategoryRepository Category { get { return _categoryRepository ??= new CategoryRepository(_unitOfWorkContext); } }
        public AccessoryRepository Accessory { get { return _accessoryRepository ??= new AccessoryRepository(_unitOfWorkContext); } }
        public BlogRepository Blog { get { return _blogRepository ??= new BlogRepository(_unitOfWorkContext); } }
        public UserRepository User { get { return _userRepository ??= new UserRepository(_unitOfWorkContext); } }
        public BlogCategoryRepository BlogCategory { get { return _blogCategoryRepository ??= new BlogCategoryRepository(_unitOfWorkContext); } }
        public RoleRepository Role { get { return _roleRepository ??= new RoleRepository(_unitOfWorkContext); } }
        public PersonalizeRepository Personalize { get { return _personalizeRepository ??= new PersonalizeRepository(_unitOfWorkContext); } }
        public AddressRepository Address { get { return _addressRepository ??= new AddressRepository(_unitOfWorkContext); } }
        public ShapeRepository Shape { get { return _shapeRepository ??= new ShapeRepository(_unitOfWorkContext); } }
        public TankMethodRepository TankMethod { get { return _tankMethodRepository ??= new TankMethodRepository(_unitOfWorkContext); } }
        public EnvironmentRepository Environment { get { return _environmentRepository ??= new EnvironmentRepository(_unitOfWorkContext); } }
        public TerrariumShapeRepository TerrariumShape { get { return _terrariumShapeRepository ??= new TerrariumShapeRepository(_unitOfWorkContext); } }
        public TerrariumTankMethodRepository TerrariumTankMethod { get { return _terrariumTankMethodRepository ??= new TerrariumTankMethodRepository(_unitOfWorkContext); } }
        public TerrariumEnvironmentRepository TerrariumEnvironment { get { return _terrariumEnvironmentRepository ??= new TerrariumEnvironmentRepository(_unitOfWorkContext); } }
        public VoucherRepository Voucher { get { return _voucherRepository ??= new VoucherRepository(_unitOfWorkContext); } }
        public TerrariumImageRepository TerrariumImage { get { return _terrariumImageRepository ??= new TerrariumImageRepository(_unitOfWorkContext); } }
    }
}
