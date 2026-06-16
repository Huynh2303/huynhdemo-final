using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Chat;
using Microsoft.EntityFrameworkCore;

namespace Demo_web_MVC.Repository.Chat
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDatabase _context;
        private readonly ILogger<ChatRepository> _logger;
        public ChatRepository (AppDatabase context, ILogger<ChatRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        //Lấy cuộc hội thoại theo ID không đồng bộ
        public async Task<Conversation?> GetConversationByIdAsync(int conversationId)
        {
            return await _context.Conversations
                .Include(x => x.Participants)
                    .ThenInclude(x => x.User)
                .Include(x => x.Messages)
                    .ThenInclude(x => x.Sender)
                .Include(x => x.Messages)
                    .ThenInclude(x => x.Attachments)
                .FirstOrDefaultAsync(x => x.Id == conversationId);
        }
        // lấy cuộc hội thoại theo user
        public async Task<List<Conversation>> GetConversationsByUserAsync(int userId)
        {
            return await _context.Conversations
                .Include(x => x.Participants)
                    .ThenInclude(x => x.User)
                .Where(x => x.Participants.Any(p => p.UserId == userId))
                .OrderByDescending(x => x.LastMessageAt ?? x.CreatedAt)
                .ToListAsync();
        }
        //lấy cuộc hội thoại hội trợ hệ thống của admin
        public async Task<List<Conversation>> GetSystemSupportConversationsAsync()
        {
            return await _context.Conversations
                .Include(x => x.Participants)
                    .ThenInclude(x => x.User)
                .Where(x => x.Type == "SystemSupport")
                .OrderByDescending(x => x.LastMessageAt ?? x.CreatedAt)
                .ToListAsync();
        }
        // lấy cuộc hội thoại hội trợ theo user
        public async Task<Conversation?> GetSystemSupportConversationByUserAsync(int userId)
        {
            return await _context.Conversations
                .Include(x => x.Participants)
                    .ThenInclude(x => x.User)
                .Include(x => x.Messages)
                    .ThenInclude(x => x.Sender)
                .Include(x => x.Messages)
                    .ThenInclude(x => x.Attachments)
                .FirstOrDefaultAsync(x =>
                    x.Type == "SystemSupport" &&
                    x.Participants.Any(p => p.UserId == userId));
        }
        //Nhận cuộc hội thoại giữa người bán và khách hàng
        public async Task<Conversation?> GetOrderSellerConversationAsync(int orderId, int userId, int sellerId)
        {
            return await _context.Conversations
                .Include(x => x.Participants)
                .FirstOrDefaultAsync(x =>
                    x.Type == "OrderSeller"
                    && x.OrderId == orderId
                    && x.Participants.Any(p => p.UserId == userId)
                    && x.Participants.Any(p => p.UserId == sellerId));
        }
        public async Task<List<ChatDropdownViewModel>> GetChatDropdownAsync(int userId)
        {
            return await _context.Conversations
                .Where(c => c.Participants.Any(p => p.UserId == userId))
                .Select(c => new ChatDropdownViewModel
                {
                    ConversationId = c.Id,

                    DisplayName = c.Participants
                        .Where(p => p.UserId != userId)
                        .Select(p => p.User.FullName ?? p.User.Username)
                        .FirstOrDefault() ?? "Hỗ trợ hệ thống",

                    LastMessage = c.Messages
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => m.Content)
                        .FirstOrDefault() ?? "Chưa có tin nhắn",

                    LastMessageAt = c.Messages
                        .OrderByDescending(m => m.CreatedAt)
                        .Select(m => (DateTime?)m.CreatedAt)
                        .FirstOrDefault(),

                    // Chưa có IsRead thì chưa xác định được tin chưa đọc
                    UnreadCount = 0
                })
                .OrderByDescending(x => x.LastMessageAt)
                .Take(5)
                .ToListAsync();
        }
    }
}
