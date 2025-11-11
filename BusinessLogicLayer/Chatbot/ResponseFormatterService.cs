using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BusinessObjects.Models;

namespace Services.Chatbot
{
    /// <summary>
    /// Service format và beautify response
    /// Nhiệm vụ:
    /// 1. Format dữ liệu common (price, date, status)
    /// 2. Tạo templates cho response
    /// 3. Thêm emoji và styling
    /// 4. Validate và sanitize output
    /// 5. Hỗ trợ các handlers format response đẹp hơn
    /// </summary>
    public class ResponseFormatterService
    {
        #region Constants

        // Emoji constants
        private const string EMOJI_MENU = "📋";
        private const string EMOJI_DISH = "🍽️";
        private const string EMOJI_TABLE = "🪑";
        private const string EMOJI_AVAILABLE = "✅";
        private const string EMOJI_OCCUPIED = "🔴";
        private const string EMOJI_RESERVED = "🟡";
        private const string EMOJI_STAR = "⭐";
        private const string EMOJI_FIRE = "🔥";
        private const string EMOJI_MONEY = "💰";
        private const string EMOJI_CALENDAR = "📅";
        private const string EMOJI_CHECKMARK = "✓";
        private const string EMOJI_ARROW = "➤";
        private const string EMOJI_BULLET = "•";

        // Currency
        private const string CURRENCY = "VNĐ";

        #endregion

        #region Public Methods - Price Formatting

        /// <summary>
        /// Format giá tiền theo chuẩn Việt Nam
        /// </summary>
        /// <param name="price">Giá tiền</param>
        /// <param name="includeEmoji">Có thêm emoji 💰 không</param>
        /// <returns>String đã format (VD: "50,000 VNĐ")</returns>
        public string FormatPrice(decimal price, bool includeEmoji = false)
        {
            var formatted = $"{price:N0} {CURRENCY}";
            return includeEmoji ? $"{EMOJI_MONEY} {formatted}" : formatted;
        }

        /// <summary>
        /// Format range giá
        /// </summary>
        /// <param name="minPrice">Giá thấp nhất</param>
        /// <param name="maxPrice">Giá cao nhất</param>
        /// <returns>String format "50,000 - 100,000 VNĐ"</returns>
        public string FormatPriceRange(decimal minPrice, decimal maxPrice)
        {
            return $"{minPrice:N0} - {maxPrice:N0} {CURRENCY}";
        }

        #endregion

        #region Public Methods - Date/Time Formatting

        /// <summary>
        /// Format ngày tháng theo tiếng Việt
        /// </summary>
        /// <param name="date">Ngày cần format</param>
        /// <param name="includeTime">Có bao gồm giờ phút không</param>
        /// <returns>String format "Thứ 2, 09/11/2025" hoặc "09/11/2025 14:30"</returns>
        public string FormatDate(DateTime date, bool includeTime = false)
        {
            if (includeTime)
            {
                return $"{date:dd/MM/yyyy HH:mm}";
            }

            var dayOfWeek = GetVietnameseDayOfWeek(date.DayOfWeek);
            return $"{dayOfWeek}, {date:dd/MM/yyyy}";
        }

        /// <summary>
        /// Format thời gian relative (VD: "5 phút trước", "hôm qua")
        /// </summary>
        public string FormatRelativeTime(DateTime dateTime)
        {
            var now = DateTime.Now;
            var diff = now - dateTime;

            if (diff.TotalMinutes < 1)
                return "vừa xong";
            else if (diff.TotalMinutes < 60)
                return $"{(int)diff.TotalMinutes} phút trước";
            else if (diff.TotalHours < 24)
                return $"{(int)diff.TotalHours} giờ trước";
            else if (diff.TotalDays < 7)
                return $"{(int)diff.TotalDays} ngày trước";
            else
                return FormatDate(dateTime);
        }

        #endregion

        #region Public Methods - Status Formatting

        /// <summary>
        /// Format trạng thái bàn với emoji
        /// </summary>
        /// <param name="status">Trạng thái (Available, Occupied, Reserved)</param>
        /// <returns>String với emoji (VD: "✅ Trống")</returns>
        public string FormatTableStatus(string status)
        {
            return status?.ToLower() switch
            {
                "available" => $"{EMOJI_AVAILABLE} Trống",
                "occupied" => $"{EMOJI_OCCUPIED} Đang sử dụng",
                "reserved" => $"{EMOJI_RESERVED} Đã đặt",
                "maintenance" => "🔧 Bảo trì",
                _ => $"❓ {status ?? "Không rõ"}"
            };
        }

