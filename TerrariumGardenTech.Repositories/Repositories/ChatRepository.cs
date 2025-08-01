using Microsoft.EntityFrameworkCore;
using TerrariumGardenTech.Repositories.Base;
using TerrariumGardenTech.Repositories.Entity;

namespace TerrariumGardenTech.Repositories.Repositories;

public class ChatRepository : GenericRepository<Chat>
{
    private readonly TerrariumGardenTechDBContext _dbContext;

    public ChatRepository(TerrariumGardenTechDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Finds an existing chat between two specific users
    ///
    /// Searches for chat where users are either User1/User2 or User2/User1
    /// (order doesn't matter - chat is bidirectional)
    ///
    /// Includes:
    /// - Both users with their roles
    /// - Last message (most recent) with sender details
    ///
    /// Used for: Checking if chat exists before creating new one, getting chat details
    /// </summary>
    /// <param name="user1Id">ID of first user</param>
    /// <param name="user2Id">ID of second user</param>
    /// <returns>Chat entity with related data, or null if no chat exists</returns>
    public async Task<Chat?> GetChatBetweenUsersAsync(int user1Id, int user2Id)
    {
        return await _context.Chats
            .Include(c => c.User1)                                              // Include User1 details
                .ThenInclude(u => u.Role)                                       // Include User1's role
            .Include(c => c.User2)                                              // Include User2 details
                .ThenInclude(u => u.Role)                                       // Include User2's role
            .Include(c => c.ChatMessages.OrderByDescending(m => m.SentAt).Take(1)) // Include last message only
                .ThenInclude(m => m.Sender)                                     // Include message sender
                    .ThenInclude(s => s.Role)                                   // Include sender's role
            .FirstOrDefaultAsync(c =>
                (c.User1Id == user1Id && c.User2Id == user2Id) ||              // Check both directions
                (c.User1Id == user2Id && c.User2Id == user1Id));               // Chat is bidirectional
    }

    /// <summary>
    /// Gets all chats for a specific user, ordered by most recent activity
    ///
    /// Returns chats where user is either User1 or User2
    /// Only returns active chats (IsActive = true)
    ///
    /// Includes:
    /// - Both users with their roles
    /// - Last message (most recent) with sender details
    ///
    /// Ordering: UpdatedAt desc (most recent activity first), then CreatedAt desc
    /// Used for: Chat list display with last message previews
    /// </summary>
    /// <param name="userId">ID of user to get chats for</param>
    /// <returns>List of chats with related data, ordered by recent activity</returns>
    public async Task<IEnumerable<Chat>> GetUserChatsAsync(int userId)
    {
        return await _context.Chats
            .Include(c => c.User1)                                              
                .ThenInclude(u => u.Role)                                       
            .Include(c => c.User2)                                              
                .ThenInclude(u => u.Role)                                       
            .Include(c => c.ChatMessages.OrderByDescending(m => m.SentAt).Take(1))
                .ThenInclude(m => m.Sender)                                     
                    .ThenInclude(s => s.Role)                                   
            .Where(c => (c.User1Id == userId || c.User2Id == userId) && c.IsActive) 
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)                
            .ToListAsync();
    }

    /// <summary>
    /// Gets a specific chat with all its messages, with user access validation
    ///
    /// Security: Only returns chat if user is a participant (User1 or User2)
    /// Only returns active chats (IsActive = true)
    ///
    /// Includes:
    /// - Both users with their roles
    /// - ALL messages ordered chronologically (oldest first)
    /// - Each message's sender with role
    ///
    /// Used for: Message retrieval, access validation before operations
    /// </summary>
    /// <param name="chatId">ID of chat to retrieve</param>
    /// <param name="userId">ID of user requesting access (must be participant)</param>
    /// <returns>Chat entity with all messages and related data, or null if not found/no access</returns>
    public async Task<Chat?> GetChatWithMessagesAsync(int chatId, int userId)
    {
        return await _context.Chats
            .Include(c => c.User1)                               
                .ThenInclude(u => u.Role)                        
            .Include(c => c.User2)                               
                .ThenInclude(u => u.Role)                        
            .Include(c => c.ChatMessages.OrderBy(m => m.SentAt)) 
                .ThenInclude(m => m.Sender)                      
                    .ThenInclude(s => s.Role)                    
            .FirstOrDefaultAsync(c => c.ChatId == chatId &&      
                (c.User1Id == userId || c.User2Id == userId) &&  
                c.IsActive);                                     
    }
}
