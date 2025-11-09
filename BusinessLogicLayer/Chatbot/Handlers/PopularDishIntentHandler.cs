using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;
using BusinessObjects.Models.Chatbot;
using Services.Interfaces;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Services.Chatbot.Handlers
{
    /// <summary>
    /// Handler xử lý intent món bán chạy - Lấy thống kê từ database
    /// </summary>
    public class PopularDishIntentHandler : IIntentHandler
    {
        #region IIntentHandler Implementation

        public string IntentType => IntentTypes.GetPopularDishes;

        /// <summary>
        /// Xử lý intent lấy món bán chạy từ database
        /// </summary>
        /// <param name="parameters">
        /// Các tham số có thể có:
        /// - "topCount" (int?): Số lượng món muốn lấy (mặc định: 10)
        /// - "categoryId" (int?): Lọc theo danh mục (nếu có)
        /// </param>
        /// <returns>List<(Dish, int)> - Danh sách món và số lượng đã bán</returns>
        public async Task<object> HandleAsync(Dictionary<string, object> parameters)
        {
            // Chạy trên background thread để không block UI
            return await Task.Run(() =>
            {
                try
                {
                    // Parse topCount parameter (default = 10)
                    int topCount = 10;
                    if (parameters.ContainsKey("topCount") && parameters["topCount"] != null)
                    {
                        var topCountObj = parameters["topCount"];
                        if (topCountObj is int intValue)
                            topCount = intValue;
                        else if (int.TryParse(topCountObj.ToString(), out var parsedValue))
                            topCount = parsedValue;
                    }

                    // Parse categoryId parameter (optional)
                    int? categoryId = null;
                    if (parameters.ContainsKey("categoryId") && parameters["categoryId"] != null)
                    {
                        var categoryIdObj = parameters["categoryId"];
                        if (categoryIdObj is int intVal)
                            categoryId = intVal;
                        else if (int.TryParse(categoryIdObj.ToString(), out var parsedVal))
                            categoryId = parsedVal;
                    }

                    // Lấy dữ liệu từ database
                    using (var context = new RestaurantMiniManagementDbContext())
                    {
                        // ✅ BƯỚC 1: Group theo DishId và tính tổng số lượng
                        var dishQuantities = context.OrderDetails
                            .GroupBy(od => od.DishId)
                            .Select(g => new
                            {
                                DishId = g.Key,
                                TotalQuantity = g.Sum(od => od.Quantity)
                            })
                            .OrderByDescending(x => x.TotalQuantity)
                            .Take(topCount)
                            .ToList(); // Execute query ngay

                        // ✅ BƯỚC 2: Lấy danh sách DishId
                        var dishIds = dishQuantities.Select(x => x.DishId).ToList();

                        // ✅ BƯỚC 3: Load Dish entities với Category
                        var dishes = context.Dishes
                            .Include(d => d.Category)
                            .Where(d => dishIds.Contains(d.DishId))
                            .ToDictionary(d => d.DishId, d => d);

                        // ✅ BƯỚC 4: Filter theo categoryId nếu có
                        if (categoryId.HasValue)
                        {
                            dishQuantities = dishQuantities
                                .Where(x => dishes.ContainsKey(x.DishId) && 
                                           dishes[x.DishId].CategoryId == categoryId.Value)
                                .ToList();
                        }

                        // ✅ BƯỚC 5: Combine data và convert sang List<(Dish, int)>
                        var result = dishQuantities
                            .Where(x => dishes.ContainsKey(x.DishId)) // Đảm bảo dish tồn tại
                            .Select(x => (dishes[x.DishId], x.TotalQuantity))
                            .ToList();

                        return result;
                    }
                }
                catch (Exception ex)
                {
                    // Throw để ChatbotService xử lý
                    throw new Exception($"Lỗi khi lấy thống kê món bán chạy: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// Format danh sách món bán chạy thành text để gửi cho Gemini
        /// </summary>
        /// <param name="data">List<(Dish, int)> từ HandleAsync</param>
        /// <returns>Text đã format, dễ đọc</returns>
        public string FormatResponse(object data)
        {
            if (data is not List<(Dish, int)> ranking)
                return "Không có dữ liệu món bán chạy.";

            if (!ranking.Any())
                return "Chưa có món nào được bán.";

            // === BUILD TEXT FORMAT ===
            var sb = new StringBuilder();
            sb.AppendLine("🔥 TOP MÓN BÁN CHẠY");
            sb.AppendLine();

            var medals = new[] { "🥇", "🥈", "🥉" };

            for (int i = 0; i < ranking.Count; i++)
            {
                var (dish, quantity) = ranking[i];

                // Medal cho top 3, số thứ tự cho các món còn lại
                var prefix = i < 3 ? medals[i] : $"  {i + 1}.";

                // Tên món - Giá - Số lượng đã bán
                sb.Append($"{prefix} {dish.Name}");
                sb.Append($" - {FormatPrice(dish.Price)}");
                sb.AppendLine($" - Đã bán: {quantity} phần");

                // Thêm thông tin danh mục và mô tả (nếu có)
                if (dish.Category != null)
                {
                    sb.AppendLine($"    ↳ Danh mục: {dish.Category.Name}");
                }

                if (!string.IsNullOrWhiteSpace(dish.Description))
                {
                    var shortDesc = TruncateText(dish.Description, 60);
                    sb.AppendLine($"    ↳ {shortDesc}");
                }

                // Dòng trống giữa các món (trừ món cuối)
                if (i < ranking.Count - 1)
                {
                    sb.AppendLine();
                }
            }

            // Thống kê tổng quan
            var totalSold = ranking.Sum(x => x.Item2);
            sb.AppendLine();
            sb.AppendLine("📊 Tổng quan:");
            sb.AppendLine($"  • Tổng số phần đã bán: {totalSold:N0}");
            sb.AppendLine($"  • Số món trong danh sách: {ranking.Count}");

            return sb.ToString().TrimEnd();
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Format giá tiền theo định dạng Việt Nam
        /// </summary>
        private string FormatPrice(decimal price)
        {
            return $"{price:N0} VNĐ";
        }

        /// <summary>
        /// Truncate text nếu quá dài
        /// </summary>
        private string TruncateText(string text, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - suffix.Length) + suffix;
        }

        #endregion
    }
}