        /// <summary>
        /// Format trạng thái đơn hàng
        /// </summary>
        public string FormatOrderStatus(string status)
        {
            return status?.ToLower() switch
            {
                "pending" => "⏳ Đang chờ",
                "in progress" => "👨‍🍳 Đang chế biến",
                "completed" => "✅ Hoàn thành",
                "cancelled" => "❌ Đã hủy",
                _ => $"❓ {status ?? "Không rõ"}"
            };
        }

        #endregion

        #region Public Methods - List Formatting

        /// <summary>
        /// Format danh sách món ăn
        /// </summary>
        /// <param name="dishes">Danh sách món</param>
        /// <param name="groupByCategory">Group theo category không</param>
        /// <returns>Text đã format với emoji và structure</returns>
        public string FormatDishList(List<Dish> dishes, bool groupByCategory = true)
        {
            if (dishes == null || !dishes.Any())
                return "Không có món ăn nào.";

            var sb = new StringBuilder();
            sb.AppendLine($"{EMOJI_MENU} THỰC ĐƠN ({dishes.Count} món)");
            sb.AppendLine();

            if (groupByCategory)
            {
                // Group by category
                var grouped = dishes
                    .GroupBy(d => d.Category?.Name ?? "Chưa phân loại")
                    .OrderBy(g => g.Key);

                foreach (var group in grouped)
                {
                    sb.AppendLine($"{EMOJI_ARROW} {group.Key.ToUpper()}");

                    foreach (var dish in group.OrderBy(d => d.Name))
                    {
                        sb.Append($"  {EMOJI_BULLET} {dish.Name}");
                        sb.Append($" - {FormatPrice(dish.Price)}");

                        if (!string.IsNullOrWhiteSpace(dish.UnitOfCalculation))
                        {
                            sb.Append($" ({dish.UnitOfCalculation})");
                        }

                        sb.AppendLine();

                        if (!string.IsNullOrWhiteSpace(dish.Description))
                        {
                            sb.AppendLine($"    ↳ {dish.Description}");
                        }
                    }

                    sb.AppendLine();
                }
            }
            else
            {
                // Simple list
                foreach (var dish in dishes.OrderBy(d => d.Name))
                {
                    sb.Append($"{EMOJI_BULLET} {dish.Name}");
                    sb.Append($" - {FormatPrice(dish.Price)}");

                    if (!string.IsNullOrWhiteSpace(dish.UnitOfCalculation))
                    {
                        sb.Append($" ({dish.UnitOfCalculation})");
                    }

                    sb.AppendLine();
                }
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Format danh sách bàn
        /// </summary>
        public string FormatTableList(List<Table> tables, bool groupByArea = true)
        {
            if (tables == null || !tables.Any())
                return "Không có bàn nào.";

            var sb = new StringBuilder();
            sb.AppendLine($"{EMOJI_TABLE} DANH SÁCH BÀN ({tables.Count} bàn)");
            sb.AppendLine();

            if (groupByArea)
            {
                // Group by area
                var grouped = tables
                    .GroupBy(t => t.Area?.AreaName ?? "Chưa phân khu")
                    .OrderBy(g => g.Key);

                foreach (var group in grouped)
                {
                    sb.AppendLine($"{EMOJI_ARROW} {group.Key.ToUpper()}");

                    foreach (var table in group.OrderBy(t => t.TableName))
                    {
                        sb.Append($"  {EMOJI_BULLET} {table.TableName}");
                        sb.Append($" - {FormatTableStatus(table.Status)}");
                        sb.AppendLine();
                    }

                    sb.AppendLine();
                }
            }
            else
            {
                // Simple list
                foreach (var table in tables.OrderBy(t => t.TableName))
                {
                    sb.Append($"{EMOJI_BULLET} {table.TableName}");
                    sb.Append($" - {FormatTableStatus(table.Status)}");
                    sb.AppendLine();
                }
            }

            return sb.ToString().TrimEnd();
        }

        #endregion

        #region Public Methods - Statistics Formatting

        /// <summary>
        /// Format thống kê tổng quan
        /// </summary>
        public string FormatSummaryStatistics(Dictionary<string, object> stats)
        {
            if (stats == null || !stats.Any())
                return "Không có dữ liệu thống kê.";

            var sb = new StringBuilder();
            sb.AppendLine($"{EMOJI_STAR} THỐNG KÊ TỔNG QUAN");
            sb.AppendLine();

            foreach (var kvp in stats)
            {
                var value = FormatStatValue(kvp.Value);
                sb.AppendLine($"{EMOJI_CHECKMARK} {kvp.Key}: {value}");
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Format top ranking (món bán chạy, etc.)
        /// </summary>
        public string FormatTopRanking<T>(
            List<(T Item, int Count)> ranking,
            Func<T, string> nameSelector,
            string title = "TOP RANKING")
        {
            if (ranking == null || !ranking.Any())
                return "Không có dữ liệu ranking.";

            var sb = new StringBuilder();
            sb.AppendLine($"{EMOJI_FIRE} {title}");
            sb.AppendLine();

            var medals = new[] { "🥇", "🥈", "🥉" };

            for (int i = 0; i < ranking.Count && i < 10; i++)
            {
                var (item, count) = ranking[i];
                var prefix = i < 3 ? medals[i] : $"  {i + 1}.";
                var name = nameSelector(item);

                sb.AppendLine($"{prefix} {name} - {count} lượt");
            }

            return sb.ToString().TrimEnd();
        }

        #endregion

        #region Public Methods - Template Methods

        /// <summary>
        /// Tạo header cho response
        /// </summary>
        public string CreateHeader(string title, string emoji = null)
        {
            var icon = emoji ?? EMOJI_STAR;
            return $"{icon} {title.ToUpper()}";
        }

        /// <summary>
        /// Tạo separator line
        /// </summary>
        public string CreateSeparator(int length = 40)
        {
            return new string('─', length);
        }

        /// <summary>
        /// Wrap text trong box
        /// </summary>
        public string WrapInBox(string content, string title = null)
        {
            var sb = new StringBuilder();
            var lines = content.Split('\n');
            var maxLength = lines.Max(l => l.Length) + 4;

            // Top border
            sb.AppendLine("┌" + new string('─', maxLength - 2) + "┐");

            // Title (if provided)
            if (!string.IsNullOrEmpty(title))
            {
                var paddedTitle = $" {title} ".PadRight(maxLength - 2);
                sb.AppendLine($"│{paddedTitle}│");
                sb.AppendLine("├" + new string('─', maxLength - 2) + "┤");
            }

            // Content
            foreach (var line in lines)
            {
                var paddedLine = $" {line} ".PadRight(maxLength - 2);
                sb.AppendLine($"│{paddedLine}│");
            }

            // Bottom border
            sb.AppendLine("└" + new string('─', maxLength - 2) + "┘");

            return sb.ToString();
        }

        #endregion

        #region Public Methods - Validation & Sanitization

        /// <summary>
        /// Sanitize text (remove special characters, trim)
        /// </summary>
        public string SanitizeText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            // Remove multiple spaces
            while (text.Contains("  "))
                text = text.Replace("  ", " ");

            // Trim
            text = text.Trim();

            // Remove dangerous characters (nếu cần)
            // text = text.Replace("<", "").Replace(">", "";

            return text;
        }

        /// <summary>
        /// Truncate text nếu quá dài
        /// </summary>
        public string TruncateText(string text, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - suffix.Length) + suffix;
        }

        /// <summary>
        /// Validate response không rỗng
        /// </summary>
        public bool ValidateResponse(string response)
        {
            return !string.IsNullOrWhiteSpace(response) && response.Length > 5;
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Get tên thứ bằng tiếng Việt
        /// </summary>
        private string GetVietnameseDayOfWeek(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                DayOfWeek.Sunday => "Chủ nhật",
                _ => dayOfWeek.ToString()
            };
        }

        /// <summary>
        /// Format giá trị statistic
        /// </summary>
        private string FormatStatValue(object value)
        {
            return value switch
            {
                decimal d => FormatPrice(d),
                int i => i.ToString("N0"),
                double db => db.ToString("N2"),
                DateTime dt => FormatDate(dt),
                _ => value?.ToString() ?? "N/A"
            };
        }

        #endregion
    }
}
