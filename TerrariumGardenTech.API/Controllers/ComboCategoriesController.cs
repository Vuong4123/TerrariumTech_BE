using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Combo;
using TerrariumGardenTech.Common.ResponseModel.Combo;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ComboCategoriesController : ControllerBase
{
    private readonly IComboCategoryService _comboCategoryService;

    public ComboCategoriesController(IComboCategoryService comboCategoryService)
    {
        _comboCategoryService = comboCategoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllCategories([FromQuery] bool includeInactive = false)
    {
        var result = await _comboCategoryService.GetAllCategoriesAsync(includeInactive);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCategoryById(int id)
    {
        var result = await _comboCategoryService.GetCategoryByIdAsync(id);
        if (result.Status == Const.SUCCESS_READ_CODE)
            return Ok(result);
        return NotFound(result);
    }

    [HttpPost]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateComboCategoryRequest request)
    {
        var result = await _comboCategoryService.CreateCategoryAsync(request);
        if (result.Status == Const.SUCCESS_CREATE_CODE)
            return CreatedAtAction(nameof(GetCategoryById), new { id = ((ComboCategoryResponse)result.Data!).ComboCategoryId }, result);
        return BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateComboCategoryRequest request)
    {
        if (id != request.ComboCategoryId)
            return BadRequest("ID mismatch");

        var result = await _comboCategoryService.UpdateCategoryAsync(request);
        if (result.Status == Const.SUCCESS_UPDATE_CODE)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _comboCategoryService.DeleteCategoryAsync(id);
        if (result.Status == Const.SUCCESS_DELETE_CODE)
            return Ok(result);
        return BadRequest(result);
    }
}

[ApiController]
[Route("api/[controller]")]
public class CombosController : ControllerBase
{
    private readonly IComboService _comboService;

    public CombosController(IComboService comboService)
    {
        _comboService = comboService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllCombos([FromQuery] GetCombosRequest request)
    {
        var result = await _comboService.GetAllCombosAsync(request);
        return Ok(result);
    }

    [HttpGet("featured")]
    [AllowAnonymous]
    public async Task<IActionResult> GetFeaturedCombos([FromQuery] int take = 10)
    {
        var result = await _comboService.GetFeaturedCombosAsync(take);
        return Ok(result);
    }

    [HttpGet("category/{categoryId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCombosByCategory(int categoryId, [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var result = await _comboService.GetCombosByCategoryAsync(categoryId, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetComboById(int id)
    {
        var result = await _comboService.GetComboByIdAsync(id);
        if (result.Status == Const.SUCCESS_READ_CODE)
            return Ok(result);
        return NotFound(result);
    }

    [HttpPost]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> CreateCombo([FromBody] CreateComboRequest request)
    {
        var result = await _comboService.CreateComboAsync(request);
        if (result.Status == Const.SUCCESS_CREATE_CODE)
            return CreatedAtAction(nameof(GetComboById), new { id = ((ComboResponse)result.Data!).ComboId }, result);
        return BadRequest(result);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> UpdateCombo(int id, [FromBody] UpdateComboRequest request)
    {
        if (id != request.ComboId)
            return BadRequest("ID mismatch");

        var result = await _comboService.UpdateComboAsync(request);
        if (result.Status == Const.SUCCESS_UPDATE_CODE)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> DeleteCombo(int id)
    {
        var result = await _comboService.DeleteComboAsync(id);
        if (result.Status == Const.SUCCESS_DELETE_CODE)
            return Ok(result);
        return BadRequest(result);
    }

    [HttpPost("{id}/toggle-active")]
    [Authorize(Roles = "Manager,Admin")]
    public async Task<IActionResult> ToggleComboActive(int id)
    {
        var result = await _comboService.ToggleComboActiveAsync(id);
        return Ok(result);
    }
}
