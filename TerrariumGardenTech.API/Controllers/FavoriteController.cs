using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TerrariumGardenTech.API.Extensions;
using TerrariumGardenTech.Common.RequestModel.Favorite;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FavoriteController : ControllerBase
    {
        private readonly IFavoriteService _svc;
        public FavoriteController(IFavoriteService svc) => _svc = svc;

        [HttpPost]
        public async Task<IActionResult> Like([FromBody] FavoriteCreateRequest req)
        {
            var userId = User.GetUserId();
            var rs = await _svc.LikeProductAsync(userId, req);
            return Ok(rs);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userId = User.GetUserId();
            var rs = await _svc.GetAllLikesAsync(userId);
            return Ok(rs);
        }

        // DELETE api/favorites/by-product?accessoryId=1  hoặc ?terrariumId=2
        [HttpDelete("by-product")]
        public async Task<IActionResult> DeleteByProduct([FromQuery] int? accessoryId, [FromQuery] int? terrariumId)
        {
            var userId = User.GetUserId();
            var rs = await _svc.DeleteLikeByProductAsync(userId, accessoryId, terrariumId);
            return Ok(rs);
        }

        // DELETE api/favorites/123
        [HttpDelete("{favoriteId:int}")]
        public async Task<IActionResult> DeleteById([FromRoute] int favoriteId)
        {
            var userId = User.GetUserId();
            var rs = await _svc.DeleteLikeByIdAsync(userId, favoriteId);
            return Ok(rs);
        }
    }
}
