using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Common.Enums;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class VoucherRepository : GenericRepository<Voucher>
{
    private readonly TerrariumGardenTechDBContext _db;

    public VoucherRepository(TerrariumGardenTechDBContext db) : base(db)
    {
        _db = db;
    }

    // ========== GET ALL ==========
    public async Task<List<Voucher>> GetAllVouchersAsync()
        => await _db.Vouchers
            .AsNoTracking()
            .OrderByDescending(v => v.ValidTo)
            .ToListAsync();

    // ========== GET BY CODE ==========
    public async Task<Voucher?> GetByCodeAsync(string code)
    {
        code = code?.Trim() ?? string.Empty;
        return await _db.Vouchers
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Code == code);
    }

    // ========== CREATE ==========
    public async Task<Voucher> CreateVoucherAsync(Voucher v)
    {
        // Chuẩn hoá dữ liệu đầu vào
        v.Code = v.Code?.Trim();
        v.TargetUserId = string.IsNullOrWhiteSpace(v.TargetUserId) ? null : v.TargetUserId.Trim();

        if (v.TotalUsage < 0) v.TotalUsage = 0;
        if (v.RemainingUsage <= 0) v.RemainingUsage = v.TotalUsage;

        // Khởi tạo RemainingUsage theo TotalUsage (đảm bảo)
        v.RemainingUsage = Math.Max(0, Math.Min(v.RemainingUsage, v.TotalUsage));

        // (khuyến nghị) đảm bảo Status là chuỗi enum hợp lệ
        // v.Status nên được set từ controller/service: v.Status = req.Status.ToString();

        _db.Vouchers.Add(v);
        await _db.SaveChangesAsync();
        return v;
    }

    // ========== UPDATE ==========
    public async Task UpdateVoucherAsync(Voucher v)
    {
        // Chuẩn hoá dữ liệu đầu vào
        v.Code = v.Code?.Trim();
        v.TargetUserId = string.IsNullOrWhiteSpace(v.TargetUserId) ? null : v.TargetUserId.Trim();

        if (v.TotalUsage < 0) v.TotalUsage = 0;
        if (v.RemainingUsage < 0) v.RemainingUsage = 0;
        if (v.RemainingUsage > v.TotalUsage) v.RemainingUsage = v.TotalUsage;

        _db.Vouchers.Update(v);
        await _db.SaveChangesAsync();
    }

    // ========== DELETE ==========
    public async Task DeleteVoucherAsync(int voucherId)
    {
        var v = await _db.Vouchers.FindAsync(voucherId);
        if (v != null)
        {
            _db.Vouchers.Remove(v);
            await _db.SaveChangesAsync();
        }
    }

    // ========== VALIDATE DÙNG VOUCHER ==========
    public async Task<(bool ok, string reason, Voucher? v, int userUsed)> CanUserUseAsync(
        string code, string userId, decimal? orderAmount = null, DateTime? now = null)
    {
        now ??= DateTime.UtcNow.Date;
        code = code?.Trim() ?? string.Empty;
        userId = userId?.Trim() ?? string.Empty;

        var v = await _db.Vouchers
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Code == code &&
                string.Equals(x.Status, VoucherStatus.Active.ToString()) &&
                (x.ValidFrom == null || x.ValidFrom <= now) &&
                (x.ValidTo == null || x.ValidTo >= now));

        if (v == null)
            return (false, "Voucher không tồn tại/không còn hiệu lực.", null, 0);

        // ✅ KIỂM TRA MIN ORDER AMOUNT
        if (orderAmount.HasValue && v.MinOrderAmount.HasValue && orderAmount.Value < v.MinOrderAmount.Value)
            return (false, $"Đơn hàng tối thiểu {v.MinOrderAmount.Value:N0}đ để sử dụng voucher này.", v, 0);

        if (v.IsPersonal && !string.Equals(v.TargetUserId, userId, StringComparison.OrdinalIgnoreCase))
            return (false, "Voucher chỉ dành cho user được tặng.", v, 0);

        if (v.RemainingUsage <= 0)
            return (false, "Voucher đã hết lượt sử dụng.", v, 0);

        var usage = await _db.Set<VoucherUsage>().FindAsync(v.VoucherId, userId);
        var used = usage?.UsedCount ?? 0;

        if (v.PerUserUsageLimit.HasValue && used >= v.PerUserUsageLimit.Value)
            return (false, "Bạn đã dùng tối đa số lần cho phép.", v, used);

        return (true, "", v, used);
    }

    public async Task<(bool ok, string message, int remaining, int userUsed)> ConsumeAsync(
        string code, string userId, decimal? orderAmount = null, DateTime? now = null)
    {
        using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            var check = await CanUserUseAsync(code, userId, orderAmount, now);
            if (!check.ok)
                return (false, check.reason, check.v?.RemainingUsage ?? 0, check.userUsed);

            var voucherId = check.v!.VoucherId;

            var affected = await _db.Vouchers
                .Where(x => x.VoucherId == voucherId && x.RemainingUsage > 0)
                .ExecuteUpdateAsync(s => s.SetProperty(x => x.RemainingUsage, x => x.RemainingUsage - 1));

            if (affected == 0)
            {
                await tx.RollbackAsync();
                return (false, "Voucher đã hết lượt sử dụng.", check.v.RemainingUsage, check.userUsed);
            }

            var usage = await _db.Set<VoucherUsage>().FindAsync(voucherId, userId);
            if (usage == null)
            {
                usage = new VoucherUsage
                {
                    VoucherId = voucherId,
                    UserId = userId,
                    UsedCount = 1
                };
                _db.Add(usage);
            }
            else
            {
                usage.UsedCount += 1;
            }

            await _db.SaveChangesAsync();

            var currentRemaining = await _db.Vouchers
                .AsNoTracking()
                .Where(x => x.VoucherId == voucherId)
                .Select(x => x.RemainingUsage)
                .FirstAsync();

            await tx.CommitAsync();
            return (true, "Sử dụng voucher thành công.", currentRemaining, usage.UsedCount);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
