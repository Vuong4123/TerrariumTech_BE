using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using TerrariumGardenTech.Common.RequestModel.Terrarium;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.Service;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AIController : ControllerBase
    {
        private readonly ITerrariumService _terrariumService;
        public AIController(ITerrariumService terrariumService)
        {
            _terrariumService = terrariumService;
        }

        [HttpPost("auto-generate")]
        public async Task<IBusinessResult> AutoGenerate([FromBody] AITerrariumRequest aiRequest)
        {
            var aiResult = await _terrariumService.PredictTerrariumAsync(aiRequest);

            var createRequest = new TerrariumCreateRequest
            {
                EnvironmentId = aiRequest.EnvironmentId,
                ShapeId = aiRequest.ShapeId,
                TankMethodId = aiRequest.TankMethodId,
                TerrariumName = aiResult.TerrariumName,
                MinPrice=aiResult.MinPrice,
                MaxPrice=aiResult.MaxPrice,
                Stock=aiResult.Stock,
                Description = aiResult.Description,
                bodyHTML = $"<p>{aiResult.Description}</p>",
                Status = "Active",
                AccessoryNames = new List<string> { "Hình hộp chữ nhật nhỏ", "Hình hộp chữ nhật vừa", "Hình hộp chữ nhật lớn" }, 
                TerrariumImages= aiResult.TerrariumImages                 
                
            };

            var result = await _terrariumService.CreateTerrarium(createRequest);
            return result;
        }


    }
}
