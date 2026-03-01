using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TestedProject.Parser
{
    /// <summary>
    /// Парсер математических выражений
    /// </summary>
    public class MathExpressionParser
    {
        private readonly string _expression;
        private List<string> _tokens;
        private int _position;

        public MathExpressionParser(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression))
                throw new ArgumentException("Выражение не может быть пустым");

            _expression = expression.Replace(" ", "").ToLower();
            ValidateExpression();
        }

        /// <summary>
        /// Валидация выражения
        /// </summary>
        private void ValidateExpression()
        {
            // Проверка на недопустимые символы
            var allowedPattern = @"^[0-9x\+\-\*\/\(\)\.\,\s]+|sin|cos|tg|ctg|pow";
            var invalidChars = Regex.Replace(_expression, @"[0-9x\+\-\*\/\(\)\.\s]|sin|cos|tg|ctg|pow", "");

            if (!string.IsNullOrEmpty(invalidChars))
                throw new ArgumentException($"Выражение содержит недопустимые символы: {invalidChars}");

            // Проверка баланса скобок
            int balance = 0;
            foreach (char c in _expression)
            {
                if (c == '(') balance++;
                if (c == ')') balance--;
                if (balance < 0)
                    throw new ArgumentException("Несбалансированные скобки: закрывающая скобка без открывающей");
            }
            if (balance != 0)
                throw new ArgumentException("Несбалансированные скобки: не все скобки закрыты");

            // Проверка на корректность функций
            var functionPattern = @"(sin|cos|tg|ctg|pow)\s*\(";
            var matches = Regex.Matches(_expression, functionPattern);
            foreach (Match match in matches)
            {
                string func = match.Groups[1].Value;
                int startPos = match.Index;

                // Для pow нужно проверить наличие запятой
                if (func == "pow")
                {
                    int endPos = FindClosingBracket(_expression, startPos + 3);
                    if (endPos == -1)
                        throw new ArgumentException("Неверный формат функции pow");

                    string args = _expression.Substring(startPos + 4, endPos - startPos - 5);
                    if (!args.Contains(','))
                        throw new ArgumentException("Функция pow должна содержать два аргумента, разделенных запятой");
                }
            }
        }

        private int FindClosingBracket(string expr, int startPos)
        {
            int balance = 1;
            for (int i = startPos + 1; i < expr.Length; i++)
            {
                if (expr[i] == '(') balance++;
                if (expr[i] == ')') balance--;
                if (balance == 0) return i;
            }
            return -1;
        }

        /// <summary>
        /// Токенизация выражения
        /// </summary>
        private void Tokenize()
        {
            _tokens = new List<string>();
            _position = 0;

            for (int i = 0; i < _expression.Length; i++)
            {
                char c = _expression[i];

                if (char.IsDigit(c) || c == '.')
                {
                    // Число
                    string number = ParseNumber(i);
                    _tokens.Add(number);
                    i += number.Length - 1;
                }
                else if (c == 'x')
                {
                    _tokens.Add("x");
                }
                else if (c == '+' || c == '-' || c == '*' || c == '/' || c == '(' || c == ')' || c == ',')
                {
                    _tokens.Add(c.ToString());
                }
                else if (c == 's' && i + 2 < _expression.Length && _expression.Substring(i, 3) == "sin")
                {
                    _tokens.Add("sin");
                    i += 2;
                }
                else if (c == 'c' && i + 2 < _expression.Length && _expression.Substring(i, 3) == "cos")
                {
                    _tokens.Add("cos");
                    i += 2;
                }
                else if (c == 't' && i + 1 < _expression.Length && _expression.Substring(i, 2) == "tg")
                {
                    _tokens.Add("tg");
                    i += 1;
                }
                else if (c == 'c' && i + 2 < _expression.Length && _expression.Substring(i, 3) == "ctg")
                {
                    _tokens.Add("ctg");
                    i += 2;
                }
                else if (c == 'p' && i + 2 < _expression.Length && _expression.Substring(i, 3) == "pow")
                {
                    _tokens.Add("pow");
                    i += 2;
                }
            }
        }

        private string ParseNumber(int start)
        {
            int end = start;
            while (end < _expression.Length &&
                  (char.IsDigit(_expression[end]) || _expression[end] == '.'))
            {
                end++;
            }
            return _expression.Substring(start, end - start);
        }

        /// <summary>
        /// Построение функции из выражения
        /// </summary>
        public Func<double, double> Parse()
        {
            Tokenize();
            _position = 0;
            var result = ParseExpression();

            if (_position < _tokens.Count)
                throw new ArgumentException("Некорректное выражение");

            return x => Evaluate(result, x);
        }

        private double Evaluate(ExpressionNode node, double x)
        {
            if (node is NumberNode numberNode)
                return numberNode.Value;

            if (node is VariableNode)
                return x;

            if (node is BinaryNode binaryNode)
            {
                var left = Evaluate(binaryNode.Left, x);
                var right = Evaluate(binaryNode.Right, x);

                // Заменяем switch expression на if-else для C# 7.3
                if (binaryNode.Operator == "+")
                    return left + right;
                else if (binaryNode.Operator == "-")
                    return left - right;
                else if (binaryNode.Operator == "*")
                    return left * right;
                else if (binaryNode.Operator == "/")
                    return left / right;
                else
                    throw new ArgumentException($"Неизвестный оператор: {binaryNode.Operator}");
            }

            if (node is FunctionNode functionNode)
            {
                if (functionNode.Name == "pow")
                {
                    if (functionNode.Arguments.Count != 2)
                        throw new ArgumentException("pow требует 2 аргумента");

                    var arg1 = Evaluate(functionNode.Arguments[0], x);
                    var arg2 = Evaluate(functionNode.Arguments[1], x);
                    return Math.Pow(arg1, arg2);
                }

                var arg = Evaluate(functionNode.Arguments[0], x);

                // Заменяем switch expression на if-else для C# 7.3
                if (functionNode.Name == "sin")
                    return Math.Sin(arg);
                else if (functionNode.Name == "cos")
                    return Math.Cos(arg);
                else if (functionNode.Name == "tg")
                    return Math.Tan(arg);
                else if (functionNode.Name == "ctg")
                    return 1 / Math.Tan(arg);
                else
                    throw new ArgumentException($"Неизвестная функция: {functionNode.Name}");
            }

            throw new ArgumentException("Неизвестный тип узла");
        }

        private ExpressionNode ParseExpression()
        {
            var left = ParseTerm();

            while (_position < _tokens.Count && (_tokens[_position] == "+" || _tokens[_position] == "-"))
            {
                var op = _tokens[_position++];
                var right = ParseTerm();
                left = new BinaryNode(op, left, right);
            }

            return left;
        }

        private ExpressionNode ParseTerm()
        {
            var left = ParseFactor();

            while (_position < _tokens.Count && (_tokens[_position] == "*" || _tokens[_position] == "/"))
            {
                var op = _tokens[_position++];
                var right = ParseFactor();
                left = new BinaryNode(op, left, right);
            }

            return left;
        }

        private ExpressionNode ParseFactor()
        {
            if (_position >= _tokens.Count)
                throw new ArgumentException("Неожиданный конец выражения");

            var token = _tokens[_position];

            if (token == "(")
            {
                _position++;
                var expr = ParseExpression();
                if (_position >= _tokens.Count || _tokens[_position] != ")")
                    throw new ArgumentException("Ожидается ')'");
                _position++;
                return expr;
            }

            if (double.TryParse(token, out double number))
            {
                _position++;
                return new NumberNode(number);
            }

            if (token == "x")
            {
                _position++;
                return new VariableNode();
            }

            if (IsFunction(token))
            {
                _position++;
                if (_position >= _tokens.Count || _tokens[_position] != "(")
                    throw new ArgumentException($"Ожидается '(' после функции {token}");

                _position++;
                var args = new List<ExpressionNode>();

                if (token == "pow")
                {
                    // pow с двумя аргументами
                    args.Add(ParseExpression());
                    if (_position >= _tokens.Count || _tokens[_position] != ",")
                        throw new ArgumentException("Ожидается ',' между аргументами pow");
                    _position++;
                    args.Add(ParseExpression());
                }
                else
                {
                    // Один аргумент для sin, cos, tg, ctg
                    args.Add(ParseExpression());
                }

                if (_position >= _tokens.Count || _tokens[_position] != ")")
                    throw new ArgumentException("Ожидается ')'");
                _position++;

                return new FunctionNode(token, args);
            }

            throw new ArgumentException($"Неожиданный токен: {token}");
        }

        private bool IsFunction(string token)
        {
            return token == "sin" || token == "cos" || token == "tg" ||
                   token == "ctg" || token == "pow";
        }
    }

    // Классы для построения дерева выражения
    internal abstract class ExpressionNode { }

    internal class NumberNode : ExpressionNode
    {
        public double Value { get; }
        public NumberNode(double value) => Value = value;
    }

    internal class VariableNode : ExpressionNode { }

    internal class BinaryNode : ExpressionNode
    {
        public string Operator { get; }
        public ExpressionNode Left { get; }
        public ExpressionNode Right { get; }

        public BinaryNode(string op, ExpressionNode left, ExpressionNode right)
        {
            Operator = op;
            Left = left;
            Right = right;
        }
    }

    internal class FunctionNode : ExpressionNode
    {
        public string Name { get; }
        public List<ExpressionNode> Arguments { get; }

        public FunctionNode(string name, List<ExpressionNode> args)
        {
            Name = name;
            Arguments = args;
        }
    }
}