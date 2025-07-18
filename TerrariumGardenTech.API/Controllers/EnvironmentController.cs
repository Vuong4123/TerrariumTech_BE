using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Environment;

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
    [HttpGet]
    public async Task<IBusinessResult> Get()
    {
        return await _environmentService.GetAllEnvironmentsAsync();
    }

    // GET api/<EnvironmentController>/5
    [HttpGet("{id}")]
    public async Task<IBusinessResult> Get(int id)
    {
        return await _environmentService.GetEnvironmentByIdAsync(id);
    }

    // POST api/<EnvironmentController>
    [HttpPost]
    public async Task<IBusinessResult> Post([FromBody] EnvironmentCreateRequest environmentCreateRequest)
    {
        return await _environmentService.CreateEnvironmentAsync(environmentCreateRequest);
    }

    // PUT api/<EnvironmentController>/5
    [HttpPut("{id}")]
    public async Task<IBusinessResult> Put([FromBody] EnvironmentUpdateRequest environmentUpdateRequest)
    {
        return await _environmentService.UpdateEnvironmentAsync(environmentUpdateRequest);
    }

    // DELETE api/<EnvironmentController>/5
    [HttpDelete("{id}")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _environmentService.DeleteEnvironmentAsync(id);
    }
}