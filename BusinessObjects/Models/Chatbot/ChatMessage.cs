using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BusinessObjects.Models.Chatbot
{
    public class ChatMessage : INotifyPropertyChanged
    {
        // Các biến private để lưu trữ giá trị thực sự
        private string _content = string.Empty;
        private ChatMessageStatus _status;

        public Guid Id { get; set; }
        public bool IsUserMessage { get; set; }
        public DateTime Timestamp { get; set; }

        // Property Content: Khi thay đổi sẽ báo cho UI biết
        public string Content
        {
            get => _content;
            set
            {
                if (_content != value)
                {
                    _content = value;
                    OnPropertyChanged(); // 🔔 Báo hiệu thay đổi!
                }
            }
        }

        // Property Status: Khi thay đổi sẽ báo cho UI biết
        public ChatMessageStatus Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged(); // 🔔 Báo hiệu thay đổi!
                }
            }
        }

        // --- Phần triển khai INotifyPropertyChanged ---
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum ChatMessageStatus
    {
        Sending,
        Sent,
        Failed,
        Processing
    }
}