using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.Common.RequestModel.Terrarium;
using TerrariumGardenTech.Service.Service;
using TerrariumGardenTech.Service.IService;

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
        public async Task<TerrariumCreateRequest> AutoGenerate([FromBody] AITerrariumRequest aiRequest)
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
                //AccessoryNames = aiResult.Accessories, 
                TerrariumImages= aiResult.TerrariumImages                 
                
            };
            return createRequest;
            //var result = await _terrariumService.CreateTerrariumAI(createRequest);
            //return result;
        }


    }
}
