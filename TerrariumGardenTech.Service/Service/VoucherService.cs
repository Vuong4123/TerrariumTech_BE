using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Repositories;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Service.IService;

namespace TerrariumGardenTech.Service.Service;

public class VoucherService : IVoucherService
{
    private readonly UnitOfWork _unitOfWork;
    public VoucherService(UnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    // ========= READ =========
    public async Task<List<Voucher>> GetAllAsync(CancellationToken ct = default)
        => await _unitOfWork.Voucher.GetAllVouchersAsync();

    public async Task<Voucher?> GetByCodeAsync(string code, CancellationToken ct = default)
        => await _unitOfWork.Voucher.GetByCodeAsync(code);

    // ========= CREATE/UPDATE/DELETE =========
    public async Task<Voucher> CreateAsync(Voucher v, CancellationToken ct = default)
    {
        // Chuẩn hoá số lượt khi tạo
        if (v.TotalUsage < 0) v.TotalUsage = 0;
        if (v.RemainingUsage <= 0) v.RemainingUsage = v.TotalUsage;

        return await _unitOfWork.Voucher.CreateVoucherAsync(v);
    }

    public async Task UpdateAsync(Voucher v, CancellationToken ct = default)
    {
        // Không để RemainingUsage âm hoặc > TotalUsage
        if (v.TotalUsage < 0) v.TotalUsage = 0;
        if (v.RemainingUsage < 0) v.RemainingUsage = 0;
        if (v.RemainingUsage > v.TotalUsage) v.RemainingUsage = v.TotalUsage;

        await _unitOfWork.Voucher.UpdateVoucherAsync(v);
    }

    public async Task DeleteAsync(int voucherId, CancellationToken ct = default)
        => await _unitOfWork.Voucher.DeleteVoucherAsync(voucherId);

    // ========= PER-USER LOGIC =========
    public async Task<(bool ok, string reason, Voucher? voucher, int userUsed)> CanUseAsync(
        string code, string userId, CancellationToken ct = default)
        => await _unitOfWork.Voucher.CanUserUseAsync(code, userId);

    public async Task<(bool ok, string message, int remaining, int userUsed)> ConsumeAsync(
        string code, string userId, CancellationToken ct = default)
        => await _unitOfWork.Voucher.ConsumeAsync(code, userId);

    // ========= Optional tiện ích (không thuộc interface) =========
    public async Task<bool> IsVoucherValidAsync(string code)
    {
        var v = await GetByCodeAsync(code);
        if (v == null) return false;

        var today = DateTime.UtcNow.Date;
        return string.Equals(v.Status, VoucherStatus.Active.ToString(), StringComparison.OrdinalIgnoreCase)
            && (v.ValidFrom == null || v.ValidFrom.Value.Date <= today)
            && (v.ValidTo == null || v.ValidTo.Value.Date >= today)
            && v.RemainingUsage > 0;
    }
    public async Task<(bool ok, string reason, Voucher? voucher, int userUsed)> CanUseWithOrderAmountAsync(
       string code, string userId, decimal orderAmount, CancellationToken ct = default)
       => await _unitOfWork.Voucher.CanUserUseAsync(code, userId, orderAmount);

    public async Task<(bool ok, string message, int remaining, int userUsed)> ConsumeWithOrderAmountAsync(
        string code, string userId, decimal orderAmount, CancellationToken ct = default)
        => await _unitOfWork.Voucher.ConsumeAsync(code, userId, orderAmount);
}
