// TerrariumGardenTech.API.Controllers/PersonalizeController.cs
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common.RequestModel.Personalize;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

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

    // GET: api/Personalize/get-all
    [HttpGet("get-all")]
    public async Task<IBusinessResult> Get()
    {
        return await _personalizeService.GetAllPersonalize();
    }

    // GET: api/Personalize/get-by/5
    [HttpGet("get-by/{id:int}")]
    public async Task<IBusinessResult> Get(int id)
    {
        return await _personalizeService.GetPersonalizeById(id);
    }

    // GET: api/Personalize/get-by-userId/123
    [HttpGet("get-by-userId/{userId:int}")]
    public async Task<IBusinessResult> GetByUserId(int userId)
    {
        // FIX: gọi đúng service GetPersonalizeByUserId
        return await _personalizeService.GetPersonalizeByUserId(userId);
    }

    // POST: api/Personalize/add-personalize
    [HttpPost("add-personalize")]
    public async Task<IBusinessResult> Post([FromBody] PersonalizeCreateRequest personalizeCreateRequest)
    {
        return await _personalizeService.CreatePersonalize(personalizeCreateRequest);
    }

    // PUT: api/Personalize/update-personalize/5
    [HttpPut("update-personalize/{id:int}")]
    public async Task<IBusinessResult> Put(int id, [FromBody] PersonalizeUpdateRequest personalizeUpdateRequest)
    {
        // đảm bảo id route khớp body
        personalizeUpdateRequest.PersonalizeId = id;
        return await _personalizeService.UpdatePersonalize(personalizeUpdateRequest);
    }

    // DELETE: api/Personalize/delete-personalize/5
    [HttpDelete("delete-personalize/{id:int}")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _personalizeService.DeletePersonalizeById(id);
    }
}
