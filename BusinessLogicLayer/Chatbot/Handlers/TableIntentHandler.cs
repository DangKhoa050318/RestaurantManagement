using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObjects.Models;
using BusinessObjects.Models.Chatbot;
using Services.Interfaces;
using Services.Implementations;

namespace Services.Chatbot.Handlers
{
    /// <summary>
    /// Handler xử lý intent về bàn - Lấy thông tin bàn từ database
    /// </summary>
    public class TableIntentHandler : IIntentHandler
    {
        #region Private Fields

        private readonly ITableService _tableService;

        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo TableIntentHandler với TableService
        /// </summary>
        public TableIntentHandler()
        {
            // Tạo instance mới của TableService
            _tableService = new TableService();
        }

        /// <summary>
        /// Constructor cho Dependency Injection (nếu cần sau này)
        /// </summary>
        /// <param name="tableService">TableService được inject</param>
        public TableIntentHandler(ITableService tableService)
        {
            _tableService = tableService ?? throw new ArgumentNullException(nameof(tableService));
        }

        #endregion

        #region IIntentHandler Implementation

        public string IntentType => IntentTypes.GetTableAvailability;

        /// <summary>
        /// Xử lý intent lấy thông tin bàn từ database
        /// </summary>
        /// <param name="parameters">
        /// Các tham số có thể có:
        /// - "status" (string?): Lọc theo trạng thái bàn (Empty, Using, Booked, Maintenance)
        /// - "areaId" (int?): Lọc theo khu vực
        /// </param>
        /// <returns>List<Table> - Danh sách bàn từ database</returns>
        public async Task<object> HandleAsync(Dictionary<string, object> parameters)
        {
            // Chạy trên background thread để không block UI
            return await Task.Run(() =>
            {
                try
                {
                    List<Table> tables;

                    // === CASE 1: Lọc theo Status (VD: chỉ lấy bàn trống) ===
                    if (parameters.ContainsKey("status") && parameters["status"] != null)
                    {
                        var status = parameters["status"].ToString();
                        tables = _tableService.GetTablesByStatus(status);
                    }
                    // === CASE 2: Lọc theo AreaId ===
                    else if (parameters.ContainsKey("areaId") && parameters["areaId"] != null)
                    {
                        var areaIdObj = parameters["areaId"];
                        int areaId;

                        // Xử lý nhiều kiểu dữ liệu có thể
                        if (areaIdObj is int intValue)
                            areaId = intValue;
                        else if (int.TryParse(areaIdObj.ToString(), out var parsedValue))
                            areaId = parsedValue;
                        else
                            throw new ArgumentException($"Invalid areaId: {areaIdObj}");

                        tables = _tableService.GetTablesByArea(areaId);
                    }
                    // === CASE 3: Lấy tất cả bàn ===
                    else
                    {
                        tables = _tableService.GetTables();
                    }

                    // Sắp xếp theo khu vực và tên bàn
                    tables = tables
                        .OrderBy(t => t.Area?.AreaName ?? "")
                        .ThenBy(t => t.TableName)
                        .ToList();

                    return tables;
                }
                catch (Exception ex)
                {
                    // Throw để ChatbotService xử lý
                    throw new Exception($"Lỗi khi lấy thông tin bàn: {ex.Message}", ex);
                }
            });
        }

        /// <summary>
        /// Format danh sách bàn thành text để gửi cho Gemini
        /// </summary>
        /// <param name="data">List<Table> từ HandleAsync</param>
        /// <returns>Text đã format, dễ đọc</returns>
        public string FormatResponse(object data)
        {
            if (data is not List<Table> tables)
                return "Không có dữ liệu bàn.";

            if (!tables.Any())
                return "Hiện tại không có bàn nào.";

            // === BUILD TEXT FORMAT ===
            var sb = new StringBuilder();
            sb.AppendLine($"🪑 THÔNG TIN BÀN ({tables.Count} bàn)");
            sb.AppendLine();

            // Group by Area
            var tablesGrouped = tables
                .GroupBy(t => t.Area?.AreaName ?? "Chưa phân khu")
                .OrderBy(g => g.Key);

            foreach (var group in tablesGrouped)
            {
                // Tên khu vực
                sb.AppendLine($"▶ {group.Key.ToUpper()}");

                foreach (var table in group)
                {
                    // Format: Tên bàn - Trạng thái với icon
                    sb.Append($"  • {table.TableName}");
                    sb.Append($" - {GetStatusText(table.Status)}");
                    sb.AppendLine();
                }

                sb.AppendLine(); // Dòng trống giữa các khu vực
            }

            // Thống kê nhanh
            var statusSummary = tables
                .GroupBy(t => t.Status)
                .Select(g => $"{GetStatusText(g.Key)}: {g.Count()}")
                .ToList();

            sb.AppendLine("📊 Tổng quan:");
            foreach (var summary in statusSummary)
            {
                sb.AppendLine($"  • {summary}");
            }

            return sb.ToString().TrimEnd();
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Lấy text hiển thị cho trạng thái bàn với icon
        /// </summary>
        private string GetStatusText(string status)
        {
            return status?.ToLower() switch
            {
                "empty" => "✅ Trống",
                "using" => "🔴 Đang sử dụng",
                "booked" => "📅 Đã đặt",
                "maintenance" => "🔧 Bảo trì",
                _ => status ?? "Không xác định"
            };
        }

        #endregion
    }
}
