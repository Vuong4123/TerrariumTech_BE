using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccessoryImageController(IAccessoryImageService _accessoryImageService) : ControllerBase
{
    // GET: api/<AccessoryImageController>
    [HttpGet]
    public async Task<IBusinessResult> Get()
    {
        return await _accessoryImageService.GetAll();
    }

    // GET api/<AccessoryImageController>/5
    [HttpGet("{id}")]
    public async Task<IBusinessResult> Get(int id)
    {
        return await _accessoryImageService.GetById(id);
    }


    // GET api/<AccessoryImageController>/accessoryId/5
    [HttpGet("accessoryId/{accessoryId}")]
    public async Task<IBusinessResult> GetByAccessoryId(int accessoryId)
    {
        return await _accessoryImageService.GetByAccessoryId(accessoryId);
    }


    // POST api/<AccessoryImageController>
    [HttpPost("upload")]
    public async Task<IBusinessResult> Post([FromQuery] int accessoryId, IFormFile imageFile)
    {
        return await _accessoryImageService.CreateAccessory(imageFile, accessoryId);
    }

    // PUT api/<AccessoryImageController>/5
    [HttpPut("{id}")]
    public async Task<IBusinessResult> Put(int id, [FromQuery] IFormFile? imageFile)
    {
        return await _accessoryImageService.UpdateAccessory(id, imageFile);
    }

    // DELETE api/<AccessoryImageController>/5
    [HttpDelete("{id}")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _accessoryImageService.DeleteById(id);
    }
}