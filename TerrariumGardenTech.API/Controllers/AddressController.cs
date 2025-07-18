﻿using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Address;
using TerrariumGardenTech.Service.ResponseModel.Address;

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
        var result = await _addressService.GetAllAddresses();
        // Check if result or result.Data is null
        if (result == null || result.Data == null) return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
        // Ensure Data is a List<Address> (or any IEnumerable<Address>)
        var addresses = (result.Data as IEnumerable<Address>)?.Select(t => new AddressResponse
        {
            Id = t.Id,
            TagName = t.TagName,
            UserId = t.UserId,
            ReceiverAddress = t.ReceiverAddress,
            ReceiverName = t.ReceiverName,
            ReceiverPhone = t.ReceiverPhone
        }).ToList();

        if (addresses == null) return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, addresses);
    }

    // GET api/<AddressController>/5
    [HttpGet("get-{id}")]
    public async Task<IBusinessResult> Get(int id)
    {
        var result = await _addressService.GetAddressById(id);
        if (result == null || result.Data == null) return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
        // Kiểm tra kiểu dữ liệu của result.Data (đảm bảo nó là Terrarium, không phải IEnumerable)
        if (result.Data is Address address)
        {
            // Ánh xạ dữ liệu từ Category sang CategoryRequest
            var addressResponse = new AddressResponse
            {
                Id = address.Id,
                TagName = address.TagName,
                UserId = address.UserId,
                ReceiverAddress = address.ReceiverAddress,
                ReceiverName = address.ReceiverName,
                ReceiverPhone = address.ReceiverPhone
            };

            // Trả về BusinessResult với dữ liệu đã ánh xạ
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, addressResponse);
        }

        // Trả về lỗi nếu không thể ánh xạ
        return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
    }

    // POST api/<AddressController>
    [HttpPost("add-address")]
    public async Task<IBusinessResult> Post([FromBody] AddressCreateRequest addressCreateRequest)
    {
        return await _addressService.CreateAddress(addressCreateRequest);
    }

    // PUT api/<AddressController>/5
    [HttpPut("uodate-adrress-{id}")]
    public async Task<IBusinessResult> Put([FromBody] AddressUpdateRequest addressUpdateRequest)
    {
        return await _addressService.UpdateAddress(addressUpdateRequest);
        //if (result == null || result.Data == null)
        //{
        //    return new BusinessResult(Const.ERROR_EXCEPTION, "Update Fail");
        //}
        //return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, result);
    }

    // DELETE api/<AddressController>/5
    [HttpDelete("{id}")]
    public async Task<IBusinessResult> Delete(int id)
    {
        return await _addressService.DeleteAddressById(id);
    }
}