using System;

namespace TestingLibrary.Exceptions
{
    public class SuccessException : Exception
    {
        public SuccessException(string message) : base(message) { }
    }
}
