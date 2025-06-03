using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Blog;
using TerrariumGardenTech.Service.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BlogController : ControllerBase
    {
        private readonly IBlogService _blogService;
        public BlogController(IBlogService blogService)
        {
            _blogService = blogService;
        }
        // GET: api/<BlogController>
        [HttpGet]
        public async Task<IBusinessResult> Get()
        {
            return await _blogService.GetAll();
            ;
        }

        // GET api/<BlogController>/5
        [HttpGet("{id}")]
        public async Task<IBusinessResult> Get(int id)
        {
            return await _blogService.GetById(id);
        }

        // POST api/<BlogController>
        [HttpPost]
        public async Task<IBusinessResult> Post([FromBody] BlogCreateRequest blogCreateRequest)
        {
            if (blogCreateRequest == null || !!ModelState.IsValid)
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Invalid request data.");
            }
            return await _blogService.CreateBlog(blogCreateRequest);
        }

        // PUT api/<BlogController>/5
        [HttpPut("{id}")]
        public async Task<IBusinessResult> Put(int id, [FromBody] BlogUpdateRequest blogUpdateRequest)
        {
            if (blogUpdateRequest == null || !ModelState.IsValid)
            {
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Invalid request data.");
            }
            blogUpdateRequest.BlogId = id; // Ensure the ID is set for the update
            return await _blogService.UpdateBlog(blogUpdateRequest);
        }

        // DELETE api/<BlogController>/5
        [HttpDelete("{id}")]
        public async Task<IBusinessResult> Delete(int id)
        {
            return await _blogService.DeleteById(id);
        }
    }
}
