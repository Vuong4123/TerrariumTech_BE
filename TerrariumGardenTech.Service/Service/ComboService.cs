using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TerrariumGardenTech.Common;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.Combo;
using TerrariumGardenTech.Common.ResponseModel.Combo;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service
{
    public class ComboService : IComboService
    {
        private readonly UnitOfWork _unitOfWork;
        private readonly ILogger<ComboService> _logger;

        public ComboService(UnitOfWork unitOfWork, ILogger<ComboService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<IBusinessResult> GetAllCombosAsync(GetCombosRequest request)
        {
            try
            {
                var totalItems = await _unitOfWork.Combo.CountCombosWithFiltersAsync(request);
                var totalPages = (int)Math.Ceiling((double)totalItems / request.PageSize);

                var combos = await _unitOfWork.Combo.GetCombosWithFiltersAsync(request);

                var result = new
                {
                    Items = await MapToCombosResponse(combos),
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = request.Page,
                    PageSize = request.PageSize
                };

                return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy danh sách combo thành công", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting combos");
                return new BusinessResult(Const.FAIL_READ_CODE, $"Lỗi khi lấy danh sách combo: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> GetFeaturedCombosAsync(int take = 10)
        {
            try
            {
                var featuredCombos = await _unitOfWork.Combo.GetFeaturedCombosAsync(take);
                var result = await MapToCombosResponse(featuredCombos);

                return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy combo nổi bật thành công", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting featured combos");
                return new BusinessResult(Const.FAIL_READ_CODE, $"Lỗi khi lấy combo nổi bật: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> GetCombosByCategoryAsync(int categoryId, int page = 1, int pageSize = 12)
        {
            try
            {
                var categoryCombos = await _unitOfWork.Combo.GetCombosByCategoryAsync(categoryId);

                var totalItems = categoryCombos.Count;
                var totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

                var pagedCombos = categoryCombos
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                var result = new
                {
                    Items = await MapToCombosResponse(pagedCombos),
                    TotalItems = totalItems,
                    TotalPages = totalPages,
                    CurrentPage = page,
                    PageSize = pageSize
                };

                return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy combo theo danh mục thành công", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting combos by category");
                return new BusinessResult(Const.FAIL_READ_CODE, $"Lỗi khi lấy combo theo danh mục: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> GetComboByIdAsync(int id)
        {
            try
            {
                var combo = await _unitOfWork.Combo
                    .Include(c => c.ComboItems)
                    .Include(c => c.ComboCategory)
                    .FirstOrDefaultAsync(r => r.ComboId == id);

                if (combo == null)
                {
                    return new BusinessResult(Const.NOT_FOUND_CODE, "Không tìm thấy combo");
                }

                var result = await MapToComboResponse(combo);
                return new BusinessResult(Const.SUCCESS_READ_CODE, "Lấy thông tin combo thành công", result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting combo by id: {Id}", id);
                return new BusinessResult(Const.FAIL_READ_CODE, $"Lỗi khi lấy thông tin combo: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> CreateComboAsync(CreateComboRequest request)
        {
            try
            {
                // Validate category exists
                var category = await _unitOfWork.ComboCategory.GetByIdAsync(request.ComboCategoryId);
                if (category == null)
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "Danh mục combo không tồn tại");
                }

                // Validate combo items
                var validationResult = await ValidateComboItems(request.Items);
                if (validationResult.Status != Const.SUCCESS_READ_CODE)
                {
                    return validationResult;
                }

                // Calculate original price
                var originalPrice = await CalculateOriginalPrice(request.Items);

                var combo = new Combo
                {
                    ComboCategoryId = request.ComboCategoryId,
                    Name = request.Name.Trim(),
                    Description = request.Description?.Trim(),
                    ImageUrl = request.ImageUrl?.Trim(),
                    OriginalPrice = originalPrice,
                    ComboPrice = request.ComboPrice,
                    DiscountPercent = request.DiscountPercent,
                    StockQuantity = request.StockQuantity,
                    IsFeatured = request.IsFeatured,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                var result = await _unitOfWork.Combo.CreateAsync(combo);
                if (result <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "Tạo combo thất bại");
                }

                // Create combo items
                await CreateComboItems(combo.ComboId, request.Items);

                await _unitOfWork.SaveAsync();

                var response = await MapToComboResponse(combo);
                return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Tạo combo thành công", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating combo");
                return new BusinessResult(Const.FAIL_CREATE_CODE, $"Lỗi khi tạo combo: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> UpdateComboAsync(UpdateComboRequest request)
        {
            try
            {
                var combo = await _unitOfWork.Combo.GetByIdAsync(request.ComboId);
                if (combo == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA_CODE, "Không tìm thấy combo");
                }

                // Validate category exists
                var category = await _unitOfWork.ComboCategory.GetByIdAsync(request.ComboCategoryId);
                if (category == null)
                {
                    return new BusinessResult(Const.FAIL_UPDATE_CODE, "Danh mục combo không tồn tại");
                }

                // Validate combo items
                var validationResult = await ValidateComboItems(request.Items);
                if (validationResult.Status != Const.SUCCESS_READ_CODE)
                {
                    return validationResult;
                }

                // Calculate new original price
                var originalPrice = await CalculateOriginalPrice(request.Items);

                // Update combo
                combo.ComboCategoryId = request.ComboCategoryId;
                combo.Name = request.Name.Trim();
                combo.Description = request.Description?.Trim();
                combo.ImageUrl = request.ImageUrl?.Trim();
                combo.OriginalPrice = originalPrice;
                combo.ComboPrice = request.ComboPrice;
                combo.DiscountPercent = request.DiscountPercent;
                combo.StockQuantity = request.StockQuantity;
                combo.IsFeatured = request.IsFeatured;
                combo.IsActive = request.IsActive;
                combo.UpdatedAt = DateTime.Now;

                await _unitOfWork.Combo.UpdateAsync(combo);

                // Remove old combo items
                await _unitOfWork.ComboItem.DeleteByComboIdAsync(request.ComboId);

                // Create new combo items
                await CreateComboItems(combo.ComboId, request.Items);

                await _unitOfWork.SaveAsync();

                var response = await MapToComboResponse(combo);
                return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Cập nhật combo thành công", response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating combo");
                return new BusinessResult(Const.FAIL_UPDATE_CODE, $"Lỗi khi cập nhật combo: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> DeleteComboAsync(int id)
        {
            try
            {
                var combo = await _unitOfWork.Combo.GetByIdAsync(id);
                if (combo == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA_CODE, "Không tìm thấy combo");
                }

                // Check if combo is in any active orders
                var hasActiveOrders = await _unitOfWork.Combo.HasActiveOrdersAsync(id);
                if (hasActiveOrders)
                {
                    return new BusinessResult(Const.FAIL_DELETE_CODE, "Không thể xóa combo đang có trong đơn hàng đang xử lý");
                }

                // Remove combo items first
                await _unitOfWork.ComboItem.DeleteByComboIdAsync(id);

                // Remove combo
                await _unitOfWork.Combo.RemoveAsync(combo);
                await _unitOfWork.SaveAsync();

                return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Xóa combo thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting combo");
                return new BusinessResult(Const.FAIL_DELETE_CODE, $"Lỗi khi xóa combo: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> ToggleComboActiveAsync(int id)
        {
            try
            {
                var combo = await _unitOfWork.Combo.GetByIdAsync(id);
                if (combo == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA_CODE, "Không tìm thấy combo");
                }

                combo.IsActive = !combo.IsActive;
                combo.UpdatedAt = DateTime.Now;

                await _unitOfWork.Combo.UpdateAsync(combo);
                await _unitOfWork.SaveAsync();

                var status = combo.IsActive ? "kích hoạt" : "vô hiệu hóa";
                return new BusinessResult(Const.SUCCESS_UPDATE_CODE, $"Đã {status} combo thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling combo active status");
                return new BusinessResult(Const.FAIL_UPDATE_CODE, $"Lỗi khi thay đổi trạng thái combo: {ex.Message}");
            }
        }

        public async Task<IBusinessResult> UpdateComboStockAsync(int comboId, int quantity)
        {
            try
            {
                var combo = await _unitOfWork.Combo.GetByIdAsync(comboId);
                if (combo == null)
                {
                    return new BusinessResult(Const.WARNING_NO_DATA_CODE, "Không tìm thấy combo");
                }

                combo.StockQuantity = Math.Max(0, combo.StockQuantity + quantity);
                combo.UpdatedAt = DateTime.Now;

                await _unitOfWork.Combo.UpdateAsync(combo);
                await _unitOfWork.SaveAsync();

                return new BusinessResult(Const.SUCCESS_UPDATE_CODE, "Cập nhật tồn kho combo thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating combo stock");
                return new BusinessResult(Const.FAIL_UPDATE_CODE, $"Lỗi khi cập nhật tồn kho combo: {ex.Message}");
            }
        }

        #region Helper Methods

        private async Task<IBusinessResult> ValidateComboItems(List<ComboItemRequest> items)
        {
            if (!items.Any())
            {
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Combo phải có ít nhất 1 sản phẩm");
            }

            foreach (var item in items)
            {
                if (item.TerrariumVariantId.HasValue)
                {
                    var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(item.TerrariumVariantId.Value);
                    if (variant == null)
                    {
                        return new BusinessResult(Const.FAIL_CREATE_CODE, $"Biến thể terrarium {item.TerrariumVariantId} không tồn tại");
                    }
                }
                else if (item.AccessoryId.HasValue)
                {
                    var accessory = await _unitOfWork.Accessory.GetByIdAsync(item.AccessoryId.Value);
                    if (accessory == null)
                    {
                        return new BusinessResult(Const.FAIL_CREATE_CODE, $"Phụ kiện {item.AccessoryId} không tồn tại");
                    }
                }
                else
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "Mỗi item phải có TerrariumVariantId hoặc AccessoryId");
                }

                if (item.Quantity <= 0)
                {
                    return new BusinessResult(Const.FAIL_CREATE_CODE, "Số lượng sản phẩm phải lớn hơn 0");
                }
            }

            return new BusinessResult(Const.SUCCESS_READ_CODE, "Validation passed");
        }

        private async Task<decimal> CalculateOriginalPrice(List<ComboItemRequest> items)
        {
            decimal totalPrice = 0;

            foreach (var item in items)
            {
                if (item.TerrariumVariantId.HasValue)
                {
                    var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(item.TerrariumVariantId.Value);
                    if (variant != null)
                    {
                        totalPrice += variant.Price * item.Quantity;
                    }
                }
                else if (item.AccessoryId.HasValue)
                {
                    var accessory = await _unitOfWork.Accessory.GetByIdAsync(item.AccessoryId.Value);
                    if (accessory != null)
                    {
                        totalPrice += accessory.Price * item.Quantity;
                    }
                }
            }

            return totalPrice;
        }

        private async Task CreateComboItems(int comboId, List<ComboItemRequest> items)
        {
            foreach (var item in items)
            {
                decimal unitPrice = 0;

                if (item.TerrariumVariantId.HasValue)
                {
                    var variant = await _unitOfWork.TerrariumVariant.GetByIdAsync(item.TerrariumVariantId.Value);
                    if (variant != null)
                    {
                        unitPrice = variant.Price;
                    }
                }
                else if (item.AccessoryId.HasValue)
                {
                    var accessory = await _unitOfWork.Accessory.GetByIdAsync(item.AccessoryId.Value);
                    if (accessory != null)
                    {
                        unitPrice = accessory.Price;
                    }
                }

                var comboItem = new ComboItem
                {
                    ComboId = comboId,
                    TerrariumVariantId = item.TerrariumVariantId,
                    TerrariumId = item.TerrariumId,
                    AccessoryId = item.AccessoryId,
                    Quantity = item.Quantity,
                    UnitPrice = unitPrice,
                    CreatedAt = DateTime.Now
                };

                await _unitOfWork.ComboItem.CreateAsync(comboItem);
            }
        }

        private async Task<List<ComboResponse>> MapToCombosResponse(List<Combo> combos)
        {
            var result = new List<ComboResponse>();

            foreach (var combo in combos)
            {
                result.Add(await MapToComboResponse(combo));
            }

            return result;
        }

        private async Task<ComboResponse> MapToComboResponse(Combo combo)
        {
            var comboItems = await MapToComboItemsResponse(combo.ComboItems.ToList());

            return new ComboResponse
            {
                ComboId = combo.ComboId,
                ComboCategoryId = combo.ComboCategoryId,
                CategoryName = combo.ComboCategory?.Name ?? "Unknown",
                Name = combo.Name,
                Description = combo.Description,
                ImageUrl = combo.ImageUrl,
                OriginalPrice = combo.OriginalPrice,
                ComboPrice = combo.ComboPrice,
                DiscountPercent = combo.DiscountPercent,
                IsActive = combo.IsActive,
                IsFeatured = combo.IsFeatured,
                StockQuantity = combo.StockQuantity,
                SoldQuantity = combo.SoldQuantity,
                Items = comboItems,
                CreatedAt = combo.CreatedAt
            };
        }

        private async Task<List<ComboItemResponse>> MapToComboItemsResponse(List<ComboItem> comboItems)
        {
            var result = new List<ComboItemResponse>();

            foreach (var item in comboItems)
            {
                result.Add(new ComboItemResponse
                {
                    ComboItemId = item.ComboItemId,
                    TerrariumVariantId = item.TerrariumVariantId,
                    TerrariumId = item.TerrariumId ?? 0,
                    AccessoryId = item.AccessoryId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                });
            }

            return result;
        }

        #endregion
    }
}
