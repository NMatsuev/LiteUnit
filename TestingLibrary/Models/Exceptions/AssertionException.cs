using System;

namespace TestingLibrary.Exceptions
{
    public class AssertionException : Exception
    {
        public AssertionException(string message) : base(message) { }
    }
}
