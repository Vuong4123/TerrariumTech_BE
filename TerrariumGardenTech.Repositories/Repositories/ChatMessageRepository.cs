using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

/// <summary>
/// Class này quản lý tất cả các thao tác liên quan đến tin nhắn trong chat:
/// 1. Lấy danh sách tin nhắn có phân trang
/// 2. Đếm số tin nhắn chưa đọc của user
/// 3. Đánh dấu tin nhắn đã đọc
/// </summary>
public class ChatMessageRepository : GenericRepository<ChatMessage>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    /// <summary>
    /// Constructor - Khởi tạo repository với database context
    /// </summary>
    /// <param name="dbContext">Entity Framework database context</param>
    public ChatMessageRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Lấy tin nhắn có phân trang cho một cuộc trò chuyện cụ thể
    ///
    /// Bộ lọc:
    /// - Chỉ lấy tin nhắn từ chat được chỉ định
    /// - Loại trừ tin nhắn đã xóa (IsDeleted = false)
    /// </summary>
    /// <param name="chatId">ID của chat cần lấy tin nhắn</param>
    /// <param name="page">Số trang (bắt đầu từ 1)</param>
    /// <param name="pageSize">Số tin nhắn mỗi trang</param>
    /// <returns>Danh sách tin nhắn với thông tin người gửi, mới nhất trước</returns>
    public async Task<IEnumerable<ChatMessage>> GetChatMessagesAsync(int chatId, int page = 1, int pageSize = 50)
    {
        return await _context.ChatMessages
            .Include(m => m.Sender)                                             // Bao gồm thông tin người gửi
                .ThenInclude(s => s.Role)                                       // Bao gồm vai trò của người gửi
            .Where(m => m.ChatId == chatId && !m.IsDeleted)                     // Lọc theo chat và loại trừ tin đã xóa
            .OrderByDescending(m => m.SentAt)                                   // Mới nhất trước (sẽ đảo ngược ở service)
            .Skip((page - 1) * pageSize)                                        // Bỏ qua các trang trước
            .Take(pageSize)                                                     // Lấy trang hiện tại
            .ToListAsync();
    }

    /// <summary>
    /// Đếm tin nhắn chưa đọc trong chat cho một user cụ thể
    ///
    /// Đếm các tin nhắn:
    /// - Trong chat được chỉ định
    /// - KHÔNG phải do user này gửi (từ người khác)
    /// - Chưa được đọc (IsRead = false)
    /// - Chưa bị xóa (IsDeleted = false)
    /// </summary>
    /// <param name="chatId">ID của chat cần đếm tin nhắn chưa đọc</param>
    /// <param name="userId">ID của user cần đếm tin nhắn chưa đọc</param>
    /// <returns>Số lượng tin nhắn chưa đọc từ người khác</returns>
    public async Task<int> GetUnreadMessageCountAsync(int chatId, int userId)
    {
        return await _context.ChatMessages
            .CountAsync(m => m.ChatId == chatId &&                              // Trong chat được chỉ định
                            m.SenderId != userId &&                             // Không phải tin nhắn của user này
                            !m.IsRead &&                                        // Chưa được đọc
                            !m.IsDeleted);                                      // Chưa bị xóa
    }

    /// <summary>
    /// Đánh dấu tất cả tin nhắn chưa đọc từ người khác là đã đọc trong chat cụ thể
    ///
    /// Cập nhật các tin nhắn:
    /// - Trong chat được chỉ định
    /// - KHÔNG phải do user này gửi (từ người khác)
    /// - Chưa được đọc (IsRead = false)
    /// - Chưa bị xóa (IsDeleted = false)
    ///
    /// </summary>
    /// <param name="chatId">ID của chat cần đánh dấu tin nhắn đã đọc</param>
    /// <param name="userId">ID của user đang đánh dấu tin nhắn đã đọc</param>
    public async Task MarkMessagesAsReadAsync(int chatId, int userId)
    {
        // Tìm tất cả tin nhắn chưa đọc từ người khác trong chat này
        var unreadMessages = await _context.ChatMessages
            .Where(m => m.ChatId == chatId &&                                   // Trong chat được chỉ định
                       m.SenderId != userId &&                                  // Không phải tin nhắn của user này
                       !m.IsRead &&                                             // Chưa được đọc
                       !m.IsDeleted)                                            // Chưa bị xóa
            .ToListAsync();

        // Đánh dấu từng tin nhắn là đã đọc
        foreach (var message in unreadMessages)
        {
            message.IsRead = true;
        }

        // Lưu thay đổi vào database
        await _context.SaveChangesAsync();
    }
}
