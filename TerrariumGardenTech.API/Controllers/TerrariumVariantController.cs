using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.TerrariumVariant;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TerrariumVariantController : ControllerBase
{
    private readonly ITerrariumVariantService _terrariumVariantService;

    public TerrariumVariantController(ITerrariumVariantService terrariumVariantService)
    {
        _terrariumVariantService =
            terrariumVariantService ?? throw new ArgumentNullException(nameof(terrariumVariantService));
    }

    // GET: api/<TerrariumVariantController>
    [HttpGet("get-all-terrariumVariant")]
    public async Task<IBusinessResult> Get()
    {
        return await _terrariumVariantService.GetAllTerrariumVariantAsync();
    }

    // GET api/<TerrariumVariantController>/5
    [HttpGet("get-terrariumVariant-{id}")]
    public async Task<IBusinessResult> Get(int id)
    {
        return await _terrariumVariantService.GetTerrariumVariantByIdAsync(id);
    }
    // GET api/<TerrariumVariantController>/5
    [HttpGet("get-VariantByTerrarium-{id}")]
    public async Task<IBusinessResult> GetByTerrariumId(int id)
    {
        return await _terrariumVariantService.GetAllVariantByTerrariumIdAsync(id);
    }
    // POST api/<TerrariumVariantController>
    [HttpPost("create-terrariumVariant")]
    public async Task<IBusinessResult> Post([FromForm] TerrariumVariantCreateRequest terrariumVariantCreateRequest)
    {
        return await _terrariumVariantService.CreateTerrariumVariantAsync(terrariumVariantCreateRequest);
    }

    // PUT api/<TerrariumVariantController>/5
    [HttpPut("update-terrariumVariant-{id}")]
    public async Task<IBusinessResult> Put([FromForm] TerrariumVariantUpdateRequest terrariumVariantUpdateRequest)
    {
        return await _terrariumVariantService.UpdateTerrariumVariantAsync(terrariumVariantUpdateRequest);
    }

    // DELETE api/<TerrariumVariantController>/5
    [HttpDelete("delete-terrariumVariant-{id}")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _terrariumVariantService.DeleteTerrariumVariantAsync(id);
    }
}