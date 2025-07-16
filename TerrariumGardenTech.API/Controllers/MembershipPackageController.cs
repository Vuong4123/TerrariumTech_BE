using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.IService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembershipPackageController : ControllerBase
    {
        private readonly IMembershipPackageService _service;

        public MembershipPackageController(IMembershipPackageService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var pkg = await _service.GetByIdAsync(id);
            return pkg == null ? NotFound() : Ok(pkg);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([FromBody] MembershipPackage req)
        {
            var id = await _service.CreateAsync(req);
            return Ok(new { message = "Tạo thành công", id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(int id, [FromBody] MembershipPackage req)
        {
            if (id != req.Id) return BadRequest("Sai ID");
            var result = await _service.UpdateAsync(req);
            return result ? Ok("Cập nhật thành công") : NotFound();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            return result ? Ok("Đã xóa") : NotFound();
        }
    }
}
