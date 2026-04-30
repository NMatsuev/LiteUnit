using System;

namespace TestingLibrary
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TestAttribute : Attribute
    {
        public TestAttribute() {}
    }
}
