using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.TerrariumVariant;
using TerrariumGardenTech.Common.ResponseModel.TerrariumVariant;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class TerrariumVariantService : ITerrariumVariantService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly ICloudinaryService _cloudinaryService;

    public TerrariumVariantService(UnitOfWork unitOfWork, ICloudinaryService cloudinaryService)
    {
        _unitOfWork = unitOfWork;
        _cloudinaryService = cloudinaryService;
    }

    public async Task<IBusinessResult> GetAllTerrariumVariantAsync()
    {
        var terrariumVariants = await _unitOfWork.TerrariumVariant.GetAllAsync2();
        if (terrariumVariants == null || !terrariumVariants.Any())
            return new BusinessResult(Const.FAIL_READ_CODE, "No terrarium variants found.");

        return new BusinessResult(Const.SUCCESS_READ_CODE, "Terrarium variants retrieved successfully.",
            terrariumVariants);
    }

    public async Task<IBusinessResult> GetAllVariantByTerrariumIdAsync(int terrariumId)
    {
        var terrariumVariants = await _unitOfWork.TerrariumVariant.GetAllByTerrariumIdAsync(terrariumId);
        if (terrariumVariants == null || !terrariumVariants.Any())
            return new BusinessResult(Const.FAIL_READ_CODE, "No terrarium variants found.");
        return new BusinessResult(Const.SUCCESS_READ_CODE, "Terrarium variants retrieved successfully.",
            terrariumVariants);
    }

    public async Task<IBusinessResult?> GetTerrariumVariantByIdAsync(int id)
    {
        try
        {
            var terrariumVariant = await _unitOfWork.TerrariumVariant.GetByIdAsync2(id);
            if (terrariumVariant == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "No terrarium variant found.");

            // ✅ LẤY ACCESSORIES CHO VARIANT
            var variantAccessories = await _unitOfWork.TerrariumVariantAccessory
                .GetByTerrariumVariantIdAsync(id);

            var response = new TerrariumVariantResponse
            {
                TerrariumVariantId = terrariumVariant.TerrariumVariantId,
                TerrariumId = terrariumVariant.TerrariumId,
                VariantName = terrariumVariant.VariantName,
                Price = terrariumVariant.Price,
                StockQuantity = terrariumVariant.StockQuantity,
                UrlImage = terrariumVariant.UrlImage,
                CreatedAt = terrariumVariant.CreatedAt,
                UpdatedAt = terrariumVariant.UpdatedAt,
                Accessories = variantAccessories.Select(va => new VariantAccessoryResponse
                {
                    AccessoryId = va.AccessoryId,
                    Name = va.Accessory.Name,
                    Description = va.Accessory.Description,
                    Price = va.Accessory.Price,
                    Quantity = va.Quantity
                }).ToList()
            };

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Terrarium variant retrieved successfully.", response);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }
    public async Task<IBusinessResult> CreateTerrariumVariantAsync(
     TerrariumVariantCreateRequest terrariumVariantCreateRequest)
    {
        try
        {
            var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(terrariumVariantCreateRequest.TerrariumId);
            if (terrarium == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Terrarium not found.");

            string? uploadedImageUrl = null;

            // Upload ảnh nếu có
            if (terrariumVariantCreateRequest.ImageFile != null)
            {
                var uploadResult = await _cloudinaryService.UploadImageAsync(
                    terrariumVariantCreateRequest.ImageFile,
                    folder: "terrariumVariant_images"
                );

                if (uploadResult.Status == Const.SUCCESS_CREATE_CODE)
                {
                    uploadedImageUrl = uploadResult.Data.ToString();
                }
                else
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "Upload ảnh thất bại: " + uploadResult.Message);
                }
            }

                    var accessory = await _unitOfWork.Accessory.GetByIdAsync(terrariumVariantCreateRequest.AccessoryId);
                    if (accessory == null)
                        return new BusinessResult(Const.FAIL_READ_CODE, $"Accessory {terrariumVariantCreateRequest.AccessoryId} not found.");

            // Tạo TerrariumVariant
            var terrariumVariant = new TerrariumVariant
            {
                TerrariumId = terrariumVariantCreateRequest.TerrariumId,
                VariantName = terrariumVariantCreateRequest.VariantName,
                Price = terrariumVariantCreateRequest.Price,
                StockQuantity = terrariumVariantCreateRequest.StockQuantity,
                UrlImage = uploadedImageUrl,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _unitOfWork.TerrariumVariant.CreateAsync(terrariumVariant);
            if (result == 0)
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Terrarium variant could not be created.");

            // ✅ TẠO ACCESSORIES CHO VARIANT
            
                    var variantAccessory = new TerrariumVariantAccessory
                    {
                        TerrariumVariantId = terrariumVariant.TerrariumVariantId,
                        AccessoryId = terrariumVariantCreateRequest.AccessoryId,
                        Quantity = terrariumVariantCreateRequest.Quantity
                    };

                    await _unitOfWork.TerrariumVariantAccessory.CreateAsync(variantAccessory);

            // Cập nhật Terrarium stock và prices
            await UpdateTerrariumStockAsync(terrariumVariantCreateRequest.TerrariumId);
            await UpdateTerrariumPricesAsync(terrariumVariantCreateRequest.TerrariumId);

            return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Terrarium variant created successfully.", terrariumVariant);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> UpdateTerrariumVariantAsync(
        TerrariumVariantUpdateRequest terrariumVariantUpdateRequest)
    {
        try
        {
            var terrariumVariant = await _unitOfWork.TerrariumVariant.GetByIdAsync(terrariumVariantUpdateRequest.TerrariumVariantId);
            if (terrariumVariant == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Terrarium variant not found.");

            // ✅ VALIDATE ACCESSORIES
                    var accessory = await _unitOfWork.Accessory.GetByIdAsync(terrariumVariantUpdateRequest.AccessoryId);
                    if (accessory == null)
                        return new BusinessResult(Const.FAIL_READ_CODE, $"Accessory {terrariumVariantUpdateRequest.AccessoryId} not found.");

            // Cập nhật thông tin variant
            terrariumVariant.VariantName = terrariumVariantUpdateRequest.VariantName;
            terrariumVariant.Price = terrariumVariantUpdateRequest.Price;
            terrariumVariant.StockQuantity = terrariumVariantUpdateRequest.StockQuantity;
            terrariumVariant.UpdatedAt = DateTime.UtcNow;

            // Upload ảnh mới nếu có
            if (terrariumVariantUpdateRequest.ImageFile != null)
            {
                var uploadResult = await _cloudinaryService.UploadImageAsync(
                    terrariumVariantUpdateRequest.ImageFile,
                    folder: "terrariumVariant_images"
                );

                if (uploadResult.Status == Const.SUCCESS_CREATE_CODE)
                {
                    terrariumVariant.UrlImage = uploadResult.Data.ToString();
                }
                else
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "Upload ảnh thất bại: " + uploadResult.Message);
                }
            }

            var result = await _unitOfWork.TerrariumVariant.UpdateAsync(terrariumVariant);
            if (result == 0)
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Terrarium variant could not be updated.");

            // ✅ CẬP NHẬT ACCESSORIES CHO VARIANT
            // Xóa accessories cũ
            var existingAccessories = await _unitOfWork.TerrariumVariantAccessory
                .FindAsync(tva => tva.TerrariumVariantId == terrariumVariant.TerrariumVariantId);

            foreach (var existing in existingAccessories)
            {
                existing.AccessoryId = terrariumVariantUpdateRequest.AccessoryId;
                existing.Quantity += terrariumVariantUpdateRequest.Quantity;
                await _unitOfWork.TerrariumVariantAccessory.UpdateAsync(existing);
            }
            // Cập nhật Terrarium stock và prices
            await UpdateTerrariumStockAsync(terrariumVariant.TerrariumId);
            await UpdateTerrariumPricesAsync(terrariumVariant.TerrariumId);

            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Terrarium variant updated successfully.", terrariumVariant);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> DeleteTerrariumVariantAsync(int id)
    {
        try
        {
            var terrariumVariant = await _unitOfWork.TerrariumVariant.GetByIdAsync(id);
            if (terrariumVariant == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Terrarium variant not found.");

            // ✅ XÓA ACCESSORIES LIÊN QUAN
            var relatedAccessories = await _unitOfWork.TerrariumVariantAccessory
                .FindAsync(tva => tva.TerrariumVariantId == id);

            foreach (var accessory in relatedAccessories)
            {
                await _unitOfWork.TerrariumVariantAccessory.RemoveAsync(accessory);
            }

            // Xóa variant
            var result = await _unitOfWork.TerrariumVariant.RemoveAsync(terrariumVariant);
            if (!result)
                return new BusinessResult(Const.FAIL_DELETE_CODE, "Terrarium variant could not be deleted.");

            // Cập nhật lại Terrarium stock và prices
            await UpdateTerrariumStockAsync(terrariumVariant.TerrariumId);
            await UpdateTerrariumPricesAsync(terrariumVariant.TerrariumId);

            return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Terrarium variant deleted successfully.", result);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }


    public async Task<IBusinessResult> UpdateTerrariumStockAsync(int terrariumId)
    {
        try
        {
            // Lấy thông tin Terrarium
            var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(terrariumId);

            if (terrarium == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Terrarium not found.");

            // Lấy tất cả các variants của Terrarium
            var existingVariants = await _unitOfWork.TerrariumVariant
                .FindAsync(tv => tv.TerrariumId == terrariumId);

            // Tính tổng số lượng tồn kho của tất cả variants
            var totalVariantStock = existingVariants.Sum(v => v.StockQuantity);

            // Cập nhật số lượng tồn kho của Terrarium bằng tổng số tồn kho của các variants
            terrarium.Stock = totalVariantStock;
            terrarium.UpdatedAt = DateTime.UtcNow;

            // Lưu thay đổi
            var result = await _unitOfWork.Terrarium.UpdateAsync(terrarium);

            if (result == 0)
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Failed to update Terrarium stock.");

            // Trả về kết quả thành công
            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Terrarium stock updated successfully.", terrarium);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> UpdateTerrariumPricesAsync(int terrariumId)
    {
        try
        {
            // Lấy thông tin Terrarium
            var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(terrariumId);
            if (terrarium == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Terrarium not found.");

            // Lấy tất cả các variants của Terrarium
            var variants = await _unitOfWork.TerrariumVariant.FindAsync(tv => tv.TerrariumId == terrariumId);

            // Kiểm tra nếu không có variants, thì không có giá
            if (variants == null || !variants.Any())
                return new BusinessResult(Const.FAIL_READ_CODE, "No variants found for the terrarium.");

            // Tính giá thấp nhất và cao nhất từ các variants
            var minPrice = variants.Min(v => v.Price);
            var maxPrice = variants.Max(v => v.Price);

            // Cập nhật giá của Terrarium
            terrarium.MinPrice = minPrice; // Cập nhật giá thấp nhất
            terrarium.MaxPrice = maxPrice; // Cập nhật giá cao nhất
            terrarium.UpdatedAt = DateTime.UtcNow;

            // Lưu lại thông tin Terrarium
            var result = await _unitOfWork.Terrarium.UpdateAsync(terrarium);

            if (result == 0)
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Failed to update Terrarium prices.");

            // Trả về kết quả thành công
            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Terrarium prices updated successfully.", terrarium);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }
}