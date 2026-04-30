using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using TestingLibrary.Exceptions;
using TestingLibrary.Services;

namespace TestingLibrary
{
    public static class Assert
    {
        //Проверка на равенство
        public static void AreEqual<T>(T expected, T actual, string message = "")
        {
            if (!Equals(expected, actual))
            {
                throw new AssertionException(FormatMessage(message, $"Ожидалось: {expected}, но получено: {actual}"));
            }
        }

        //Проверка на равенство для double с указанием точности
        public static void AreEqual(double expected, double actual, double delta, string message = "")
        {
            if (Math.Abs(expected - actual) > delta)
            {
                throw new AssertionException(FormatMessage(message,
                    $"Ожидалось: {expected} с точностью ±{delta}, но получено: {actual}. Разница: {Math.Abs(expected - actual)}"));
            }
        }

        //Проверка на равенство для double с указанием точности
        public static void AreEqual(float expected, float actual, float delta, string message = "")
        {
            if (Math.Abs(expected - actual) > delta)
            {
                throw new AssertionException(FormatMessage(message,
                    $"Ожидалось: {expected} с точностью ±{delta}, но получено: {actual}. Разница: {Math.Abs(expected - actual)}"));
            }
        }

        //Проверка на неравенство для double с указанием точности
        public static void AreNotEqual(double expected, double actual, double delta, string message = "")
        {
            if (Math.Abs(expected - actual) <= delta)
            {
                throw new AssertionException(FormatMessage(message,
                    $"Ожидалось: не {expected} с точностью ±{delta}, но получено: {actual}. Разница: {Math.Abs(expected - actual)}"));
            }
        }

        //Проверка на неравенство для double с указанием точности
        public static void AreNotEqual(float expected, float actual, float delta, string message = "")
        {
            if (Math.Abs(expected - actual) <= delta)
            {
                throw new AssertionException(FormatMessage(message,
                    $"Ожидалось: не {expected} с точностью ±{delta}, но получено: {actual}. Разница: {Math.Abs(expected - actual)}"));
            }
        }

        //Проверка на неравенство
        public static void AreNotEqual<T>(T expected, T actual, string message = "")
        {
            if (Equals(expected, actual))
            {
                throw new AssertionException(FormatMessage(message, $"Ожидалось не: {expected}, но получено: {actual}"));
            }
        }

        //Проверка на идентичность ссылок
        public static void AreSame(object expected, object actual, string message = "")
        {
            if (!ReferenceEquals(expected, actual))
            {
                throw new AssertionException(FormatMessage(message, "Объекты не ссылаются на один и тот же экземпляр"));
            }
        }

        //Проверка что ссылки разные
        public static void AreNotSame(object expected, object actual, string message = "")
        {
            if (ReferenceEquals(expected, actual))
            {
                throw new AssertionException(FormatMessage(message, "Объекты ссылаются на один и тот же экземпляр"));
            }
        }

        //Проверка на истинность
        public static void IsTrue(bool condition, string message = "")
        {
            if (!condition)
            {
                throw new AssertionException(FormatMessage(message, "Ожидалось True, но получено False"));
            }
        }

        //Проверка на ложность
        public static void IsFalse(bool condition, string message = "")
        {
            if (condition)
            {
                throw new AssertionException(FormatMessage(message, "Ожидалось False, но получено True"));
            }
        }

        //Проверка на null
        public static void IsNull(object obj, string message = "")
        {
            if (obj != null)
            {
                throw new AssertionException(FormatMessage(message, $"Ожидалось null, но получено: {obj}"));
            }
        }

        //Проверка что не null
        public static void IsNotNull(object obj, string message = "")
        {
            if (obj == null)
            {
                throw new AssertionException(FormatMessage(message, "Ожидалось не null, но получено null"));
            }
        }

        //Проверка что объект является экземпляром типа
        public static void IsInstanceOf<TExpected>(object obj, string message = "")
        {
            if (!(obj is TExpected))
            {
                throw new AssertionException(FormatMessage(message,
                    $"Ожидался тип: {typeof(TExpected).Name}, но получен: {obj?.GetType().Name ?? "null"}"));
            }
        }

        //Проверка что объект НЕ является экземпляром типа
        public static void IsNotInstanceOf<TNotExpected>(object obj, string message = "")
        {
            if (obj is TNotExpected)
            {
                throw new AssertionException(FormatMessage(message,
                    $"Объект не должен быть типом: {typeof(TNotExpected).Name}"));
            }
        }

