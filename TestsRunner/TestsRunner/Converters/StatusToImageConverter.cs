using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using TestsRunner.Enums;

namespace TestsRunner.Converters
{
    // Конвертер для отображения иконок статуса - ИЗМЕНЕНО на IValueConverter
    public class StatusToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TestStatus status)
            {
                // Путь к изображениям зависит от того, где они хранятся в проекте
                string imagePath = status switch
                {
                    TestStatus.Passed => "../../../Images/test_passed.png",
                    TestStatus.Failed => "../../../Images/test_failed.png",
                    TestStatus.Running => "../../../Images/test_running.png",
                    _ => "../../../Images/test_none.png"
                };

                try
                {
                    // Создаем BitmapImage с правильными настройками
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath, UriKind.RelativeOrAbsolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    return bitmap;
                }
                catch
                {
                    // В случае ошибки возвращаем null, Image просто не отобразится
                    return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
