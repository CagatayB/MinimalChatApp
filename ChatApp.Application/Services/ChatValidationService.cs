using System;
using System.Collections.Generic;
using System.Text;

namespace ChatApp.Application.Services
{
    public class ChatValidationService
    {
        public bool IsValidMessage(string content)
        {
            // Prevent empty messages or spam (e.g., > 1000 chars)
            return !string.IsNullOrWhiteSpace(content) && content.Length <= 1000;
        }
    }
}
