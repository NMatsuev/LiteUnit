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

            if (method.IsParameterized)
            {
                foreach (var testCase in method.TestCases)
                {
                    Children.Add(new TestCaseViewModel(testCase, method.MethodName));
                }
            }

            UpdateFromModel();
        }

        public TestMethodModel Method => _method;

        public override ItemType ItemType => ItemType.Method;

        public bool IsParameterized => _method.IsParameterized;

        public string DisplayString => IsParameterized
            ? $"{_method.MethodName} ({_method.TestCases.Count} кейсов)"
            : _method.MethodName;

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
                    tooltip += string.Format("{0:s30}", $" Категории: {Categories}");

                if (HasError)
                    tooltip += string.Format("{0:s30}", $" Ошибка: {ErrorMessage}");

                if (Duration.TotalMilliseconds > 0)
                    tooltip += string.Format("{0:s30}", $" ({DurationDisplay})");

                return tooltip;
            }
        }

        public void UpdateFromModel()
        {
            if (IsParameterized)
            {
                foreach (var child in Children.OfType<TestCaseViewModel>())
                {
                    child.UpdateFromModel();
                }

                //Агрегируем статус из TestCase
                if (_method.TestCases.All(tc => tc.Status == TestStatus.Passed))
                    Status = TestStatus.Passed;
                else if (_method.TestCases.Any(tc => tc.Status == TestStatus.Failed))
                    Status = TestStatus.Failed;
                else if (_method.TestCases.Any(tc => tc.Status == TestStatus.Running))
                    Status = TestStatus.Running;
                else
                    Status = TestStatus.None;

                //Для параметризованных методов свои поля не используем
                Duration = TimeSpan.Zero;
                ErrorMessage = string.Empty;
            }
            else
            {
                //Обычный тест - данные хранятся в самом методе
                Duration = _method.Duration;
                ErrorMessage = _method.ErrorMessage;
                Status = _method.Status;
            }

            Categories = _method.Categories != null ? string.Join(", ", _method.Categories) : string.Empty;

            OnPropertyChanged(nameof(ToolTip));
            OnPropertyChanged(nameof(HasError));
            OnPropertyChanged(nameof(DurationDisplay));
            OnPropertyChanged(nameof(DisplayString));
        }
    }
}