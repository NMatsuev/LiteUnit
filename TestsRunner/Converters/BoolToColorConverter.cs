using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TestsRunner.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isRunning && isRunning)
                return new SolidColorBrush(Color.FromRgb(255, 200, 200)); //Светло-красный при выполнении
            else
                return new SolidColorBrush(Color.FromRgb(240, 240, 240)); //Светло-серый в остальное время
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}