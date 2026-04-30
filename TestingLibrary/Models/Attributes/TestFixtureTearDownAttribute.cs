using System;

namespace TestingLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TestFixtureTearDownAttribute : Attribute
    {
        public TestFixtureTearDownAttribute() { }
    }
}
