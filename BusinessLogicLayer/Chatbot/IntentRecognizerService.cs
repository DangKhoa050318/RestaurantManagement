using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessObjects.Models.Chatbot;

namespace Services.Chatbot
{
    /// <summary>
    /// Service phân tích intent từ câu hỏi user
    /// Nhiệm vụ:
    /// 1. Nhận câu hỏi user + lịch sử hội thoại
    /// 2. Gọi GeminiApiService để phân tích intent
    /// 3. Validate và normalize intent
    /// 4. Có thể áp dụng rule-based detection trước khi gọi AI (để tiết kiệm API calls)
    /// 5. Return ChatIntent với IntentType và Parameters
    /// </summary>
    public class IntentRecognizerService
    {
        #region Private Fields

        private readonly GeminiApiService _geminiService;

        // Cache để lưu intent của các câu hỏi phổ biến (optional)
        private readonly Dictionary<string, ChatIntent> _intentCache;

        // Flag để bật/tắt rule-based detection
        private readonly bool _useRuleBasedDetection;

        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo IntentRecognizerService
        /// </summary>
        /// <param name="useRuleBasedDetection">Có sử dụng rule-based detection trước AI hay không</param>
        public IntentRecognizerService(bool useRuleBasedDetection = true)
        {
            _geminiService = new GeminiApiService();
            _intentCache = new Dictionary<string, ChatIntent>(StringComparer.OrdinalIgnoreCase);
            _useRuleBasedDetection = useRuleBasedDetection;

            Console.WriteLine($"[IntentRecognizerService] Initialized (RuleBasedDetection: {useRuleBasedDetection})");
        }

        /// <summary>
        /// Constructor cho Dependency Injection
        /// </summary>
        public IntentRecognizerService(GeminiApiService geminiService, bool useRuleBasedDetection = true)
        {
            _geminiService = geminiService ?? throw new ArgumentNullException(nameof(geminiService));
            _intentCache = new Dictionary<string, ChatIntent>(StringComparer.OrdinalIgnoreCase);
            _useRuleBasedDetection = useRuleBasedDetection;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Phân tích intent từ câu hỏi user - METHOD CHÍNH
        /// Pipeline: 
        /// 1. Check cache
        /// 2. Try rule-based detection (nếu enabled)
        /// 3. Fallback to AI (Gemini)
        /// 4. Validate & normalize
        /// 5. Cache result
        /// </summary>
        /// <param name="userMessage">Câu hỏi của user</param>
        /// <param name="conversationHistory">Lịch sử hội thoại (optional)</param>
        /// <returns>ChatIntent với IntentType và Parameters</returns>
        public async Task<ChatIntent> RecognizeIntentAsync(
            string userMessage,
            List<ChatMessage> conversationHistory = null)
        {
            try
            {
                // === VALIDATE INPUT ===
                if (string.IsNullOrWhiteSpace(userMessage))
                {
                    throw new ArgumentException("User message cannot be empty.");
                }

                // Normalize message (lowercase, trim)
                var normalizedMessage = NormalizeMessage(userMessage);

                // === BƯỚC 1: CHECK CACHE ===
                // Nếu câu hỏi này đã được hỏi trước đó
                if (_intentCache.TryGetValue(normalizedMessage, out var cachedIntent))
                {
                    Console.WriteLine($"[IntentRecognizerService] Cache hit: {cachedIntent.IntentType}");
                    return cachedIntent;
                }

                ChatIntent intent;

                // === BƯỚC 2: TRY RULE-BASED DETECTION ===
                // Kiểm tra các pattern đơn giản trước khi gọi AI
                if (_useRuleBasedDetection)
                {
                    intent = TryRuleBasedDetection(normalizedMessage);
                    
                    if (intent != null && intent.IntentType != IntentTypes.Unknown)
                    {
                        Console.WriteLine($"[IntentRecognizerService] Rule-based detection: {intent.IntentType}");
                        
                        // Cache kết quả
                        _intentCache[normalizedMessage] = intent;
                        
                        return intent;
                    }
                }

                // === BƯỚC 3: FALLBACK TO AI (GEMINI) ===
                // Gọi Gemini API để phân tích intent phức tạp
                Console.WriteLine($"[IntentRecognizerService] Calling Gemini API for intent analysis...");
                
                intent = await _geminiService.AnalyzeIntentAsync(
                    userMessage,
                    conversationHistory
                );

                // === BƯỚC 4: VALIDATE & NORMALIZE INTENT ===
                intent = ValidateAndNormalizeIntent(intent);

                // === BƯỚC 5: CACHE RESULT ===
                // Cache để lần sau không cần gọi AI nữa
                _intentCache[normalizedMessage] = intent;

                Console.WriteLine($"[IntentRecognizerService] Recognized intent: {intent.IntentType} (Confidence: {intent.Confidence})");

                return intent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IntentRecognizerService] Error recognizing intent: {ex.Message}");
                
                // Return Unknown intent nếu lỗi
                return new ChatIntent
                {
                    IntentType = IntentTypes.Unknown,
                    Parameters = new Dictionary<string, object>(),
                    Confidence = 0.0
                };
            }
        }

