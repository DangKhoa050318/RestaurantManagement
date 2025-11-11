using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BusinessObjects.Models.Chatbot;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Services.Chatbot
{
    /// <summary>
    /// Service tương tác với Gemini 2.0 Flash Experimental API
    /// Model: gemini-2.0-flash-exp
    /// Features:
    /// - Faster inference (~2x faster than Gemini 1.5)
    /// - Better context understanding
    /// - Improved JSON output
    /// - Up to 8192 output tokens
    /// </summary>
    public class GeminiApiService
    {
        #region Private Fields

        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiEndpoint;
        private readonly string _model;
        private readonly double _temperature;
        private readonly int _maxTokens;
        private readonly double _topP;
        private readonly int _topK;

        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo GeminiApiService với config từ appsettings.json
        /// </summary>
        public GeminiApiService()
        {
            var configuration = LoadConfiguration();

            _apiKey = configuration["Gemini:ApiKey"] 
                ?? throw new Exception("Gemini API Key not found in appsettings.json. Please add your API key.");
            
            _model = configuration["Gemini:Model"] ?? "gemini-2.0-flash-exp";
            
            _apiEndpoint = configuration["Gemini:ApiEndpoint"] 
                ?? $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent";
            
            _temperature = double.Parse(configuration["Gemini:Temperature"] ?? "0.7");
            _maxTokens = int.Parse(configuration["Gemini:MaxTokens"] ?? "8192");
            _topP = double.Parse(configuration["Gemini:TopP"] ?? "0.95");
            _topK = int.Parse(configuration["Gemini:TopK"] ?? "40");

            // Khởi tạo HttpClient với timeout
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            Console.WriteLine($"[GeminiApiService] Initialized with model: {_model}");
            Console.WriteLine($"[GeminiApiService] Temperature: {_temperature}, MaxTokens: {_maxTokens}");
        }

        /// <summary>
        /// Constructor cho Dependency Injection
        /// </summary>
        public GeminiApiService(IConfiguration configuration)
        {
            _apiKey = configuration["Gemini:ApiKey"] 
                ?? throw new ArgumentNullException("Gemini:ApiKey");
            
            _model = configuration["Gemini:Model"] ?? "gemini-2.0-flash-exp";
            _apiEndpoint = configuration["Gemini:ApiEndpoint"] 
                ?? $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent";
            
            _temperature = double.Parse(configuration["Gemini:Temperature"] ?? "0.7");
            _maxTokens = int.Parse(configuration["Gemini:MaxTokens"] ?? "8192");
            _topP = double.Parse(configuration["Gemini:TopP"] ?? "0.95");
            _topK = int.Parse(configuration["Gemini:TopK"] ?? "40");

            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Phân tích intent từ câu hỏi user
        /// Sử dụng Gemini 2.0 Flash để xác định intent nhanh và chính xác
        /// </summary>
        /// <param name="userMessage">Câu hỏi của user</param>
        /// <param name="conversationHistory">Lịch sử hội thoại</param>
        /// <returns>ChatIntent với IntentType và Parameters</returns>
        public async Task<ChatIntent> AnalyzeIntentAsync(
            string userMessage,
            List<ChatMessage>? conversationHistory = null)  // ✅ Thêm ? để nullable
        {
            try
            {
                var prompt = BuildIntentAnalysisPrompt(userMessage, conversationHistory);
                
                var response = await CallGeminiApiAsync(prompt, isJsonMode: true);
                
                var intent = ParseIntentFromResponse(response);

                Console.WriteLine($"[GeminiApiService] Analyzed Intent: {intent.IntentType} (Confidence: {intent.Confidence:F2})");

                return intent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GeminiApiService] Error analyzing intent: {ex.Message}");
                
                return new ChatIntent
                {
                    IntentType = IntentTypes.Unknown,
                    Parameters = new Dictionary<string, object>(),
                    Confidence = 0.0
                };
            }
        }

        /// <summary>
        /// Tạo response tự nhiên từ dữ liệu đã format
        /// Sử dụng khả năng hiểu ngữ cảnh tốt của Gemini 2.0 Flash
        /// </summary>
        /// <param name="userQuestion">Câu hỏi gốc</param>
        /// <param name="dataText">Dữ liệu đã format</param>
        /// <param name="conversationHistory">Lịch sử hội thoại</param>
        /// <returns>Response tự nhiên bằng tiếng Việt</returns>
        public async Task<string> GenerateNaturalResponseAsync(
            string userQuestion,
            string dataText,
            List<ChatMessage>? conversationHistory = null)  // ✅ Thêm ?
        {
            try
            {
                var prompt = BuildResponseGenerationPrompt(userQuestion, dataText, conversationHistory);
                
                var response = await CallGeminiApiAsync(prompt, isJsonMode: false);

                Console.WriteLine($"[GeminiApiService] Generated response (length: {response.Length})");

                return response.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GeminiApiService] Error generating response: {ex.Message}");
                
                // Fallback: Return dữ liệu raw
                return $"Dạ, đây là thông tin bạn yêu cầu:\n\n{dataText}";
            }
        }

        /// <summary>
        /// Tạo response fallback khi không hiểu câu hỏi
        /// </summary>
        /// <param name="userMessage">Câu hỏi của user</param>
        /// <param name="conversationHistory">Lịch sử hội thoại</param>
        /// <returns>Response lịch sự giải thích</returns>
        public async Task<string> GenerateFallbackResponseAsync(
            string userMessage,
            List<ChatMessage>? conversationHistory = null)  // ✅ Thêm ?
        {
            try
            {
                var prompt = BuildFallbackPrompt(userMessage, conversationHistory);
                var response = await CallGeminiApiAsync(prompt, isJsonMode: false);
                
                return response.Trim();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GeminiApiService] Error generating fallback: {ex.Message}");
                
                return "Xin lỗi, tôi chưa hiểu câu hỏi của bạn. " +
                       "Bạn có thể hỏi về:\n" +
                       "• Thực đơn món ăn\n" +
                       "• Tình trạng bàn\n" +
                       "• Đơn đặt hàng\n" +
                       "• Món ăn bán chạy";
            }
        }

        #endregion

        #region Private Core Methods

        /// <summary>
        /// Gọi Gemini 2.0 Flash API với prompt
        /// </summary>
        /// <param name="prompt">Prompt text</param>
        /// <param name="isJsonMode">Có yêu cầu JSON output không</param>
        /// <returns>Response text từ Gemini</returns>
        private async Task<string> CallGeminiApiAsync(string prompt, bool isJsonMode = false)
        {
            try
            {
                // === BUILD REQUEST BODY theo Gemini 2.0 format ===
                var requestBody = new GeminiRequest
                {
                    Contents = new[]
                    {
                        new GeminiContent
                        {
                            Parts = new[]
                            {
                                new GeminiPart { Text = prompt }
                            }
                        }
                    },
                    GenerationConfig = new GeminiGenerationConfig
                    {
                        Temperature = _temperature,
                        MaxOutputTokens = _maxTokens,
                        TopP = _topP,
                        TopK = _topK
                        
                    }
                };

                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var json = JsonSerializer.Serialize(requestBody, options);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // === GỬI REQUEST ===
                var url = $"{_apiEndpoint}?key={_apiKey}";
                var response = await _httpClient.PostAsync(url, content);

                // === KIỂM TRA STATUS CODE ===
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    
                    // Parse error message
                    try
                    {
                        var errorJson = JsonSerializer.Deserialize<JsonElement>(errorContent);
                        var errorMessage = errorJson.GetProperty("error").GetProperty("message").GetString();
                        throw new HttpRequestException($"Gemini API error: {response.StatusCode} - {errorMessage}");
                    }
                    catch
                    {
                        throw new HttpRequestException($"Gemini API error: {response.StatusCode} - {errorContent}");
                    }
                }

                // === PARSE RESPONSE ===
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<GeminiResponse>(responseJson, options);

                // Extract text từ response
                var text = result?.Candidates?.FirstOrDefault()
                    ?.Content?.Parts?.FirstOrDefault()
                    ?.Text;

                if (string.IsNullOrEmpty(text))
                {
                    throw new Exception("Empty response from Gemini API");
                }

                return text;
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Failed to call Gemini API: {ex.Message}", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception($"Failed to parse Gemini API response: {ex.Message}", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new Exception("Gemini API request timeout", ex);
            }
        }

        #endregion

        #region Prompt Building Methods

        /// <summary>
        /// Build prompt phân tích intent - Optimized cho Gemini 2.0
        /// </summary>
        private string BuildIntentAnalysisPrompt(string userMessage, List<ChatMessage>? conversationHistory)  // ✅ Thêm ?
        {
            var sb = new StringBuilder();

            sb.AppendLine("# Phân tích Intent cho Chatbot Nhà hàng");
            sb.AppendLine();
            sb.AppendLine("## Danh sách Intent hỗ trợ:");
            sb.AppendLine();
            sb.AppendLine("1. **GetMenu** - User hỏi về thực đơn, món ăn, giá cả");
            sb.AppendLine("   - Keywords: thực đơn, menu, món ăn, món gì, có gì, giá");
            sb.AppendLine("   - Examples: \"Thực đơn hôm nay có gì?\", \"Món phở giá bao nhiêu?\",\"Show me the menu for today\"");
            sb.AppendLine();
            sb.AppendLine("2. **GetTableAvailability** - User hỏi về bàn trống, tình trạng bàn");
            sb.AppendLine("   - Keywords: bàn, table, bàn trống, còn bàn, đặt bàn, available, reservation");
            sb.AppendLine("   - Examples: \"Còn bàn trống không?\", \"Bàn nào còn trống?\",\"Is there any table available?\",\"I want to reserve a table\"");
            sb.AppendLine();
            sb.AppendLine("3. **GetPopularDishes** - User hỏi về món bán chạy, món hot");
            sb.AppendLine("   - Keywords: bán chạy, popular, hot, nổi bật, yêu thích, best-seller");
            sb.AppendLine("   - Examples: \"Món nào bán chạy nhất?\", \"Top món hot?\",\"What are the best-selling dishes?\"");
            sb.AppendLine();
            sb.AppendLine("4. **Unknown** - Câu hỏi ngoài phạm vi, không liên quan");
            sb.AppendLine();

            // Lịch sử hội thoại
            if (conversationHistory != null && conversationHistory.Any())
            {
                sb.AppendLine("## Lịch sử hội thoại:");
                foreach (var msg in conversationHistory.TakeLast(3))
                {
                    var role = msg.IsUserMessage ? "User" : "Assistant";
                    sb.AppendLine($"- **{role}**: {msg.Content}");
                }
                sb.AppendLine();
            }

            sb.AppendLine("## Câu hỏi cần phân tích:");
            sb.AppendLine($"\"{userMessage}\"");
            sb.AppendLine();
            sb.AppendLine("## Output JSON format:");
            sb.AppendLine("```json");
            sb.AppendLine("{");
            sb.AppendLine("  \"intent\": \"IntentName\",");

            sb.AppendLine("  \"parameters\": {");

            sb.AppendLine("  }, ");

            sb.AppendLine("  \"confidence\": 0.95");
            sb.AppendLine("}");
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("**CHỈ TRẢ VỀ JSON, KHÔNG THÊM TEXT GIẢI THÍCH.**");

            return sb.ToString();
        }

        /// <summary>
        /// Build prompt tạo response tự nhiên - Optimized cho Gemini 2.0
        /// </summary>
        private string BuildResponseGenerationPrompt(
            string userQuestion,
            string dataText,
            List<ChatMessage>? conversationHistory)  // ✅ Thêm ?
        {
            var sb = new StringBuilder();

            sb.AppendLine("# Trợ lý Ảo Nhà hàng");
            sb.AppendLine();
            sb.AppendLine("## Vai trò:");
            sb.AppendLine("Bạn là trợ lý ảo thân thiện, chuyên nghiệp của nhà hàng.");
            sb.AppendLine("Nói tiếng Việt lịch sự, dùng \"dạ/ạ\" phù hợp.");
            sb.AppendLine();

            // Lịch sử hội thoại
            if (conversationHistory != null && conversationHistory.Any())
            {
                sb.AppendLine("## Lịch sử hội thoại:");
                foreach (var msg in conversationHistory.TakeLast(3))
                {
                    var role = msg.IsUserMessage ? "Khách hàng" : "Bạn";
                    sb.AppendLine($"**{role}**: {msg.Content}");
                }
                sb.AppendLine();
            }

            sb.AppendLine("## Câu hỏi của khách:");
            sb.AppendLine($"\"{userQuestion}\"");
            sb.AppendLine();
            sb.AppendLine("## Dữ liệu từ hệ thống:");
            sb.AppendLine("```");
            sb.AppendLine(dataText);
            sb.AppendLine("```");
            sb.AppendLine();
            sb.AppendLine("## Yêu cầu:");
            sb.AppendLine("1. Trả lời câu hỏi dựa trên dữ liệu trên");
            sb.AppendLine("2. Dùng tiếng Việt thân thiện, tự nhiên");
            sb.AppendLine("3. Giữ nguyên format và emoji từ dữ liệu");
            sb.AppendLine("4. Ngắn gọn, dễ hiểu");
            sb.AppendLine("5. Kết thúc bằng câu hỏi để tiếp tục hỗ trợ");
            sb.AppendLine();
            sb.AppendLine("**CHỈ TRẢ VỀ CÂU TRẢ LỜI, KHÔNG THÊM META TEXT.**");

            return sb.ToString();
        }

        /// <summary>
        /// Build prompt fallback
        /// </summary>
        private string BuildFallbackPrompt(string userMessage, List<ChatMessage>? conversationHistory)  // ✅ Thêm ?
        {
            var sb = new StringBuilder();

            sb.AppendLine("# Trợ lý Ảo Nhà hàng - Fallback Response");
            sb.AppendLine();

            if (conversationHistory != null && conversationHistory.Any())
            {
                sb.AppendLine("## Lịch sử:");
                foreach (var msg in conversationHistory.TakeLast(2))
                {
                    sb.AppendLine($"- **{(msg.IsUserMessage ? "Khách" : "Bạn")}**: {msg.Content}");
                }
                sb.AppendLine();
            }

            sb.AppendLine("## Câu hỏi của khách:");
            sb.AppendLine($"\"{userMessage}\"");
            sb.AppendLine();
            sb.AppendLine("## Tình huống:");
            sb.AppendLine("Câu hỏi này nằm ngoài phạm vi của chatbot nhà hàng.");
            sb.AppendLine();
            sb.AppendLine("## Yêu cầu:");
            sb.AppendLine("1. Xin lỗi lịch sự");
            sb.AppendLine("2. Giải thích bạn chỉ có thể hỗ trợ về:");
            sb.AppendLine("   - Thực đơn món ăn");
            sb.AppendLine("   - Tình trạng bàn");
            sb.AppendLine("   - Món ăn bán chạy");
            sb.AppendLine("3. Mời khách hỏi lại về các chủ đề trên");

            return sb.ToString();
        }

        #endregion

        #region Parsing Methods

        /// <summary>
        /// Parse response từ Gemini thành ChatIntent
        /// </summary>
        private ChatIntent ParseIntentFromResponse(string response)
        {
            try
            {
                // ✅ BƯỚC 1: Làm sạch response trước khi parse
                var cleanedResponse = CleanJsonResponse(response);

                Console.WriteLine($"[GeminiApiService] Cleaned JSON: {cleanedResponse}");

                // ✅ BƯỚC 2: Parse JSON đã được làm sạch
                var json = JsonSerializer.Deserialize<JsonElement>(cleanedResponse);

                return new ChatIntent
                {
                    IntentType = json.GetProperty("intent").GetString() ?? IntentTypes.Unknown,
                    Parameters = ParseParameters(json.GetProperty("parameters")),
                    Confidence = json.GetProperty("confidence").GetDouble()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GeminiApiService] Failed to parse intent JSON: {ex.Message}");
                Console.WriteLine($"[GeminiApiService] Raw response: {response}");

                // Fallback: Try to find JSON in text
                try
                {
                    var jsonStart = response.IndexOf('{');
                    var jsonEnd = response.LastIndexOf('}');

                    if (jsonStart >= 0 && jsonEnd > jsonStart)
                    {
                        var jsonText = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                        var json = JsonSerializer.Deserialize<JsonElement>(jsonText);

                        return new ChatIntent
                        {
                            IntentType = json.GetProperty("intent").GetString() ?? IntentTypes.Unknown,
                            Parameters = ParseParameters(json.GetProperty("parameters")),
                            Confidence = json.GetProperty("confidence").GetDouble()
                        };
                    }
                }
                catch
                {
                    // Ignore fallback error
                }

                // Final fallback
                return new ChatIntent
                {
                    IntentType = IntentTypes.Unknown,
                    Parameters = new Dictionary<string, object>(),
                    Confidence = 0.0
                };
            }
        }

        /// <summary>
        /// Parse parameters từ JSON element
        /// </summary>
        private Dictionary<string, object> ParseParameters(JsonElement parametersElement)
        {
            var parameters = new Dictionary<string, object>();

            try
            {
                foreach (var property in parametersElement.EnumerateObject())
                {
                    parameters[property.Name] = property.Value.ValueKind switch
                    {
                        JsonValueKind.String => property.Value.GetString() ?? string.Empty,  // ✅ Fix null
                        JsonValueKind.Number => property.Value.GetInt32(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        _ => property.Value.GetRawText()
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GeminiApiService] Error parsing parameters: {ex.Message}");
            }

            return parameters;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// ✅ Làm sạch JSON response từ Gemini (loại bỏ markdown, whitespace thừa)
        /// </summary>
        private string CleanJsonResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response))
                return "{}";

            // Loại bỏ markdown code blocks
            response = response.Trim();
            
            // Remove ```json ... ```
            if (response.StartsWith("```json"))
            {
                response = response.Substring(7); // Remove "```json"
            }
            else if (response.StartsWith("```"))
            {
                response = response.Substring(3); // Remove "```"
            }

            if (response.EndsWith("```"))
            {
                response = response.Substring(0, response.Length - 3);
            }

            // Trim whitespace
            response = response.Trim();

            // Nếu không tìm thấy JSON, trích xuất phần JSON
            if (!response.StartsWith("{"))
            {
                var jsonStart = response.IndexOf('{');
                if (jsonStart >= 0)
                {
                    response = response.Substring(jsonStart);
                }
            }

            if (!response.EndsWith("}"))
            {
                var jsonEnd = response.LastIndexOf('}');
                if (jsonEnd >= 0)
                {
                    response = response.Substring(0, jsonEnd + 1);
                }
            }

            return response;
        }

        /// <summary>
        /// Load configuration từ appsettings.json
        /// </summary>
        private IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            return builder.Build();
        }

        #endregion

        #region DTOs for Gemini 2.0 API

        /// <summary>
        /// Gemini API Request DTO
        /// </summary>
        private class GeminiRequest
        {
            [JsonPropertyName("contents")]
            public required GeminiContent[] Contents { get; set; }  // ✅ Thêm required

            [JsonPropertyName("generationConfig")]
            public required GeminiGenerationConfig GenerationConfig { get; set; }  // ✅ Thêm required
        }

        private class GeminiContent
        {
            [JsonPropertyName("parts")]
            public required GeminiPart[] Parts { get; set; }  // ✅ Thêm required
        }

        private class GeminiPart
        {
            [JsonPropertyName("text")]
            public required string Text { get; set; }  // ✅ Thêm required
        }

        private class GeminiGenerationConfig
        {
            [JsonPropertyName("temperature")]
            public double Temperature { get; set; }

            [JsonPropertyName("maxOutputTokens")]
            public int MaxOutputTokens { get; set; }

            [JsonPropertyName("topP")]
            public double TopP { get; set; }

            [JsonPropertyName("topK")]
            public int TopK { get; set; }
        }

        /// <summary>
        /// Gemini API Response DTO
        /// </summary>
        private class GeminiResponse
        {
            [JsonPropertyName("candidates")]
            public GeminiCandidate[]? Candidates { get; set; }  // ✅ Nullable
        }

        private class GeminiCandidate
        {
            [JsonPropertyName("content")]
            public GeminiContent? Content { get; set; }  // ✅ Nullable

            [JsonPropertyName("finishReason")]
            public string? FinishReason { get; set; }  // ✅ Nullable

            [JsonPropertyName("index")]
            public int Index { get; set; }
        }

        #endregion
    }
}
