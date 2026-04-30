using System;

namespace TestingLibrary.Attributes 
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class TestCaseSourceAttribute : Attribute
    {
        public string MethodName { get; }

        public TestCaseSourceAttribute(string methodName)
        {
            MethodName = methodName;
        }
    }
}
