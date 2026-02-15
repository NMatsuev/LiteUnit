using System;

namespace TestingLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class TestFixtureAttribute : Attribute
    {
        public TestFixtureAttribute() { }
    }
}
