using TestingLibrary;
using TestingLibrary.Attributes;

namespace TestingProject
{
    [TestFixture]
    public class ExpressionTests
    {
        [Test]
        [Category("Expression")]
        public void IsTrue_PassingTest_SimpleCondition()
        {
            int x = 5, y = 3;

            Assert.IsTrue(() => x + y == 8);
        }

        [Test]
        [Category("Expression")]
        public void IsTrue_FailingTest_SimpleCondition()
        {
            int x = 5, y = 3;

            Assert.IsTrue(() => x + y == 10);
        }

        [Test]
        [Category("Expression")]
        public void IsFalse_PassingTest_SimpleCondition()
        {
            int value = 10;

            Assert.IsFalse(() => value > 20);
        }

        [Test]
        [Category("Expression")]
        public void IsFalse_FailingTest_SimpleCondition()
        {
            int value = 10;

            Assert.IsFalse(() => value < 20);
        }

        [Test]
        [Category("Expression")]
        public void IsTrue_WithComplexCondition_Passing()
        {
            int age = 25;
            string name = "John";

            Assert.IsTrue(() => age >= 18 && !string.IsNullOrEmpty(name));
        }

        [Test]
        [Category("Expression")]
        public void IsTrue_WithComplexCondition_Failing()
        {
            int age = 15;
            string name = "John";

            Assert.IsTrue(() => age >= 18 && !string.IsNullOrEmpty(name));
        }

        [Test]
        [Category("Expression")]
        public void IsFalse_WithComplexCondition_Passing()
        {
            int balance = -10;

            Assert.IsFalse(() => balance > 0);
        }

        [Test]
        [Category("Expression")]
        public void IsFalse_WithComplexCondition_Failing()
        {
            int balance = 100;

            Assert.IsFalse(() => balance > 0);
        }

        [Test]
        [Category("Expression")]
        public void AreEqual_PassingTest_Numbers()
        {
            Assert.AreEqual(() => 5 * 5, () => 20 + 5);
        }

        [Test]
        [Category("Expression")]
        public void AreEqual_FailingTest_Numbers()
        {
            Assert.AreEqual(() => 5 * 5, () => 25 + 1);
        }

        [Test]
        [Category("Expression")]
        public void AreEqual_PassingTest_Strings()
        {
            string str1 = "Hello";
            string str2 = "World";

            Assert.AreEqual(() => str1 + " " + str2, () => "Hello World");
        }

        [Test]
        [Category("Expression")]
        public void AreEqual_FailingTest_Strings()
        {
            string test = "Test";

            Assert.AreEqual(() => test.ToLower(), () => test);
        }

        [Test]
        [Category("Expression")]
        public void AreEqual_PassingTest_WithMathOperations()
        {
            Assert.AreEqual(() => System.Math.Pow(2, 3), () => 8);
        }

        [Test]
        [Category("Expression")]
        public void AreEqual_FailingTest_WithMathOperations()
        {
            Assert.AreEqual(() => System.Math.Sqrt(16), () => 5);
        }

        [Test]
        [Category("Expression")]
        public void IsTrue_WithParameter_PassingTest()
        {
            Assert.IsTrue((int x) => x * x > 20, 5);
        }

        [Test]
        [Category("Expression")]
        public void IsTrue_WithParameter_FailingTest()
        {
            Assert.IsTrue((int x) => x * x > 20, 3);
        }

        [Test]
        [Category("Expression")]
        public void IsTrue_WithParameterAndComplexCondition_Passing()
        {
            Assert.IsTrue((int salary) =>
                salary > 15000 + 10000 && salary - (15000 + 10000) > 5000,
                50000);
        }

        [Test]
        [Category("Expression")]
        public void IsTrue_WithParameterAndComplexCondition_Failing()
        {
            Assert.IsTrue((int salary) =>
                salary > 15000 + 10000 && salary - (15000 + 10000) > 5000,
                20000);
        }

        [Test]
        [Category("Expression")]
        public void MixedMultipleChecks_Passing()
        {
            int a = 10, b = 5, c = 15;

            Assert.IsTrue(() => a + b == c);
            Assert.IsFalse(() => a * b > 100);
            Assert.AreEqual(() => a - b, () => 5);
        }

        [Test]
        [Category("Expression")]
        public void MixedMultipleChecks_Failing()
        {
            int a = 10, b = 5, c = 20;

            Assert.IsTrue(() => a + b == c);
            Assert.IsFalse(() => a * b > 100);
            Assert.AreEqual(() => a - b, () => 5);
        }
    }
}