using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Terrarium;
using TerrariumGardenTech.Common.ResponseModel.Base;
using TerrariumGardenTech.Common.ResponseModel.Terrarium;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.Mappers;

namespace TerrariumGardenTech.Service.Service;

public class TerrariumService : ITerrariumService
{
    private readonly UnitOfWork _unitOfWork;

    public TerrariumService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<IBusinessResult> GetAll(TerrariumGetAllRequest request)
    {
        try
        {
            // Gọi phương thức từ repository để nạp dữ liệu Terrarium với ảnh
            var tuple = await _unitOfWork.Terrarium.GetAllWithImagesAsync(request);

            var terrariumList = tuple.Item1;
            var enumerable = terrariumList.ToList();
            
            if (enumerable.Any())
            {
                if (!request.Pagination.IsPagingEnabled)
                {
                    var tuple_ = await _unitOfWork.Terrarium.GetAllWithImagesAsync(request);

                    return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", tuple_.Item1);
                }

                var terrariums = enumerable.Select(t => new TerrariumDetailResponse
                {
                    TerrariumId = t.TerrariumId,
                    EnvironmentId = t.EnvironmentId,
                    ShapeId = t.ShapeId,
                    TankMethodId = t.TankMethodId,
                    TerrariumName = t.TerrariumName,
                    Description = t.Description,
                    MinPrice = t.MinPrice,
                    MaxPrice = t.MaxPrice,
                    Stock = t.Stock,
                    Status = t.Status,
                    TerrariumImages = t.TerrariumImages?.Select(ti => new TerrariumImageResponse
                    {
                        TerrariumImageId = ti.TerrariumImageId,
                        TerrariumId = ti.TerrariumId,
                        ImageUrl = ti.ImageUrl ?? string.Empty
                    }).ToList()
                }).ToList();

                var tableResponse = new QueryTableResult(request, terrariums, tuple.Item2);

                return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", tableResponse);
            }

            return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No data found.");
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> GetById(int id)
    {
        try
        {
            // Lấy Terrarium từ repository
            var terrarium = await _unitOfWork.Terrarium.GetTerrariumWithImagesByIdAsync(id);

            if (terrarium == null)
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No data found.");

            // Ánh xạ Terrarium sang TerrariumResponse
            var terrariumResponse = new TerrariumResponse
            {
                TerrariumId = terrarium.TerrariumId,
                EnvironmentId = terrarium.EnvironmentId,
                ShapeId = terrarium.ShapeId,
                TankMethodId = terrarium.TankMethodId,
                TerrariumName = terrarium.TerrariumName,
                Description = terrarium.Description,
                MinPrice = terrarium.MinPrice,
                MaxPrice = terrarium.MaxPrice, // Sử dụng giá thấp nhất và cao nhất
                Stock = terrarium.Stock,
                Status = terrarium.Status,
                Accessories = terrarium.TerrariumAccessory.Select(a => new TerrariumAccessoryResponse
                {
                    AccessoryId = a.Accessory.AccessoryId,
                    Name = a.Accessory.Name,
                    Description = a.Accessory.Description,
                    Price = a.Accessory.Price
                }).ToList(),
                BodyHTML = terrarium.bodyHTML,
                CreatedAt = terrarium.CreatedAt ?? DateTime.UtcNow, // Dùng giá trị mặc định nếu CreatedAt là null
                UpdatedAt = terrarium.UpdatedAt ?? DateTime.UtcNow, // Tương tự cho UpdatedAt
                TerrariumImages = terrarium.TerrariumImages.Select(ti => new TerrariumImageResponse
                {
                    TerrariumImageId = ti.TerrariumImageId,
                    TerrariumId = ti.TerrariumId,
                    ImageUrl = ti.ImageUrl
                }).ToList()
            };

            // Trả về kết quả với dữ liệu đã ánh xạ
            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", terrariumResponse);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }


    public async Task<IBusinessResult> Save(Terrarium terrarium)
    {
        try
        {
            var result = -1;
            var terrariumEntity = _unitOfWork.Terrarium.GetByIdAsync(terrarium.TerrariumId);
            if (terrariumEntity != null)
            {
                result = await _unitOfWork.Terrarium.UpdateAsync(terrarium);
                if (result > 0)
                    return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, terrarium);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            // Create new terrarium if it does not exist
            result = await _unitOfWork.Terrarium.CreateAsync(terrarium);
            if (result > 0) return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, terrarium);

            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }

    //public async Task<IBusinessResult> UpdateTerrarium(TerrariumUpdateRequest terrariumUpdateRequest)
    //{
    //    try
    //    {
    //        var terra = await _unitOfWork.Terrarium
    //            .GetByIdAsync(terrariumUpdateRequest.TerrariumId); // Include quan hệ

    //        if (terra == null)
    //            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

    //        // Cập nhật thuộc tính cơ bản
    //        terra.TerrariumName = terrariumUpdateRequest.TerrariumName;
    //        terra.Description = terrariumUpdateRequest.Description;
    //        terra.EnvironmentId = terrariumUpdateRequest.EnvironmentId;
    //        terra.ShapeId = terrariumUpdateRequest.ShapeId;
    //        terra.TankMethodId = terrariumUpdateRequest.TankMethodId;
    //        terra.Price = terrariumUpdateRequest.Price;
    //        terra.Stock = terrariumUpdateRequest.Stock;
    //        terra.Status = terrariumUpdateRequest.Status;
    //        terra.bodyHTML = terrariumUpdateRequest.bodyHTML ?? string.Empty;
    //        terra.UpdatedAt = DateTime.UtcNow;

    //        var ctx = _unitOfWork.Terrarium.Context();

    //        // ===== XÓA DỮ LIỆU QUAN HỆ CŨ =====
    //        ctx.TerrariumAccessory.RemoveRange(ctx.TerrariumAccessory.Where(x => x.TerrariumId == terra.TerrariumId));
    //        await _unitOfWork.Terrarium.SaveChangesAsync(); // Lưu các thay đổi đã xóa
    //        // ===== THÊM DỮ LIỆU QUAN HỆ MỚI =====

    //        // 1. Accessories
    //        if (terrariumUpdateRequest.AccessoryNames?.Any() == true)
    //        {
    //            var accessories = await _unitOfWork.Accessory
    //                .FindAsync(a => terrariumUpdateRequest.AccessoryNames.Contains(a.Name));

    //            terra.TerrariumAccessory = accessories.Select(a => new TerrariumAccessory
    //            {
    //                TerrariumId = terra.TerrariumId,
    //                AccessoryId = a.AccessoryId
    //            }).ToList();
    //        }

    //        // Cập nhật & lưu
    //        await _unitOfWork.Terrarium.UpdateAsync(terra);

    //        var resultDto = terra.ToTerrariumResponse(); // Sử dụng mapper

    //        return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, resultDto);
    //    }
    //    catch (Exception ex)
    //    {
    //        return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
    //    }
    //}
    public async Task<IBusinessResult> UpdateTerrarium(TerrariumUpdateRequest terrariumUpdateRequest)
    {
        try
        {
            var terra = await _unitOfWork.Terrarium
                .GetByIdAsync(terrariumUpdateRequest.TerrariumId); // Include quan hệ

            if (terra == null)
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

            // Cập nhật thuộc tính cơ bản
            terra.TerrariumName = terrariumUpdateRequest.TerrariumName;
            terra.Description = terrariumUpdateRequest.Description;
            terra.EnvironmentId = terrariumUpdateRequest.EnvironmentId;
            terra.ShapeId = terrariumUpdateRequest.ShapeId;
            terra.TankMethodId = terrariumUpdateRequest.TankMethodId;
            terra.Stock = terrariumUpdateRequest.Stock;
            terra.Status = terrariumUpdateRequest.Status;
            terra.bodyHTML = terrariumUpdateRequest.bodyHTML ?? string.Empty;
            terra.UpdatedAt = DateTime.UtcNow;

            // Thiết lập giá trị mặc định nếu không có giá từ variant
            decimal defaultPrice = 100; // Giá trị mặc định khi không có variant
            terra.MinPrice = terrariumUpdateRequest.MinPrice ?? defaultPrice;
            terra.MaxPrice = terrariumUpdateRequest.MaxPrice ?? defaultPrice;

            var ctx = _unitOfWork.Terrarium.Context();

            // ===== XÓA DỮ LIỆU QUAN HỆ CŨ =====
            ctx.TerrariumAccessory.RemoveRange(ctx.TerrariumAccessory.Where(x => x.TerrariumId == terra.TerrariumId));
            await _unitOfWork.Terrarium.SaveChangesAsync(); // Lưu các thay đổi đã xóa

            // ===== THÊM DỮ LIỆU QUAN HỆ MỚI =====
            if (terrariumUpdateRequest.AccessoryNames?.Any() == true)
            {
                var accessories = await _unitOfWork.Accessory
                    .FindAsync(a => terrariumUpdateRequest.AccessoryNames.Contains(a.Name));

                terra.TerrariumAccessory = accessories.Select(a => new TerrariumAccessory
                {
                    TerrariumId = terra.TerrariumId,
                    AccessoryId = a.AccessoryId
                }).ToList();
            }
            else
            {
                // Nếu không có accessories mới, giữ nguyên accessories cũ
                var existingAccessories = await _unitOfWork.TerrariumAccessory
                    .FindAsync(ta => ta.TerrariumId == terra.TerrariumId);

                terra.TerrariumAccessory = existingAccessories.ToList();
            }

            // Cập nhật và lưu lại
            await _unitOfWork.Terrarium.UpdateAsync(terra);

            var resultDto = terra.ToTerrariumResponse(); // Sử dụng mapper

            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, resultDto);        
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }


    public async Task<IBusinessResult> CreateTerrarium(TerrariumCreateRequest terrariumCreateRequest)
    {
        try
        {
            // Lấy danh sách Accessory theo tên
            var AccessoryNames = await _unitOfWork.Accessory.GetByName(terrariumCreateRequest.AccessoryNames);
            if (AccessoryNames == null || AccessoryNames.Count == 0)
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

            // Kiểm tra nếu không có variant, hãy gán giá trị mặc định cho MinPrice và MaxPrice
            decimal defaultPrice = 100; // Giá trị mặc định khi không có variant
            decimal minPrice = terrariumCreateRequest.MinPrice ?? defaultPrice;
            decimal maxPrice = terrariumCreateRequest.MaxPrice ?? defaultPrice;

            // Tạo mới Terrarium
            var newTerrarium = new Terrarium
            {
                TerrariumName = terrariumCreateRequest.TerrariumName,
                EnvironmentId = terrariumCreateRequest.EnvironmentId,
                ShapeId = terrariumCreateRequest.ShapeId,
                TankMethodId = terrariumCreateRequest.TankMethodId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                bodyHTML = terrariumCreateRequest.bodyHTML,
                Description = terrariumCreateRequest.Description,
                Stock = terrariumCreateRequest.Stock,
                Status = terrariumCreateRequest.Status,
                CreatedAt = DateTime.Now
            };

            // Tạo Terrarium vào cơ sở dữ liệu
            var result = await _unitOfWork.Terrarium.CreateAsync(newTerrarium);

            if (result > 0)
            {
                // Gán Accessory vào Terrarium
                foreach (var accessory in AccessoryNames)
                {
                    var terrariumAccessory = new TerrariumAccessory
                    {
                        AccessoryId = accessory.AccessoryId,
                        TerrariumId = newTerrarium.TerrariumId
                    };
                    _unitOfWork.TerrariumAccessory.Context().Add(terrariumAccessory);
                    await _unitOfWork.TerrariumAccessory.SaveChangesAsync();
                }

                return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, newTerrarium);
            }

            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }

    public async Task<IBusinessResult> DeleteById(int id)
    {
        try
        {
            var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(id);
            if (terrarium != null)
            {
                // Xóa các bản ghi liên quan trong bảng TerrariumImage
                var relatedImages = await _unitOfWork.TerrariumImage.GetAllByTerrariumIdAsync(id);
                //.GetAllAsync(x => x.TerrariumId == id);

                foreach (var image in
                         relatedImages)
                    await _unitOfWork.TerrariumImage.RemoveAsync(image); // Gọi RemoveAsync cho từng đối tượng.
                // Xóa các bản ghi liên quan trong bảng TerrariumVariant
                var relatedVariants =
                    await _unitOfWork.TerrariumVariant
                        .GetAllByTerrariumIdAsync(
                            id); // Cần phương thức GetAllByTerrariumIdAsync trong Repository của TerrariumVariant
                foreach (var variant in
                         relatedVariants)
                    await _unitOfWork.TerrariumVariant
                        .RemoveAsync(variant); // Xóa các bản ghi liên quan trong TerrariumVariant
                var result = await _unitOfWork.Terrarium.RemoveAsync(terrarium);
                if (result) return new BusinessResult(Const.SUCCESS_DELETE_CODE, Const.SUCCESS_DELETE_MSG, result);

                return new BusinessResult(Const.FAIL_DELETE_CODE, Const.FAIL_DELETE_MSG);
            }

            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> FilterTerrariumsAsync(int? environmentId, int? shapeId, int? tankMethodId)
    {
        try
        {
            // Gọi repository để lấy dữ liệu đã lọc
            var terrariumList = await _unitOfWork.Terrarium.FilterTerrariumsAsync(environmentId, shapeId, tankMethodId);

            if (terrariumList != null && terrariumList.Any())
            {
                // Ánh xạ dữ liệu từ Terrarium sang TerrariumDetailResponse
                var terrariums = terrariumList.Select(t => new TerrariumDetailResponse
                {
                    TerrariumId = t.TerrariumId,
                    TerrariumName = t.TerrariumName,
                    Description = t.Description,
                    MinPrice = (decimal)t.MinPrice,
                    MaxPrice = (decimal)t.MaxPrice,
                    Stock = t.Stock,
                    Status = t.Status,
                    TerrariumImages = t.TerrariumImages?.Select(ti => new TerrariumImageResponse
                    {
                        TerrariumImageId = ti.TerrariumImageId,
                        TerrariumId = ti.TerrariumId,
                        ImageUrl = ti.ImageUrl
                    }).ToList() ?? new List<TerrariumImageResponse>()
                }).ToList();

                return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, terrariums);
            }

            return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No terrariums match the filter.");
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public Task<IBusinessResult> GetTerrariumByNameAsync(string terrariumName)
    {
        throw new NotImplementedException();
    }
}