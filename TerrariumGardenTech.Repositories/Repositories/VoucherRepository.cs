using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;
using TerrariumGardenTech.Repositories.Enums;

namespace TerrariumGardenTech.Repositories.Repositories;

public class VoucherRepository : GenericRepository<Voucher>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    //public VoucherRepository() { }

    public VoucherRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _context = dbContext;
    }

    // Override phương thức GetAll để lấy Voucher
    public IQueryable<Voucher> GetAllVouchers()
    {
        return _context.Vouchers.AsQueryable();
    }

    // Thêm phương thức kiểm tra Voucher có tồn tại không
    public async Task<bool> AnyAsync(Expression<Func<Voucher, bool>> predicate)
    {
        return await _context.Vouchers.AnyAsync(predicate);
    }

    // Thêm phương thức lấy Voucher theo code
    public async Task<Voucher> GetVoucherByCodeAsync(string code)
    {
        return await _context.Vouchers
            .FirstOrDefaultAsync(v => v.Code == code);
    }

    // Thêm phương thức lấy tất cả Voucher đang hoạt động
    public async Task<IEnumerable<Voucher>> GetActiveVouchersAsync()
    {
        return await _context.Vouchers
            .Where(v => v.Status == VoucherStatus.Active.ToString())
            .ToListAsync();
    }

    // Thêm phương thức cập nhật Voucher
    public async Task UpdateVoucherAsync(Voucher voucher)
    {
        _context.Vouchers.Update(voucher);
        await _context.SaveChangesAsync();
    }

    // Thêm phương thức xóa Voucher
    public async Task DeleteVoucherAsync(int voucherId)
    {
        var voucher = await _context.Vouchers.FindAsync(voucherId);
        if (voucher != null)
        {
            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();
        }
    }
}