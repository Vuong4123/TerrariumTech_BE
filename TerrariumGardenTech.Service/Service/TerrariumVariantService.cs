using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.Blog;
using TerrariumGardenTech.Service.RequestModel.TerrariumVariant;

namespace TerrariumGardenTech.Service.Service;

public class TerrariumVariantService(UnitOfWork _unitOfWork, ICloudinaryService _cloudinaryService) : ITerrariumVariantService
{

    public async Task<IBusinessResult> GetAllTerrariumVariantAsync()
    {
        var terrariumVariants = await _unitOfWork.TerrariumVariant.GetAllAsync();
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
        return new BusinessResult(Const.SUCCESS_READ_CODE, "Terrarium variants retrieved successfully.", terrariumVariants);
    }
    public async Task<IBusinessResult?> GetTerrariumVariantByIdAsync(int id)
    {
        var terrariumVariants = await _unitOfWork.TerrariumVariant.GetByIdAsync(id);
        if (terrariumVariants == null) return new BusinessResult(Const.FAIL_READ_CODE, "No terrarium variants found.");
        return new BusinessResult(Const.SUCCESS_READ_CODE, "Terrarium variants retrieved successfully.",
            terrariumVariants);
    }

    public async Task<IBusinessResult> CreateTerrariumVariantAsync(TerrariumVariantCreateRequest terrariumVariantCreateRequest)
    {
        try
        {
            // Lấy thông tin Terrarium
            var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(terrariumVariantCreateRequest.TerrariumId);

            if (terrarium == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Terrarium not found.");

            string? uploadedImageUrl = null;

            // Nếu có ảnh thì upload
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
            // Tạo mới TerrariumVariant
            var terrariumVariant = new TerrariumVariant
            {
                TerrariumId = terrariumVariantCreateRequest.TerrariumId,
                VariantName = terrariumVariantCreateRequest.VariantName,
                Price = terrariumVariantCreateRequest.Price,
                StockQuantity = terrariumVariantCreateRequest.StockQuantity,
                UrlImage = uploadedImageUrl

            };

            // Lưu TerrariumVariant vào cơ sở dữ liệu
            var result = await _unitOfWork.TerrariumVariant.CreateAsync(terrariumVariant);

            if (result == 0)
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Terrarium variant could not be created.");

            // Cập nhật lại số lượng tồn kho của Terrarium sau khi tạo mới variant
            await UpdateTerrariumStockAsync(terrariumVariantCreateRequest.TerrariumId);
            // Cập nhật lại giá của Terrarium sau khi thêm variant
            await UpdateTerrariumPricesAsync(terrariumVariantCreateRequest.TerrariumId);

            // Trả về kết quả thành công kèm theo dữ liệu TerrariumVariant đã tạo
            return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Terrarium variant created successfully.", terrariumVariant);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }
    
    public async Task<IBusinessResult> UpdateTerrariumVariantAsync(TerrariumVariantUpdateRequest terrariumVariantUpdateRequest)
    {
        try
        {
            // Lấy thông tin TerrariumVariant
            var terrariumVariant = await _unitOfWork.TerrariumVariant.GetByIdAsync(terrariumVariantUpdateRequest.TerrariumVariantId);
            if (terrariumVariant == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Terrarium variant not found.");

            var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(terrariumVariant.TerrariumId);
            if (terrarium == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Terrarium not found.");

            // Kiểm tra tồn kho của Terrarium
            var availableStock = terrarium.Stock;

            // Cập nhật số lượng tồn kho variant nếu số lượng variant thay đổi
            var stockDifference = terrariumVariantUpdateRequest.StockQuantity - terrariumVariant.StockQuantity;

            // Kiểm tra nếu số lượng tồn kho variant không vượt quá số lượng tồn kho của Terrarium
            if (stockDifference > availableStock)
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Insufficient stock in Terrarium.");

            // Cập nhật thông tin variant
            terrariumVariant.VariantName = terrariumVariantUpdateRequest.VariantName;
            terrariumVariant.Price = terrariumVariantUpdateRequest.Price;
            terrariumVariant.StockQuantity = terrariumVariantUpdateRequest.StockQuantity;
            terrariumVariant.UpdatedAt = DateTime.UtcNow;

            // Kiểm tra nếu có ảnh mới
            string? uploadedImageUrl = terrariumVariant.UrlImage; // Giữ lại UrlImage cũ nếu không có ảnh mới

            if (terrariumVariantUpdateRequest.ImageFile != null)
            {
                // Upload ảnh mới nếu có
                var uploadResult = await _cloudinaryService.UploadImageAsync(
                    terrariumVariantUpdateRequest.ImageFile,
                    folder: "terrariumVariant_images"
                );

                if (uploadResult.Status == Const.SUCCESS_CREATE_CODE)
                {
                    uploadedImageUrl = uploadResult.Data.ToString(); // Cập nhật UrlImage mới
                }
                else
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "Upload ảnh thất bại: " + uploadResult.Message);
                }
            }

            // Cập nhật UrlImage cho TerrariumVariant
            terrariumVariant.UrlImage = uploadedImageUrl;

            // Lưu thay đổi variant
            var result = await _unitOfWork.TerrariumVariant.UpdateAsync(terrariumVariant);

            if (result == 0)
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Terrarium variant could not be updated.");

            // Cập nhật lại số lượng tồn kho của Terrarium sau khi cập nhật variant
            await UpdateTerrariumStockAsync(terrariumVariant.TerrariumId);
            // Cập nhật lại giá của Terrarium sau khi cập nhật variant
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
        var terrariumVariant = _unitOfWork.TerrariumVariant.GetById(id);
        if (terrariumVariant != null)
        {
            var result = await _unitOfWork.TerrariumVariant.RemoveAsync(terrariumVariant);
            if (result)
                return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Terrarium variant deleted successfully.", result);
            return new BusinessResult(Const.FAIL_DELETE_CODE, "Terrarium variant could not be deleted.");
        }

        return new BusinessResult(Const.FAIL_READ_CODE, "Terrarium variant not found.");
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
            terrarium.MinPrice = minPrice;  // Cập nhật giá thấp nhất
            terrarium.MaxPrice = maxPrice;  // Cập nhật giá cao nhất
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