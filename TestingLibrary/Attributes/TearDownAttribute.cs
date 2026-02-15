using System;

namespace TestingLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class TearDownAttribute : Attribute
    {
        public TearDownAttribute() { }
    }
}
