using TerrariumGardenTech.Common;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.Base;
using TerrariumGardenTech.Service.IService;
using TerrariumGardenTech.Service.RequestModel.TerrariumVariant;

namespace TerrariumGardenTech.Service.Service;

public class TerrariumVariantService : ITerrariumVariantService
{
    private readonly UnitOfWork _unitOfWork;

    public TerrariumVariantService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IBusinessResult> CreateTerrariumVariantAsync(
        TerrariumVariantCreateRequest terrariumVariantCreateRequest)
    {
        try
        {
            var terrariumVariant = new TerrariumVariant
            {
                TerrariumId = terrariumVariantCreateRequest.TerrariumId,
                VariantName = terrariumVariantCreateRequest.VariantName,
                Price = terrariumVariantCreateRequest.Price,
                StockQuantity = terrariumVariantCreateRequest.StockQuantity
            };
            var result = await _unitOfWork.TerrariumVariant.CreateAsync(terrariumVariant);
            if (result == 0)
                return new BusinessResult(Const.FAIL_CREATE_CODE, "Terrarium variant could not be created.");
            return new BusinessResult(Const.SUCCESS_CREATE_CODE, "Terrarium variant created successfully.", result);
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

    public async Task<IBusinessResult> GetAllTerrariumVariantAsync()
    {
        var terrariumVariants = await _unitOfWork.TerrariumVariant.GetAllAsync();
        if (terrariumVariants == null || !terrariumVariants.Any())
            return new BusinessResult(Const.FAIL_READ_CODE, "No terrarium variants found.");
        return new BusinessResult(Const.SUCCESS_READ_CODE, "Terrarium variants retrieved successfully.",
            terrariumVariants);
    }

    public async Task<IBusinessResult?> GetTerrariumVariantByIdAsync(int id)
    {
        var terrariumVariants = await _unitOfWork.TerrariumVariant.GetByIdAsync(id);
        if (terrariumVariants == null) return new BusinessResult(Const.FAIL_READ_CODE, "No terrarium variants found.");
        return new BusinessResult(Const.SUCCESS_READ_CODE, "Terrarium variants retrieved successfully.",
            terrariumVariants);
    }

    public async Task<IBusinessResult> UpdateTerrariumVariantAsync(
        TerrariumVariantUpdateRequest terrariumVariantUpdateRequest)
    {
        try
        {
            var result = -1;
            var terrariumVariant = _unitOfWork.TerrariumVariant.GetById(terrariumVariantUpdateRequest.TerrariumId);
            if (terrariumVariant != null)
            {
                _unitOfWork.TerrariumVariant.Context().Entry(terrariumVariant).CurrentValues
                    .SetValues(terrariumVariantUpdateRequest);
                result = await _unitOfWork.TerrariumVariant.UpdateAsync(terrariumVariant);
                if (result == 0)
                    return new BusinessResult(Const.FAIL_DELETE_CODE, "Terrarium variant could not be update.");
                return new BusinessResult(Const.SUCCESS_DELETE_CODE, "Terrarium variant update successfully.", result);
            }

            return new BusinessResult(Const.FAIL_READ_CODE, "Terrarium variant not found.");
        }
        catch (Exception ex)
        {
            return new BusinessResult(Const.ERROR_EXCEPTION, ex.Message);
        }
    }
}