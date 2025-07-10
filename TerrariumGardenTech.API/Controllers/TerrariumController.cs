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
            var terrariums = terrariumList?.Select(t => new TerrariumResponse
            {
                TerrariumId = t.TerrariumId,
                Name = t.TerrariumName,
                Description = t.Description,
                Price = (decimal)t.Price,
                Stock = t.Stock,
                Status = t.Status,
                Environments = t.TerrariumEnvironments.Select(te => te.EnvironmentTerrarium.EnvironmentName).ToList(), // Assuming EnvironmentId is an int
                Shapes = t.TerrariumShapes.Select(te => te.Shape.ShapeName).ToList(),
                TankMethods = t.TerrariumTankMethods.Select(te => te.TankMethod.TankMethodType).ToList(),
                Accessories = t.TerrariumAccessory.Select(a => new TerrariumAccessoryResponse
                {
                    AccessoryId = a.Accessory.AccessoryId,
                    Name = a.Accessory.Name,
                    Description = a.Accessory.Description,
                    Price = a.Accessory.Price
                }).ToList(),
                CreatedAt = today, // Use a default value if CreatedAt is null
                UpdatedAt = today,  // Similar for UpdatedAt
                BodyHTML = t.bodyHTML,
                // Ánh xạ TerrariumImages thành một danh sách đầy đủ các thông tin
                TerrariumImages = t.TerrariumImages.Select(ti => new TerrariumImageResponse
                {
                    TerrariumImageId = ti.TerrariumImageId,
                    TerrariumId = ti.TerrariumId,
                    ImageUrl = ti.ImageUrl,
                    AltText = ti.AltText,
                    IsPrimary = ti.IsPrimary
                }).ToList()
            }).ToList();

            if (terrariums == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
            }

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", terrariums);

        }
        #region Code để dành :))
        // GET: api/<TerrariumController>
        // [HttpGet("get-details")]
        // public async Task<IBusinessResult> GetDetail()
        // {
        //     var result = await _terrariumService.GetAll();

        //     // Check if result or result.Data is null
        //     if (result == null || result.Data == null)
        //     {
        //         return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
        //     }

        //     // Ensure Data is a List<Terrarium> (or any IEnumerable<Terrarium>)
        //     var terrariums = (result.Data as IEnumerable<Terrarium>)?.Select(t => new TerrariumDetailResponse
        //     {
        //         TerrariumId = t.TerrariumId,
        //         Name = t.Name,
        //         Description = t.Description,
        //         Price = (decimal)t.Price,
        //     }).ToList();

        //     if (terrariums == null)
        //     {
        //         return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
        //     }

        //     return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", terrariums);

        // }

        // [HttpGet("get-all-form-param")]
        // public async Task<IBusinessResult> GetAll([FromQuery] string type = null,
        //                                         [FromQuery] string shape = null,
        //                                         [FromQuery] string tankMethod = null,
        //                                         [FromQuery] string theme = null,
        //                                         [FromQuery] string size = null)
        // {
        //     try
        //     {
        //         // Gọi phương thức GetAll từ service để lấy dữ liệu
        //         var result = await _terrariumService.GetAllOfParam(type, shape, tankMethod, theme, size);

        //         // Check if result or result.Data is null
        //         if (result == null || result.Data == null)
        //         {
        //             return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
        //         }

        //         // Ensure Data is a List<Terrarium> (or any IEnumerable<Terrarium>)
        //         var terrariums = (result.Data as IEnumerable<Terrarium>)?.Select(t => new TerrariumResponse
        //         {
        //             TerrariumId = t.TerrariumId,
        //             Name = t.Name,
        //             Description = t.Description,
        //             Price = (decimal)t.Price,
        //             Stock = t.Stock,
        //             Status = t.Status,
        //             Type = t.Type,
        //             Shape = t.Shape,
        //             TankMethod = t.TankMethod,
        //             Theme = t.Theme,
        //             CreatedAt = t.CreatedAt ?? DateTime.MinValue, // Use a default value if CreatedAt is null
        //             UpdatedAt = t.UpdatedAt ?? DateTime.MinValue,  // Similar for UpdatedAt
        //             AccessoryId = t.AccessoryId ?? 0,// If nullable, default to 0 if null
        //             Size = t.Size,
        //             BodyHTML = t.bodyHTML
        //         }).ToList();

        //         if (terrariums == null)
        //         {
        //             return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
        //         }

        //         return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", terrariums);
        //     }
        //     catch (Exception ex)
        //     {
        //         // Xử lý ngoại lệ nếu có lỗi trong quá trình lấy dữ liệu
        //         return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        //     }
        // }

        // GET api/terrarium
        // [HttpGet("get-all-form-param-detail")]
        // public async Task<IBusinessResult> GetAllOfParamDetail([FromQuery] string type = null,
        //                                         [FromQuery] string shape = null,
        //                                         [FromQuery] string tankMethod = null,
        //                                         [FromQuery] string theme = null,
        //                                         [FromQuery] string size = null)
        // {
        //     try
        //     {
        //         // Gọi phương thức GetAll từ service để lấy dữ liệu
        //         var result = await _terrariumService.GetAllOfParam(type, shape, tankMethod, theme, size);

        //         // Check if result or result.Data is null
        //         if (result == null || result.Data == null)
        //         {
        //             return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
        //         }

        //         // Ensure Data is a List<Terrarium> (or any IEnumerable<Terrarium>)
        //         var terrariums = (result.Data as IEnumerable<Terrarium>)?.Select(t => new TerrariumDetailResponse
        //         {
        //             TerrariumId = t.TerrariumId,
        //             Name = t.Name,
        //             Description = t.Description,
        //             Price = (decimal)t.Price,
        //         }).ToList();

        //         if (terrariums == null)
        //         {
        //             return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
        //         }

        //         return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", terrariums);
        //     }
        //     catch (Exception ex)
        //     {
        //         // Xử lý ngoại lệ nếu có lỗi trong quá trình lấy dữ liệu
        //         return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        //     }
        // }
        #endregion

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
                    Name = terrarium.TerrariumName,
                    Description = terrarium.Description,
                    Price = (decimal)terrarium.Price  ,
                    Stock = terrarium.Stock,
                    Status = terrarium.Status,
                    Environments = terrarium.TerrariumEnvironments.Select(te => te.EnvironmentTerrarium.EnvironmentName).ToList(),
                    Shapes = terrarium.TerrariumShapes.Select(s => s.Shape.ShapeName).ToList(),
                    TankMethods = terrarium.TerrariumTankMethods.Select(ta => ta.TankMethod.TankMethodType).ToList(),
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
                        AltText = ti.AltText,
                        IsPrimary = ti.IsPrimary
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
