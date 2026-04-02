using ChatApp.Application.NewFolder;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace ChatApp.API.Hubs
{
    public class ChatHub : Hub<IChatClient>
    {
        private readonly ChatDbContext _context;

        public ChatHub(ChatDbContext context)
        {
            _context = context;
        }

        public async Task SendPrivateMessage(string receiverId, string message)
        {
            var senderId = Context.UserIdentifier ?? "Anonymous"; // In prod, this comes from JWT authentication

            // 1. Create the Domain Entity
            var chatMessage = new ChatMessage(senderId, receiverId, message);

            // 2. Persist to Database
            _context.Messages.Add(chatMessage);
            await _context.SaveChangesAsync();

            // 3. SignalR
            // We send it to the specific user. SignalR handles the connection mapping
            await Clients.User(receiverId).ReceiveMessage(senderId, message, chatMessage.SentAt);

            // Also send it back to the sender so their UI updates
            await Clients.Caller.ReceiveMessage(senderId, message, chatMessage.SentAt);
        }

        public async Task NotifyTyping(string receiverId)
        {
            await Clients.User(receiverId).UserTyping(Context.UserIdentifier ?? "Anonymous");
        }
    }
}
