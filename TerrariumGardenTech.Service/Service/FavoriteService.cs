using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Common.RequestModel.Favorite;
using TerrariumGardenTech.Common.ResponseModel.Favorite;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service
{
    public class FavoriteService : IFavoriteService
    {
        private readonly UnitOfWork _unitOfWork;

        public FavoriteService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        public async Task<IBusinessResult> LikeProductAsync(int userId, FavoriteCreateRequest req)
        {
            try
            {
                var isAcc = req.AccessoryId.HasValue;
                var isTer = req.TerrariumId.HasValue;
                if (isAcc == isTer)
                    return new BusinessResult(Const.FAIL_VALIDATE_CODE, "Chọn 1 trong 2: AccessoryId hoặc TerrariumId.");

                if (isAcc)
                {
                    var acc = await _unitOfWork.Accessory.GetByIdAsync(req.AccessoryId.Value);
                    if (acc == null) return new BusinessResult(Const.FAIL_READ_CODE, "Accessory không tồn tại.");

                    var existed = await _unitOfWork.Favorite.GetByUserAndAccessoryAsync(userId, req.AccessoryId.Value);
                    if (existed != null)
                        return new BusinessResult(Const.SUCCESS_READ_CODE, "Đã thích.", await BuildResponseAsync(existed));

                    var fav = new Favorite { UserId = userId, AccessoryId = req.AccessoryId, CreatedAt = DateTime.UtcNow };
                    await _unitOfWork.Favorite.CreateAsync(fav);
                    await _unitOfWork.SaveAsync();
                    return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Đã thêm vào yêu thích.", await BuildResponseAsync(fav));
                }
                else
                {
                    var ter = await _unitOfWork.Terrarium.GetByIdAsync(req.TerrariumId.Value);
                    if (ter == null) return new BusinessResult(Const.FAIL_READ_CODE, "Terrarium không tồn tại.");

                    var existed = await _unitOfWork.Favorite.GetByUserAndTerrariumAsync(userId, req.TerrariumId.Value);
                    if (existed != null)
                        return new BusinessResult(Const.SUCCESS_READ_CODE, "Đã thích.", await BuildResponseAsync(existed));

                    var fav = new Favorite { UserId = userId, TerrariumId = req.TerrariumId, CreatedAt = DateTime.UtcNow };
                    await _unitOfWork.Favorite.CreateAsync(fav);
                    await _unitOfWork.SaveAsync();
                    return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Đã thêm vào yêu thích.", await BuildResponseAsync(fav));
                }
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        private async Task<FavoriteItemResponse> BuildResponseAsync(Favorite fav)
        {
            // nạp lại kèm navigation tối thiểu
            if (fav.AccessoryId.HasValue)
            {
                var acc = await _unitOfWork.Accessory.GetByIdAsync(fav.AccessoryId.Value);
                var thumb = (acc?.AccessoryImages?.FirstOrDefault()?.ImageUrl) ?? "";
                return new FavoriteItemResponse
                {
                    FavoriteId = fav.FavoriteId,
                    Type = LikeType.Accessory,
                    ProductId = acc.AccessoryId,
                    Name = acc.Name,
                    Price = acc.Price,
                    ThumbnailUrl = thumb,
                    CreatedAt = fav.CreatedAt
                };
            }
            else
            {
                var ter = await _unitOfWork.Terrarium.GetByIdAsync(fav.TerrariumId.Value);
                var thumb = (ter?.TerrariumImages?.FirstOrDefault()?.ImageUrl) ?? "";
                return new FavoriteItemResponse
                {
                    FavoriteId = fav.FavoriteId,
                    Type = LikeType.Terrarium,
                    ProductId = ter.TerrariumId,
                    Name = ter.TerrariumName,
                    Price = ter.MinPrice, // hoặc null nếu không muốn hiển thị
                    ThumbnailUrl = thumb,
                    CreatedAt = fav.CreatedAt
                };
            }
        }

        public async Task<IBusinessResult> GetAllLikesAsync(int userId)
        {
            try
            {
                var list = await _unitOfWork.Favorite.GetByUserAsync(userId);

                var data = list.Select(f =>
                {
                    if (f.Accessory != null)
                    {
                        var thumb = f.Accessory.AccessoryImages?.FirstOrDefault()?.ImageUrl ?? "";
                        return new FavoriteItemResponse
                        {
                            FavoriteId = f.FavoriteId,
                            Type = LikeType.Accessory,
                            ProductId = f.Accessory.AccessoryId,
                            Name = f.Accessory.Name,
                            Price = f.Accessory.Price,
                            ThumbnailUrl = thumb,
                            CreatedAt = f.CreatedAt
                        };
                    }
                    else
                    {
                        var thumb = f.Terrarium?.TerrariumImages?.FirstOrDefault()?.ImageUrl ?? "";
                        return new FavoriteItemResponse
                        {
                            FavoriteId = f.FavoriteId,
                            Type = LikeType.Terrarium,
                            ProductId = f.Terrarium.TerrariumId,
                            Name = f.Terrarium.TerrariumName,
                            Price = f.Terrarium.MinPrice,
                            ThumbnailUrl = thumb,
                            CreatedAt = f.CreatedAt
                        };
                    }
                });

                return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy danh sách yêu thích thành công.", data);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }


        public async Task<IBusinessResult> DeleteLikeByProductAsync(int userId, int? accessoryId, int? terrariumId)
        {
            try
            {
                if (accessoryId.HasValue == terrariumId.HasValue)
                    return new BusinessResult(Const.FAIL_VALIDATE_CODE, "Truyền AccessoryId hoặc TerrariumId.");

                Favorite fav = accessoryId.HasValue
                    ? await _unitOfWork.Favorite.GetByUserAndAccessoryAsync(userId, accessoryId.Value)
                    : await _unitOfWork.Favorite.GetByUserAndTerrariumAsync(userId, terrariumId.Value);

                if (fav == null)
                    return new BusinessResult(Const.FAIL_READ_CODE, "Mục yêu thích không tồn tại.");

                await _unitOfWork.Favorite.RemoveAsync(fav);
                await _unitOfWork.SaveAsync();
                return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Đã xoá khỏi yêu thích.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> DeleteLikeByIdAsync(int userId, int favoriteId)
        {
            try
            {
                var fav = await _unitOfWork.Favorite.GetByIdAsync(favoriteId);
                if (fav == null || fav.UserId != userId)
                    return new BusinessResult(Const.FAIL_READ_CODE, "Mục yêu thích không tồn tại.");

                await _unitOfWork.Favorite.RemoveAsync(fav);
                await _unitOfWork.SaveAsync();
                return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Đã xoá khỏi yêu thích.");
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
            }
        }





    }
}
