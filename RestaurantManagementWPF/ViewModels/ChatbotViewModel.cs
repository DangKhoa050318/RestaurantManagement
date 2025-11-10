using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using BusinessObjects.Models.Chatbot;
using RestaurantManagementWPF.Helpers;
using Services.Chatbot;
using System.Diagnostics;

namespace RestaurantManagementWPF.ViewModels
{
    /// <summary>
    /// ViewModel quản lý giao diện chatbot
    /// Xử lý tương tác giữa user và AI assistant
    /// </summary>
    public class ChatbotViewModel : BaseViewModel
    {
        #region Private Fields

        private string _userInput = string.Empty;
        private bool _isSending;
        private readonly ChatbotService _chatbotService;

        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo ChatbotViewModel
        /// </summary>
        public ChatbotViewModel()
        {
            // Khởi tạo collection lưu trữ tin nhắn
            Messages = new ObservableCollection<ChatMessage>();

            // Khởi tạo ChatbotService
            _chatbotService = new ChatbotService();

            // Khởi tạo command gửi tin nhắn
            SendMessageCommand = new RelayCommand(ExecuteSendMessage, CanSendMessage);

            // Thêm tin nhắn chào mừng khi khởi tạo
            AddWelcomeMessage();
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Danh sách tin nhắn trong cuộc hội thoại
        /// </summary>
        public ObservableCollection<ChatMessage> Messages { get; set; }

        /// <summary>
        /// Nội dung tin nhắn user đang nhập
        /// </summary>
        public string UserInput
        {
            get => _userInput;
            set
            {
                SetProperty(ref _userInput, value);
                // Trigger kiểm tra lại CanExecute
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        /// <summary>
        /// Trạng thái đang gửi tin nhắn
        /// </summary>
        public bool IsSending
        {
            get => _isSending;
            set
            {
                SetProperty(ref _isSending, value);
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Command gửi tin nhắn
        /// </summary>
        public ICommand SendMessageCommand { get; }

        #endregion

        #region Command Methods

        /// <summary>
        /// Kiểm tra command có thể thực thi hay không
        /// </summary>
        private bool CanSendMessage(object? parameter)
        {
            return !string.IsNullOrWhiteSpace(UserInput) && !IsSending;
        }

        /// <summary>
        /// Thực thi gửi tin nhắn
        /// </summary>
        private async void ExecuteSendMessage(object? parameter)
        {
            if (string.IsNullOrWhiteSpace(UserInput))
                return;

            IsSending = true;

            try
            {
                // === BƯỚC 1: Thêm tin nhắn của user ===
                var userMessage = new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    Content = UserInput,
                    IsUserMessage = true,
                    Timestamp = DateTime.Now,
                    Status = ChatMessageStatus.Sent
                };
                Messages.Add(userMessage);

                // Lưu input và xóa textbox
                var inputText = UserInput;
                UserInput = string.Empty;
                // ✅ LOG 2: Đã lưu tin nhắn user
                Debug.WriteLine($"[DEBUG UI] ✅ Đã thêm tin nhắn user. Tổng số tin: {Messages.Count}");

                // === BƯỚC 2: Thêm tin nhắn "đang xử lý" ===
                var botMessage = new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    Content = "⏳ Đang xử lý...",
                    IsUserMessage = false,
                    Timestamp = DateTime.Now,
                    Status = ChatMessageStatus.Processing
                };
                Messages.Add(botMessage);

                Debug.WriteLine($"[ChatbotViewModel] 🔄 Đang tạo ChatbotRequest...");

                // === BƯỚC 3: Gọi ChatbotService ===
                var request = new ChatbotRequest
                {
                    UserMessage = inputText,
                    RequestTime = DateTime.Now,
                    ConversationHistory = new System.Collections.Generic.List<ChatMessage>(Messages)
                };
                Debug.WriteLine("--------------------------------------------------");
                Debug.WriteLine($"[DEBUG-OUT] 🚀 ĐANG GỬI YÊU CẦU XUỐNG SERVICE:");
                Debug.WriteLine($"   - Loại dữ liệu: {request.GetType().Name}");
                Debug.WriteLine($"   - Nội dung user: '{request.UserMessage}'");
                Debug.WriteLine($"   - Thời gian gửi: {request.RequestTime:HH:mm:ss}");
                Debug.WriteLine($"   - Lịch sử kèm theo: {request.ConversationHistory.Count} tin nhắn");
                Debug.WriteLine("--------------------------------------------------");

                var response = await _chatbotService.ProcessMessageAsync(request);

                Debug.WriteLine($"[DEBUG-IN] ✅ SERVICE ĐÃ PHẢN HỒI:");
                Debug.WriteLine($"   - Thành công: {response.Success}");
                Debug.WriteLine($"   - Độ dài phản hồi: {response.Response?.Length ?? 0} ký tự");
                Debug.WriteLine("--------------------------------------------------");

                // === BƯỚC 4: Cập nhật response ===
                if (response.Success)
                {
                    botMessage.Content = response.Response;
                    botMessage.Status = ChatMessageStatus.Sent;
                }
                else
                {
                    botMessage.Content = $"❌ Lỗi: {response.ErrorMessage ?? "Không thể xử lý yêu cầu"}";
                    botMessage.Status = ChatMessageStatus.Failed;
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi
                var errorMessage = new ChatMessage
                {
                    Id = Guid.NewGuid(),
                    Content = $"❌ Lỗi: {ex.Message}",
                    IsUserMessage = false,
                    Timestamp = DateTime.Now,
                    Status = ChatMessageStatus.Failed
                };
                Messages.Add(errorMessage);

                // Log error
                Console.WriteLine($"[ChatbotViewModel] Error: {ex.Message}");
                Console.WriteLine($"[ChatbotViewModel] StackTrace: {ex.StackTrace}");
            }
            finally
            {
                IsSending = false;
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Thêm tin nhắn chào mừng khi khởi tạo
        /// </summary>
        private void AddWelcomeMessage()
        {
            var welcomeMessage = new ChatMessage
            {
                Id = Guid.NewGuid(),
                Content = "Xin chào! 👋 Tôi là trợ lý ảo của nhà hàng.\n\n" +
                         "Tôi có thể giúp bạn:\n" +
                         "📋 Xem thực đơn món ăn\n" +
                         "🪑 Kiểm tra tình trạng bàn\n" +
                         "⭐ Xem món ăn bán chạy\n\n" +
                         "Bạn muốn biết điều gì? 😊",
                IsUserMessage = false,
                Timestamp = DateTime.Now,
                Status = ChatMessageStatus.Sent
            };
            Messages.Add(welcomeMessage);
        }

        #endregion
    }
}
