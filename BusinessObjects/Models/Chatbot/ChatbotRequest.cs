using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.Chatbot
{
    public class ChatbotRequest
    {
        public string UserMessage { get; set; } = string.Empty;
        public DateTime RequestTime { get; set; }
        public List<ChatMessage> ConversationHistory { get; set; } = new();
    }
}
