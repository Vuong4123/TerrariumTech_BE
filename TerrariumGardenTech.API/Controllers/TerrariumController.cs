using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Terrarium;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

// using TerrariumGardenTech.Service.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = "Admin,Staff")]
public class TerrariumController : ControllerBase
{
    private readonly ITerrariumService _terrariumService;
    public DateTime today = DateTime.Now;

    public TerrariumController(ITerrariumService terrariumService)
    {
        _terrariumService = terrariumService;
    }

    // GET: api/<TerrariumController>
    [HttpGet("get-all")]
    public async Task<IBusinessResult> Get([FromQuery] TerrariumGetAllRequest request)
    {
        return await _terrariumService.GetAll(request);
    }

    /// <summary>
    /// Lấy danh sách Terrarium được tạo bởi AI (GeneratedByAI = true)
    /// Hỗ trợ include, sort, và pagination qua query string.
    /// </summary>
    /// <example>
    /// GET /api/Terrarium/get-all-generated?Pagination.PageNumber=1&amp;Pagination.PageSize=10&amp;Pagination.IsPagingEnabled=true&amp;IncludeProperties=TerrariumImages
    /// </example>
    [HttpGet("get-all-generated")]
    //[ProducesResponseType(typeof(BusinessResult), 200)]
    //[ProducesResponseType(typeof(BusinessResult), 400)]
    public async Task<IActionResult> GetAllGeneratedByAI([FromQuery] TerrariumGetAllRequest request)
    {
        var result = await _terrariumService.GetAllGeneratedByAI(request);

        // Giữ 200 cho cả trường hợp có data hoặc không có (tùy convention của bạn)
        if (result.Status == Const.SUCCESS_READ_CODE || result.Status == Const.WARNING_NO_DATA_CODE)
            return Ok(result);

        // Các lỗi nhập liệu / bad request
        if (result.Status == Const.BAD_REQUEST_CODE)
            return BadRequest(result);

        // Mặc định
        return StatusCode(500, result);
    }

    [HttpGet("filter")]
    public async Task<IBusinessResult> FilterTerrariums([FromQuery] int? environmentId, [FromQuery] int? shapeId,
        [FromQuery] int? tankMethodId)
    {
        return await _terrariumService.FilterTerrariumsAsync(environmentId, shapeId, tankMethodId);
    }


    //[HttpGet("get-by-name/{name}")]
    //public async Task<IBusinessResult> GetByAccesname(string name)
    //{
    //    return await _terrariumService.GetByAccesname(name);
    //}

    [HttpGet("get-by-terrariumname/{name}")]
    public async Task<IBusinessResult> GetTerrariumByName(string name)
    {
        return await _terrariumService.GetTerrariumByNameAsync(name);
    }

    // GET api/<TerrariumController>/5
    [HttpGet("get/{id}")]
    public async Task<IBusinessResult> GetById(int id)
    {
        return await _terrariumService.GetById(id);
    }

    //// GET: api/Terrarium/get-suggestions/{userId}
    //[HttpGet("get-suggestions/{userId}")]
    //public async Task<IBusinessResult> GetTerrariumSuggestions(int userId)
    //{
    //    var result = await _terrariumService.GetTerrariumSuggestions(userId);

    //    // Kiểm tra nếu kết quả hoặc dữ liệu là null
    //    if (result == null || result.Data == null)
    //        return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");

    //    // Trả về kết quả gợi ý Terrarium
    //    return result;
    //}

    // POST api/<TerrariumController>
    [HttpPost("add-terrarium")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Post([FromBody] TerrariumCreate terrariumCreate)
    {
        return await _terrariumService.CreateTerrarium(terrariumCreate);
    }
    //// GET: api/Terrarium/get-suggestions/{userId}
    //[HttpGet("get-suggestions/{userId}")]
    //public async Task<IBusinessResult> GetTerrariumSuggestions(int userId)
    //{
    //    var result = await _terrariumService.GetTerrariumSuggestions(userId);

    //    // Kiểm tra nếu kết quả hoặc dữ liệu là null
    //    if (result == null || result.Data == null)
    //        return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");

    //    // Trả về kết quả gợi ý Terrarium
    //    return result;
    //}

    // POST api/<TerrariumController>
    [HttpPost("add-terrariumbyai")]
    [Authorize]

    public async Task<IBusinessResult> PostAI([FromBody] TerrariumCreateRequest terrariumCreate)
    {
        return await _terrariumService.CreateTerrariumAI(terrariumCreate);
    }

    // PUT api/<TerrariumController>/5
    [HttpPut("update-terrarium/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Put(TerrariumUpdateRequest terrariumUpdate)
    {
        return await _terrariumService.UpdateTerrarium(terrariumUpdate);
    }

    // DELETE api/<TerrariumController>/5
    [HttpDelete("delete-terraium/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _terrariumService.DeleteById(id);
    }

    /// <summary>Top N sản phẩm nổi bật (bán nhiều nhất all-time). Mặc định N=3</summary>
    [HttpGet("featured")]
    public async Task<IActionResult> Featured([FromQuery] int top = 3)
    {
        var rs = await _terrariumService.GetTopBestSellersAllTimeAsync(top);
        return StatusCode(rs.Status, rs);
    }

    /// <summary>Top N bán chạy trong X ngày (mặc định 7 ngày, top=3)</summary>
    [HttpGet("best-sellers")]
    public async Task<IActionResult> BestSellers(
        [FromQuery] int days = 7,
        [FromQuery] int top = 3)
    {
        var rs = await _terrariumService.GetTopBestSellersLastDaysAsync(days, top);
        return StatusCode(rs.Status, rs);
    }

    /// <summary>Top N được đánh giá cao nhất. Mặc định N=3</summary>
    [HttpGet("top-rated")]
    public async Task<IActionResult> TopRated([FromQuery] int top = 3)
    {
        var rs = await _terrariumService.GetTopRatedAsync(top);
        return StatusCode(rs.Status, rs);
    }

    /// <summary>N sản phẩm mới nhất. Mặc định N=12</summary>
    [HttpGet("newest")]
    public async Task<IActionResult> Newest([FromQuery] int top = 12)
    {
        var rs = await _terrariumService.GetNewestAsync(top);
        return StatusCode(rs.Status, rs);
    }
}