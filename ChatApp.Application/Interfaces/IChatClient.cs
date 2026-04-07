using System;
using System.Collections.Generic;
using System.Text;

namespace ChatApp.Application.Interfaces
{
    public interface IChatClient
    {
        Task ReceiveMessage(string senderId, string message, DateTime timestamp);
        Task UserTyping(string userId);
        Task UpdateOnlineStatus(string userId, bool isOnline);
        Task ReceiveError(string errorMessage);
    }
}
