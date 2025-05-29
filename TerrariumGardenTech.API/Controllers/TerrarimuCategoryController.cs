using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.TerrariumCategory;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TerrarimuCategoryController : ControllerBase
    {
        private readonly ITerrariumCategoryService _terrarimuCategoryService;
        public TerrarimuCategoryController(ITerrariumCategoryService terrarimuCategoryService)
        {
            _terrarimuCategoryService = terrarimuCategoryService ?? throw new ArgumentNullException(nameof(terrarimuCategoryService));
        }
        // GET: api/<TerrarimuCategoryController>
        [HttpGet]
        public async Task<IBusinessResult> Get()
        {
            return await _terrarimuCategoryService.GetAll();
        }

        // GET api/<TerrarimuCategoryController>/5
        [HttpGet("{id}")]
        public async Task<IBusinessResult> Get(int id)
        {
            return await _terrarimuCategoryService.GetById(id) ;
        }

        // POST api/<TerrarimuCategoryController>
        [HttpPost]
        public async Task<IBusinessResult> Post(TerrariumCategoryRequest terrariumCategoryRequest)
        {
            if (terrariumCategoryRequest == null || !ModelState.IsValid)
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Invalid request data.");
            }
            return await _terrarimuCategoryService.CreateTerrariumCategory(terrariumCategoryRequest);
        }

        // PUT api/<TerrarimuCategoryController>/5
        [HttpPut("{id}")]
        public async Task<IBusinessResult> Put(TerrariumCategoryRequest terrariumCategoryRequest)
        {
            if (terrariumCategoryRequest == null || !ModelState.IsValid)
            {
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Invalid request data.");
            }
            return await _terrarimuCategoryService.UpdateTerrariumCategory(terrariumCategoryRequest);
        }

        // DELETE api/<TerrarimuCategoryController>/5
        [HttpDelete("{id}")]
        public async Task<IBusinessResult> Delete(int id)
        {
            return await _terrarimuCategoryService.DeleteById(id);
        }
    }
}
