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
        public string ErrorMessage { get; set; }
        public TimeSpan Duration { get; set; }
        public string[] Categories { get; set; }
        public bool IsIgnored { get; set; }
        public string IgnoreReason { get; set; }

        public bool IsAsync => MethodInfo.GetCustomAttribute<AsyncStateMachineAttribute>() != null;

        public List<TestCaseData> TestCases { get; set; } = new List<TestCaseData>();

        public bool IsParameterized => TestCases.Any();
    }


    public class TestCaseData
    {
        public object[] Arguments { get; set; }
        public string DisplayName { get; set; }
        public TestStatus Status { get; set; }
        public string ErrorMessage { get; set; }
        public TimeSpan Duration { get; set; }

        public string DisplayString =>
            DisplayName ?? $"({string.Join(", ", Arguments ?? Array.Empty<object>())})";
    }
}