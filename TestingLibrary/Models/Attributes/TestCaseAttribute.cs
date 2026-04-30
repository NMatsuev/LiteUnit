using System;

namespace TestingLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class TestCaseAttribute : Attribute
    {
        public TestCaseAttribute(params object[] testParams)
        {
            TestParams = testParams;
        }

        public object[] TestParams { get; set; }
    }
}
