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

    public async Task<IBusinessResult> CreateTerrariumVariantAsync(
     TerrariumVariantCreateRequest terrariumVariantCreateRequest)
    {
        try
        {
            var terrarium = await _unitOfWork.Terrarium.GetByIdAsync(terrariumVariantCreateRequest.TerrariumId);
            if (terrarium == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "Terrarium not found.");

            // ✅ VALIDATE TẤT CẢ ACCESSORIES
            foreach (var accessoryRequest in terrariumVariantCreateRequest.Accessories)
            {
                var accessory = await _unitOfWork.Accessory.GetByIdAsync(accessoryRequest.AccessoryId);
                if (accessory == null)
                    return new BusinessResult(Const.FAIL_READ_CODE, $"Accessory {accessoryRequest.AccessoryId} not found.");
            }

            // Tạo TerrariumVariant
            var terrariumVariant = new TerrariumVariant
            {
                TerrariumId = terrariumVariantCreateRequest.TerrariumId,
                VariantName = terrariumVariantCreateRequest.VariantName,
                Price = terrariumVariantCreateRequest.Price,
                StockQuantity = terrariumVariantCreateRequest.StockQuantity,
                UrlImage = terrariumVariantCreateRequest.UrlImage,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _unitOfWork.TerrariumVariant.CreateAsync(terrariumVariant);
            if (result == 0)
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Terrarium variant could not be created.");

            // ✅ TẠO TẤT CẢ ACCESSORIES CHO VARIANT
            var accessoriesResponse = new List<VariantAccessoryResponse>();

            foreach (var accessoryRequest in terrariumVariantCreateRequest.Accessories)
            {
                var variantAccessory = new TerrariumVariantAccessory
                {
                    TerrariumVariantId = terrariumVariant.TerrariumVariantId,
                    AccessoryId = accessoryRequest.AccessoryId,
                    Quantity = accessoryRequest.Quantity
                };

                await _unitOfWork.TerrariumVariantAccessory.CreateAsync(variantAccessory);

                // Lấy thông tin accessory để response
                var accessory = await _unitOfWork.Accessory.GetByIdAsync(accessoryRequest.AccessoryId);
                if (accessory != null)
                {
                    accessoriesResponse.Add(new VariantAccessoryResponse
                    {
                        AccessoryId = accessory.AccessoryId,
                        Name = accessory.Name,
                        Description = accessory.Description,
                        Price = accessory.Price,
                        Quantity = accessoryRequest.Quantity
                    });
                }
            }

            // Cập nhật Terrarium stock và prices
            await UpdateTerrariumStockAsync(terrariumVariantCreateRequest.TerrariumId);
            await UpdateTerrariumPricesAsync(terrariumVariantCreateRequest.TerrariumId);

            // ✅ TẠO RESPONSE OBJECT
            var response = new TerrariumVariantCreateResponse
            {
                TerrariumVariantId = terrariumVariant.TerrariumVariantId,
                TerrariumId = terrariumVariant.TerrariumId,
                VariantName = terrariumVariant.VariantName,
                Price = terrariumVariant.Price,
                UrlImage = terrariumVariant.UrlImage,
                StockQuantity = terrariumVariant.StockQuantity,
                CreatedAt = terrariumVariant.CreatedAt,
                Accessories = accessoriesResponse
            };

            return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Terrarium variant created successfully.", response);
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

            // ✅ VALIDATE TẤT CẢ ACCESSORIES
            foreach (var accessoryRequest in terrariumVariantUpdateRequest.Accessories)
            {
                var accessory = await _unitOfWork.Accessory.GetByIdAsync(accessoryRequest.AccessoryId);
                if (accessory == null)
                    return new BusinessResult(Const.FAIL_READ_CODE, $"Accessory {accessoryRequest.AccessoryId} not found.");
            }
            terrariumVariant.VariantName = terrariumVariantUpdateRequest.VariantName;
            terrariumVariant.Price = terrariumVariantUpdateRequest.Price;
            terrariumVariant.StockQuantity = terrariumVariantUpdateRequest.StockQuantity;
            terrariumVariant.UrlImage = terrariumVariantUpdateRequest.UrlImage;
            terrariumVariant.UpdatedAt = DateTime.UtcNow;

            var result = await _unitOfWork.TerrariumVariant.UpdateAsync(terrariumVariant);
            if (result == 0)
                return new BusinessResult(Const.FAIL_UPDATE_CODE, "Terrarium variant could not be updated.");

            // ✅ CẬP NHẬT ACCESSORIES CHO VARIANT
            // Xóa tất cả accessories cũ
            var existingAccessories = await _unitOfWork.TerrariumVariantAccessory
                .FindAsync(tva => tva.TerrariumVariantId == terrariumVariant.TerrariumVariantId);

            foreach (var existing in existingAccessories)
            {
                await _unitOfWork.TerrariumVariantAccessory.RemoveAsync(existing);
            }

            // Thêm accessories mới và tạo response
            var accessoriesResponse = new List<VariantAccessoryResponse>();

            foreach (var accessoryRequest in terrariumVariantUpdateRequest.Accessories)
            {
                var variantAccessory = new TerrariumVariantAccessory
                {
                    TerrariumVariantId = terrariumVariant.TerrariumVariantId,
                    AccessoryId = accessoryRequest.AccessoryId,
                    Quantity = accessoryRequest.Quantity
                };

                await _unitOfWork.TerrariumVariantAccessory.CreateAsync(variantAccessory);

                // Lấy thông tin accessory để response
                var accessory = await _unitOfWork.Accessory.GetByIdAsync(accessoryRequest.AccessoryId);
                if (accessory != null)
                {
                    accessoriesResponse.Add(new VariantAccessoryResponse
                    {
                        AccessoryId = accessory.AccessoryId,
                        Name = accessory.Name,
                        Description = accessory.Description,
                        Price = accessory.Price,
                        Quantity = accessoryRequest.Quantity
                    });
                }
            }

            // Cập nhật Terrarium stock và prices
            await UpdateTerrariumStockAsync(terrariumVariant.TerrariumId);
            await UpdateTerrariumPricesAsync(terrariumVariant.TerrariumId);

            // ✅ TẠO RESPONSE OBJECT
            var response = new TerrariumVariantUpdateResponse
            {
                TerrariumVariantId = terrariumVariant.TerrariumVariantId,
                TerrariumId = terrariumVariant.TerrariumId,
                VariantName = terrariumVariant.VariantName,
                Price = terrariumVariant.Price,
                UrlImage = terrariumVariant.UrlImage,
                StockQuantity = terrariumVariant.StockQuantity,
                UpdatedAt = terrariumVariant.UpdatedAt,
                Accessories = accessoriesResponse
            };

            return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Terrarium variant updated successfully.", response);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> GetAllTerrariumVariantAsync()
    {
        try
        {
            var terrariumVariants = await _unitOfWork.TerrariumVariant.GetAllAsync2();
            if (terrariumVariants == null || !terrariumVariants.Any())
                return new BusinessResult(Const.FAIL_READ_CODE, "No terrarium variants found.");

            var responseList = new List<TerrariumVariantResponse>();

            foreach (var variant in terrariumVariants)
            {
                var response = new TerrariumVariantResponse
                {
                    TerrariumVariantId = variant.TerrariumVariantId,
                    TerrariumId = variant.TerrariumId,
                    VariantName = variant.VariantName,
                    Price = variant.Price,
                    StockQuantity = variant.StockQuantity,
                    UrlImage = variant.UrlImage,
                    CreatedAt = variant.CreatedAt,
                    UpdatedAt = variant.UpdatedAt,
                    TerrariumVariantAccessories = variant.TerrariumVariantAccessories.Select(va => new TerrariumVariantAccessoryResponse
                    {
                        TerrariumVariantAccessoryId = va.TerrariumVariantAccessoryId,
                        TerrariumVariantId = va.TerrariumVariantId,
                        AccessoryId = va.AccessoryId,
                        Quantity = va.Quantity
                    }).ToList()
                };

                responseList.Add(response);
            }

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Terrarium variants retrieved successfully.", responseList);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult> GetAllVariantByTerrariumIdAsync(int terrariumId)
    {
        try
        {
            var terrariumVariants = await _unitOfWork.TerrariumVariant.GetAllByTerrariumIdAsync(terrariumId);
            if (terrariumVariants == null || !terrariumVariants.Any())
                return new BusinessResult(Const.FAIL_READ_CODE, "No terrarium variants found.");

            var responseList = new List<TerrariumVariantResponse>();

            foreach (var variant in terrariumVariants)
            {
                var response = new TerrariumVariantResponse
                {
                    TerrariumVariantId = variant.TerrariumVariantId,
                    TerrariumId = variant.TerrariumId,
                    VariantName = variant.VariantName,
                    Price = variant.Price,
                    StockQuantity = variant.StockQuantity,
                    UrlImage = variant.UrlImage,
                    CreatedAt = variant.CreatedAt,
                    UpdatedAt = variant.UpdatedAt,
                    TerrariumVariantAccessories = variant.TerrariumVariantAccessories.Select(va => new TerrariumVariantAccessoryResponse
                    {
                        TerrariumVariantAccessoryId = va.TerrariumVariantAccessoryId,
                        TerrariumVariantId = va.TerrariumVariantId,
                        AccessoryId = va.AccessoryId,
                        Quantity = va.Quantity
                    }).ToList()
                };

                responseList.Add(response);
            }

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Terrarium variants retrieved successfully.", responseList);
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }

    public async Task<IBusinessResult?> GetTerrariumVariantByIdAsync(int id)
    {
        try
        {
            var terrariumVariant = await _unitOfWork.TerrariumVariant.GetByIdAsync2(id);
            if (terrariumVariant == null)
                return new BusinessResult(Const.FAIL_READ_CODE, "No terrarium variant found.");

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
                TerrariumVariantAccessories = terrariumVariant.TerrariumVariantAccessories.Select(va => new TerrariumVariantAccessoryResponse
                {
                    TerrariumVariantAccessoryId = va.TerrariumVariantAccessoryId,
                    TerrariumVariantId = va.TerrariumVariantId,
                    AccessoryId = va.AccessoryId,
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