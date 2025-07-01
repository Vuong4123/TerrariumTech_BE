using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.TerrariumImage;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TerrariumImageController : ControllerBase
    {
        private readonly ITerrariumImageService _terrariumImageService;
        public TerrariumImageController(ITerrariumImageService terrariumImageService)
        {
            _terrariumImageService = terrariumImageService;
        }
        // GET: api/<TerrariumImageController>
        [HttpGet("get-all")]
        public async Task<IBusinessResult> Get()
        {
            return await _terrariumImageService.GetAllTerrariumImageAsync();
        }

        // GET api/<TerrariumImageController>/5
        [HttpGet("get-{id}")]
        public async Task<IBusinessResult?> Get(int id)
        {
            return await _terrariumImageService.GetTerrariumImageByIdAsync(id);
        }

        // POST api/<TerrariumImageController>
        [HttpPost("add-terrariumImage")]
        public async Task<IBusinessResult> Post([FromBody] TerrariumImageCreateRequest terrariumImageCreateRequest)
        {
            return await _terrariumImageService.CreateTerrariumImageAsync(terrariumImageCreateRequest); 
        }

        // PUT api/<TerrariumImageController>/5
        [HttpPut("update-terrariumImage-{id}")]
        public async Task<IBusinessResult> Put([FromBody] TerrariumImageUpdateRequest terrariumImageUpdateRequest)
        {
            return await _terrariumImageService.UpdateTerrariumImageAsync(terrariumImageUpdateRequest); 
        }

        // DELETE api/<TerrariumImageController>/5
        [HttpDelete("delete-terrariumImage-{id}")]
        public async Task<IBusinessResult> Delete(int id)
        {
            return await _terrariumImageService.DeleteTerrariumImageAsync(id); 
        }
    }
}
