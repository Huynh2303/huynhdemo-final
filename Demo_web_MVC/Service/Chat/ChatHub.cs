using Microsoft.AspNetCore.SignalR;

namespace Demo_web_MVC.Service.Chat
{
    public class ChatHub : Hub
    {
        public async Task JoinConversation(int conversationId)
        {
            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                $"conversation-{conversationId}");
        }

        public async Task LeaveConversation(int conversationId)
        {
            await Groups.RemoveFromGroupAsync(
                Context.ConnectionId,
                $"conversation-{conversationId}");
        }
    }
}
