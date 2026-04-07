using ChatApp.Application.Interfaces;
using ChatApp.Application.Services;
using ChatApp.Domain.Entities;
using ChatApp.Infrastructure.Persistence;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ChatApp.API.Hubs
{
    public class ChatHub : Hub<IChatClient>
    {
        private readonly ChatDbContext _context;
        private readonly IChatValidationService _validator;

        public ChatHub(ChatDbContext context, IChatValidationService validator)
        {
            _context = context;
            _validator = validator;
        }

        public async Task SendPrivateMessage(string receiverId, string message)
        {
            if (!_validator.IsValidMessage(message))
            {
                await Clients.Caller.ReceiveError("Message cannot be empty and must be under 500 characters.");
                return;
            }

            // Context.UserIdentifier is now automatically the User ID from the JWT
            var senderId = Context.UserIdentifier!;
            if (string.IsNullOrEmpty(senderId)) return;

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
