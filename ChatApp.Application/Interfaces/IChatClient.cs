using System;
using System.Collections.Generic;
using System.Text;

namespace ChatApp.Application.NewFolder
{
    public interface IChatClient
    {
        Task ReceiveMessage(string senderId, string message, DateTime timestamp);
        Task UserTyping(string userId);
        Task UpdateOnlineStatus(string userId, bool isOnline);
    }
}
