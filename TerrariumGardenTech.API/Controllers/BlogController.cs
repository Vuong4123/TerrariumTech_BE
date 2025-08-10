using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Blog;
using TerrariumGardenTech.Common.ResponseModel.Blog;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

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
    [Authorize(Roles = "Admin,Staff,Manager,User")]
    public async Task<IBusinessResult> Get()
    {
        return await _blogService.GetAll();
    }

    // GET api/<BlogController>/5
    [HttpGet("get/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager,User")]
    public async Task<IBusinessResult> Get(int id)
    {
        return await _blogService.GetById(id);
    }

    // POST api/<BlogController>
    [HttpPost("add-blog")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Post([FromForm] BlogCreateRequest blogCreateRequest)
    {
        return await _blogService.CreateBlog(blogCreateRequest);
    }

    // PUT api/<BlogController>/5
    [HttpPut("update-blog/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Put(int id, [FromForm] BlogUpdateRequest request)
    {
        request.BlogId = id;
        return await _blogService.UpdateBlog(request);
    }

    // DELETE api/<BlogController>/5
    [HttpDelete("delete-blog/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _blogService.DeleteById(id);
    }
}