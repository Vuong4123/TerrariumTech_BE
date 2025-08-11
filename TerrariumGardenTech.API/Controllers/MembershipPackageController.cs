using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.MemberShip;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MembershipPackageController : ControllerBase
{
    private readonly IMembershipService _membershipService;
    private readonly IMembershipPackageService _service;

    public MembershipPackageController(IMembershipPackageService service, IMembershipService membershipService)
    {
        _service = service;
        _membershipService = membershipService;
    }

    // Get all MembershipPackages
    [HttpGet]
    public async Task<IBusinessResult> GetAll()
    {
        var data = await _service.GetAllAsync();
        if (data == null || !data.Any())
            return new BusinessResult(Const.NOT_FOUND_CODE, "Không tìm thấy gói membership", null);

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy tất cả gói membership thành công", data);
    }

    // Get MembershipPackage by ID
    [HttpGet("{id}")]
    public async Task<IBusinessResult> Get(int id)
    {
        var pkg = await _service.GetByIdAsync(id);
        if (pkg == null)
            return new BusinessResult(Const.NOT_FOUND_CODE, "Gói membership không tồn tại", null);

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy thông tin gói membership thành công", pkg);
    }

    // Create a new Membership for a user

    [HttpPost("create")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IBusinessResult> CreateMembership([FromBody] CreateMembershipPackageRequest request)
    {
        if (!ModelState.IsValid)
            return new BusinessResult(Const.BAD_REQUEST_CODE, "Dữ liệu không hợp lệ", ModelState);

        try
        {
            var package = new Common.Entity.MembershipPackage
            {
                Type = request.Type,
                DurationDays = request.DurationDays,
                Price = request.Price,
                Description = request.Description,
                IsActive = request.IsActive
            };

            var membershipId = await _service.CreateAsync(package);

            return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Tạo membership package thành công",
                new { membershipId });
        }
        catch (UnauthorizedAccessException ex)
        {
            return new BusinessResult(Const.UNAUTHORIZED_CODE, ex.Message, null);
        }
        catch (ArgumentException ex)
        {
            return new BusinessResult(Const.BAD_REQUEST_CODE, ex.Message, null);
        }
    }
    // Create a new Membership for a user

    [HttpPost("createmembershipforuser")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IBusinessResult> CreateMembershipForUser([FromBody] CreateMembershipForUserRequest request)
    {
        if (!ModelState.IsValid)
            return new BusinessResult(Const.BAD_REQUEST_CODE, "Dữ liệu không hợp lệ", ModelState);

        try
        {
            // 1) Kiểm tra package
            var package = await _service.GetByIdAsync(request.PackageId);
            if (package == null)
                return new BusinessResult(Const.NOT_FOUND_CODE, "Gói membership không tồn tại", null);

            // 2) Tạo membership cho user (logic chặn trùng Active nên đặt trong service)
            var membershipId = await _membershipService.CreateMembershipForUserAsync(
                request.UserId, request.PackageId, request.StartDate);

            // 3) Trả về gọn: id
            return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Tạo membership cho người dùng thành công",
                new { membershipId });
        }
        catch (UnauthorizedAccessException ex)
        {
            return new BusinessResult(Const.UNAUTHORIZED_CODE, ex.Message, null);
        }
        catch (ArgumentException ex)
        {
            return new BusinessResult(Const.BAD_REQUEST_CODE, ex.Message, null);
        }
    }


    // Update a MembershipPackage by ID
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IBusinessResult> Update(int id, [FromBody] UpdateMembershipRequest req)
    {
        // Kiểm tra xem ID trong URL có khớp với ID trong body request không
        if (id != req.PackageId)
            return new BusinessResult(Const.BAD_REQUEST_CODE, "Sai ID", null);

        var packageToUpdate = await _service.GetByIdAsync(id);
        if (packageToUpdate == null)
            return new BusinessResult(Const.NOT_FOUND_CODE, "Gói membership không tồn tại", null);

        // Cập nhật thông tin gói
        packageToUpdate.Description = req.Description;
        packageToUpdate.Price = req.Price;
        packageToUpdate.DurationDays = req.DurationDays;

        var result = await _service.UpdateAsync(packageToUpdate);

        return result
            ? new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Cập nhật thành công", null)
            : new BusinessResult(Const.NOT_FOUND_CODE, "Gói membership không tồn tại", null);
    }

    // Delete a MembershipPackage by ID
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IBusinessResult> Delete(int id)
    {
        var result = await _service.DeleteAsync(id);
        return result
            ? new BusinessResult(Const.SUCCESS_DELETE_CODE, "Đã xóa gói membership", null)
            : new BusinessResult(Const.NOT_FOUND_CODE, "Không tìm thấy gói membership", null);
    }
}