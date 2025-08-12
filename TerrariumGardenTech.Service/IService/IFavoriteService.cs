using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TerrariumGardenTech.Common.RequestModel.Favorite;
using TerrariumGardenTech.Service.Base;

namespace TerrariumGardenTech.Service.IService
{
    public interface IFavoriteService
    {
        Task<IBusinessResult> LikeProductAsync(int userId, FavoriteCreateRequest req);
        Task<IBusinessResult> GetAllLikesAsync(int userId);
        Task<IBusinessResult> DeleteLikeByProductAsync(int userId, int? accessoryId, int? terrariumId);
        Task<IBusinessResult> DeleteLikeByIdAsync(int userId, int favoriteId);


    }
}
