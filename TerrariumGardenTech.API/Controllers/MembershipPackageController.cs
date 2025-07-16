using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.MemberShip;
using TerrariumGardenTech.Service.RequestModel.MemberShip.TerrariumGardenTech.Service.RequestModel.MemberShip;
using TerrariumGardenTech.Service.Service;

namespace TerrariumGardenTech.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MembershipPackageController : ControllerBase
    {
        private readonly IMembershipPackageService _service;
        private readonly IMembershipService _membershipService;

        public MembershipPackageController(IMembershipPackageService service, IMembershipService membershipService)
        {
            _service = service;
            _membershipService = membershipService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var pkg = await _service.GetByIdAsync(id);
            if (pkg == null)
                return NotFound(new { message = "Gói membership không tồn tại", statusCode = 404 });

            return Ok(new { data = pkg, statusCode = 200 });
        }

        [HttpPost("create")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateMembership([FromBody] CreateMembershipForUserRequest request)
        {
            try
            {
                // Kiểm tra xem package có tồn tại không
                var package = await _service.GetByIdAsync(request.PackageId);  // Thay _membershipPackageService bằng _service
                if (package == null)
                    return NotFound(new { message = "Gói membership không tồn tại", statusCode = 404 });

                // Tạo membership cho người dùng
                var membershipId = await _membershipService.CreateMembershipForUserAsync(request.UserId, request.PackageId, request.StartDate);

                // Trả về một response đơn giản cho người dùng
                var response = new
                {
                    membershipId = membershipId,
                    message = "Tạo membership cho người dùng thành công",
                    statusCode = 200
                };

                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message, statusCode = 401 });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message, statusCode = 400 });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMembershipRequest req)
        {
            // Kiểm tra xem ID trong URL có khớp với ID trong body request không
            if (id != req.PackageId)
                return BadRequest(new { message = "Sai ID", statusCode = 400 });

            // Kiểm tra xem gói membership có tồn tại không
            var packageToUpdate = await _service.GetByIdAsync(id);
            if (packageToUpdate == null)
                return NotFound(new { message = "Gói membership không tồn tại", statusCode = 404 });

            // Tạo đối tượng MembershipPackage từ UpdateMembershipRequest
            packageToUpdate.Description = req.Description;  // Cập nhật mô tả
            packageToUpdate.Price = req.Price;  // Cập nhật giá
            packageToUpdate.DurationDays = req.DurationDays;  // Cập nhật thời gian gói

            // Cập nhật gói membership
            var result = await _service.UpdateAsync(packageToUpdate);

            // Nếu cập nhật thành công, trả về thông báo thành công
            if (result)
            {
                return Ok(new { message = "Cập nhật thành công", statusCode = 200 });
            }

            // Nếu không tìm thấy gói, trả về lỗi không tìm thấy
            return NotFound(new { message = "Gói membership không tồn tại", statusCode = 404 });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (result)
            {
                return Ok(new { message = "Đã xóa", statusCode = 200 });
            }
            return NotFound(new { message = "Không tìm thấy gói membership", statusCode = 404 });
        }
    }
}
