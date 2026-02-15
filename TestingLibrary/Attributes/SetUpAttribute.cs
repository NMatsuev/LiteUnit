using System;

namespace TestingLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class SetUpAttribute : Attribute
    {
        public SetUpAttribute() { }
    }
}
