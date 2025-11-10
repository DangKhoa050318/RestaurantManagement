using System;

namespace BusinessObjects.Models.Chatbot
{
    public class ChatbotResponse
    {
        public string Response { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public ChatIntent? DetectedIntent { get; set; }
    }
}
