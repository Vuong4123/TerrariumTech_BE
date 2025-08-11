using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common.RequestModel.AccessoryImage;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccessoryImageController(IAccessoryImageService _accessoryImageService) : ControllerBase
{
    // GET: api/<AccessoryImageController>
    [HttpGet("get-all")]

    public async Task<IBusinessResult> Get()
    {
        return await _accessoryImageService.GetAll();
    }

    // GET api/<AccessoryImageController>/5
    [HttpGet("get-by/{id}")]
    public async Task<IBusinessResult> Get(int id)
    {
        return await _accessoryImageService.GetById(id);
    }


    // GET api/<AccessoryImageController>/accessoryId/5
    [HttpGet("get-accessoryId/{accessoryId}")]
    public async Task<IBusinessResult> GetByAccessoryId(int accessoryId)
    {
        return await _accessoryImageService.GetByAccessoryId(accessoryId);
    }


    // POST api/<AccessoryImageController>
    [HttpPost("add-accessoryimage")]
    [Authorize(Roles = "Admin,Staff,Manager,User")]
    public async Task<IBusinessResult> Post([FromForm] AccessoryImageUploadRequest request)
    {
        return await _accessoryImageService.CreateAccessoryImage(request.ImageFile, request.AccessoryId);
    }

    // PUT api/<AccessoryImageController>/5
    [HttpPut("update-accessoryimage/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager,User")]
    public async Task<IBusinessResult> Put(int id, [FromForm] AccessoryImageUploadUpdateRequest request)
    {
        request.AccessoryImageId = id; // Set the ID for the update request
        return await _accessoryImageService.UpdateAccessoryImage(request);
    }

    // DELETE api/<AccessoryImageController>/5
    [HttpDelete("delete-accessoryimage/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager,User")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _accessoryImageService.DeleteById(id);
    }
}