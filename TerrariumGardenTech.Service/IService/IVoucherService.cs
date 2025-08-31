using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Service.IService;

public interface IVoucherService
{
    Task<List<Voucher>> GetAllAsync(CancellationToken ct = default);
    Task<Voucher?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Voucher> CreateAsync(Voucher v, CancellationToken ct = default);
    Task UpdateAsync(Voucher v, CancellationToken ct = default);
    Task DeleteAsync(int voucherId, CancellationToken ct = default);
    Task<(bool ok, string reason, Voucher? voucher, int userUsed)> CanUseWithOrderAmountAsync(
       string code, string userId, decimal orderAmount, CancellationToken ct = default);

    Task<(bool ok, string message, int remaining, int userUsed)> ConsumeWithOrderAmountAsync(
        string code, string userId, decimal orderAmount, CancellationToken ct = default);

    Task<(bool ok, string reason, Voucher? voucher, int userUsed)> CanUseAsync(string code, string userId, CancellationToken ct = default);
    Task<(bool ok, string message, int remaining, int userUsed)> ConsumeAsync(string code, string userId, CancellationToken ct = default);
}