using System.Collections.Generic;
using System.Threading;
using TestingLibrary;
using TestingLibrary.Attributes;
using TestingLibrary.Models;

namespace TestingProject
{
    public class TestCaseSourceTests
    {
        [TestFixture]
        public class TimeoutTests
        {
            [Test]
            [CancelAfter(1000)]
            [Category("TestCaseSource")]
            [TestCaseSource(nameof(MultiplyTestCases))]
            public void TestMultiply(CancellationToken token, int val1, int val2, int res)
            {
                Assert.AreEqual(val1 * val2, res);
            }

            public static IEnumerable<TestCaseData> MultiplyTestCases()
            {
                yield return new TestCaseData("Positive numbers", 2, 3, 6);
                yield return new TestCaseData("Negative numbers", -2, -3, 6);
                yield return new TestCaseData("Mixed numbers", -2, 3, -6);
                yield return new TestCaseData("Zero", 0, 5, 0);
            }

        }
    }
}
