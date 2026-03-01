using System.Reflection;
using TestsRunner.Enums;

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
    }
}