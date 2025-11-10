using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;
using BusinessObjects.Models.Chatbot;
using Services.Interfaces;
using Services.Implementations;  // ✅ Thêm dòng này

namespace Services.Chatbot.Handlers
{
    /// <summary>
    /// Handler xử lý intent "GetMenu" - Lấy thông tin thực đơn món ăn
    /// Truy xuất dữ liệu từ DishService/DishRepository
    /// </summary>
    public class MenuIntentHandler : IIntentHandler
    {
        #region Private Fields

        private readonly IDishService _dishService;

        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo MenuIntentHandler với DishService
        /// </summary>
        public MenuIntentHandler()
        {
            // ✅ TẠO INSTANCE MỚI thay vì dùng Singleton
            _dishService = new DishService();
        }

        /// <summary>
        /// Constructor cho Dependency Injection (nếu cần sau này)
        /// </summary>
        /// <param name="dishService">DishService được inject</param>
        public MenuIntentHandler(IDishService dishService)
        {
            _dishService = dishService ?? throw new ArgumentNullException(nameof(dishService));
        }

        #endregion

        #region IIntentHandler Implementation

        /// <summary>
        /// Loại intent mà handler này xử lý
        /// </summary>
        public string IntentType => IntentTypes.GetMenu;

        /// <summary>
        /// Xử lý intent lấy thực đơn từ database
        /// </summary>
        /// <param name="parameters">
        /// Các tham số có thể có:
        /// - "categoryId" (int?): Lọc theo danh mục (nếu có)
        /// - "keyword" (string?): Tìm kiếm theo tên món (nếu có)
        /// </param>
        /// <returns>List<Dish> - Danh sách món ăn từ database</returns>
        public async Task<object> HandleAsync(Dictionary<string, object> parameters)
        {
            // Chạy trên background thread để không block UI
            return await Task.Run(() =>
            {
                try
                {
                    List<Dish> dishes;

                    // === CASE 1: Lọc theo CategoryId ===
                    if (parameters.ContainsKey("categoryId") && parameters["categoryId"] != null)
                    {
                        // Parse categoryId từ parameters
                        var categoryIdObj = parameters["categoryId"];
                        int categoryId;

                        // Xử lý nhiều kiểu dữ liệu có thể (int, string, long...)
                        if (categoryIdObj is int intValue)
                            categoryId = intValue;
                        else if (int.TryParse(categoryIdObj.ToString(), out var parsedValue))
                            categoryId = parsedValue;
                        else
                            throw new ArgumentException($"Invalid categoryId: {categoryIdObj}");

                        // Gọi service lấy dishes theo category
                        dishes = _dishService.GetDishesByCategoryId(categoryId);
                    }
                    // === CASE 2: Tìm kiếm theo keyword ===
                    else if (parameters.ContainsKey("keyword") && parameters["keyword"] != null)
                    {
                        var keyword = parameters["keyword"].ToString();
                        
                        // Gọi service search dishes
                        dishes = _dishService.SearchDishesByName(keyword);
                    }
                    // === CASE 3: Lấy tất cả món ăn ===
                    else
                    {
                        // Không có filter → lấy hết
                        dishes = _dishService.GetDishes();
                    }

                    // Sắp xếp theo tên món để dễ đọc
                    dishes = dishes.OrderBy(d => d.Name).ToList();

                    return dishes;
                }
                catch (Exception ex)
                {
                    // Log error (nếu có logging service)
                    // _logger.LogError(ex, "Error in MenuIntentHandler.HandleAsync");

                    // Throw để ChatbotService xử lý
                    throw new Exception($"Lỗi khi lấy thực đơn: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// Format danh sách món ăn thành text để gửi cho Gemini
        /// </summary>
        /// <param name="data">List<Dish> từ HandleAsync</param>
        /// <returns>Text đã format, dễ đọc</returns>
        public string FormatResponse(object data)
        {
            // Cast về đúng kiểu
            if (data is not List<Dish> dishes)
            {
                return "Không có dữ liệu món ăn.";
            }

            // Kiểm tra danh sách rỗng
            if (dishes == null || !dishes.Any())
            {
                return "Hiện tại không có món ăn nào trong thực đơn.";
            }

            // === BUILD TEXT FORMAT ===
            var sb = new StringBuilder();
            sb.AppendLine($"📋 THỰC ĐƠN ({dishes.Count} món)");
            sb.AppendLine();

            // Group by Category (nếu có)
            var dishesGrouped = dishes
                .GroupBy(d => d.Category?.Name ?? "Chưa phân loại")
                .OrderBy(g => g.Key);

            foreach (var group in dishesGrouped)
            {
                // Tên category
                sb.AppendLine($"▶ {group.Key.ToUpper()}");
                
                foreach (var dish in group)
                {
                    // Format: Tên món - Giá - Đơn vị tính
                    sb.Append($"  • {dish.Name}");
                    sb.Append($" - {FormatPrice(dish.Price)}");
                    
                    if (!string.IsNullOrWhiteSpace(dish.UnitOfCalculation))
                    {
                        sb.Append($" ({dish.UnitOfCalculation})");
                    }

                    // Thêm mô tả (nếu có)
                    if (!string.IsNullOrWhiteSpace(dish.Description))
                    {
                        sb.AppendLine();
                        sb.AppendLine($"    ↳ {dish.Description}");
                    }
                    else
                    {
                        sb.AppendLine();
                    }
                }
                
                sb.AppendLine(); // Dòng trống giữa các category
            }

            return sb.ToString().TrimEnd();
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Format giá tiền theo định dạng Việt Nam
        /// </summary>
        /// <param name="price">Giá tiền</param>
        /// <returns>String đã format (VD: "50,000 VNĐ")</returns>
        private string FormatPrice(decimal price)
        {
            return $"{price:N0} VNĐ";
        }

        #endregion
    }
}
