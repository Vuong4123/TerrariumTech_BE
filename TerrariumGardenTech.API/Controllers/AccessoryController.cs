using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Accessory;
using TerrariumGardenTech.Service.ResponseModel.Accessory;
using TerrariumGardenTech.Service.ResponseModel.Terrarium;
using TerrariumGardenTech.Service.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin,Staff")]
    public class AccessoryController : ControllerBase
    {
        private readonly IAccessoryService _accessoryService;
        public AccessoryController(IAccessoryService accessoryService)
        {
            _accessoryService = accessoryService ?? throw new ArgumentNullException(nameof(accessoryService));
        }
        // GET: api/<AccessoryController>
        [HttpGet("get-all")]
        public async Task<IBusinessResult> Get()
        {
            var result = await _accessoryService.GetAll();
            // Check if result or result.Data is null
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }

            // Ensure Data is a List<Terrarium> (or any IEnumerable<Terrarium>)
            var accsessory = (result.Data as IEnumerable<Accessory>)?.Select(a => new AccessoryResponse
            {
                AccessoryId = a.AccessoryId,
                Name = a.Name,
                Description = a.Description,
                Price = (decimal)a.Price,
                Stock = a.StockQuantity,
                Status = a.Status,
                CategoryId = a.CategoryId,
                CreatedAt = a.CreatedAt ?? DateTime.MinValue, // Use a default value if CreatedAt is null
                UpdatedAt = a.UpdatedAt ?? DateTime.MinValue,  // Similar for UpdatedAt

            }).ToList();

            if (accsessory == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
            }

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", accsessory);
        }

        [HttpGet("get-details")]
        public async Task<IBusinessResult> GetDetail()
        {
            var result = await _accessoryService.GetAll();
            // Check if result or result.Data is null
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }

            // Ensure Data is a List<Terrarium> (or any IEnumerable<Terrarium>)
            var accsessory = (result.Data as IEnumerable<Accessory>)?.Select(a => new AccessoryDetailResponse
            {
                AccessoryId = a.AccessoryId,
                Name = a.Name,
                Description = a.Description,
                Price = (decimal)a.Price,

            }).ToList();

            if (accsessory == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
            }

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", accsessory);
        }

        // GET api/<AccessoryController>/5
        [HttpGet("get-{id}")]
        public async Task<IBusinessResult> Get(int id)
        {
            var result = await _accessoryService.GetById(id);

            // Kiểm tra nếu result hoặc result.Data là null
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }

            // Kiểm tra kiểu dữ liệu của result.Data (đảm bảo nó là Category, không phải IEnumerable)
            if (result.Data is Accessory accessory)
            {
                // Ánh xạ dữ liệu từ Category sang CategoryRequest
                var accessorryResponse = new AccessoryResponse
                {
                    AccessoryId = accessory.AccessoryId,
                    Name = accessory.Name,
                    Description = accessory.Description,
                    Price = (decimal)accessory.Price,
                    Stock = accessory.StockQuantity,
                    Status = accessory.Status,
                    CategoryId = accessory.CategoryId,
                    CreatedAt = accessory.CreatedAt ?? DateTime.MinValue, // Use a default value if CreatedAt is null
                    UpdatedAt = accessory.UpdatedAt ?? DateTime.MinValue,  // Similar for UpdatedAt
                };

                // Trả về BusinessResult với dữ liệu đã ánh xạ
                return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", accessorryResponse);
            }

            // Trả về lỗi nếu không thể ánh xạ
            return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
        }

        // POST api/<AccessoryController>
        [HttpPost("add-accessory")]
        public async Task<IBusinessResult> Post(AccessoryCreateRequest accessoryCreateRequest)
        {
            if (accessoryCreateRequest == null || !ModelState.IsValid)
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Invalid request data.");
            }
            return await _accessoryService.CreateAccessory(accessoryCreateRequest);
        }

        // PUT api/<AccessoryController>/5
        [HttpPut("update-accessory{id}")]
        public async Task<IBusinessResult> Put(AccessoryUpdateRequest accessoryUpdateRequest)
        {
            if (accessoryUpdateRequest == null || !ModelState.IsValid)
            {
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Invalid request data.");
            }
            return await _accessoryService.UpdateAccessory(accessoryUpdateRequest);
        }

        // DELETE api/<AccessoryController>/5
        [HttpDelete("delete-accessory{id}")]
        public async Task<IBusinessResult> Delete(int id)
        {
            var result = await _accessoryService.DeleteById(id);
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }
            if (result.Data is bool isDeleted)
            {
                return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Accessory deleted successfully.");
            }
            return new BusinessResult(Const.FAIL_DELETE_CODE, "Failed to delete role.");
        }
    }
}
