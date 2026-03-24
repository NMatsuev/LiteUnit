using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace TestsRunner.Converters
{
    public class TimeSpanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TimeSpan timeSpan && timeSpan.TotalMilliseconds > 0)
                return Visibility.Visible;

            //Если параметр "Running", показываем когда IsRunning = true
            if (parameter?.ToString() == "Running" && value is bool isRunning)
                return isRunning ? Visibility.Visible : Visibility.Collapsed;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}