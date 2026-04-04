using System;
using System.Collections.Generic;
using System.Text;

namespace ChatApp.Domain.Entities
{
    public class ChatMessage
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public string SenderId { get; private set; } = string.Empty;
        public string ReceiverId { get; private set; } = string.Empty;
        public string Content { get; private set; } = string.Empty;
        public DateTime SentAt { get; private set; } = DateTime.UtcNow;
        public MessageStatus Status { get; private set; } = MessageStatus.Sent;
        public void MarkAsRead() => Status = MessageStatus.Read;

        public ChatMessage(string senderId, string receiverId, string content)
        {
            SenderId = senderId;
            ReceiverId = receiverId;
            Content = content;
        }
    }
}
