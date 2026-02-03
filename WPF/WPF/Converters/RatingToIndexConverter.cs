using System;
using System.Globalization;
using System.Windows.Data;

namespace WPF.Converters
{
    public class RatingToIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal rating)
            {
                return (int)rating - 1;
            }
            if (value is int intRating)
            {
                return intRating - 1;
            }
            return 4;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int index)
            {
                return (decimal)(index + 1);
            }
            return 5m;
        }
    }
}

