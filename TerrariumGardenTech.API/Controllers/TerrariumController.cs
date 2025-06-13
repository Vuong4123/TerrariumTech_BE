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
    public class TerrariumController : ControllerBase
    {
        private readonly ITerrariumService _terrariumService;
        public TerrariumController(ITerrariumService terrariumService)
        {
            _terrariumService = terrariumService;
        }   
        // GET: api/<TerrariumController>
        [HttpGet]
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

        // GET api/<TerrariumController>/5
        [HttpGet("{id}")]
        public async Task<IBusinessResult> GetById(int id)
        {
            return await _terrariumService.GetById(id);
        }

        // POST api/<TerrariumController>
        [HttpPost]
        public async Task<IBusinessResult> Post([FromBody] TerrariumCreateRequest terrariumCreate)
        {
            if (terrariumCreate == null || !ModelState.IsValid)
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Invalid request data.");
            }
            return await _terrariumService.CreateTerrarium(terrariumCreate);
        }

        // PUT api/<TerrariumController>/5
        [HttpPut("{id}")]
        public async Task<IBusinessResult> Put(TerrariumUpdateRequest terrariumUpdate)
        {
            if (terrariumUpdate == null || !ModelState.IsValid)
            {
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Invalid request data.");
            }
            return await _terrariumService.UpdateTerrarium(terrariumUpdate);
        }

        // DELETE api/<TerrariumController>/5
        [HttpDelete("{id}")]
        public async Task<IBusinessResult> Delete(int id)
        {
            return await _terrariumService.DeleteById(id);
        }
    }
}
