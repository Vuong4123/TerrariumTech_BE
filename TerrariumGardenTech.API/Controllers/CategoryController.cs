using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Category;
using TerrariumGardenTech.Service.ResponseModel.Category;
using TerrariumGardenTech.Service.ResponseModel.Terrarium;
using TerrariumGardenTech.Service.Service;

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
            var result =  await _categoryService.GetAll();
            // Check if result or result.Data is null
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }

            // Ensure Data is a List<Terrarium> (or any IEnumerable<Category>)
            var categories = (result.Data as IEnumerable<Category>)?.Select(c => new CategoryUpdateRequest
            {
                CategoryId = c.CategoryId,
                CategoryName = c.Name,  
                Description = c.Description,
            }).ToList();
            if (categories == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
            }
            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", categories);
        }

        // GET api/<CategoryController>/5
        [HttpGet("{id}")]
        public async Task<IBusinessResult> Get(int id)
        {

            var result = await _categoryService.GetById(id);

            // Kiểm tra nếu result hoặc result.Data là null
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }

            // Kiểm tra kiểu dữ liệu của result.Data (đảm bảo nó là Category, không phải IEnumerable)
            if (result.Data is Category category)
            {
                // Ánh xạ dữ liệu từ Category sang CategoryRequest
                var categoryRequest = new CategoryResponse
                {
                    CategoryId = category.CategoryId,
                    CategoryName = category.Name,
                    Description = category.Description
                };

                // Trả về BusinessResult với dữ liệu đã ánh xạ
                return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", categoryRequest);
            }

            // Trả về lỗi nếu không thể ánh xạ
            return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");


        }

        // POST api/<CategoryController>
        [HttpPost]
        public async Task<IBusinessResult> Post(CategoryCreateRequest categoryRequest)
        {
            if (categoryRequest == null || !ModelState.IsValid)
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Invalid request data.");
            }
            return await _categoryService.CreateCategory(categoryRequest);
        }

        // PUT api/<CategoryController>/5
        [HttpPut("{id}")]
        public async Task<IBusinessResult> Put(CategoryUpdateRequest categoryRequest)
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
            var result = await _categoryService.DeleteById(id);
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }
            if (result.Data is bool isDeleted)
            {
                return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Category deleted successfully.");
            }
            return new BusinessResult(Const.FAIL_DELETE_CODE, "Failed to delete role.");
        }
    }
}
