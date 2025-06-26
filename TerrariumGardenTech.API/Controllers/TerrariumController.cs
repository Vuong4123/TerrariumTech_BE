using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Terrarium;
using TerrariumGardenTech.Service.ResponseModel.Terrarium;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(Roles = "Admin,Staff")]
    public class TerrariumController : ControllerBase
    {
        private readonly ITerrariumService _terrariumService;
        public TerrariumController(ITerrariumService terrariumService)
        {
            _terrariumService = terrariumService;
        }
        // GET: api/<TerrariumController>
        [HttpGet("get-all")]
        public async Task<IBusinessResult> Get()
        {
            var result =  await _terrariumService.GetAll();

            // Check if result or result.Data is null
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }

            // Ensure Data is a List<Terrarium> (or any IEnumerable<Terrarium>)
            var terrariums = (result.Data as IEnumerable<Terrarium>)?.Select(t => new TerrariumResponse
            {
                TerrariumId = t.TerrariumId,
                Name = t.Name,
                Description = t.Description,
                Price = (decimal)t.Price,
                Stock = t.Stock,
                Status = t.Status,
                Type = t.Type,
                Shape = t.Shape,
                TankMethod = t.TankMethod,
                Theme = t.Theme,
                CreatedAt = t.CreatedAt ?? DateTime.MinValue, // Use a default value if CreatedAt is null
                UpdatedAt = t.UpdatedAt ?? DateTime.MinValue,  // Similar for UpdatedAt
                AccessoryId = t.AccessoryId ?? 0,// If nullable, default to 0 if null
                Size = t.Size,
                BodyHTML = t.bodyHTML
            }).ToList();

            if (terrariums == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
            }

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", terrariums);

        }
        // GET: api/<TerrariumController>
        [HttpGet("get-details")]
        public async Task<IBusinessResult> GetDetail()
        {
            var result = await _terrariumService.GetAll();

            // Check if result or result.Data is null
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }

            // Ensure Data is a List<Terrarium> (or any IEnumerable<Terrarium>)
            var terrariums = (result.Data as IEnumerable<Terrarium>)?.Select(t => new TerrariumDetailResponse
            {
                TerrariumId = t.TerrariumId,
                Name = t.Name,
                Description = t.Description,
                Price = (decimal)t.Price,
            }).ToList();

            if (terrariums == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
            }

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", terrariums);

        }

        // GET api/terrarium
        [HttpGet("get-all-form-param")]
        public async Task<IBusinessResult> GetAll([FromQuery] string type = null,
                                                [FromQuery] string shape = null,
                                                [FromQuery] string tankMethod = null,
                                                [FromQuery] string theme = null,
                                                [FromQuery] int? accessoryId = null,
                                                [FromQuery] string size = null)
        {
            try
            {
                // Gọi phương thức GetAll từ service để lấy dữ liệu
                var result = await _terrariumService.GetAllOfParam(type, shape, tankMethod, theme, accessoryId, size);

                // Check if result or result.Data is null
                if (result == null || result.Data == null)
                {
                    return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
                }

                // Ensure Data is a List<Terrarium> (or any IEnumerable<Terrarium>)
                var terrariums = (result.Data as IEnumerable<Terrarium>)?.Select(t => new TerrariumResponse
                {
                    TerrariumId = t.TerrariumId,
                    Name = t.Name,
                    Description = t.Description,
                    Price = (decimal)t.Price,
                    Stock = t.Stock,
                    Status = t.Status,
                    Type = t.Type,
                    Shape = t.Shape,
                    TankMethod = t.TankMethod,
                    Theme = t.Theme,
                    CreatedAt = t.CreatedAt ?? DateTime.MinValue, // Use a default value if CreatedAt is null
                    UpdatedAt = t.UpdatedAt ?? DateTime.MinValue,  // Similar for UpdatedAt
                    AccessoryId = t.AccessoryId ?? 0,// If nullable, default to 0 if null
                    Size = t.Size,
                    BodyHTML = t.bodyHTML
                }).ToList();

                if (terrariums == null)
                {
                    return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
                }

                return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", terrariums);
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có lỗi trong quá trình lấy dữ liệu
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
            }
        }

        // GET api/terrarium
        [HttpGet("get-all-form-param-detail")]
        public async Task<IBusinessResult> GetAllOfParamDetail([FromQuery] string type = null,
                                                [FromQuery] string shape = null,
                                                [FromQuery] string tankMethod = null,
                                                [FromQuery] string theme = null,
                                                [FromQuery] int? accessoryId = null,
                                                [FromQuery] string size = null)
        {
            try
            {
                // Gọi phương thức GetAll từ service để lấy dữ liệu
                var result = await _terrariumService.GetAllOfParam(type, shape, tankMethod, theme, accessoryId, size);

                // Check if result or result.Data is null
                if (result == null || result.Data == null)
                {
                    return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
                }

                // Ensure Data is a List<Terrarium> (or any IEnumerable<Terrarium>)
                var terrariums = (result.Data as IEnumerable<Terrarium>)?.Select(t => new TerrariumDetailResponse
                {
                    TerrariumId = t.TerrariumId,
                    Name = t.Name,
                    Description = t.Description,
                    Price = (decimal)t.Price,
                }).ToList();

                if (terrariums == null)
                {
                    return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
                }

                return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", terrariums);
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu có lỗi trong quá trình lấy dữ liệu
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
            }
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

            // Kiểm tra kiểu dữ liệu của result.Data (đảm bảo nó là Terrarium, không phải IEnumerable)
            if (result.Data is Terrarium terrarium)
            {
                // Ánh xạ dữ liệu từ Category sang CategoryRequest
                var terrariumResponse = new TerrariumResponse
                {
                    TerrariumId = terrarium.TerrariumId,
                    Name = terrarium.Name,
                    Description = terrarium.Description,
                    Price = (decimal)terrarium.Price,
                    Stock = terrarium.Stock,
                    Status = terrarium.Status,
                    Type = terrarium.Type,
                    Shape = terrarium.Shape,
                    TankMethod = terrarium.TankMethod,
                    Theme = terrarium.Theme,
                    CreatedAt = terrarium.CreatedAt ?? DateTime.MinValue, // Use a default value if CreatedAt is null
                    UpdatedAt = terrarium.UpdatedAt ?? DateTime.MinValue,  // Similar for UpdatedAt
                    AccessoryId = terrarium.AccessoryId ?? 0,// If nullable, default to 0 if null
                    Size = terrarium.Size,
                    BodyHTML = terrarium.bodyHTML
                };

                // Trả về BusinessResult với dữ liệu đã ánh xạ
                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, terrariumResponse);
            }

            // Trả về lỗi nếu không thể ánh xạ
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
        [HttpPut("update-terrarium{id}")]
        public async Task<IBusinessResult> Put([FromBody] TerrariumUpdateRequest terrariumUpdate)
        {
            if (terrariumUpdate == null || !ModelState.IsValid)
            {
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Invalid request data.");
            }
            return await _terrariumService.UpdateTerrarium(terrariumUpdate);
        }

        // DELETE api/<TerrariumController>/5
        [HttpDelete("delete-terraium{id}")]
        public async Task<IBusinessResult> Delete(int id)
        {
            var result = await _terrariumService.DeleteById(id);
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }
            if (result.Data is bool isDeleted)
            {
                return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Terrarium deleted successfully.");
            }
            return new BusinessResult(Const.FAIL_DELETE_CODE, "Failed to delete terrarium.");
        }
    }
}
