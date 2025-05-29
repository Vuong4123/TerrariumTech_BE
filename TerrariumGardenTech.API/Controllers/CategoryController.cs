using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Category;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        public CategoryController(ICategoryService categoryService)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        }
        // GET: api/<CategoryController>
        [HttpGet]
        public async Task<IBusinessResult> Get()
        {
            return await _categoryService.GetAll();
        }

        // GET api/<CategoryController>/5
        [HttpGet("{id}")]
        public async Task<IBusinessResult> Get(int id)
        {
            return await _categoryService.GetById(id);
        }

        // POST api/<CategoryController>
        [HttpPost]
        public async Task<IBusinessResult> Post(CategoryRequest categoryRequest)
        {
            if (categoryRequest == null || !ModelState.IsValid)
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Invalid request data.");
            }
            return await _categoryService.CreateCategory(categoryRequest);
        }

        // PUT api/<CategoryController>/5
        [HttpPut("{id}")]
        public async Task<IBusinessResult> Put(CategoryRequest categoryRequest)
        {
            if (categoryRequest == null || !ModelState.IsValid)
            {
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Invalid request data.");
            }
            return await _categoryService.UpdateCategory(categoryRequest);
        }

        // DELETE api/<CategoryController>/5
        [HttpDelete("{id}")]
        public async Task<IBusinessResult> Delete(int id)
        {
            return await _categoryService.DeleteById(id);
        }
    }
}
