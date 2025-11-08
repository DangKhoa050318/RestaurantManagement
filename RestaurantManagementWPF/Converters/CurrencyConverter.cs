using System.Globalization;
using System.Windows.Data;

namespace RestaurantManagementWPF.Converters
{
    /// <summary>
    /// Converts decimal values to currency format
    /// </summary>
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                return decimalValue.ToString("C0", new CultureInfo("vi-VN"));
            }
            if (value is double doubleValue)
            {
                return doubleValue.ToString("C0", new CultureInfo("vi-VN"));
            }
            return "0 ?";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                stringValue = stringValue.Replace("?", "").Replace(",", "").Trim();
                if (decimal.TryParse(stringValue, out decimal result))
                {
                    return result;
                }
            }
            return 0m;
        }
    }
}
