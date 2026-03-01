using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TestedProject.Parser;
using TestingLibrary;
using TestingLibrary.Attributes;

namespace TestingProject
{
    [TestFixture]
    public class MathExpressionParserTests
    {
        private MathExpressionParser _parser;
        private Func<double, double> _function;
        private List<double> _results;
        private static int _testCounter;

        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Console.WriteLine("[TestFixtureSetUp] Инициализация тестов парсера");
            _testCounter = 0;
        }

        [SetUp]
        public void SetUp()
        {
            Console.WriteLine("[SetUp] Подготовка перед тестом");
            _results = new List<double>();
        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine("[TearDown] Очистка после теста");
            _testCounter++;
            _results.Clear();
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Console.WriteLine("[TestFixtureTearDown] Завершение тестов парсера");
            Console.WriteLine($"Всего выполнено тестов парсера: {_testCounter}");
        }

        #region Basic Operations Tests

        [Test]
        [Category("BasicOperations")]
        public void Parse_SimpleAddition_ReturnsCorrectResult()
        {
            //Arrange
            string expression = "x + 5";
            double x = 3;

            //Act
            _parser = new MathExpressionParser(expression);
            _function = _parser.Parse();
            double result = _function(x);
            _results.Add(result);

            //Assert
            Assert.AreEqual(8, result);
            Assert.IsTrue(result > 0);
            Assert.IsNotNull(_function);
        }

        [Test]
        [Category("BasicOperations")]
        public void Parse_SimpleSubtraction_ReturnsCorrectResult()
        {
            //Arrange
            string expression = "x - 3";
            double x = 10;

            //Act
            _parser = new MathExpressionParser(expression);
            _function = _parser.Parse();
            double result = _function(x);

            // Assert
            Assert.AreEqual(7, result, 0.00001);
            Assert.AreNotEqual(10, result);
            Assert.IsFalse(result > 10);
        }

        [Test]
        [Category("ComplexExpressions")]
        public void Parse_NestedParentheses_ReturnsCorrectResult()
        {
            //Arrange
            string expression = "2 * ((x + 3) * 2)";
            double x = 2;

            //Act
            _parser = new MathExpressionParser(expression);
            _function = _parser.Parse();
            double result = _function(x);

            //Assert
            Assert.AreEqual(20, result, 0.0001);
        }

        #endregion

        #region Function Tests

        [Test]
        [Category("Functions")]
        [TestCase(0, 0)]
        [TestCase(Math.PI / 2, 1)]
        [TestCase(Math.PI, 0)]
        public void Parse_SinFunction_ReturnsCorrectResult(double x, double expected)
        {
            //Arrange
            string expression = "sin(x)";

            //Act
            _parser = new MathExpressionParser(expression);
            _function = _parser.Parse();
            double result = _function(x);

            //Assert
            Assert.AreEqual(expected, result, 0.0001);
        }

        [Test]
        [Category("Functions")]
        [TestCase(0, 1)]
        [TestCase(Math.PI / 2, 0)]
        [TestCase(Math.PI, -1)]
        public void Parse_CosFunction_ReturnsCorrectResult(double x, double expected)
        {
            //Arrange
            string expression = "cos(x)";

            //Act
            _parser = new MathExpressionParser(expression);
            _function = _parser.Parse();
            double result = _function(x);

            //Assert
            Assert.AreEqual(expected, result, 0.0001);
        }

        #endregion

        #region Validation Tests

        [Test]
        [Category("Validation")]
        public void Parse_EmptyExpression_ThrowsArgumentException()
        {
            //Act & Assert
            var exception = Assert.Catch<ArgumentException>(() =>
            {
                _parser = new MathExpressionParser("");
            });

            Assert.IsNotNull(exception);
            Assert.IsInstanceOf<ArgumentException>(exception);
        }

