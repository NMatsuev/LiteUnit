using System;

namespace TestingLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TestFixtureSetUpAttribute : Attribute
    {
        public TestFixtureSetUpAttribute() { }
    }
}
