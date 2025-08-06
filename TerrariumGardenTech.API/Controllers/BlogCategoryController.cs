using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.BlogCategory;
using TerrariumGardenTech.Common.ResponseModel.BlogCategory;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers;

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
        return await _blogCategoryService.GetAllBlogCategory();
    }

    // GET api/<BlogCategoryController>/5
    [HttpGet("get/{id}")]
    public async Task<IBusinessResult> Get(int id)
    {
        return await _blogCategoryService.GetById(id);
    }

    // POST api/<BlogCategoryController>
    [HttpPost("add-blogCategory")]
    public async Task<IBusinessResult> Post([FromBody] BlogCategoryCreateRequest blogCategoryCreateRequest)
    {        
        return await _blogCategoryService.CreateBlogCategory(blogCategoryCreateRequest);
    }

    // PUT api/<BlogCategoryController>/5
    [HttpPut("update-blogCategory/{id}")]
    public async Task<IBusinessResult> Put([FromBody] BlogCategoryUpdateRequest blogCategoryUpdateRequest)
    {
        return await _blogCategoryService.UpdateBlogCategory(blogCategoryUpdateRequest);
    }

    // DELETE api/<BlogCategoryController>/5
    [HttpDelete("delete-blogCategory/{id}")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _blogCategoryService.DeleteById(id);
    }
}