        //Проверка что метод/действие выбрасывает исключение
        public static TException Catch<TException>(Action action, string message = "") where TException : Exception
        {
            try
            {
                action();
            }
            catch (TException ex)
            {
                return ex;
            }
            catch (Exception ex)
            {
                throw new AssertionException(FormatMessage(message,
                    $"Ожидалось исключение: {typeof(TException).Name}, но получено: {ex.GetType().Name}"));
            }

            throw new AssertionException(FormatMessage(message,
                $"Ожидалось исключение: {typeof(TException).Name}, но исключение не было выброшено"));
        }

        //Проверка что метод/действие выбрасывает любое исключение
        public static Exception Catch(Action action, string message = "")
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                return ex;
            }

            throw new AssertionException(FormatMessage(message, "Ожидалось исключение, но оно не было выброшено"));
        }

        //Проверка что метод/действие НЕ выбрасывает исключение
        public static void DoesNotThrow(Action action, string message = "")
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                throw new AssertionException(FormatMessage(message,
                    $"Не ожидалось исключение, но получено: {ex.GetType().Name}"));
            }
        }

        //Принудительное падение теста
        public static void Fail(string message = "")
        {
            throw new AssertionException(FormatMessage(message, "Тест намеренно провален"));
        }

        //Успешное завершение теста
        public static void Pass(string message = "")
        {
            throw new SuccessException(FormatMessage(message, "Тест успешно завершен"));
        }

        //Проверка на вхождение в коллекцию
        public static void Contains(object expected, System.Collections.ICollection collection, string message = "")
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            foreach (var item in collection)
            {
                if (Equals(item, expected))
                    return;
            }

            throw new AssertionException(FormatMessage(message,
                $"Коллекция не содержит элемент: {expected}"));
        }

        //Проверка на пустую коллекцию
        public static void IsEmpty(System.Collections.ICollection collection, string message = "")
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (collection.Count > 0)
            {
                throw new AssertionException(FormatMessage(message,
                    $"Коллекция не пуста. Содержит {collection.Count} элемент(ов)"));
            }
        }

        //Проверка что коллекция не пуста
        public static void IsNotEmpty(System.Collections.ICollection collection, string message = "")
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (collection.Count == 0)
            {
                throw new AssertionException(FormatMessage(message, "Коллекция пуста"));
            }
        }

        public static void IsTrue(Expression<Func<bool>> expression, string message = "")
        {
            var compiled = expression.Compile();
            var result = compiled();

            if (!result)
            {
                var details = ExpressionTreeAnalyzer.Analyze(expression);
                throw new AssertionException(FormatMessage(message,
                    $"Выражение вернуло False.\n{details}"));
            }
        }

        /// <summary>
        /// Проверяет, что выражение ложно. При сбое выводит детальный разбор дерева выражения.
        /// </summary>
        public static void IsFalse(Expression<Func<bool>> expression, string message = "")
        {
            var compiled = expression.Compile();
            var result = compiled();

            if (result)
            {
                var details = ExpressionTreeAnalyzer.Analyze(expression);
                throw new AssertionException(FormatMessage(message,
                    $"Выражение вернуло True, ожидалось False.\n{details}"));
            }
        }

        /// <summary>
        /// Проверяет, что два выражения равны. При сбое выводит детальный разбор.
        /// </summary>
        public static void AreEqual<T>(
            Expression<Func<T>> expectedExpression,
            Expression<Func<T>> actualExpression,
            string message = "")
        {
            var expectedValue = expectedExpression.Compile()();
            var actualValue = actualExpression.Compile()();

            if (!Equals(expectedValue, actualValue))
            {
                var expectedDetails = ExpressionTreeAnalyzer.Analyze(expectedExpression);
                var actualDetails = ExpressionTreeAnalyzer.Analyze(actualExpression);

                throw new AssertionException(FormatMessage(message,
                    $"Значения не равны.\n" +
                    $"Ожидаемое выражение: {expectedDetails}\n" +
                    $"Фактическое выражение: {actualDetails}\n" +
                    $"Ожидалось: {expectedValue}, получено: {actualValue}"));
            }
        }

        /// <summary>
        /// Проверяет условие с параметрами. Позволяет переиспользовать выражения.
        /// </summary>
        public static void IsTrue<T>(Expression<Func<T, bool>> expression, T parameter, string message = "")
        {
            var compiled = expression.Compile();
            var result = compiled(parameter);

            if (!result)
            {
                var details = ExpressionTreeAnalyzer.Analyze(expression, parameter);
                throw new AssertionException(FormatMessage(message,
                    $"Выражение вернуло False для параметра: {parameter}\n{details}"));
            }
        }


        //Вспомогательный метод для форматирования сообщения
        private static string FormatMessage(string userMessage, string defaultMessage)
        {
            return string.IsNullOrEmpty(userMessage) ? defaultMessage : $"{userMessage}: {defaultMessage}";
        }
    }
}