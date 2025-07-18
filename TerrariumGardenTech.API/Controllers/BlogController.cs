using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Blog;
using TerrariumGardenTech.Service.ResponseModel.Blog;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = "Admin,Staff")]
public class BlogController : ControllerBase
{
    private readonly IBlogService _blogService;

    public BlogController(IBlogService blogService)
    {
        _blogService = blogService;
    }

    // GET: api/<BlogController>
    [HttpGet("get-all")]
    public async Task<IBusinessResult> Get()
    {
        var result = await _blogService.GetAll();
        // Check if result or result.Data is null
        if (result == null || result.Data == null) return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");

        // Ensure Data is a List<Terrarium> (or any IEnumerable<Category>)
        var blogs = (result.Data as IEnumerable<Blog>)?.Select(b => new BlogResponse
        {
            BlogCategoryId = b.BlogCategoryId,
            BlogId = b.BlogId,
            UserId = b.UserId,
            Title = b.Title,
            Content = b.Content,
            bodyHTML = b.bodyHTML ?? string.Empty,
            UrlImage = b.UrlImage ?? string.Empty, // Ensure UrlImage is not null
            CreatedAt = b.CreatedAt ?? DateTime.MinValue, // Use a default value if CreatedAt is null
            UpdatedAt = b.UpdatedAt ?? DateTime.MinValue, // Similar for UpdatedAt
            Status = b.Status
        }).ToList();
        if (blogs == null) return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
        return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", blogs);
    }

    // GET api/<BlogController>/5
    [HttpGet("get-{id}")]
    public async Task<IBusinessResult> Get(int id)
    {
        var result = await _blogService.GetById(id);

        // Kiểm tra nếu result hoặc result.Data là null
        if (result == null || result.Data == null) return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");

        // Kiểm tra kiểu dữ liệu của result.Data (đảm bảo nó là Category, không phải IEnumerable)
        if (result.Data is Blog blog)
        {
            // Ánh xạ dữ liệu từ Category sang CategoryRequest
            var blogResponse = new BlogResponse
            {
                BlogId = blog.BlogId,
                BlogCategoryId = blog.BlogCategoryId,
                UserId = blog.UserId,
                Title = blog.Title,
                Content = blog.Content,
                bodyHTML = blog.bodyHTML ?? string.Empty, // Ensure bodyHtml is not null
                UrlImage = blog.UrlImage ?? string.Empty, // Ensure UrlImage is not null
                CreatedAt = blog.CreatedAt ?? DateTime.MinValue, // Use a default value if CreatedAt is null
                UpdatedAt = blog.UpdatedAt ?? DateTime.MinValue, // Similar for UpdatedAt
                Status = blog.Status
            };

            // Trả về BusinessResult với dữ liệu đã ánh xạ
            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", blogResponse);
        }

        // Trả về lỗi nếu không thể ánh xạ
        return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
    }

    // POST api/<BlogController>
    [HttpPost("add-blog")]
    public async Task<IBusinessResult> Post([FromBody] BlogCreateRequest blogCreateRequest)
    {
        return await _blogService.CreateBlog(blogCreateRequest);
    }

    // PUT api/<BlogController>/5
    [HttpPut("update-blog-{id}")]
    public async Task<IBusinessResult> Put(int id, [FromBody] BlogUpdateRequest blogUpdateRequest)
    {
        if (blogUpdateRequest == null || !ModelState.IsValid)
            return new BusinessResult(Const.FAIL_UPDATE_CODE, "Invalid request data.");
        blogUpdateRequest.BlogId = id; // Ensure the ID is set for the update
        return await _blogService.UpdateBlog(blogUpdateRequest);
    }

    // DELETE api/<BlogController>/5
    [HttpDelete("delete-blog-{id}")]
    public async Task<IBusinessResult> Delete(int id)
    {
        var result = await _blogService.DeleteById(id);
        if (result == null || result.Data == null) return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
        if (result.Data is bool isDeleted)
            return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Blog deleted successfully.");
        return new BusinessResult(Const.FAIL_DELETE_CODE, "Failed to delete role.");
    }
}