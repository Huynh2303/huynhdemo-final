using Demo_web_MVC.Data.AppDatabase;
using Demo_web_MVC.Models;
using Demo_web_MVC.Models.ViewModel.Chat;
using Demo_web_MVC.Repository.Chat;
using System;

namespace Demo_web_MVC.Service.Chat
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly AppDatabase _context;

        public ChatService(IChatRepository chatRepository, AppDatabase context)
        {
            _chatRepository = chatRepository;
            _context = context;
        }

        public async Task<Conversation> GetOrCreateSystemSupportConversationAsync(int userId)
        {
            var conversation = await _chatRepository.GetSystemSupportConversationByUserAsync(userId);

            if (conversation != null)
                return conversation;

            conversation = new Conversation
            {
                Type = "SystemSupport",
                Status = "Open",
                CreatedAt = DateTime.Now
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            var participant = new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId = userId,
                JoinedAt = DateTime.Now
            };

            _context.ConversationParticipants.Add(participant);
            await _context.SaveChangesAsync();

            return conversation;
        }

        public async Task<Conversation> GetOrCreateOrderSellerConversationAsync(
    int orderId,
    int userId,
    int sellerId)
        {
            if (userId == sellerId)
            {
                throw new InvalidOperationException("Bạn không thể chat với shop của chính mình.");
            }

            var conversation = await _chatRepository.GetOrderSellerConversationAsync(
                orderId,
                userId,
                sellerId);

            if (conversation != null)
                return conversation;

            conversation = new Conversation
            {
                Type = "OrderSeller",
                Status = "Open",
                OrderId = orderId,
                CreatedAt = DateTime.Now
            };

            _context.Conversations.Add(conversation);
            await _context.SaveChangesAsync();

            _context.ConversationParticipants.Add(new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId = userId,
                JoinedAt = DateTime.Now
            });

            _context.ConversationParticipants.Add(new ConversationParticipant
            {
                ConversationId = conversation.Id,
                UserId = sellerId,
                JoinedAt = DateTime.Now
            });

            await _context.SaveChangesAsync();

            return conversation;
        }

        public async Task<List<Conversation>> GetConversationsAsync(int userId, string role)
        {
            if (role == "ADMIN")
                return await _chatRepository.GetSystemSupportConversationsAsync();

            return await _chatRepository.GetConversationsByUserAsync(userId);
        }

        public async Task<Conversation?> GetConversationDetailAsync(
            int conversationId,
            int userId,
            string role)
        {
            var conversation = await _chatRepository.GetConversationByIdAsync(conversationId);

            if (conversation == null)
                return null;

            if (role == "ADMIN" && conversation.Type == "SystemSupport")
                return conversation;

            var isParticipant = conversation.Participants
                .Any(x => x.UserId == userId);

            if (!isParticipant)
                return null;

            return conversation;
        }

        public async Task<ChatMessage> SendMessageAsync(
            int conversationId,
            int senderId,
            string role,
            string content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new Exception("Nội dung tin nhắn không được để trống.");

            var conversation = await GetConversationDetailAsync(
                conversationId,
                senderId,
                role);

            if (conversation == null)
                throw new Exception("Bạn không có quyền gửi tin nhắn trong cuộc hội thoại này.");

            var message = new ChatMessage
            {
                ConversationId = conversationId,
                SenderId = senderId,
                Content = content.Trim(),
                MessageType = "Text",
                CreatedAt = DateTime.Now
            };

            _context.ChatMessages.Add(message);

            conversation.LastMessage = content.Trim();
            conversation.LastMessageAt = DateTime.Now;

            await _context.SaveChangesAsync();

            return message;
        }
        public async Task<List<Conversation>>GetOrderSellerConversationsAsync(int sellerId)
        {
            var conversations =
                await _chatRepository.GetConversationsByUserAsync(sellerId);

            return conversations
                .Where(x => x.Type == "OrderSeller")
                .OrderByDescending(x => x.LastMessageAt ?? x.CreatedAt)
                .ToList();
        }
        public async Task<List<ChatDropdownViewModel>> GetChatDropdownAsync(int userId)
        {
            return await _chatRepository.GetChatDropdownAsync(userId);
        }
    }
}