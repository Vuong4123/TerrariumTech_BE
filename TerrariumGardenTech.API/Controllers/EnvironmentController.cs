using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common.RequestModel.Environment;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class EnvironmentController : ControllerBase
{
    private readonly IEnvironmentService _environmentService;

    public EnvironmentController(IEnvironmentService environmentService)
    {
        _environmentService = environmentService;
    }

    // GET: api/<EnvironmentController>
    [HttpGet("get-all")]
    //[Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Get()
    {
        return await _environmentService.GetAllEnvironmentsAsync();
    }

    // GET api/<EnvironmentController>/5
    [HttpGet("get/{id}")]
    public async Task<IBusinessResult> Get(int id)
    {
        return await _environmentService.GetEnvironmentByIdAsync(id);
    }

    // POST api/<EnvironmentController>
    [HttpPost("add-environment")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Post([FromBody] EnvironmentCreateRequest environmentCreateRequest)
    {
        return await _environmentService.CreateEnvironmentAsync(environmentCreateRequest);
    }

    // PUT api/<EnvironmentController>/5
    [HttpPut("update-environment/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Put([FromBody] EnvironmentUpdateRequest environmentUpdateRequest)
    {
        return await _environmentService.UpdateEnvironmentAsync(environmentUpdateRequest);
    }

    // DELETE api/<EnvironmentController>/5
    [HttpDelete("delete-environment/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _environmentService.DeleteEnvironmentAsync(id);
    }
}