using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObjects.Models.Chatbot;
using Services.Chatbot.Handlers;

namespace Services.Chatbot
{
    /// <summary>
    /// Service chính xử lý chatbot - Orchestrator của toàn bộ pipeline
    /// Nhiệm vụ:
    /// 1. Nhận request từ ViewModel
    /// 2. Gọi IntentRecognizerService phân tích intent
    /// 3. Route đến đúng handler (MenuIntentHandler, TableIntentHandler...)
    /// 4. Lấy data từ handler
    /// 5. Format data
    /// 6. Gọi GeminiApiService tạo response tự nhiên
    /// 7. Return response về ViewModel
    /// </summary>
    public class ChatbotService
    {
        #region Private Fields

        // Service phân tích intent từ câu hỏi user
        private readonly IntentRecognizerService _intentRecognizer;

        // Service gọi Gemini API để tạo response tự nhiên
        private readonly GeminiApiService _geminiService;

        // Dictionary lưu trữ các handlers, key = IntentType
        private readonly Dictionary<string, IIntentHandler> _handlers;

        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo ChatbotService với các dependencies
        /// </summary>
        public ChatbotService()
        {
            // Khởi tạo các services
            _intentRecognizer = new IntentRecognizerService();
            _geminiService = new GeminiApiService();

            // Khởi tạo và đăng ký các handlers
            _handlers = new Dictionary<string, IIntentHandler>();
            RegisterHandlers();
        }

        /// <summary>
        /// Constructor cho Dependency Injection (nếu cần sau này)
        /// </summary>
        public ChatbotService(
            IntentRecognizerService intentRecognizer,
            GeminiApiService geminiService,
            IEnumerable<IIntentHandler> handlers)
        {
            _intentRecognizer = intentRecognizer ?? throw new ArgumentNullException(nameof(intentRecognizer));
            _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));

            // Convert handlers thành dictionary
            _handlers = handlers?.ToDictionary(h => h.IntentType, h => h)
                ?? throw new ArgumentNullException(nameof(handlers));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Xử lý tin nhắn từ user - METHOD CHÍNH
        /// Pipeline: User input → Intent analysis → Handler → Format → Gemini → Response
        /// </summary>
        /// <param name="request">ChatbotRequest chứa tin nhắn và lịch sử hội thoại</param>
        /// <returns>ChatbotResponse chứa câu trả lời</returns>
        public async Task<ChatbotResponse> ProcessMessageAsync(ChatbotRequest request)
        {
            try
            {
                // === BƯỚC 1: VALIDATE INPUT ===
                if (request == null)
                    throw new ArgumentNullException(nameof(request));

                if (string.IsNullOrWhiteSpace(request.UserMessage))
                    throw new ArgumentException("User message cannot be empty.");

                // === BƯỚC 2: PHÂN TÍCH INTENT ===
                // Gọi IntentRecognizerService (sẽ gọi Gemini API để phân tích)
                var intent = await _intentRecognizer.RecognizeIntentAsync(
                    request.UserMessage,
                    request.ConversationHistory
                );

                // Log intent (optional)
                Console.WriteLine($"[ChatbotService] Detected Intent: {intent.IntentType} (Confidence: {intent.Confidence})");

                // === BƯỚC 3: ROUTE ĐẾN HANDLER TƯƠNG ỨNG ===
                if (_handlers.TryGetValue(intent.IntentType, out var handler))
                {
                    // === BƯỚC 4: LẤY DỮ LIỆU TỪ DATABASE ===
                    var rawData = await handler.HandleAsync(intent.Parameters);

                    // === BƯỚC 5: FORMAT DỮ LIỆU THÀNH TEXT ===
                    var formattedText = handler.FormatResponse(rawData);

                    // === BƯỚC 6: GỬI CHO GEMINI ĐỂ TẠO RESPONSE TỰ NHIÊN ===
                    var naturalResponse = await _geminiService.GenerateNaturalResponseAsync(
                        userQuestion: request.UserMessage,
                        dataText: formattedText,
                        conversationHistory: request.ConversationHistory
                    );

                    // === BƯỚC 7: RETURN RESPONSE ===
                    return new ChatbotResponse
                    {
                        Response = naturalResponse,
                        Success = true,
                        DetectedIntent = intent
                    };
                }
                else
                {
                    // === TRƯỜNG HỢP INTENT KHÔNG XÁC ĐỊNH ===
                    // Không tìm thấy handler phù hợp
                    var fallbackResponse = await _geminiService.GenerateFallbackResponseAsync(
                        request.UserMessage,
                        request.ConversationHistory
                    );

                    return new ChatbotResponse
                    {
                        Response = fallbackResponse,
                        Success = true,
                        DetectedIntent = intent
                    };
                }
            }
            catch (Exception ex)
            {
                // === XỬ LÝ LỖI ===
                // Log error (nếu có logging service)
                Console.WriteLine($"[ChatbotService] Error: {ex.Message}");

                return new ChatbotResponse
                {
                    Response = "Xin lỗi, đã có lỗi xảy ra khi xử lý yêu cầu của bạn. Vui lòng thử lại.",
                    Success = false,
                    ErrorMessage = ex.Message,
                    DetectedIntent = null
                };
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Đăng ký tất cả các handlers vào dictionary
        /// </summary>
        private void RegisterHandlers()
        {
            // Tạo instances của các handlers
            var handlers = new List<IIntentHandler>
            {
                new MenuIntentHandler(),
                new TableIntentHandler(),
                new PopularDishIntentHandler(),
                
                // ✨ THÊM CÁC HANDLER MỚI CHO REVENUE STATISTICS
                new RevenueStatisticsIntentHandler(IntentTypes.GetTableRevenue),
                new RevenueStatisticsIntentHandler(IntentTypes.GetDailyRevenue),
                new RevenueStatisticsIntentHandler(IntentTypes.GetPeakHours),
                new RevenueStatisticsIntentHandler(IntentTypes.GetTableRevenueRanking)
            };

            // Đăng ký vào dictionary
            foreach (var handler in handlers)
            {
                _handlers[handler.IntentType] = handler;
            }

            Console.WriteLine($"[ChatbotService] Registered {_handlers.Count} intent handlers:");
            foreach (var intentType in _handlers.Keys)
            {
                Console.WriteLine($"  - {intentType}");
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả intent types đã đăng ký
        /// </summary>
        public IEnumerable<string> GetRegisteredIntents()
        {
            return _handlers.Keys;
        }

        /// <summary>
        /// Kiểm tra xem intent có được hỗ trợ hay không
        /// </summary>
        public bool IsIntentSupported(string intentType)
        {
            return _handlers.ContainsKey(intentType);
        }

        #endregion
    }
}
