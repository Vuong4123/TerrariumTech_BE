using Microsoft.EntityFrameworkCore;
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
        private VoucherRepository _voucherRepository;
        private TerrariumImageRepository _terrariumImageRepository;
        private NotificationRepository _notificationRepository;
        private TerrariumVariantRepository _terrariumVariantRepository;
        private TerrariumAccessoryRepository _terrariumAccessoryRepository;
        private MemberShipRepository _memberShipRepository;
        private MembershipPackageRepository _membershipPackageRepository;
        
        private AccessoryImageRepository _accessoryImageRepository;
        private PaymentTransitionRepository _paymentTransitionRepository;
        private OrderRepository _orderRepository;
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
        public VoucherRepository Voucher { get { return _voucherRepository ??= new VoucherRepository(_unitOfWorkContext); } }
        public TerrariumImageRepository TerrariumImage { get { return _terrariumImageRepository ??= new TerrariumImageRepository(_unitOfWorkContext); } }
        public NotificationRepository Notification { get { return _notificationRepository ??= new NotificationRepository(_unitOfWorkContext); } }
        public TerrariumVariantRepository TerrariumVariant { get { return _terrariumVariantRepository ??= new TerrariumVariantRepository(_unitOfWorkContext); } }
        public TerrariumAccessoryRepository TerrariumAccessory { get { return _terrariumAccessoryRepository ??= new TerrariumAccessoryRepository(_unitOfWorkContext); } }
        public MemberShipRepository MemberShipRepository { get { return _memberShipRepository ??= new MemberShipRepository(_unitOfWorkContext); } }

        
        public AccessoryImageRepository AccessoryImage { get { return _accessoryImageRepository ??= new AccessoryImageRepository(_unitOfWorkContext); } }

        public MembershipPackageRepository MembershipPackageRepository
        {
            get { return _membershipPackageRepository ??= new MembershipPackageRepository(_unitOfWorkContext); }
        }

        public PaymentTransitionRepository PaymentTransitionRepository
        {
            get { return _paymentTransitionRepository ??= new PaymentTransitionRepository(_unitOfWorkContext); }
        }

        public OrderRepository OrderRepository
        {
            get { return _orderRepository ??= new OrderRepository(_unitOfWorkContext); }
        }

        public async Task<int> SaveAsync()
        {
            return await _unitOfWorkContext.SaveChangesAsync();
        }


    }
}