        [Test]
        [Category("Validation")]
        public void Parse_InvalidCharacters_ThrowsArgumentException()
        {
            //Act & Assert
            var exception = Assert.Catch<ArgumentException>(() =>
            {
                _parser = new MathExpressionParser("2x + y"); // y - недопустимый символ
            });

            Assert.IsNotNull(exception);
        }

        #endregion

        #region Edge Cases Tests

        [Test]
        [Category("EdgeCases")]
        public void Parse_JustNumber_ReturnsConstant()
        {
            //Arrange
            string expression = "42";
            double x = 100;

            //Act
            _parser = new MathExpressionParser(expression);
            _function = _parser.Parse();
            double result = _function(x);

            //Assert
            Assert.AreEqual(42, result, 0.0001);
            Assert.AreNotEqual(x, result, 0.0001);
        }

        [Test]
        [Category("EdgeCases")]
        public void Parse_JustVariable_ReturnsX()
        {
            //Arrange
            string expression = "x";
            double x = 123.456;

            //Act
            _parser = new MathExpressionParser(expression);
            _function = _parser.Parse();
            double result = _function(x);

            //Assert
            Assert.AreEqual(x, result, 0.0001);
            Assert.AreSame(x.GetType(), result.GetType());
        }

        #endregion

        #region Async Tests

        [Test]
        [Category("Async")]
        public async Task Parse_AsyncEvaluation_CompletesSuccessfully()
        {
            //Arrange
            string expression = "2*x*x*x*x*x + 1";
            double x = 5;

            //Act
            double result = await Task.Run(() =>
            {
                _parser = new MathExpressionParser(expression);
                _function = _parser.Parse();
                Thread.Sleep(2000);
                return _function(x);
            });

            //Assert
            Assert.AreEqual(2*5*5*5*5*5+ 1, result, 0.0001);
            Assert.IsEmpty(_results);
        }

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults()
        {
            //Arrange
            var expressions = new[]
            {
                "x + 1",
                "2*x",
                "x*x",
                "sin(x)"
            };
            double x = Math.PI / 4;
            var results = new List<double>();

            //Act
            await Task.Run(() =>
            {
                foreach (var expr in expressions)
                {
                    _parser = new MathExpressionParser(expr);
                    _function = _parser.Parse();
                    Thread.Sleep(2000);
                    results.Add(_function(x));
                }
            });

            //Assert
            Assert.AreEqual(4, results.Count);
            Assert.IsNotEmpty(results);
            Assert.Contains(x + 1, results);
            Assert.Contains(2 * x, results);
        }

        [Test]
        [Category("Async")]
        [Ignore]
        public async Task Parse_LongAsyncOperation_CompletesSuccessfully()
        {
            //Arrange
            string expression = "pow(x, 10)";
            double x = 2;

            // Act
            float result = await Task.Run(() =>
            {
                System.Threading.Thread.Sleep(2000); // Симуляция долгой операции
                _parser = new MathExpressionParser(expression);
                _function = _parser.Parse();
                return (float)_function(x);
            });

            //Assert
            Assert.AreEqual(1024, result, 0.0001f);
        }

        #endregion

        #region Pass/Fail Tests

        [Test]
        [Category("Success")]
        public void Parse_ValidExpression_PassesSuccessfully()
        {
            //Arrange
            string expression = "2*x + 3";

            //Act & Assert
            Assert.DoesNotThrow(() =>
            {
                _parser = new MathExpressionParser(expression);
                _function = _parser.Parse();
                _function(5);
            });
            Assert.Pass("Парсинг успешно завершен");
        }

        [Test]
        [Category("Failure")]
        public void Parse_InvalidExpression_FailsWithException()
        {
            //Arrange
            string expression = "2x + 3"; 

            //Act & Assert
            var exception = Assert.Catch(() =>
            {
                _parser = new MathExpressionParser(expression);
            });

            Assert.IsNotNull(exception);
        }

        [Test]
        [Category("Failure")]
        public void Parse_FailsWithException()
        {
            //Arrange
            string expression = "2x + 3";
            Assert.Fail();
        }

        #endregion
    }
}