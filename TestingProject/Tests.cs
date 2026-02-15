using System;
using TestingLibrary;
using TestedProject;
using TestingLibrary.Attributes;

namespace TestingProject
{
    [TestFixture]
    public class Tests
    {
        [Test]
        static void TestOne()
        {
            Console.WriteLine(nameof(TestOne));
        }
        [Test]
        static void TestTwo()
        {
            Console.WriteLine(nameof(TestTwo));
        }
    }
}
