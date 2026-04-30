using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace TestingLibrary.Services
{
    /// <summary>
    /// Анализатор дерева выражений для получения детальной информации о структуре и значениях
    /// </summary>
    public static class ExpressionTreeAnalyzer
    {
        public static string Analyze(LambdaExpression lambda, params object[] parameterValues)
        {
            var visitor = new DetailedExpressionVisitor(parameterValues);
            visitor.Visit(lambda.Body);
            return visitor.GetAnalysis();
        }

        public static string Analyze<T>(Expression<Func<T, bool>> lambda, T parameter)
        {
            return Analyze(lambda, new object[] { parameter });
        }

        public static string Analyze(Expression<Func<bool>> lambda)
        {
            return Analyze(lambda, Array.Empty<object>());
        }

        private class DetailedExpressionVisitor : ExpressionVisitor
        {
            private readonly StringBuilder _analysis = new StringBuilder();
            private readonly Dictionary<ParameterExpression, object> _parameterValues;
            private int _indentLevel = 0;

            public DetailedExpressionVisitor(object[] parameterValues)
            {
                _parameterValues = new Dictionary<ParameterExpression, object>();
            }

            public string GetAnalysis() => _analysis.ToString();

            private void AppendLine(string text)
            {
                _analysis.Append(' ', _indentLevel * 2);
                _analysis.AppendLine(text);
            }

            private IDisposable Indent()
            {
                _indentLevel++;
                return new DisposableAction(() => _indentLevel--);
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {
                using (Indent())
                {
                    AppendLine($"┌─ Бинарная операция: {GetOperatorText(node.NodeType)}");

                    AppendLine("├─ Левый операнд:");
                    Visit(node.Left);

                    AppendLine("├─ Правый операнд:");
                    Visit(node.Right);

                    // Вычисляем результат операции, если возможно
                    try
                    {
                        var result = Expression.Lambda(node).Compile().DynamicInvoke();
                        AppendLine($"└─ Результат операции: {FormatValue(result)}");
                    }
                    catch
                    {
                        AppendLine("└─ Результат операции: [невозможно вычислить]");
                    }
                }
                return node;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                AppendLine($"● Константа: {FormatValue(node.Value)} (тип: {node.Type.Name})");
                return node;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                var value = _parameterValues.ContainsKey(node) ? _parameterValues[node] : "неизвестно";
                AppendLine($"● Параметр: {node.Name} = {FormatValue(value)} (тип: {node.Type.Name})");
                return node;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                using (Indent())
                {
                    AppendLine($"┌─ Параметр: {node.Member.Name}");
                    Visit(node.Expression);

                    // Пытаемся получить значение
                    try
                    {
                        var value = Expression.Lambda(node).Compile().DynamicInvoke();
                        AppendLine($"└─ Значение: {FormatValue(value)}");
                    }
                    catch
                    {
                        AppendLine("└─ Значение: [невозможно получить]");
                    }
                }
                return node;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                using (Indent())
                {
                    AppendLine($"┌─ Вызов метода: {node.Method.Name}");

                    if (node.Object != null)
                    {
                        AppendLine("├─ Объект:");
                        Visit(node.Object);
                    }

                    if (node.Arguments.Any())
                    {
                        AppendLine("├─ Аргументы:");
                        using (Indent())
                        {
                            foreach (var arg in node.Arguments)
                            {
                                Visit(arg);
                            }
                        }
                    }

                    // Пытаемся получить результат
                    try
                    {
                        var result = Expression.Lambda(node).Compile().DynamicInvoke();
                        AppendLine($"└─ Результат: {FormatValue(result)}");
                    }
                    catch
                    {
                        AppendLine("└─ Результат: [исключение при вызове]");
                    }
                }
                return node;
            }

            protected override Expression VisitUnary(UnaryExpression node)
            {
                using (Indent())
                {
                    AppendLine($"┌─ Унарная операция: {GetUnaryOperatorText(node.NodeType)}");
                    AppendLine("├─ Операнд:");
                    Visit(node.Operand);

                    try
                    {
                        var result = Expression.Lambda(node).Compile().DynamicInvoke();
                        AppendLine($"└─ Результат: {FormatValue(result)}");
                    }
                    catch
                    {
                        AppendLine("└─ Результат: [невозможно вычислить]");
                    }
                }
                return node;
            }

            protected override Expression VisitConditional(ConditionalExpression node)
            {
                using (Indent())
                {
                    AppendLine("┌─ Условная операция (if-then-else)");
                    AppendLine("├─ Условие:");
                    Visit(node.Test);
                    AppendLine("├─ Если True:");
                    Visit(node.IfTrue);
                    AppendLine("└─ Если False:");
                    Visit(node.IfFalse);
                }
                return node;
            }

            private string GetOperatorText(ExpressionType type)
            {
                switch (type)
                {
                    case ExpressionType.Add:
                        return "+";
                    case ExpressionType.Subtract:
                        return "-";
                    case ExpressionType.Multiply:
                        return "*";
                    case ExpressionType.Divide:
                        return "/";
                    case ExpressionType.Modulo:
                        return "%";
                    case ExpressionType.Equal:
                        return "==";
                    case ExpressionType.NotEqual:
                        return "!=";
                    case ExpressionType.GreaterThan:
                        return ">";
                    case ExpressionType.GreaterThanOrEqual:
                        return ">=";
                    case ExpressionType.LessThan:
                        return "<";
                    case ExpressionType.LessThanOrEqual:
                        return "<=";
                    case ExpressionType.AndAlso:
                        return "&&";
                    case ExpressionType.OrElse:
                        return "||";
                    case ExpressionType.And:
                        return "&";
                    case ExpressionType.Or:
                        return "|";
                    default:
                        return type.ToString();
                }
            }

            private string GetUnaryOperatorText(ExpressionType type)
            {
                switch (type)
                {
                    case ExpressionType.Not:
                        return "!";
                    case ExpressionType.Negate:
                        return "-";
                    case ExpressionType.UnaryPlus:
                        return "+";
                    default:
                        return type.ToString();
                }
            }

            private string FormatValue(object value)
            {
                if (value == null) return "null";
                if (value is string str) return $"\"{str}\"";
                if (value is IEnumerable enumerable)
                {
                    var items = enumerable.Cast<object>().Take(5).Select(FormatValue);
                    var count = enumerable.Cast<object>().Count();
                    var more = count > 5 ? ", ..." : "";
                    return $"[{string.Join(", ", items)}{more}]";
                }
                return value.ToString();
            }
        }

        private class DisposableAction : IDisposable
        {
            private readonly Action _action;
            public DisposableAction(Action action) => _action = action;
            public void Dispose() => _action();
        }

    }
}
