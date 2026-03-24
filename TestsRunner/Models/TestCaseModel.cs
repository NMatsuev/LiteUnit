using TestsRunner.Models.Enums;

namespace TestsRunner.Models
{
    public class TestCaseModel
    {
        public object[] Arguments { get; set; }
        public string DisplayName { get; set; }
        public TestStatus Status { get; set; }
        public string ErrorMessage { get; set; }
        public TimeSpan Duration { get; set; }
        public string DisplayString =>
            DisplayName ?? $"({string.Join(", ", Arguments ?? Array.Empty<object>())})";
        public int? CancelAfterTimeout { get; set; }
        public bool HasCancelAfter => CancelAfterTimeout.HasValue && CancelAfterTimeout.Value > 0;
        public CancellationTokenSource CancellationTokenSource { get; set; }
    }
}
