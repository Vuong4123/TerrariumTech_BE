using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common.RequestModel.TerrariumImage;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TerrariumImageController : ControllerBase
{
    private readonly ITerrariumImageService _terrariumImageService;

    public TerrariumImageController(ITerrariumImageService terrariumImageService)
    {
        _terrariumImageService = terrariumImageService;
    }

    // GET: api/<TerrariumImageController>
    [HttpGet("get-all")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Get()
    {
        return await _terrariumImageService.GetAllTerrariumImageAsync();
    }

    // GET api/<TerrariumImageController>/5
    [HttpGet("get/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult?> Get(int id)
    {
        return await _terrariumImageService.GetTerrariumImageByIdAsync(id);
    }

    // GET api/<TerrariumImageController>/TerrariumId/5
    [HttpGet("get-by-terrariumId/{terrariumId}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> GetByAccessoryId(int terrariumId)
    {
        return await _terrariumImageService.GetByTerrariumId(terrariumId);
    }

    // POST api/<TerrariumImageController>
    [HttpPost("upload")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Post([FromForm] TerrariumImageUploadRequest request)
    {
        return await _terrariumImageService.CreateTerrariumImageAsync(request.ImageFile, request.TerrariumId);
    }

    // PUT api/<TerrariumImageController>/5
    [HttpPut("update-terrariumImage/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Put(int id, [FromForm] TerrariumImageUploadUpdateRequest request)
    {
        request.TerrariumImageId = id; // Set the ID for the update request
        return await _terrariumImageService.UpdateTerrariumImageAsync(request);
    }

    // DELETE api/<TerrariumImageController>/5
    [HttpDelete("delete-terrariumImage/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _terrariumImageService.DeleteTerrariumImageAsync(id);
    }
}