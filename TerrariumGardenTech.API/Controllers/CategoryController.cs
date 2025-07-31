using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Category;
using TerrariumGardenTech.Common.ResponseModel.Category;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers;

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
    [HttpGet("get-all")]
    public async Task<IBusinessResult> Get()
    {
        return await _categoryService.GetAll();
    }

    // GET api/<CategoryController>/5
    [HttpGet("get/{id}")]
    public async Task<IBusinessResult> Get(int id)
    {
        return await _categoryService.GetById(id);
    }

    // POST api/<CategoryController>
    [HttpPost("add-category")]
    public async Task<IBusinessResult> Post(CategoryCreateRequest categoryRequest)
    {
        return await _categoryService.CreateCategory(categoryRequest);
    }

    // PUT api/<CategoryController>/5
    [HttpPut("update-category/{id}")]
    public async Task<IBusinessResult> Put(CategoryUpdateRequest categoryRequest)
    {
       return await _categoryService.UpdateCategory(categoryRequest);
    }

    // DELETE api/<CategoryController>/5
    [HttpDelete("delete-category/{id}")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _categoryService.DeleteById(id);
    }
}