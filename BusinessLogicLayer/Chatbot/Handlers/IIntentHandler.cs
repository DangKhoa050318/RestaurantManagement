using System.Collections.Generic;
using System.Threading.Tasks;

namespace Services.Chatbot.Handlers
{
    /// <summary>
    /// Interface định nghĩa contract cho các Intent Handler
    /// Mỗi handler sẽ xử lý một loại intent cụ thể từ user
    /// </summary>
    public interface IIntentHandler
    {
        /// <summary>
        /// Loại intent mà handler này xử lý
        /// VD: "GetMenu", "GetTableAvailability", "GetPopularDishes"
        /// </summary>
        string IntentType { get; }

        /// <summary>
        /// Xử lý intent và lấy dữ liệu từ database
        /// </summary>
        /// <param name="parameters">Các tham số trích xuất từ câu hỏi user (VD: date, categoryId, ...)</param>
        /// <returns>Dữ liệu raw từ database (List<Dish>, List<Table>, ...)</returns>
        Task<object> HandleAsync(Dictionary<string, object> parameters);

        /// <summary>
        /// Format dữ liệu raw thành text để gửi cho Gemini
        /// </summary>
        /// <param name="data">Dữ liệu raw từ HandleAsync</param>
        /// <returns>Text đã format, dễ đọc cho Gemini</returns>
        string FormatResponse(object data);
    }
}
