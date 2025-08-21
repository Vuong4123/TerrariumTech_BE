using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common.RequestModel.TerraniumLayout;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.Service;
using static TerrariumGardenTech.Common.Enums.CommonData;

[ApiController]
[Route("api/[controller]")]
public class TerrariumLayoutController : ControllerBase
{
    private readonly ITerrariumService _service;

    public TerrariumLayoutController(ITerrariumService service)
    {
        _service = service;
    }

    // CREATE layout từ terrarium có sẵn
    [HttpPost("create")]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateLayoutRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _service.GetByIdAsync(id);
            return result != null ? Ok(result) : NotFound();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("all")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("my-layouts")]
    [Authorize]
    public async Task<IActionResult> GetMyLayouts(int userId)
    {
        try
        {
            var result = await _service.GetByUserIdAsync(userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    public async Task<IActionResult> GetPending()
    {
        try
        {
            var result = await _service.GetPendingAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLayoutRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request);
            return result ? Ok("Updated successfully") : NotFound("Layout not found");
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/review")]
    [Authorize(Roles = "Admin,Manager,Staff")]
    public async Task<IActionResult> Review(int id, ReviewRequest request)
    {
        try
        {
            var managerId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var result = await _service.ReviewAsync(id, managerId, request.Status, request.Price, request.Notes);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public class ReviewRequest
{
    public string Status { get; set; }
    public decimal? Price { get; set; }
    public string? Notes { get; set; }
}
