using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Personalize;
using TerrariumGardenTech.Service.ResponseModel.Personalize;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PersonalizeController : ControllerBase
{
    private readonly IPersonalizeService _personalizeService;

    public PersonalizeController(IPersonalizeService personalizeService)
    {
        _personalizeService = personalizeService;
    }

    // GET: api/<PersonalizeController>
    [HttpGet("get-all")]
    public async Task<IBusinessResult> Get()
    {
        var result = await _personalizeService.GetAllPersonalize();
        // Check if result or result.Data is null
        if (result == null || result.Data == null) return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
        // Ensure Data is a List<Personalize> (or any IEnumerable<Personalize>)
        var personalizes = (result.Data as IEnumerable<Personalize>)?.Select(t => new PersonalizeResponse
        {
            PersonalizeId = t.PersonalizeId,
            UserId = t.UserId,
            Shape = t.Shape,
            Theme = t.Theme,
            TankMethod = t.TankMethod,
            Type = t.Type,
            size = t.Size
        }).ToList();
        if (personalizes == null) return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, personalizes);
    }


    // GET api/<PersonalizeController>/5Add commentMore actions
    [HttpGet("get-{id}")]
    public async Task<IBusinessResult> Get(int id)
    {
        var result = await _personalizeService.GetPersonalizeById(id);

        // Kiểm tra nếu result hoặc result.Data là null
        if (result == null || result.Data == null) return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");

        // Kiểm tra kiểu dữ liệu của result.Data (đảm bảo nó là Terrarium, không phải IEnumerable)
        if (result.Data is Personalize personalize)
        {
            // Ánh xạ dữ liệu từ Category sang CategoryRequest
            var personalizeResponse = new PersonalizeResponse
            {
                PersonalizeId = personalize.PersonalizeId,
                UserId = personalize.UserId,
                Shape = personalize.Shape,
                Theme = personalize.Theme,
                TankMethod = personalize.TankMethod,
                Type = personalize.Type,
                size = personalize.Size
            };

            // Trả về BusinessResult với dữ liệu đã ánh xạ
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, personalizeResponse);
        }

        // Trả về lỗi nếu không thể ánh xạ
        return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
    }

    // POST api/<PersonalizeController>
    [HttpPost("add-personlize")]
    public async Task<IBusinessResult> Post([FromBody] PersonalizeCreateRequest personalizeCreateRequest)
    {
        //var result =
        return await _personalizeService.CreatePersonalize(personalizeCreateRequest);
        //if (result == null || result.Data == null)
        //{
        //    return new BusinessResult(Const.ERROR_EXCEPTION, "Create Fail");
        //}
        //return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, result);
    }

    // PUT api/<PersonalizeController>/5
    [HttpPut("update-{id}")]
    public async Task<IBusinessResult> Put([FromBody] PersonalizeUpdateRequest personalizeUpdateRequest)
    {
        if (personalizeUpdateRequest == null || !ModelState.IsValid)
            return new BusinessResult(Const.FAIL_UPDATE_CODE, "Update Fail");
        return await _personalizeService.UpdatePersonalize(personalizeUpdateRequest);
    }

    // DELETE api/<PersonalizeController>/5
    [HttpDelete("delete-{id}")]
    public async Task<IBusinessResult> Delete(int id)
    {
        var result = await _personalizeService.DeletePersonalizeById(id);
        if (result == null || result.Data == null) return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
        if (result.Data is bool isDeleted)
            return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Terrarium deleted successfully.");
        return new BusinessResult(Const.FAIL_DELETE_CODE, "Failed to delete personalize.");
    }
}