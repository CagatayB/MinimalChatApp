using Microsoft.AspNetCore.SignalR;

namespace ChatApp.API
{
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string? GetUserId(HubConnectionContext connection)
        {
            // For testing purposes, we take the UserId from a string
            return connection.GetHttpContext()?.Request.Query["userId"];
        }
    }
}
