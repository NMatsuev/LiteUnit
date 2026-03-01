using System;
using TestingLibrary;
using TestedProject;
using TestingLibrary.Attributes;
using System.Threading;

namespace TestingProject
{
    [TestFixture]
    public class Tests
    {
        [Test]
        static void TestOne()
        {
            Console.WriteLine(nameof(TestOne));
            Thread.Sleep(3000);
        }
        [Test]
        static void TestTwo()
        {
            Console.WriteLine(nameof(TestTwo));
            Assert.IsTrue(3 == 3);
            Thread.Sleep(1000);
        }
    }
}
