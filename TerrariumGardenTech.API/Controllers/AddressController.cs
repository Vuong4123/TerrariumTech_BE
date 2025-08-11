using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Address;
using TerrariumGardenTech.Common.ResponseModel.Address;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AddressController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressController(IAddressService addressService)
    {
        _addressService = addressService ?? throw new ArgumentNullException(nameof(addressService));
    }

    // GET: api/<AddressController>
    [HttpGet("get-all")]
    public async Task<IBusinessResult> Get()
    {
        return await _addressService.GetAllAddresses();
    }

    // GET api/<AddressController>/5
    [HttpGet("get/{id}")]
    public async Task<IBusinessResult> Get(int id)
    {
        return await _addressService.GetAddressById(id);
    }

    [HttpGet("getall-by-user-id/{userId}")]
    [Authorize(Roles = "Admin,Staff,Manager,User")]
    public async Task<IBusinessResult> GetByUserId(int userId)
    {
        return await _addressService.GetAddressesByUserId(userId);
    }

    // POST api/<AddressController>
    [HttpPost("add-address")]
    [Authorize(Roles = "Admin,Staff,Manager,User")]
    public async Task<IBusinessResult> Post([FromBody] AddressCreateRequest addressCreateRequest)
    {
        return await _addressService.CreateAddress(addressCreateRequest);
    }

    // PUT api/<AddressController>/5
    [HttpPut("uodate-adrress/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager,User")]
    public async Task<IBusinessResult> Put([FromBody] AddressUpdateRequest addressUpdateRequest)
    {
        return await _addressService.UpdateAddress(addressUpdateRequest);
    }

    // DELETE api/<AddressController>/5
    [HttpDelete("delete-address/{id}")]
    [Authorize(Roles = "Admin,Staff,Manager,User")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _addressService.DeleteAddressById(id);
    }
}