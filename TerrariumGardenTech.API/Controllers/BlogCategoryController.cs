using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.BlogCategory;
using TerrariumGardenTech.Service.ResponseModel.BlogCategory;
using TerrariumGardenTech.Service.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin,Staff")]
    public class BlogCategoryController : ControllerBase
    {
        private readonly IBlogCategoryService _blogCategoryService;
        public BlogCategoryController(IBlogCategoryService blogCategoryService)
        {
            _blogCategoryService = blogCategoryService;
        }

        // GET: api/<BlogCategoryController>
        [HttpGet("get-all")]
        public async Task<IBusinessResult> Get()
        {
            var result = await _blogCategoryService.GetAllBlogCategory();
            // Check if result or result.Data is null
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }

            // Ensure Data is a List<Terrarium> (or any IEnumerable<Category>)
            var blogs = (result.Data as IEnumerable<BlogCategory>)?.Select(b => new BlogCategoryResponse
            {
                BlogCategoryId = b.BlogCategoryId,
                CategoryName = b.CategoryName, // Assuming BlogCategory is a navigation property
                Description = b.Description,
            }).ToList();
            if (blogs == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
            }
            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", blogs);

        }

        // GET api/<BlogCategoryController>/5
        [HttpGet("get-{id}")]
        public async Task<IBusinessResult> Get(int id)
        {
            var result = await _blogCategoryService.GetById(id);

            // Kiểm tra nếu result hoặc result.Data là null
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }

            // Kiểm tra kiểu dữ liệu của result.Data (đảm bảo nó là Category, không phải IEnumerable)
            if (result.Data is BlogCategory blogCategory)
            {
                // Ánh xạ dữ liệu từ Category sang CategoryRequest
                var categoryReSponse = new BlogCategoryResponse
                {
                    BlogCategoryId = blogCategory.BlogCategoryId,
                    CategoryName = blogCategory.CategoryName,
                    Description = blogCategory.Description
                };

                // Trả về BusinessResult với dữ liệu đã ánh xạ
                return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", categoryReSponse);
            }

            // Trả về lỗi nếu không thể ánh xạ
            return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
        }

        // POST api/<BlogCategoryController>
        [HttpPost("add-blogCategory")]   
        public async Task<IBusinessResult> Post( BlogCategoryCreateRequest blogCategoryCreateRequest)
        {
            if (blogCategoryCreateRequest == null || !ModelState.IsValid)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Invalid request data.");
            }
            return await _blogCategoryService.CreateBlogCategory(blogCategoryCreateRequest);

        }

        // PUT api/<BlogCategoryController>/5
        [HttpPut("update-blogCategory-{id}")]
        public async Task<IBusinessResult> Put([FromBody] BlogCategoryUpdateRequest blogCategoryUpdateRequest)
        {

            if (blogCategoryUpdateRequest == null || !ModelState.IsValid)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Invalid request data.");
            }
            return await _blogCategoryService.UpdateBlogCategory( blogCategoryUpdateRequest);
        }

        // DELETE api/<BlogCategoryController>/5
        [HttpDelete("delete-blogCategory-{id}")]
        public async Task<IBusinessResult> Delete(int id)
        {
            var result = await _blogCategoryService.DeleteById(id);
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }
            if (result.Data is bool isDeleted)
            {
                return new BusinessResult(Const.SUCCESS_DELETE_CODE, "BlogCategory deleted successfully.");
            }
            return new BusinessResult(Const.FAIL_DELETE_CODE, "Failed to delete role.");
        }
    }
}
