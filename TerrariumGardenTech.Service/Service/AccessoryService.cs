using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.RequestModel.Accessory;
using TerrariumGardenTech.Common.ResponseModel.Accessory;
using TerrariumGardenTech.Common.ResponseModel.Base;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class AccessoryService : IAccessoryService
{
    private readonly UnitOfWork _unitOfWork;

    public AccessoryService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }


    // Lấy tất cả Accessory
    public async Task<IBusinessResult> GetAll(AccessoryGetAllRequest request)
    {
        // Lấy danh sách accessory và phân trang
        var tuple = await _unitOfWork.Accessory.GetFilterAndPagedAsync(request);

        var list = tuple.Item1;
        var enumerable = list.ToList();

        if (!enumerable.Any())
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

        if (!request.Pagination.IsPagingEnabled)
        {
            var tuple_ = await _unitOfWork.Accessory.GetFilterAndPagedAsync(request);
            return new BusinessResult(Const.SUCCESS_READ_CODE, "Data retrieved successfully.", tuple_.Item1);
        }

        // Lấy list AccessoryId
        var accessoryIds = enumerable.Select(a => a.AccessoryId).ToList();

        // Lấy rating & count cho các accessory chỉ qua 1 query
        var ratingStats = await _unitOfWork.Accessory.GetAccessoryRatingStatsAsync(accessoryIds);
        // ratingStats là Dictionary<int, (double AverageRating, int FeedbackCount)>
        // ✅ Thêm: purchase counts (tổng quantity đã bán)
        var purchaseCounts = await _unitOfWork.Accessory.GetAccessoryPurchaseCountsAsync(accessoryIds);
        // Map sang DTO, bổ sung rating
        var accessories = enumerable.Select(a => new AccessoryResponse
        {
            AccessoryId = a.AccessoryId,
            Name = a.Name,
            Size = a.Size,
            Description = a.Description,
            Price = (decimal)a.Price,
            StockQuantity = a.StockQuantity,
            Status = a.Status,
            CategoryId = a.CategoryId,
            CreatedAt = a.CreatedAt ?? DateTime.MinValue,
            UpdatedAt = a.UpdatedAt ?? DateTime.MinValue,
            AccessoryImages = a.AccessoryImages.Select(ai => new AccessoryImageResponse
            {
                AccessoryImageId = ai.AccessoryImageId,
                ImageUrl = ai.ImageUrl,
                AccessoryId = ai.AccessoryId
            }).ToList(),
            AverageRating = ratingStats.ContainsKey(a.AccessoryId) ? ratingStats[a.AccessoryId].AverageRating : 0,
            FeedbackCount = ratingStats.ContainsKey(a.AccessoryId) ? ratingStats[a.AccessoryId].FeedbackCount : 0,
            PurchaseCount = purchaseCounts.ContainsKey(a.AccessoryId) ? purchaseCounts[a.AccessoryId] : 0
        }).ToList();

        var tableResponse = new QueryTableResult(request, accessories, tuple.Item2);

        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, tableResponse);
    }

    // Lấy Accessory theo ID
    public async Task<IBusinessResult> GetById(int id)
    {
        var accessory = await _unitOfWork.Accessory.GetAccessoryWithImagesByIdAsync(id);
        if (accessory == null)
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);

        // Rating
        var ratingStats = await _unitOfWork.Accessory.GetAccessoryRatingStatsAsync(new List<int> { id });
        var avgRating = ratingStats.ContainsKey(id) ? ratingStats[id].AverageRating : 0;
        var feedbackCount = ratingStats.ContainsKey(id) ? ratingStats[id].FeedbackCount : 0;

        // ✅ Lượt mua
        var purchaseStats = await _unitOfWork.Accessory.GetAccessoryPurchaseCountsAsync(new List<int> { id });
        var purchaseCount = purchaseStats.TryGetValue(id, out var pc) ? pc : 0;

        var accessoryResponse = new AccessoryResponse
        {
            AccessoryId = accessory.AccessoryId,
            Name = accessory.Name,
            Size = accessory.Size,
            Description = accessory.Description,
            Price = (decimal)accessory.Price,
            StockQuantity = accessory.StockQuantity,
            Status = accessory.Status,
            CategoryId = accessory.CategoryId,
            CreatedAt = accessory.CreatedAt ?? DateTime.MinValue,
            UpdatedAt = accessory.UpdatedAt ?? DateTime.MinValue,
            AccessoryImages = accessory.AccessoryImages.Select(ai => new AccessoryImageResponse
            {
                AccessoryImageId = ai.AccessoryImageId,
                ImageUrl = ai.ImageUrl,
                AccessoryId = ai.AccessoryId
            }).ToList(),
            AverageRating = avgRating,
            FeedbackCount = feedbackCount,
            // ✅ gắn lượt mua
            PurchaseCount = purchaseCount
        };

        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, accessoryResponse);
    }

    // Overload mới: có request để phân trang/sort/include
    public async Task<IBusinessResult> FilterAccessoryAsync(int? categoryId)
    {
        if (categoryId is null)
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, "CategoryId is required.");

        // Lấy theo category (repo hiện tại của bạn trả IEnumerable<Accessory>)
        var accessories = await _unitOfWork.Accessory.FilterAccessoryAsync(categoryId);
        var list = (accessories ?? Enumerable.Empty<Accessory>())
            .OrderByDescending(a => a.UpdatedAt ?? a.CreatedAt ?? DateTime.MinValue) // mới -> cũ
            .ThenByDescending(a => a.AccessoryId)
            .ToList();

        if (list.Count == 0)
            return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No accessories matched the given filter.");

        // Lấy stats theo list id
        var ids = list.Select(a => a.AccessoryId).ToList();
        var ratingStats = await _unitOfWork.Accessory.GetAccessoryRatingStatsAsync(ids);
        var purchaseStats = await _unitOfWork.Accessory.GetAccessoryPurchaseCountsAsync(ids);

        var data = list.Select(a => new AccessoryResponse
        {
            AccessoryId = a.AccessoryId,
            Name = a.Name,
            Size = a.Size,
            Description = a.Description,
            Price = (decimal)a.Price,
            StockQuantity = a.StockQuantity,
            Status = a.Status,
            CategoryId = a.CategoryId,
            CreatedAt = a.CreatedAt ?? DateTime.MinValue,
            UpdatedAt = a.UpdatedAt ?? DateTime.MinValue,
            AccessoryImages = (a.AccessoryImages ?? new List<AccessoryImage>())
                                .Select(ai => new AccessoryImageResponse
                                {
                                    AccessoryImageId = ai.AccessoryImageId,
                                    ImageUrl = ai.ImageUrl,
                                    AccessoryId = ai.AccessoryId
                                }).ToList(),
            AverageRating = ratingStats.TryGetValue(a.AccessoryId, out var r) ? r.AverageRating : 0,
            FeedbackCount = ratingStats.TryGetValue(a.AccessoryId, out var r2) ? r2.FeedbackCount : 0,
            PurchaseCount = purchaseStats.TryGetValue(a.AccessoryId, out var pc) ? pc : 0
        }).ToList();

        return new BusinessResult(Const.SUCCESS_READ_CODE, Const.SUCCESS_READ_MSG, data);
    }

    public async Task<IBusinessResult> Save(Accessory accessory)
    {
        try
        {
            var result = -1;
            var accessoryEntity = _unitOfWork.Accessory.GetByIdAsync(accessory.AccessoryId);
            if (accessoryEntity != null)
            {
                result = await _unitOfWork.Accessory.UpdateAsync(accessory);
                if (result > 0)
                    return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG, accessory);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            // Create new terrarium if it does not exist
            result = await _unitOfWork.Accessory.CreateAsync(accessory);
            if (result > 0) return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG, accessory);

            return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }

    public async Task<IBusinessResult> CreateAccessory(AccessoryCreateRequest accessoryCreateRequest)
    {
        var categoryExists =
            await _unitOfWork.Category.AnyAsync(c => c.CategoryId == accessoryCreateRequest.CategoryId);

        if (!categoryExists) return new BusinessResult(Const.FAIL_CREATE_CODE, "CategoryId không tồn tại.");

        var accessory = new Accessory
        {
            Name = accessoryCreateRequest.Name,
            Size = accessoryCreateRequest.Size,
            Description = accessoryCreateRequest.Description,
            Price = accessoryCreateRequest.Price,
            StockQuantity = accessoryCreateRequest.StockQuantity,
            CategoryId = accessoryCreateRequest.CategoryId,
            CreatedAt = accessoryCreateRequest.CreatedAt ?? DateTime.Now,
            UpdatedAt = accessoryCreateRequest.UpdatedAt ?? DateTime.Now,
            Status = accessoryCreateRequest.Status
        };
        var result = await _unitOfWork.Accessory.CreateAsync(accessory);
        if (result > 0) return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_CREATE_MSG);

        return new BusinessResult(Const.FAIL_CREATE_CODE, Const.FAIL_CREATE_MSG);
    }

    public async Task<IBusinessResult> UpdateAccessory(AccessoryUpdateRequest accessoryUpdateRequest)
    {
        try
        {
            // Check if the Category exists
            var categoryExists =
                await _unitOfWork.Category.AnyAsync(c => c.CategoryId == accessoryUpdateRequest.CategoryId);

            if (!categoryExists) return new BusinessResult(Const.FAIL_CREATE_CODE, "CategoryId không tồn tại.");

            var result = -1;

            // Fetch the existing accessory
            var access = await _unitOfWork.Accessory.GetByIdAsync(accessoryUpdateRequest.AccessoryId);

            if (access != null)
            {
                // Update the accessory values, including UpdatedAt
                access.Name = accessoryUpdateRequest.Name;
                access.Size = accessoryUpdateRequest.Size;
                access.Description = accessoryUpdateRequest.Description;
                access.Price = accessoryUpdateRequest.Price;
                access.StockQuantity = accessoryUpdateRequest.StockQuantity;
                access.CategoryId = accessoryUpdateRequest.CategoryId;
                access.Status = accessoryUpdateRequest.Status;

                // Set UpdatedAt to the value from the request or the current timestamp
                access.UpdatedAt = accessoryUpdateRequest.UpdatedAt ?? DateTime.Now;

                // Perform the update in the database
                result = await _unitOfWork.Accessory.UpdateAsync(access);

                if (result > 0)
                    return new BusinessResult(Const.SUCCESS_UPDATE_CODE, Const.SUCCESS_UPDATE_MSG);

                return new BusinessResult(Const.FAIL_UPDATE_CODE, Const.FAIL_UPDATE_MSG);
            }

            return new BusinessResult(Const.WARNING_NO_DATA_CODE, Const.WARNING_NO_DATA_MSG);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.ToString());
        }
    }

    public async Task<IBusinessResult> DeleteById(int id)
    {
        var accessory = await _unitOfWork.Accessory.GetByIdAsync(id);
        if (accessory == null) return new BusinessResult(Const.FAIL_READ_CODE, Const.FAIL_READ_MSG);
        // Xóa các bản ghi liên quan trong bảng AccessoryImage
        var relatedImages = await _unitOfWork.AccessoryImage.GetAllByAccessoryIdAsync(id);
        foreach (var image in
                 relatedImages)
            await _unitOfWork.AccessoryImage.RemoveAsync(image); // Xóa từng hình ảnh liên quan đến Accessory
        var terrariumAccessory = await _unitOfWork.TerrariumAccessory.GetAllTerrariumByAccessory(id);
        var terrariumIds = terrariumAccessory.Select(ts => ts.TerrariumId).Distinct().ToList();
        var terrariums = await _unitOfWork.Terrarium.GetTerrariumByIdsAsync(terrariumIds);

        using (var transaction = await _unitOfWork.Accessory.BeginTransactionAsync())
        {
            try
            {
                foreach (var terrariumAccessories in terrariumAccessory)
                    await _unitOfWork.TerrariumAccessory.RemoveAsync(terrariumAccessories);
                //Xoa cac terrarium va cac doi tuong lien quan
                if (terrariums != null)
                    foreach (var terrarium in terrariums)
                        //xoa cac doi tuong lien quan den Terrarium
                        await DeleteRelatedTerrariumAsync(terrarium);

                var result = await _unitOfWork.Accessory.RemoveAsync(accessory);
                if (result)
                {
                    //neu xoa thanh cong, commit giao dich
                    await transaction.CommitAsync();
                    return new BusinessResult(Const.SUCCESS_CREATE_CODE, Const.SUCCESS_DELETE_MSG);
                }

                // xxoa that bai, huy giao dich
                await transaction.RollbackAsync();
                return new BusinessResult(Const.FAIL_DELETE_CODE, "Failed to delete the shape.");
            }
            catch (Exception)
            {
                // Nếu có lỗi, hủy giao dịch và ghi log
                await transaction.RollbackAsync();
                return new BusinessResult(Const.FAIL_DELETE_CODE, "An error occurred while deleting the environment.");
            }
        }
    }


    private async Task DeleteRelatedTerrariumAsync(Terrarium terrarium)
    {
        var terrariumImages = await _unitOfWork.TerrariumImage.GetAllByTerrariumIdAsync(terrarium.TerrariumId);
        foreach (var terrariumImage in terrariumImages) await _unitOfWork.TerrariumImage.RemoveAsync(terrariumImage);


        var terrariumVariants = await _unitOfWork.TerrariumVariant.GetAllByTerrariumIdAsync(terrarium.TerrariumId);
        foreach (var terrariumVariant in terrariumVariants)
            await _unitOfWork.TerrariumVariant.RemoveAsync(terrariumVariant);

        await _unitOfWork.Terrarium.RemoveAsync(terrarium);
    }

    public async Task<IBusinessResult> GetByAccesname(string name)
    {
        try
        {
            // Tìm Accessory theo tên
            var accessory = await _unitOfWork.Accessory.GetByNameAsync(name);

            if (accessory == null)
                return new BusinessResult(Const.WARNING_NO_DATA_CODE, "No accessory found with that name.");

            // Mapping dữ liệu từ Accessory sang response model nếu cần thiết
            var accessoryResponse = accessory.Select(a => new AccessoryResponse
            {
                AccessoryId = a.AccessoryId,
                Name = a.Name,
                Size = a.Size,
                Description = a.Description,
                Price = a.Price,
                StockQuantity = a.StockQuantity ?? 0,
                Status = a.Status,
                CategoryId = a.CategoryId,
                AccessoryImages = a.AccessoryImages.Select(ti => new AccessoryImageResponse
                {
                    AccessoryId = ti.AccessoryId,
                    AccessoryImageId = ti.AccessoryImageId,
                    ImageUrl = ti.ImageUrl,
                }).ToList()
            }).ToList();

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Accessory found.", accessoryResponse);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }
}