        /// <summary>
        /// Clear cache (khi cần reset)
        /// </summary>
        public void ClearCache()
        {
            _intentCache.Clear();
            Console.WriteLine($"[IntentRecognizerService] Cache cleared");
        }

        /// <summary>
        /// Lấy số lượng intent đã cache
        /// </summary>
        public int GetCacheCount()
        {
            return _intentCache.Count;
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Normalize message: lowercase, trim, remove extra spaces
        /// </summary>
        private string NormalizeMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return string.Empty;

            // Lowercase và trim
            var normalized = message.ToLowerInvariant().Trim();

            // Remove multiple spaces
            while (normalized.Contains("  "))
            {
                normalized = normalized.Replace("  ", " ");
            }

            return normalized;
        }

        /// <summary>
        /// Rule-based intent detection
        /// Kiểm tra các keywords đơn giản để phát hiện intent nhanh
        /// Tiết kiệm API calls cho các câu hỏi phổ biến
        /// </summary>
        private ChatIntent TryRuleBasedDetection(string normalizedMessage)
        {
            // === RULE 1: GetMenu Intent ===
            var menuKeywords = new[] 
            { 
                "thực đơn", "menu", "món ăn", "món", "có món gì", 
                "có gì", "đồ ăn", "danh sách món", "list món"
            };
            
            if (menuKeywords.Any(keyword => normalizedMessage.Contains(keyword)))
            {
                return new ChatIntent
                {
                    IntentType = IntentTypes.GetMenu,
                    Parameters = new Dictionary<string, object>(),
                    Confidence = 0.85
                };
            }

            // === RULE 2: GetTableAvailability Intent ===
            var tableKeywords = new[] 
            { 
                "bàn", "table", "bàn trống", "còn bàn", "available", 
                "đặt bàn", "booking", "tình trạng bàn", "bàn nào"
            };
            
            if (tableKeywords.Any(keyword => normalizedMessage.Contains(keyword)))
            {
                return new ChatIntent
                {
                    IntentType = IntentTypes.GetTableAvailability,
                    Parameters = new Dictionary<string, object>(),
                    Confidence = 0.85
                };
            }

            // === RULE 3: GetPopularDishes Intent ===
            var popularKeywords = new[] 
            { 
                "bán chạy", "popular", "hot", "nổi bật", "được yêu thích",
                "món hot", "best seller", "top", "phổ biến", "nhiều người"
            };
            
            if (popularKeywords.Any(keyword => normalizedMessage.Contains(keyword)))
            {
                return new ChatIntent
                {
                    IntentType = IntentTypes.GetPopularDishes,
                    Parameters = new Dictionary<string, object>(),
                    Confidence = 0.85
                };
            }

            // === ✨ RULE 4: GetTableRevenue Intent (THÊM MỚI) ===
            var tableRevenueKeywords = new[] 
            { 
                "tổng bill", "bill bàn", "doanh thu bàn", "revenue table",
                "bàn", "chi tiêu", "thanh toán bàn"
            };
            
            // Phải có keyword về bàn + keyword về tiền
            var hasTableKeyword = tableRevenueKeywords.Any(k => normalizedMessage.Contains(k));
            var hasMoneyKeyword = normalizedMessage.Contains("bill") || 
                                   normalizedMessage.Contains("doanh thu") ||
                                   normalizedMessage.Contains("tiền") ||
                                   normalizedMessage.Contains("revenue");
            
            if (hasTableKeyword && hasMoneyKeyword)
            {
                // Thử extract tên bàn (VD: "A01", "B02")
                var tableNameMatch = System.Text.RegularExpressions.Regex.Match(
                    normalizedMessage, 
                    @"\b([a-z]\d{2}|vip\d+)\b"
                );

                var parameters = new Dictionary<string, object>();
                if (tableNameMatch.Success)
                {
                    parameters["tableName"] = tableNameMatch.Value.ToUpper();
                }

                return new ChatIntent
                {
                    IntentType = IntentTypes.GetTableRevenue,
                    Parameters = parameters,
                    Confidence = 0.80
                };
            }

            // === ✨ RULE 5: GetDailyRevenue Intent (CẬP NHẬT) ===
            var revenueKeywords = new[] 
            { 
                "doanh thu", "revenue", "tổng thu", "tiền", "bán được", "thu nhập"
            };
            
            var dateKeywords = new[] 
            { 
                "ngày", "tháng", "hôm nay", "hôm qua", "tuần", "date",
                "theo ngày",  // ✨ THÊM MỚI
                "theo tháng", // ✨ THÊM MỚI
                "theo tuần",  // ✨ THÊM MỚI
                "hàng ngày",  // ✨ THÊM MỚI
                "hàng tháng"  // ✨ THÊM MỚI
            };
            
            var hasRevenueKeyword = revenueKeywords.Any(k => normalizedMessage.Contains(k));
            var hasDateKeyword = dateKeywords.Any(k => normalizedMessage.Contains(k));
            
            // ✨ HOẶC: Nếu chỉ có "doanh thu" mà không có context khác (bàn, món...) → cũng coi là GetDailyRevenue
            var hasTableContext = normalizedMessage.Contains("bàn");
            var hasDishContext = normalizedMessage.Contains("món");
            
            if (hasRevenueKeyword && (hasDateKeyword || (!hasTableContext && !hasDishContext)))
            {
                Console.WriteLine($"[IntentRecognizerService] Matched GetDailyRevenue: revenue={hasRevenueKeyword}, date={hasDateKeyword}");
                
                return new ChatIntent
                {
                    IntentType = IntentTypes.GetDailyRevenue,
                    Parameters = new Dictionary<string, object>(),
                    Confidence = 0.80
                };
            }

            // === ✨ RULE 6: GetPeakHours Intent (THÊM MỚI) ===
            var peakHoursKeywords = new[] 
            { 
                "đông khách", "giờ đông", "peak", "ranh", "cao điểm",
                "đông nhất", "nhiều khách", "giờ nào đông"
            };
            
            if (peakHoursKeywords.Any(keyword => normalizedMessage.Contains(keyword)))
            {
                return new ChatIntent
                {
                    IntentType = IntentTypes.GetPeakHours,
                    Parameters = new Dictionary<string, object>(),
                    Confidence = 0.85
                };
            }

            // === ✨ RULE 7: GetTableRevenueRanking Intent (THÊM MỚI) ===
            var rankingKeywords = new[] 
            { 
                "cao nhất", "thấp nhất", "top", "ranking", "xếp hạng",
                "nhiều nhất", "ít nhất", "best", "worst"
            };
            
            var hasRankingKeyword = rankingKeywords.Any(k => normalizedMessage.Contains(k));
            var hasBanKeyword = normalizedMessage.Contains("bàn");
            
            if (hasRankingKeyword && hasBanKeyword)
            {
                // Xác định ranking type (highest/lowest)
                var isLowest = normalizedMessage.Contains("thấp nhất") || 
                               normalizedMessage.Contains("ít nhất") ||
                               normalizedMessage.Contains("worst");

                return new ChatIntent
                {
                    IntentType = IntentTypes.GetTableRevenueRanking,
                    Parameters = new Dictionary<string, object>
                    {
                        { "type", isLowest ? "lowest" : "highest" }
                    },
                    Confidence = 0.80
                };
            }

            // === RULE 8: Greetings (Map to Unknown or create GreetingIntent) ===
            var greetingKeywords = new[] 
            { 
                "xin chào", "chào", "hello", "hi", "hey" 
            };
            
            if (greetingKeywords.Any(keyword => normalizedMessage.Contains(keyword)))
            {
                return new ChatIntent
                {
                    IntentType = IntentTypes.Unknown,
                    Parameters = new Dictionary<string, object> 
                    { 
                        { "greeting", true } 
                    },
                    Confidence = 0.90
                };
            }

            // === NO MATCH: Return null để fallback to AI ===
            return null;
        }

