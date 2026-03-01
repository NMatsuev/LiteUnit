using TestsRunner.Models;
using TestsRunner.Models.Enums;

namespace TestsRunner.ViewModels.Implementations
{
    public class MethodViewModel : TreeViewItemBase
    {
        private TestMethodModel _method;
        private TimeSpan _duration;
        private string _categories;
        private string _errorMessage;

        public MethodViewModel(TestMethodModel method)
        {
            _method = method;
            DisplayName = method.MethodName;
            UpdateFromModel();
        }

        public TestMethodModel Method => _method;

        public override ItemType ItemType => ItemType.Method;

        public TimeSpan Duration
        {
            get => _duration;
            private set => SetProperty(ref _duration, value);
        }

        public string DurationDisplay
        {
            get
            {
                if (_duration.TotalMilliseconds > 0)
                {
                    if (_duration.TotalSeconds >= 1)
                        return $"{_duration.TotalSeconds:F2} с";
                    else
                        return $"{_duration.TotalMilliseconds:F0} мс";
                }
                return string.Empty;
            }
        }

        public string Categories
        {
            get => _categories;
            private set => SetProperty(ref _categories, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            private set => SetProperty(ref _errorMessage, value);
        }

        public bool HasError => !string.IsNullOrEmpty(_errorMessage);

        public string ToolTip
        {
            get
            {
                var tooltip = "";
                if (!string.IsNullOrEmpty(Categories))
                    tooltip += string.Format("{0:s30}", $"{Categories}");
                if (HasError)
                    tooltip += string.Format("{0:s40}", $"{ErrorMessage}");
                if (Duration.TotalMilliseconds > 0)
                    tooltip += $" {DurationDisplay}";
                return tooltip;
            }
        }

        public void UpdateFromModel()
        {
            Duration = _method.Duration;
            Categories = _method.Categories != null ? string.Join(", ", _method.Categories) : string.Empty;
            ErrorMessage = _method.ErrorMessage;

            Status = _method.Status;

            OnPropertyChanged(nameof(ToolTip));
            OnPropertyChanged(nameof(HasError));
            OnPropertyChanged(nameof(DurationDisplay));
        }
    }
}