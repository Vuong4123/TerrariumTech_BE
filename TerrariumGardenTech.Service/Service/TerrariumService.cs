using Newtonsoft.Json;
using System.Text;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Terrarium;
using TerrariumGardenTech.Common.RequestModel.TerrariumVariant;
using TerrariumGardenTech.Common.ResponseModel.Accessory;
using TerrariumGardenTech.Common.ResponseModel.Base;
using TerrariumGardenTech.Common.ResponseModel.Terrarium;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.Mappers;

namespace TerrariumGardenTech.Service.Service;
public class AITerrariumRequest
{
    public int EnvironmentId { get; set; }
    public int ShapeId { get; set; }
    public int TankMethodId { get; set; }
}

public class AITerrariumResponse
{
    public string TerrariumName { get; set; }
    public string Description { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }
    public int Stock { get; set; }
    public string ImageUrl { get; set; }
    public List<string> TerrariumImages { get; set; }
}

public class TerrariumService : ITerrariumService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly ICloudinaryService _cloudinaryService;

    public TerrariumService(UnitOfWork unitOfWork,ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _cloudinaryService = cloudinaryService ?? throw new ArgumentNullException(nameof(cloudinaryService));
    }
    public async Task<AITerrariumResponse> PredictTerrariumAsync(AITerrariumRequest request)
    {
        var httpClient = new HttpClient();
        var json = JsonConvert.SerializeObject(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync("http://127.0.0.1:8000/predict", content);
        if (!response.IsSuccessStatusCode)
            throw new Exception("Lỗi khi tự sinh ra terrarium");

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<AITerrariumResponse>(responseContent);
    }

    public async Task<IBusinessResult> GetAll(TerrariumGetAllRequest request)
    {
        var tuple = await _unitOfWork.Terrarium.GetFilterAndPagedAsync(request);
        var list = tuple.Item1;
        var enumerable = list.ToList();

        if (!enumerable.Any())
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

        if (!request.Pagination.IsPagingEnabled)
        {
            var tuple_ = await _unitOfWork.Terrarium.GetFilterAndPagedAsync(request);
            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", tuple_.Item1);
        }

        var terrariumIds = enumerable.Select(t => t.TerrariumId).ToList();
        var ratingStats = await _unitOfWork.Terrarium.GetTerrariumRatingStatsAsync(terrariumIds);

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
            }).ToList(),
            // Thêm rating
            AverageRating = ratingStats.ContainsKey(t.TerrariumId) ? ratingStats[t.TerrariumId].AverageRating : 0,
            FeedbackCount = ratingStats.ContainsKey(t.TerrariumId) ? ratingStats[t.TerrariumId].FeedbackCount : 0
        }).ToList();

        var tableResponse = new QueryTableResult(request, terrariums, tuple.Item2);

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", tableResponse);
    }

    public async Task<IBusinessResult> GetById(int id)
    {
        try
        {
            // Lấy Terrarium từ repository
            var terrarium = await _unitOfWork.Terrarium.GetTerrariumWithImagesByIdAsync(id);

            if (terrarium == null)
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No data found.");

            // Lấy rating & số feedback cho Terrarium này (dùng repository batch)
            var ratingStats = await _unitOfWork.Terrarium.GetTerrariumRatingStatsAsync(new List<int> { id });
            var avgRating = ratingStats.ContainsKey(id) ? ratingStats[id].AverageRating : 0;
            var feedbackCount = ratingStats.ContainsKey(id) ? ratingStats[id].FeedbackCount : 0;

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
                MaxPrice = terrarium.MaxPrice,
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
                CreatedAt = terrarium.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = terrarium.UpdatedAt ?? DateTime.UtcNow,
                TerrariumImages = terrarium.TerrariumImages.Select(ti => new TerrariumImageResponse
                {
                    TerrariumImageId = ti.TerrariumImageId,
                    TerrariumId = ti.TerrariumId,
                    ImageUrl = ti.ImageUrl
                }).ToList(),
                // Bổ sung hai trường mới
                AverageRating = avgRating,
                FeedbackCount = feedbackCount
            };

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
    public async Task<IBusinessResult> UpdateTerrarium(TerrariumUpdateRequest terrariumUpdateRequest)
    {
        try
        {
            var terra = await _unitOfWork.Terrarium
                .GetByIdAsync(terrariumUpdateRequest.TerrariumId); // Include quan hệ

            if (terra == null)
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

            // Cập nhật thuộc tính cơ bản
            terra.EnvironmentId = terrariumUpdateRequest.EnvironmentId;
            terra.ShapeId = terrariumUpdateRequest.ShapeId;
            terra.TankMethodId = terrariumUpdateRequest.TankMethodId;
            terra.TerrariumName = terrariumUpdateRequest.TerrariumName;
            terra.Description = terrariumUpdateRequest.Description;
            terra.Status = terrariumUpdateRequest.Status;
            terra.bodyHTML = terrariumUpdateRequest.bodyHTML ?? string.Empty;
            terra.UpdatedAt = DateTime.UtcNow;

            // Thiết lập giá trị mặc định nếu không có giá từ variant
            //decimal defaultPrice = 100; // Giá trị mặc định khi không có variant
            //terra.MinPrice = terrariumUpdateRequest.MinPrice ?? defaultPrice;
            //terra.MaxPrice = terrariumUpdateRequest.MaxPrice ?? defaultPrice;

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


            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG);        
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    //public async Task<IBusinessResult> GetByAccesname(string name)
    //{
    //    try
    //    {
    //        // Tìm Terrarium theo tên accessory (nếu GetByNameAsync trả về List<Terrarium>)
    //        var terrariumList = await _unitOfWork.Terrarium.GetByNameAsync(name);

    //        if (terrariumList == null || !terrariumList.Any())
    //            return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No accessory found with that name.");

    //        // Lấy tất cả TerrariumId để query rating batch
    //        var terrariumIds = terrariumList.Select(t => t.TerrariumId).ToList();
    //        var ratingStats = await _unitOfWork.Terrarium.GetTerrariumRatingStatsAsync(terrariumIds);

    //        // Mapping dữ liệu từ Terrarium sang response model
    //        var terrariumResponse = terrariumList.Select(t => new TerrariumResponse
    //        {
    //            TerrariumId = t.TerrariumId,
    //            EnvironmentId = t.EnvironmentId,
    //            ShapeId = t.ShapeId,
    //            TankMethodId = t.TankMethodId,
    //            TerrariumName = t.TerrariumName,
    //            Description = t.Description,
    //            //MinPrice = t.MinPrice,
    //            //MaxPrice = t.MaxPrice,
    //            //Stock = t.Stock,
    //            //Status = t.Status,
    //            //BodyHTML = t.bodyHTML ?? string.Empty,
    //            //CreatedAt = t.CreatedAt ?? DateTime.UtcNow,
    //            //UpdatedAt = t.UpdatedAt ?? DateTime.UtcNow,
    //            Accessories = t.TerrariumAccessory.Select(a => new TerrariumAccessoryResponse
    //            {
    //                AccessoryId = a.Accessory.AccessoryId,
    //                Name = a.Accessory.Name,
    //                Description = a.Accessory.Description,
    //                Price = a.Accessory.Price
    //            }).ToList(),
    //            TerrariumImages = t.TerrariumImages.Select(ti => new TerrariumImageResponse
    //            {
    //                TerrariumImageId = ti.TerrariumImageId,
    //                TerrariumId = ti.TerrariumId,
    //                ImageUrl = ti.ImageUrl ?? string.Empty
    //            }).ToList(),
    //            // Bổ sung rating
    //            AverageRating = ratingStats.ContainsKey(t.TerrariumId) ? ratingStats[t.TerrariumId].AverageRating : 0,
    //            FeedbackCount = ratingStats.ContainsKey(t.TerrariumId) ? ratingStats[t.TerrariumId].FeedbackCount : 0
    //        }).ToList();

    //        return new BusinessResult(Const.SUCCESS_READ_CODE, "Accessory found.", terrariumResponse);
    //    }
    //    catch (Exception ex)
    //    {
    //        return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
    //    }
    //}

    public async Task<IBusinessResult> GetTerrariumSuggestions(int userId)
    {
        try
        {
            // Lấy dữ liệu từ bảng Personalize dựa trên userId
            var userPersonalize = await _unitOfWork.Personalize.GetByUserIdAsync(userId);
            if (userPersonalize == null)
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "User personalize data not found.");

            // Lấy dữ liệu Terrariums theo các thuộc tính đã chọn trong Personalize
            var suggestedTerrariums = await _unitOfWork.Terrarium.GetAllAsync(); // Đảm bảo gọi await để lấy List<Terrarium>

            // Sử dụng Where sau khi có danh sách
            var filteredTerrariums = suggestedTerrariums
                .Where(t => t.EnvironmentId == userPersonalize.EnvironmentId &&
                            t.ShapeId == userPersonalize.ShapeId &&
                            t.TankMethodId == userPersonalize.TankMethodId)
                .ToList();

            if (suggestedTerrariums == null || !suggestedTerrariums.Any())
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No matching terrariums found.");

            // Mapping dữ liệu Terrarium sang response
            var terrariumResponses = suggestedTerrariums.Select(t => new TerrariumResponse
            {
                TerrariumId = t.TerrariumId,
                TerrariumName = t.TerrariumName,
                Description = t.Description,
                MinPrice = t.MinPrice,
                MaxPrice = t.MaxPrice,
                Stock = t.Stock,
                Status = t.Status,
                EnvironmentId = t.EnvironmentId,
                ShapeId = t.EnvironmentId,
                TankMethodId = t.EnvironmentId,
                TerrariumImages = t.TerrariumImages.Select(img => new TerrariumImageResponse
                {
                    TerrariumId = img.TerrariumId,
                    TerrariumImageId = img.TerrariumImageId,
                    ImageUrl = img.ImageUrl
                }).ToList() // Ánh xạ đúng kiểu
            }).ToList();

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Terrarium suggestions retrieved successfully.", terrariumResponses);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }
    public async Task<IBusinessResult> CreateTerrariumAI(TerrariumCreateRequest terrariumCreateRequest)
    {
        try
        {
            // Lấy danh sách Accessory theo tên
            var AccessoryNames = await _unitOfWork.Accessory.GetByName(terrariumCreateRequest.AccessoryNames);
            if (AccessoryNames == null || AccessoryNames.Count == 0)
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
           
            //// Kiểm tra nếu không có variant, hãy gán giá trị mặc định cho MinPrice và MaxPrice
            //decimal defaultPrice = 100; // Giá trị mặc định khi không có variant
            //decimal minPrice = terrariumCreateRequest.MinPrice ?? defaultPrice;
            //decimal maxPrice = terrariumCreateRequest.MaxPrice ?? defaultPrice;

            // Tạo mới Terrarium
            var newTerrarium = new Terrarium
            {
                TerrariumName = terrariumCreateRequest.TerrariumName,
                EnvironmentId = terrariumCreateRequest.EnvironmentId,
                ShapeId = terrariumCreateRequest.ShapeId,
                TankMethodId = terrariumCreateRequest.TankMethodId,
                bodyHTML = terrariumCreateRequest.bodyHTML,
                MinPrice=terrariumCreateRequest.MinPrice,
                MaxPrice=terrariumCreateRequest.MaxPrice,
                Stock=terrariumCreateRequest.Stock,
           
                Description = terrariumCreateRequest.Description,
                Status = terrariumCreateRequest.Status,


                CreatedAt = DateTime.Now
            };
           
            // Tạo Terrarium vào cơ sở dữ liệu
            var result = await _unitOfWork.Terrarium.CreateAsync(newTerrarium);
            if (terrariumCreateRequest.TerrariumImages != null)
            {
                foreach (var imageUrl in terrariumCreateRequest.TerrariumImages)
                {
                    var terrariumImage = new TerrariumImage
                    {
                        TerrariumId = newTerrarium.TerrariumId,
                        ImageUrl = imageUrl
                    };
                    _unitOfWork.TerrariumImage.Context().Add(terrariumImage);
                }
                await _unitOfWork.TerrariumImage.SaveChangesAsync();
            }


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

    public async Task<IBusinessResult> CreateTerrarium(TerrariumCreate terrariumCreateRequest)
    {
        try
        {
            // Nếu có AccessoryNames thì mới tìm
            List<Accessory> accessories = new();
            if (terrariumCreateRequest.AccessoryNames != null && terrariumCreateRequest.AccessoryNames.Any())
            {
                accessories = await _unitOfWork.Accessory.GetByName(terrariumCreateRequest.AccessoryNames);
            }

            //// Kiểm tra nếu không có variant, hãy gán giá trị mặc định cho MinPrice và MaxPrice
            //decimal defaultPrice = 100; // Giá trị mặc định khi không có variant
            //decimal minPrice = terrariumCreateRequest.MinPrice ?? defaultPrice;
            //decimal maxPrice = terrariumCreateRequest.MaxPrice ?? defaultPrice;

            // Tạo mới Terrarium
            var newTerrarium = new Terrarium
            {
                TerrariumName = terrariumCreateRequest.TerrariumName,
                EnvironmentId = terrariumCreateRequest.EnvironmentId,
                ShapeId = terrariumCreateRequest.ShapeId,
                TankMethodId = terrariumCreateRequest.TankMethodId,
                bodyHTML = terrariumCreateRequest.bodyHTML,
                Description = terrariumCreateRequest.Description,
                Status = terrariumCreateRequest.Status,


                CreatedAt = DateTime.Now
            };

            // Tạo Terrarium vào cơ sở dữ liệu
            var result = await _unitOfWork.Terrarium.CreateAsync(newTerrarium);


            if (result > 0)
            {
                // Nếu có accessories thì gán
                if (accessories != null && accessories.Any())
                {
                    foreach (var accessory in accessories)
                    {
                        var terrariumAccessory = new TerrariumAccessory
                        {
                            AccessoryId = accessory.AccessoryId,
                            TerrariumId = newTerrarium.TerrariumId
                        };
                        _unitOfWork.TerrariumAccessory.Context().Add(terrariumAccessory);
                    }
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

    public async Task<IBusinessResult> GetTerrariumByNameAsync(string terrariumName)
    {
        try
        {
            // Lấy danh sách Terrarium theo tên
            var terrariumList = await _unitOfWork.Terrarium.GetTerrariumByNameAsync(terrariumName);

            if (terrariumList == null || !terrariumList.Any())
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No terrarium found with that name.");

            // Lấy rating trung bình và feedback count cho từng Terrarium (batch)
            var terrariumIds = terrariumList.Select(t => t.TerrariumId).ToList();
            var ratingStats = await _unitOfWork.Terrarium.GetTerrariumRatingStatsAsync(terrariumIds);

            var terrariumResponseList = terrariumList.Select(t => new TerrariumResponse
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
                Accessories = t.TerrariumAccessory.Select(a => new TerrariumAccessoryResponse
                {
                    AccessoryId = a.Accessory.AccessoryId,
                    Name = a.Accessory.Name,
                    Description = a.Accessory.Description,
                    Price = a.Accessory.Price
                }).ToList(),
                BodyHTML = t.bodyHTML,
                CreatedAt = t.CreatedAt ?? DateTime.UtcNow,
                UpdatedAt = t.UpdatedAt ?? DateTime.UtcNow,
                TerrariumImages = t.TerrariumImages.Select(ti => new TerrariumImageResponse
                {
                    TerrariumImageId = ti.TerrariumImageId,
                    TerrariumId = ti.TerrariumId,
                    ImageUrl = ti.ImageUrl ?? string.Empty
                }).ToList(),
                AverageRating = ratingStats.ContainsKey(t.TerrariumId) ? ratingStats[t.TerrariumId].AverageRating : 0,
                FeedbackCount = ratingStats.ContainsKey(t.TerrariumId) ? ratingStats[t.TerrariumId].FeedbackCount : 0
            }).ToList();

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Terrarium(s) found.", terrariumResponseList);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }
}