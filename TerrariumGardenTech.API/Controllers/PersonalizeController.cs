using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Personalize;
using TerrariumGardenTech.Common.ResponseModel.Personalize;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

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
        return await _personalizeService.GetAllPersonalize();
    }


    // GET api/<PersonalizeController>/5Add commentMore actions
    [HttpGet("get-{id}")]
    public async Task<IBusinessResult> Get(int id)
    {
        return await _personalizeService.GetPersonalizeById(id);
    }

    // POST api/<PersonalizeController>
    [HttpPost("add-personlize")]
    public async Task<IBusinessResult> Post([FromBody] PersonalizeCreateRequest personalizeCreateRequest)
    {
        return await _personalizeService.CreatePersonalize(personalizeCreateRequest);
    }

    // PUT api/<PersonalizeController>/5
    [HttpPut("update-{id}")]
    public async Task<IBusinessResult> Put([FromBody] PersonalizeUpdateRequest personalizeUpdateRequest)
    {
        return await _personalizeService.UpdatePersonalize(personalizeUpdateRequest);
    }

    // DELETE api/<PersonalizeController>/5
    [HttpDelete("delete-{id}")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return  await _personalizeService.DeletePersonalizeById(id);
    }
}