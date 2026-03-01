using TestsRunner.Models;
using TestsRunner.Models.Enums;

namespace TestsRunner.ViewModels.Implementations
{
    public class TestCaseViewModel : TreeViewItemBase
    {
        private TestCaseData _testCase;
        private string _methodName;

        public TestCaseViewModel(TestCaseData testCase, string methodName)
        {
            _testCase = testCase;
            _methodName = methodName;
            DisplayName = testCase.DisplayString;
        }

        public TestCaseData TestCase => _testCase;

        public override ItemType ItemType => ItemType.TestCase;

        public string DurationDisplay
        {
            get
            {
                if (_testCase.Duration.TotalMilliseconds > 0)
                {
                    if (_testCase.Duration.TotalSeconds >= 1)
                        return $"{_testCase.Duration.TotalSeconds:F2} с";
                    else
                        return $"{_testCase.Duration.TotalMilliseconds:F0} мс";
                }
                return string.Empty;
            }
        }

        public string FullDisplayName => $"{_methodName}{DisplayName}";

        public string ToolTip
        {
            get
            {
                var tooltip = "";

                if (!string.IsNullOrEmpty(_testCase.ErrorMessage))
                    tooltip += string.Format("{0:s30}", $" Ошибка: {_testCase.ErrorMessage}");

                if (_testCase.Duration.TotalMilliseconds > 0)
                    tooltip += string.Format("{0:s30}", $" ({DurationDisplay})");

                return tooltip;
            }
        }

        public void UpdateFromModel()
        {
            Status = _testCase.Status;
            OnPropertyChanged(nameof(Status));
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(ToolTip));
        }
    }
}