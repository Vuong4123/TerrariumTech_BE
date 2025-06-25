using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.ResponseModel.Personalize;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PersonalizeController : ControllerBase
    {

        private readonly IPersonalizeService _personalizeService;
        public PersonalizeController(IPersonalizeService personalizeService)
        {
            _personalizeService = personalizeService;
        }

        // GET: api/<PersonalizeController>
        [HttpGet("get-all")]
        public async Task<IBusinessResult> Get()
        {
            var result = await _personalizeService.GetAllPersonalize();
            // Check if result or result.Data is null
            if (result == null || result.Data == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "No data found.");
            }
            // Ensure Data is a List<Personalize> (or any IEnumerable<Personalize>)
            var personalizes = (result.Data as IEnumerable<Personalize>)?.Select(t => new PersonalizeResponse
            {
                PersonalizeId = t.PersonalizeId,
                UserId = t.UserId,  
                Shape = t.Shape,
                Theme = t.Theme,
                TankMethod = t.TankMethod,
                Type = t.Type,
                size = t.size
            }).ToList();
            if (personalizes == null)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, "Data could not be mapped.");
            }
            return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, personalizes);
        }

        // GET api/<PersonalizeController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<PersonalizeController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<PersonalizeController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<PersonalizeController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
