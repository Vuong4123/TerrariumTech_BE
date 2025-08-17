using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using TerrariumGardenTech.Common.Entity;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories
{
    public class CartRepository : GenericRepository<Cart>
    {
        private IDbContextTransaction? _currentTx;
        public CartRepository(TerrariumGardenTechDBContext context) : base(context)
        {
        }

        // Cập nhật phương thức GetByUserIdAsync để bao gồm Accessory và TerrariumVariant
        public async Task<Cart> GetByUserIdAsync(int userId)
        {
            // Truy vấn giỏ hàng của người dùng theo userId
            return await _context.Carts
                .Include(c => c.CartItems)  // Bao gồm CartItems
                    .ThenInclude(ci => ci.Accessory)  // Bao gồm Accessory cho từng CartItem
                .Include(c => c.CartItems)  // Bao gồm CartItems
                    .ThenInclude(ci => ci.TerrariumVariant)  // Bao gồm TerrariumVariant cho từng CartItem
                .FirstOrDefaultAsync(c => c.UserId == userId);  // Lấy giỏ hàng đầu tiên có userId khớp
        }

        public async Task<Cart?> GetHydratedByUserIdAsync(int userId, bool track = false)
        {
            var q = _context.Carts.AsQueryable().AsSplitQuery();
            if (!track) q = q.AsNoTracking();

            return await q.Where(c => c.UserId == userId)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Accessory)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.TerrariumVariant)
                        .ThenInclude(v => v.Terrarium)
                            .ThenInclude(t => t.TerrariumVariants)   // ← variants
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.TerrariumVariant)
                        .ThenInclude(v => v.Terrarium)
                            .ThenInclude(t => t.TerrariumImages)     // ← images
                .SingleOrDefaultAsync();
        }

        // option theo CartId (đôi khi hữu ích khi Patch xong muốn reload cả giỏ)
        public async Task<Cart?> GetHydratedByCartIdAsync(int cartId, bool track = false)
        {
            var q = _context.Carts.AsQueryable();
            q = q.AsSplitQuery();
            if (!track) q = q.AsNoTracking();

            return await q
                .Where(c => c.CartId == cartId)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Accessory)
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.TerrariumVariant)
                        .ThenInclude(v => v.Terrarium)
                            .ThenInclude(t => t.TerrariumVariants)
                .SingleOrDefaultAsync();
        }
        // ✅ dùng để trả về 1 line đầy đủ sau PATCH
        public Task<CartItem?> GetHydratedByIdAsync(int id) =>
            _context.CartItems
                .Include(ci => ci.Accessory)
                .Include(ci => ci.TerrariumVariant)
                    .ThenInclude(v => v.Terrarium)
                        .ThenInclude(t => t.TerrariumVariants)
                .Include(ci => ci.Cart)
                .SingleOrDefaultAsync(ci => ci.CartItemId == id);

        // Giữ nguyên phương thức ClearAsync
        public async Task<bool> ClearAsync(int userId)
        {
            var cart = await GetByUserIdAsync(userId);
            if (cart != null)
            {
                // Xóa các mục trong giỏ hàng
                _context.CartItems.RemoveRange(cart.CartItems); // Xóa hết CartItems
                await _context.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public Task CommitTransactionAsync(CancellationToken ct = default)
        => _currentTx?.CommitAsync(ct) ?? Task.CompletedTask;

        public Task RollbackTransactionAsync(CancellationToken ct = default)
            => _currentTx?.RollbackAsync(ct) ?? Task.CompletedTask;
    }

}
