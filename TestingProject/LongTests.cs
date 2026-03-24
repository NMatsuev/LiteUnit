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
    public class LongTests
    {
        private MathExpressionParser _parser;
        private Func<double, double> _function;
        private List<double> _results;
        private static int _testCounter;



        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults1()
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
                    results.Add(_function(x));
                }
                ;
                Thread.Sleep(6000);
            });

            //Assert
            Assert.AreEqual(4, results.Count);
            Assert.IsNotEmpty(results);
            Assert.Contains(x + 1, results);
            Assert.Contains(2 * x, results);
        }

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults2()
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
                    results.Add(_function(x));
                }
                ;
                Thread.Sleep(6000);
            });

            //Assert
            Assert.AreEqual(4, results.Count);
            Assert.IsNotEmpty(results);
            Assert.Contains(x + 1, results);
            Assert.Contains(2 * x, results);
        }

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults3()
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
                    results.Add(_function(x));
                }
                ;
                Thread.Sleep(5000);
            });

            //Assert
            Assert.AreEqual(4, results.Count);
            Assert.IsNotEmpty(results);
            Assert.Contains(x + 1, results);
            Assert.Contains(2 * x, results);
        }

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults4()
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
                    results.Add(_function(x));
                }
                ;
                Thread.Sleep(2000);
            });

            //Assert
            Assert.AreEqual(4, results.Count);
            Assert.IsNotEmpty(results);
            Assert.Contains(x + 1, results);
            Assert.Contains(2 * x, results);
        }
    }
}