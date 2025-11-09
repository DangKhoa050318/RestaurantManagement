using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects.Models.Chatbot
{
    public class ChatIntent
    {
        public string IntentType { get; set; } = string.Empty;
        public Dictionary<string, object> Parameters { get; set; } = new();
        public double Confidence { get; set; }
    }

    public static class IntentTypes
    {
        public const string GetMenu = "GetMenu";
        public const string GetTableAvailability = "GetTableAvailability";
        public const string GetTotalBookings = "GetTotalBookings";
        public const string GetPopularDishes = "GetPopularDishes";
        // ✨ Các intent mới cho thống kê doanh thu
        public const string GetTableRevenue = "GetTableRevenue";           // Tổng bill của bàn
        public const string GetDailyRevenue = "GetDailyRevenue";           // Doanh thu theo ngày
        public const string GetPeakHours = "GetPeakHours";                 // Giờ đông khách
        public const string GetTableRevenueRanking = "GetTableRevenueRanking"; // Bàn có doanh thu cao/thấp

        public const string Unknown = "Unknown";
    }
}
