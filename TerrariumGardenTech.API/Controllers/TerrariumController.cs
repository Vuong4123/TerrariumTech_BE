using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Category;
using TerrariumGardenTech.Service.RequestModel.Terrarium;
using TerrariumGardenTech.Service.ResponseModel.Terrarium;
// using TerrariumGardenTech.Service.Service;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin,Staff")]
    public class TerrariumController : ControllerBase
    {
        private readonly ITerrariumService _terrariumService;
        public DateTime today = DateTime.Now;
        public TerrariumController(ITerrariumService terrariumService)
        {
            _terrariumService = terrariumService;
        }
        // GET: api/<TerrariumController>
        [HttpGet("get-all")]
        public async Task<IBusinessResult> Get()
        {
            var result = await _terrariumService.GetAll();

            // Check if result or result.Data is null
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }

            // Ensure Data is a List<Terrarium> (or any IEnumerable<Terrarium>)
            var terrariumList = result.Data as IEnumerable<Terrarium>;
            var terrariums = terrariumList?.Select(t => new TerrariumDetailResponse
            {
                TerrariumId = t.TerrariumId,
                Name = t.TerrariumName,
                Description = t.Description,
                Price = (decimal)t.Price,
                Stock = t.Stock,
                Status = t.Status,
                TerrariumImages = t.TerrariumImages.Select(ti => new TerrariumImageResponse
                {
                    TerrariumImageId = ti.TerrariumImageId,
                    TerrariumId = ti.TerrariumId,
                    ImageUrl = ti.ImageUrl,
                }).ToList()
            }).ToList();

            if (terrariums == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
            }

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", terrariums);

        }
        [HttpGet("filter")]
        public async Task<IBusinessResult> FilterTerrariums([FromQuery] int? environmentId, [FromQuery] int? shapeId, [FromQuery] int? tankMethodId)
        {
            var result = await _terrariumService.FilterTerrariumsAsync(environmentId, shapeId, tankMethodId);

            if (result?.Data == null)
            {
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No data found.");
            }

            var terrariumList = result.Data as IEnumerable<Terrarium>;
            if (terrariumList == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Unexpected data format.");
            }

            var terrariums = terrariumList.Select(t => new TerrariumDetailResponse
            {
                TerrariumId = t.TerrariumId,
                Name = t.TerrariumName,
                Description = t.Description,
                Price = t.Price ?? 0,
                Stock = t.Stock,
                Status = t.Status,
                TerrariumImages = t.TerrariumImages?.Select(ti => new TerrariumImageResponse
                {
                    TerrariumImageId = ti.TerrariumImageId,
                    TerrariumId = ti.TerrariumId,
                    ImageUrl = ti.ImageUrl,
                }).ToList() ?? new List<TerrariumImageResponse>()
            }).ToList();

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", terrariums);
        }


        // GET api/<TerrariumController>/5
        [HttpGet("get-{id}")]
        public async Task<IBusinessResult> GetById(int id)
        {

            var result = await _terrariumService.GetById(id);

            // Kiểm tra nếu result hoặc result.Data là null
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }

            // Kiểm tra kiểu dữ liệu của result.Data (đảm bảo nó là Category, không phải IEnumerable)
            if (result.Data is Terrarium terrarium)
            {
                // Ánh xạ dữ liệu từ Category sang CategoryRequest
                var terrariumResponse = new TerrariumResponse
                {
                    TerrariumId = terrarium.TerrariumId,
                    EnvironmentId = terrarium.EnvironmentId,
                    ShapeId = terrarium.ShapeId,
                    TankMethodId = terrarium.TankMethodId,
                    Name = terrarium.TerrariumName,
                    Description = terrarium.Description,
                    Price = (decimal)terrarium.Price  ,
                    Stock = terrarium.Stock,
                    Status = terrarium.Status,
                    Accessories = terrarium.TerrariumAccessory.Select(a => new TerrariumAccessoryResponse
                    {
                        AccessoryId = a.Accessory.AccessoryId,
                        Name = a.Accessory.Name,
                        Description = a.Accessory.Description,
                        Price = a.Accessory.Price
                    }).ToList(),
                    BodyHTML = terrarium.bodyHTML,
                    CreatedAt = today, // Use a default value if CreatedAt is null
                    UpdatedAt = today, // Similar for UpdatedAt
                                       // Ánh xạ TerrariumImages thành một danh sách đầy đủ các thông tin
                    TerrariumImages = terrarium.TerrariumImages.Select(ti => new TerrariumImageResponse
                    {
                        TerrariumImageId = ti.TerrariumImageId,
                        TerrariumId = ti.TerrariumId,
                        ImageUrl = ti.ImageUrl,
                    }).ToList()
                };

                // Trả về BusinessResult với dữ liệu đã ánh xạ
                return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", terrariumResponse);
            }

            return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
        }

        // POST api/<TerrariumController>
        [HttpPost("add-terrarium")]
        public async Task<IBusinessResult> Post([FromBody] TerrariumCreateRequest terrariumCreate)
        {
            if (terrariumCreate == null || !ModelState.IsValid)
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Invalid request data.");
            }
            return await _terrariumService.CreateTerrarium(terrariumCreate);
        }

        // PUT api/<TerrariumController>/5
        [HttpPut("update-terrarium-{id}")]
        public async Task<IBusinessResult> Put( TerrariumUpdateRequest terrariumUpdate)
        {
            if (terrariumUpdate == null || !ModelState.IsValid)
            {
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Invalid request data.");
            }
            return await _terrariumService.UpdateTerrarium(terrariumUpdate);
        }

        // DELETE api/<TerrariumController>/5
        [HttpDelete("delete-terraium-{id}")]
        public async Task<IBusinessResult> Delete(int id)
        {
           return await _terrariumService.DeleteById(id);
        }
    }
}
