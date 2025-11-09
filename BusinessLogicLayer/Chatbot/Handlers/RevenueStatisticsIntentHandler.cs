using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;
using BusinessObjects.Models.Chatbot;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;

namespace Services.Chatbot.Handlers
{
    /// <summary>
    /// Handler xử lý các intent về thống kê doanh thu và đơn hàng
    /// Hỗ trợ:
    /// - Tổng bill của bàn cụ thể
    /// - Doanh thu theo ngày/tháng
    /// - Giờ đông khách nhất
    /// - Ranking bàn theo doanh thu
    /// </summary>
    public class RevenueStatisticsIntentHandler : IIntentHandler
    {
        #region IIntentHandler Implementation

        public string IntentType { get; private set; }

        /// <summary>
        /// Constructor với intent type động
        /// </summary>
        public RevenueStatisticsIntentHandler(string intentType)
        {
            IntentType = intentType;
        }

        /// <summary>
        /// Xử lý các intent về thống kê doanh thu
        /// </summary>
        /// <param name="parameters">
        /// Parameters có thể có:
        /// - "tableId" hoặc "tableName" (string): Tên bàn (VD: "A01")
        /// - "date" (DateTime): Ngày cần thống kê
        /// - "startDate", "endDate" (DateTime): Khoảng thời gian
        /// - "type" (string): "highest" hoặc "lowest" cho ranking
        /// - "topCount" (int): Số lượng kết quả (mặc định: 5)
        /// </param>
        public async Task<object> HandleAsync(Dictionary<string, object> parameters)
        {
            return await Task.Run(() =>
            {
                try
                {
                    using (var context = new RestaurantMiniManagementDbContext())
                    {
                        return IntentType switch
                        {
                            IntentTypes.GetTableRevenue => GetTableRevenue(context, parameters),
                            IntentTypes.GetDailyRevenue => GetDailyRevenue(context, parameters),
                            IntentTypes.GetPeakHours => GetPeakHours(context, parameters),
                            IntentTypes.GetTableRevenueRanking => GetTableRevenueRanking(context, parameters),
                            _ => throw new NotSupportedException($"Intent type '{IntentType}' is not supported")
                        };
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Lỗi khi xử lý thống kê doanh thu: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// Format response dựa theo intent type
        /// </summary>
        public string FormatResponse(object data)
        {
            return IntentType switch
            {
                IntentTypes.GetTableRevenue => FormatTableRevenueResponse(data),
                IntentTypes.GetDailyRevenue => FormatDailyRevenueResponse(data),
                IntentTypes.GetPeakHours => FormatPeakHoursResponse(data),
                IntentTypes.GetTableRevenueRanking => FormatTableRankingResponse(data),
                _ => "Không có dữ liệu."
            };
        }

        #endregion

        #region Private Methods - Data Retrieval

        /// <summary>
        /// Lấy tổng bill của một bàn cụ thể
        /// </summary>
        private object GetTableRevenue(RestaurantMiniManagementDbContext context, Dictionary<string, object> parameters)
        {
            // Parse tableName parameter
            if (!parameters.ContainsKey("tableName") || parameters["tableName"] == null)
            {
                throw new ArgumentException("Thiếu tên bàn. Vui lòng cung cấp tên bàn (VD: A01, B02)");
            }

            var tableName = parameters["tableName"].ToString().Trim();

            // Parse optional date range
            DateTime? startDate = ParseDateParameter(parameters, "startDate");
            DateTime? endDate = ParseDateParameter(parameters, "endDate");

            // Query orders của bàn đó
            var query = context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Dish)
                .Where(o => o.Table != null && o.Table.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase))
                .Where(o => o.Status == "Completed"); // Chỉ tính đơn đã hoàn thành

            // Filter theo date range nếu có
            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderTime <= endDate.Value);
            }

            var orders = query.ToList();

            // Tính tổng doanh thu
            var totalRevenue = orders.Sum(o => o.TotalAmount);
            var orderCount = orders.Count;

            return new
            {
                TableName = tableName,
                TotalRevenue = totalRevenue,
                OrderCount = orderCount,
                Orders = orders,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        /// <summary>
        /// Lấy tổng doanh thu theo ngày/tháng
        /// </summary>
        private object GetDailyRevenue(RestaurantMiniManagementDbContext context, Dictionary<string, object> parameters)
        {
            // Parse date parameter
            DateTime targetDate;
            if (parameters.ContainsKey("date") && parameters["date"] != null)
            {
                targetDate = ParseDateParameter(parameters, "date") ?? DateTime.Today;
            }
            else
            {
                targetDate = DateTime.Today;
            }

            // Query orders trong ngày đó
            var startOfDay = targetDate.Date;
            var endOfDay = startOfDay.AddDays(1).AddSeconds(-1);

            var orders = context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Dish)
                .Where(o => o.OrderTime >= startOfDay && o.OrderTime <= endOfDay)
                .Where(o => o.Status == "Completed")
                .ToList();

            // Thống kê
            var totalRevenue = orders.Sum(o => o.TotalAmount);
            var orderCount = orders.Count;
            var totalCustomers = orders.Count(o => o.CustomerId != null);

            // Group by giờ để xem giờ nào bán nhiều nhất
            var revenueByHour = orders
                .GroupBy(o => o.OrderTime.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    Revenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();

            return new
            {
                Date = targetDate,
                TotalRevenue = totalRevenue,
                OrderCount = orderCount,
                TotalCustomers = totalCustomers,
                RevenueByHour = revenueByHour,
                Orders = orders
            };
        }

        /// <summary>
        /// Tìm giờ đông khách nhất
        /// </summary>
        private object GetPeakHours(RestaurantMiniManagementDbContext context, Dictionary<string, object> parameters)
        {
            // Parse optional date range
            DateTime? startDate = ParseDateParameter(parameters, "startDate");
            DateTime? endDate = ParseDateParameter(parameters, "endDate");

            // Nếu không có date range, lấy 30 ngày gần nhất
            if (!startDate.HasValue)
            {
                startDate = DateTime.Today.AddDays(-30);
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.Today;
            }

            // Query orders trong khoảng thời gian
            var orders = context.Orders
                .Where(o => o.OrderTime >= startDate.Value && o.OrderTime <= endDate.Value)
                .Where(o => o.Status == "Completed")
                .ToList();

            // Group by giờ
            var ordersByHour = orders
                .GroupBy(o => o.OrderTime.Hour)
                .Select(g => new
                {
                    Hour = g.Key,
                    OrderCount = g.Count(),
                    TotalRevenue = g.Sum(o => o.TotalAmount),
                    AvgRevenue = g.Average(o => o.TotalAmount)
                })
                .OrderByDescending(x => x.OrderCount)
                .ToList();

            return new
            {
                StartDate = startDate,
                EndDate = endDate,
                PeakHours = ordersByHour,
                TotalOrders = orders.Count
            };
        }

        /// <summary>
        /// Ranking bàn theo doanh thu (cao nhất/thấp nhất)
        /// </summary>
        private object GetTableRevenueRanking(RestaurantMiniManagementDbContext context, Dictionary<string, object> parameters)
        {
            // Parse topCount
            int topCount = 5;
            if (parameters.ContainsKey("topCount") && parameters["topCount"] != null)
            {
                if (parameters["topCount"] is int intVal)
                    topCount = intVal;
                else if (int.TryParse(parameters["topCount"].ToString(), out var parsed))
                    topCount = parsed;
            }

            // Parse ranking type (highest/lowest)
            string rankingType = "highest";
            if (parameters.ContainsKey("type") && parameters["type"] != null)
            {
                rankingType = parameters["type"].ToString().ToLower();
            }

            // Parse optional date range
            DateTime? startDate = ParseDateParameter(parameters, "startDate");
            DateTime? endDate = ParseDateParameter(parameters, "endDate");

            // Query orders
            var query = context.Orders
                .Include(o => o.Table)
                    .ThenInclude(t => t.Area)
                .Where(o => o.Table != null)
                .Where(o => o.Status == "Completed");

            // Filter theo date range
            if (startDate.HasValue)
            {
                query = query.Where(o => o.OrderTime >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(o => o.OrderTime <= endDate.Value);
            }

            // Group by table và tính tổng doanh thu
            var tableRevenues = query
                .GroupBy(o => new
                {
                    o.TableId,
                    o.Table
                })
                .Select(g => new
                {
                    Table = g.Key.Table,
                    TotalRevenue = g.Sum(o => o.TotalAmount),
                    OrderCount = g.Count()
                })
                .ToList();

            // Sort theo ranking type
            var sortedRevenues = rankingType == "lowest"
                ? tableRevenues.OrderBy(x => x.TotalRevenue).Take(topCount).ToList()
                : tableRevenues.OrderByDescending(x => x.TotalRevenue).Take(topCount).ToList();

            return new
            {
                RankingType = rankingType,
                TopCount = topCount,
                StartDate = startDate,
                EndDate = endDate,
                TableRevenues = sortedRevenues
            };
        }

        #endregion

        #region Private Methods - Response Formatting

        /// <summary>
        /// Format response cho GetTableRevenue
        /// </summary>
        private string FormatTableRevenueResponse(object data)
        {
            dynamic result = data;
            var sb = new StringBuilder();

            sb.AppendLine($"💰 DOANH THU BÀN {result.TableName}");
            sb.AppendLine();

            // Thời gian
            if (result.StartDate != null || result.EndDate != null)
            {
                sb.Append($"📅 Thời gian: ");
                if (result.StartDate != null)
                    sb.Append($"{((DateTime)result.StartDate):dd/MM/yyyy}");
                if (result.StartDate != null && result.EndDate != null)
                    sb.Append(" - ");
                if (result.EndDate != null)
                    sb.Append($"{((DateTime)result.EndDate):dd/MM/yyyy}");
                sb.AppendLine();
                sb.AppendLine();
            }

            // Thống kê
            sb.AppendLine($"📊 Tổng quan:");
            sb.AppendLine($"  • Tổng doanh thu: {FormatPrice(result.TotalRevenue)}");
            sb.AppendLine($"  • Số đơn hàng: {result.OrderCount}");

            if (result.OrderCount > 0)
            {
                var avgRevenue = (decimal)result.TotalRevenue / result.OrderCount;
                sb.AppendLine($"  • Doanh thu trung bình/đơn: {FormatPrice(avgRevenue)}");
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Format response cho GetDailyRevenue
        /// </summary>
        private string FormatDailyRevenueResponse(object data)
        {
            dynamic result = data;
            var sb = new StringBuilder();

            sb.AppendLine($"💵 DOANH THU NGÀY {((DateTime)result.Date):dd/MM/yyyy}");
            sb.AppendLine();

            sb.AppendLine($"📊 Tổng quan:");
            sb.AppendLine($"  • Tổng doanh thu: {FormatPrice(result.TotalRevenue)}");
            sb.AppendLine($"  • Số đơn hàng: {result.OrderCount}");
            sb.AppendLine($"  • Số khách hàng: {result.TotalCustomers}");

            if (result.OrderCount > 0)
            {
                var avgRevenue = (decimal)result.TotalRevenue / result.OrderCount;
                sb.AppendLine($"  • Doanh thu trung bình/đơn: {FormatPrice(avgRevenue)}");
            }

            // ✅ SỬA LẠI
            IEnumerable<dynamic> revenueByHour = result.RevenueByHour;
            if (revenueByHour.Any())
            {
                sb.AppendLine();
                sb.AppendLine($"⏰ Top 3 giờ có doanh thu cao:");
                
                int index = 0;
                foreach (var hour in revenueByHour.Take(3))
                {
                    sb.AppendLine($"  {index + 1}. {hour.Hour:D2}:00 - {FormatPrice(hour.Revenue)} ({hour.OrderCount} đơn)");
                    index++;
                }
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Format response cho GetPeakHours
        /// </summary>
        private string FormatPeakHoursResponse(object data)
        {
            dynamic result = data;
            var sb = new StringBuilder();

            sb.AppendLine($"⏰ GIỜ ĐÔNG KHÁCH");
            sb.AppendLine();

            sb.Append($"📅 Dữ liệu từ: ");
            sb.Append($"{((DateTime)result.StartDate):dd/MM/yyyy}");
            sb.AppendLine($" đến {((DateTime)result.EndDate):dd/MM/yyyy}");
            sb.AppendLine();

            // ✅ SỬA LẠI: Sử dụng IEnumerable thay vì List
            IEnumerable<dynamic> peakHours = result.PeakHours;

            if (!peakHours.Any())
            {
                return "Không có dữ liệu đơn hàng trong khoảng thời gian này.";
            }

            sb.AppendLine($"🔥 Top 5 giờ đông khách nhất:");
            
            int index = 0;
            foreach (var hour in peakHours.Take(5))
            {
                var prefix = index < 3 ? new[] { "🥇", "🥈", "🥉" }[index] : $"  {index + 1}.";

                sb.Append($"{prefix} {hour.Hour:D2}:00");
                sb.Append($" - {hour.OrderCount} đơn");
                sb.AppendLine($" - {FormatPrice(hour.TotalRevenue)}");
                
                index++;
            }

            sb.AppendLine();
            sb.AppendLine($"📊 Tổng số đơn: {result.TotalOrders:N0}");

            return sb.ToString().TrimEnd();
        }

        /// <summary>
        /// Format response cho GetTableRevenueRanking
        /// </summary>
        private string FormatTableRankingResponse(object data)
        {
            dynamic result = data;
            var sb = new StringBuilder();

            var title = result.RankingType == "lowest" 
                ? $"📉 TOP {result.TopCount} BÀN CÓ DOANH THU THẤP NHẤT"
                : $"📈 TOP {result.TopCount} BÀN CÓ DOANH THU CAO NHẤT";

            sb.AppendLine(title);
            sb.AppendLine();

            // Thời gian
            if (result.StartDate != null || result.EndDate != null)
            {
                sb.Append($"📅 Thời gian: ");
                if (result.StartDate != null)
                    sb.Append($"{((DateTime)result.StartDate):dd/MM/yyyy}");
                if (result.StartDate != null && result.EndDate != null)
                    sb.Append(" - ");
                if (result.EndDate != null)
                    sb.Append($"{((DateTime)result.EndDate):dd/MM/yyyy}");
                sb.AppendLine();
                sb.AppendLine();
            }

            // ✅ SỬA LẠI
            IEnumerable<dynamic> tableRevenues = result.TableRevenues;

            if (!tableRevenues.Any())
            {
                return "Không có dữ liệu bàn trong khoảng thời gian này.";
            }

            var medals = new[] { "🥇", "🥈", "🥉" };

            int index = 0;
            foreach (var item in tableRevenues)
            {
                var table = (Table)item.Table;
                var prefix = index < 3 ? medals[index] : $"  {index + 1}.";

                sb.Append($"{prefix} {table.TableName}");
                if (table.Area != null)
                {
                    sb.Append($" ({table.Area.AreaName})");
                }
                sb.AppendLine();
                sb.AppendLine($"    ↳ Doanh thu: {FormatPrice(item.TotalRevenue)}");
                sb.AppendLine($"    ↳ Số đơn: {item.OrderCount}");

                if (index < tableRevenues.Count() - 1)
                {
                    sb.AppendLine();
                }
                
                index++;
            }

            return sb.ToString().TrimEnd();
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Parse date parameter từ dictionary
        /// </summary>
        private DateTime? ParseDateParameter(Dictionary<string, object> parameters, string key)
        {
            if (!parameters.ContainsKey(key) || parameters[key] == null)
                return null;

            var value = parameters[key];

            if (value is DateTime dt)
                return dt;

            if (DateTime.TryParse(value.ToString(), out var parsed))
                return parsed;

            return null;
        }

        /// <summary>
        /// Format giá tiền
        /// </summary>
        private string FormatPrice(decimal price)
        {
            return $"{price:N0} VNĐ";
        }

        #endregion
    }
}