using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Repositories.Repositories;

namespace TerrariumGardenTech.Repositories;

public class UnitOfWork
{
    private readonly TerrariumGardenTechDBContext _unitOfWorkContext;

    private AccessoryImageRepository _accessoryImageRepository;
    private AccessoryRepository _accessoryRepository;
    private AddressRepository _addressRepository;
    private BlogCategoryRepository _blogCategoryRepository;
    private BlogRepository _blogRepository;
    private CartItemRepository _cartItemRepository;
    private CartRepository _cartRepository;
    private CategoryRepository _categoryRepository;
    private EnvironmentRepository _environmentRepository;
    private MembershipPackageRepository _membershipPackageRepository;
    private MemberShipRepository _memberShipRepository;
    private NotificationRepository _notificationRepository;
    private OrderRepository _orderRepository;
    private PaymentRepository _paymentRepository;
    private PersonalizeRepository _personalizeRepository;
    private RoleRepository _roleRepository;
    private ShapeRepository _shapeRepository;
    private TankMethodRepository _tankMethodRepository;
    private TerrariumAccessoryRepository _terrariumAccessoryRepository;
    private TerrariumImageRepository _terrariumImageRepository;
    private TerrariumRepository _terrariumRepository;
    private TerrariumVariantRepository _terrariumVariantRepository;
    private UserRepository _userRepository;
    private VoucherRepository _voucherRepository;
    private FeedbackRepository _feedbackRepository;
    private TransportRepository _transportRepository;
    private TransportLogRepository _transportLogRepository;
    private OrderRequestRefundRepository _orderRequestRefundRepository;
    private ChatRepository _chatRepository;
    private ChatMessageRepository _chatMessageRepository;
    private FeedbackImageRepository _feedbackImageRepository;
    private WalletRepository _walletRepository;
    private WalletTransactionRepository _walletTransactionRepository;
    private FavoriteRepository _favoriteRepository;
    public UnitOfWork()
    {
        _unitOfWorkContext = new TerrariumGardenTechDBContext();
    }
    public WalletTransactionRepository WalletTransactionRepository
    {
        get { return _walletTransactionRepository ??= new WalletTransactionRepository(_unitOfWorkContext); }
    }
    public WalletRepository Wallet
    {
        get { return _walletRepository ??= new WalletRepository(_unitOfWorkContext); }
    }

    public TerrariumRepository Terrarium
    {
        get { return _terrariumRepository ??= new TerrariumRepository(_unitOfWorkContext); }
    }

    public CategoryRepository Category
    {
        get { return _categoryRepository ??= new CategoryRepository(_unitOfWorkContext); }
    }

    public AccessoryRepository Accessory
    {
        get { return _accessoryRepository ??= new AccessoryRepository(_unitOfWorkContext); }
    }

    public BlogRepository Blog
    {
        get { return _blogRepository ??= new BlogRepository(_unitOfWorkContext); }
    }

    public UserRepository User
    {
        get { return _userRepository ??= new UserRepository(_unitOfWorkContext); }
    }

    public BlogCategoryRepository BlogCategory
    {
        get { return _blogCategoryRepository ??= new BlogCategoryRepository(_unitOfWorkContext); }
    }

    public RoleRepository Role
    {
        get { return _roleRepository ??= new RoleRepository(_unitOfWorkContext); }
    }

    public PersonalizeRepository Personalize
    {
        get { return _personalizeRepository ??= new PersonalizeRepository(_unitOfWorkContext); }
    }

    public AddressRepository Address
    {
        get { return _addressRepository ??= new AddressRepository(_unitOfWorkContext); }
    }

    public ShapeRepository Shape
    {
        get { return _shapeRepository ??= new ShapeRepository(_unitOfWorkContext); }
    }

    public TankMethodRepository TankMethod
    {
        get { return _tankMethodRepository ??= new TankMethodRepository(_unitOfWorkContext); }
    }

    public EnvironmentRepository Environment
    {
        get { return _environmentRepository ??= new EnvironmentRepository(_unitOfWorkContext); }
    }

    public VoucherRepository Voucher
    {
        get { return _voucherRepository ??= new VoucherRepository(_unitOfWorkContext); }
    }

    public TerrariumImageRepository TerrariumImage
    {
        get { return _terrariumImageRepository ??= new TerrariumImageRepository(_unitOfWorkContext); }
    }

    public NotificationRepository Notification
    {
        get { return _notificationRepository ??= new NotificationRepository(_unitOfWorkContext); }
    }

    public TerrariumVariantRepository TerrariumVariant
    {
        get { return _terrariumVariantRepository ??= new TerrariumVariantRepository(_unitOfWorkContext); }
    }

    public TerrariumAccessoryRepository TerrariumAccessory
    {
        get { return _terrariumAccessoryRepository ??= new TerrariumAccessoryRepository(_unitOfWorkContext); }
    }

    public MemberShipRepository MemberShip
    {
        get { return _memberShipRepository ??= new MemberShipRepository(_unitOfWorkContext); }
    }


    public AccessoryImageRepository AccessoryImage
    {
        get { return _accessoryImageRepository ??= new AccessoryImageRepository(_unitOfWorkContext); }
    }

    public MembershipPackageRepository MembershipPackageRepository
    {
        get { return _membershipPackageRepository ??= new MembershipPackageRepository(_unitOfWorkContext); }
    }

    public PaymentRepository Payment
    {
        get { return _paymentRepository ??= new PaymentRepository(_unitOfWorkContext); }
    }

    public OrderRepository Order
    {
        get { return _orderRepository ??= new OrderRepository(_unitOfWorkContext); }
    }

    public CartRepository Cart
    {
        get { return _cartRepository ??= new CartRepository(_unitOfWorkContext); }
    }

    public CartItemRepository CartItem
    {
        get { return _cartItemRepository ??= new CartItemRepository(_unitOfWorkContext); }
    }

    public ChatRepository Chat
    {
        get { return _chatRepository ??= new ChatRepository(_unitOfWorkContext); }
    }
    public ChatMessageRepository ChatMessage
    {
        get { return _chatMessageRepository ??= new ChatMessageRepository(_unitOfWorkContext); }
    }
    public CartItemRepository CartItems
    {
        get { return _cartItemRepository ??= new CartItemRepository(_unitOfWorkContext); }
    }

    public async Task<int> SaveAsync()
    {
        return await _unitOfWorkContext.SaveChangesAsync();
    }

    public FeedbackRepository Feedback
    {
        get => _feedbackRepository ??= new FeedbackRepository(_unitOfWorkContext);
    }

    public TransportRepository Transport
    {
        get => _transportRepository ??= new TransportRepository(_unitOfWorkContext);
    }

    public TransportLogRepository TransportLog
    {
        get => _transportLogRepository ??= new TransportLogRepository(_unitOfWorkContext);
    }

    public OrderRequestRefundRepository OrderRequestRefund
    {
        get => _orderRequestRefundRepository ?? new OrderRequestRefundRepository(_unitOfWorkContext);
    }
    public FeedbackImageRepository FeedbackImage
    {
        get => _feedbackImageRepository ??= new FeedbackImageRepository(_unitOfWorkContext);
    }
    public FavoriteRepository Favorite
    {
        get => _favoriteRepository ??= new FavoriteRepository(_unitOfWorkContext);
    }
}