        /// <summary>
        /// Validate và normalize intent từ AI
        /// Đảm bảo intent hợp lệ trước khi return
        /// </summary>
        private ChatIntent ValidateAndNormalizeIntent(ChatIntent intent)
        {
            if (intent == null)
            {
                return new ChatIntent
                {
                    IntentType = IntentTypes.Unknown,
                    Parameters = new Dictionary<string, object>(),
                    Confidence = 0.0
                };
            }

            // === VALIDATE INTENT TYPE ===
            // Check xem IntentType có hợp lệ không (có trong IntentTypes class)
            var validIntentTypes = new[]
            {
                IntentTypes.GetMenu,
                IntentTypes.GetTableAvailability,
                IntentTypes.GetTotalBookings,
                IntentTypes.GetPopularDishes,
                // ✨ Thêm các intent type mới
                IntentTypes.GetTableRevenue,
                IntentTypes.GetDailyRevenue,
                IntentTypes.GetPeakHours,
                IntentTypes.GetTableRevenueRanking,
                IntentTypes.Unknown
            };

            if (!validIntentTypes.Contains(intent.IntentType))
            {
                Console.WriteLine($"[IntentRecognizerService] Invalid intent type: {intent.IntentType}, defaulting to Unknown");
                intent.IntentType = IntentTypes.Unknown;
            }

            // === VALIDATE CONFIDENCE ===
            // Đảm bảo confidence trong khoảng [0, 1]
            if (intent.Confidence < 0.0)
                intent.Confidence = 0.0;
            else if (intent.Confidence > 1.0)
                intent.Confidence = 1.0;

            // === VALIDATE PARAMETERS ===
            // Đảm bảo Parameters không null
            intent.Parameters ??= new Dictionary<string, object>();

            // === LOW CONFIDENCE HANDLING ===
            // Nếu confidence quá thấp, chuyển sang Unknown
            const double MIN_CONFIDENCE_THRESHOLD = 0.3;
            
            if (intent.Confidence < MIN_CONFIDENCE_THRESHOLD && intent.IntentType != IntentTypes.Unknown)
            {
                Console.WriteLine($"[IntentRecognizerService] Low confidence ({intent.Confidence}), changing to Unknown");
                intent.IntentType = IntentTypes.Unknown;
            }

            return intent;
        }

        #endregion
    }
}
