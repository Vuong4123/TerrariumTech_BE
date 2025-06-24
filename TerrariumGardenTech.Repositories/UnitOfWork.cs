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
        private RoleReppsitory _roleReppsitory;
        private PersonnalizeRepository _personnalizeRepository;


        public UnitOfWork()
        {
            _unitOfWorkContext = new TerrariumGardenTechDBContext();
        }


        public TerrariumRepository Terrarium {  get { return _terrariumRepository ??= new TerrariumRepository(_unitOfWorkContext); } }
        public CategoryRepository Category { get { return _categoryRepository ??= new CategoryRepository(_unitOfWorkContext); } }
        public AccessoryRepository Accessory { get { return _accessoryRepository ??= new AccessoryRepository(_unitOfWorkContext); } }
        public BlogRepository Blog { get { return _blogRepository ??= new BlogRepository(_unitOfWorkContext); } }
        public UserRepository User { get { return _userRepository ??= new UserRepository(_unitOfWorkContext); } }
        public BlogCategoryRepository BlogCategory { get { return _blogCategoryRepository ??= new BlogCategoryRepository(_unitOfWorkContext); } }
        public RoleReppsitory Role { get { return _roleReppsitory ??= new RoleReppsitory(_unitOfWorkContext); } }
        public PersonnalizeRepository Personalize { get { return _personnalizeRepository ??= new PersonnalizeRepository(_unitOfWorkContext);  } }

    }
}
