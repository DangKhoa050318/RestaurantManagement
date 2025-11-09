using System.Globalization;
using System.Windows;
using System.Windows.Data;
using BusinessObjects.Models;

namespace RestaurantManagementWPF.Converters
{
    public class TableSelectionConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length == 2 && values[0] is int currentTableId && values[1] is Table selectedTable)
            {
                return currentTableId == selectedTable?.TableId ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
