using System.Reflection;
using System.Runtime.CompilerServices;
using TestsRunner.Models.Enums;

namespace TestsRunner.Models
{
    public class TestMethodModel
    {
        public MethodInfo MethodInfo { get; set; }
        public string MethodName => MethodInfo.Name;
        public TestStatus Status { get; set; }
        public bool IsRunnable { get; set; } = true;
        public string ErrorMessage { get; set; }
        public TimeSpan Duration { get; set; }
        public string[] Categories { get; set; }
        public bool IsIgnored { get; set; }
        public string IgnoreReason { get; set; }
        public bool IsAsync => MethodInfo.GetCustomAttribute<AsyncStateMachineAttribute>() != null;
        public List<TestCaseModel> TestCases { get; set; } = new List<TestCaseModel>();
        public bool IsParameterized => TestCases.Count != 0;
        public int? CancelAfterTimeout { get; set; }
        public bool HasCancelAfter => CancelAfterTimeout.HasValue && CancelAfterTimeout.Value > 0;
        public CancellationTokenSource CancellationTokenSource { get; set; }
    }
}