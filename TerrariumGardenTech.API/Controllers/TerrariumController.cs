using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Terrarium;

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
            return await _terrariumService.GetAll();
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
