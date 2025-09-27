using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Common.RequestModel.PaymentConfig;
using TerrariumGardenTech.Common.ResponseModel.PaymentConfig;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class PaymentConfigService : IPaymentConfigService
{
    private readonly UnitOfWork _unitOfWork;

    public PaymentConfigService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PaymentConfigResponse>> GetAllAsync()
    {
        var configs = await _unitOfWork.PaymentConfigRepository.GetAllAsync();
        return configs.Select(x => new PaymentConfigResponse
        {
            DepositPercent = x.DepositPercent,
            FullPaymentDiscountPercent = x.FullPaymentDiscountPercent,
            FreeshipAmount = x.FreeshipAmount,
            OrderAmount = x.OrderAmount,
            Description = x.Description,
            UpdatedAt = x.UpdatedAt
        }).ToList();
    }

    public async Task<PaymentConfigResponse?> GetByIdAsync(int id)
    {
        var config = await _unitOfWork.PaymentConfigRepository.GetByIdAsync(id);
        if (config == null) return null;
        return new PaymentConfigResponse
        {
            DepositPercent = config.DepositPercent,
            FullPaymentDiscountPercent = config.FullPaymentDiscountPercent,
            FreeshipAmount = config.FreeshipAmount,
            OrderAmount = config.OrderAmount,
            Description = config.Description,
            UpdatedAt = config.UpdatedAt
        };
    }

    public async Task<int> CreateAsync(PaymentConfigRequest request)
    {
        var entity = new PaymentConfig
        {
            DepositPercent = request.DepositPercent,
            FullPaymentDiscountPercent = request.FullPaymentDiscountPercent,
            FreeshipAmount = request.FreeshipAmount,
            OrderAmount = request.OrderAmount,
            Description = request.Description,
            UpdatedAt = DateTime.UtcNow
        };
        await _unitOfWork.PaymentConfigRepository.CreateAsync(entity);
        await _unitOfWork.SaveAsync();
        return entity.Id;
    }

    public async Task<bool> UpdateAsync(int id, PaymentConfigRequest request)
    {
        var config = await _unitOfWork.PaymentConfigRepository.GetByIdAsync(id);
        if (config == null) return false;
        config.DepositPercent = request.DepositPercent;
        config.FullPaymentDiscountPercent = request.FullPaymentDiscountPercent;
        config.FreeshipAmount = request.FreeshipAmount;
        config.OrderAmount = request.OrderAmount;
        config.Description = request.Description;
        config.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.PaymentConfigRepository.UpdateAsync(config);
        await _unitOfWork.SaveAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var config = await _unitOfWork.PaymentConfigRepository.GetByIdAsync(id);
        if (config == null) return false;
        await _unitOfWork.PaymentConfigRepository.RemoveAsync(config);
        await _unitOfWork.SaveAsync();
        return true;
    }
}