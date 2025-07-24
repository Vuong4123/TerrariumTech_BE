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

    [HttpGet("filter")]
    public async Task<IBusinessResult> FilterTerrariums([FromQuery] int? environmentId, [FromQuery] int? shapeId,
        [FromQuery] int? tankMethodId)
    {
        return await _terrariumService.FilterTerrariumsAsync(environmentId, shapeId, tankMethodId);
    }


    // GET api/<TerrariumController>/5
    [HttpGet("get-{id}")]
    public async Task<IBusinessResult> GetById(int id)
    {
        return await _terrariumService.GetById(id);
    }

    // GET: api/Terrarium/get-suggestions/{userId}
    [HttpGet("get-suggestions/{userId}")]
    public async Task<IBusinessResult> GetTerrariumSuggestions(int userId)
    {
        var result = await _terrariumService.GetTerrariumSuggestions(userId);

        // Kiểm tra nếu kết quả hoặc dữ liệu là null
        if (result == null || result.Data == null)
            return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");

        // Trả về kết quả gợi ý Terrarium
        return result;
    }

    // POST api/<TerrariumController>
    [HttpPost("add-terrarium")]
    public async Task<IBusinessResult> Post([FromBody] TerrariumCreateRequest terrariumCreate)
    {
        if (terrariumCreate == null || !ModelState.IsValid)
            return new BusinessResult(Const.FAIL_CREATE_CODE, "Invalid request data.");
        return await _terrariumService.CreateTerrarium(terrariumCreate);
    }

    // PUT api/<TerrariumController>/5
    [HttpPut("update-terrarium-{id}")]
    public async Task<IBusinessResult> Put(TerrariumUpdateRequest terrariumUpdate)
    {
        if (terrariumUpdate == null || !ModelState.IsValid)
            return new BusinessResult(Const.FAIL_UPDATE_CODE, "Invalid request data.");
        return await _terrariumService.UpdateTerrarium(terrariumUpdate);
    }

    // DELETE api/<TerrariumController>/5
    [HttpDelete("delete-terraium-{id}")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _terrariumService.DeleteById(id);
    }
}