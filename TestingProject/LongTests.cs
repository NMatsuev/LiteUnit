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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults5()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults6()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults7()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults8()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults9()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults10()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults11()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults12()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults13()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults14()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults15()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults16()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults17()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults18()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults19()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults20()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults21()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults22()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults23()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults24()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults25()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults26()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults27()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults28()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults29()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults30()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults31()
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

        [Test]
        [Category("Async")]
        public async Task Parse_MultipleAsyncEvaluations_AllReturnCorrectResults32()
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