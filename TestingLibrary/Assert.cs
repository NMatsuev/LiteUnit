using System;
using TestingLibrary.Exceptions;

namespace TestingLibrary
{
    public static class Assert
    {
        // Проверка на равенство
        public static void AreEqual<T>(T expected, T actual, string message = "")
        {
            if (!Equals(expected, actual))
            {
                throw new AssertionException(FormatMessage(message, $"Ожидалось: {expected}, но получено: {actual}"));
            }
        }

        // Проверка на неравенство
        public static void AreNotEqual<T>(T expected, T actual, string message = "")
        {
            if (Equals(expected, actual))
            {
                throw new AssertionException(FormatMessage(message, $"Ожидалось не: {expected}, но получено: {actual}"));
            }
        }

        // Проверка на идентичность ссылок
        public static void AreSame(object expected, object actual, string message = "")
        {
            if (!ReferenceEquals(expected, actual))
            {
                throw new AssertionException(FormatMessage(message, "Объекты не ссылаются на один и тот же экземпляр"));
            }
        }

        // Проверка что ссылки разные
        public static void AreNotSame(object expected, object actual, string message = "")
        {
            if (ReferenceEquals(expected, actual))
            {
                throw new AssertionException(FormatMessage(message, "Объекты ссылаются на один и тот же экземпляр"));
            }
        }

        // Проверка на истинность
        public static void IsTrue(bool condition, string message = "")
        {
            if (!condition)
            {
                throw new AssertionException(FormatMessage(message, "Ожидалось True, но получено False"));
            }
        }

        // Проверка на ложность
        public static void IsFalse(bool condition, string message = "")
        {
            if (condition)
            {
                throw new AssertionException(FormatMessage(message, "Ожидалось False, но получено True"));
            }
        }

        // Проверка на null
        public static void IsNull(object obj, string message = "")
        {
            if (obj != null)
            {
                throw new AssertionException(FormatMessage(message, $"Ожидалось null, но получено: {obj}"));
            }
        }

        // Проверка что не null
        public static void IsNotNull(object obj, string message = "")
        {
            if (obj == null)
            {
                throw new AssertionException(FormatMessage(message, "Ожидалось не null, но получено null"));
            }
        }

        // Проверка что объект является экземпляром типа
        public static void IsInstanceOf<TExpected>(object obj, string message = "")
        {
            if (!(obj is TExpected))
            {
                throw new AssertionException(FormatMessage(message,
                    $"Ожидался тип: {typeof(TExpected).Name}, но получен: {obj?.GetType().Name ?? "null"}"));
            }
        }

        // Проверка что объект НЕ является экземпляром типа
        public static void IsNotInstanceOf<TNotExpected>(object obj, string message = "")
        {
            if (obj is TNotExpected)
            {
                throw new AssertionException(FormatMessage(message,
                    $"Объект не должен быть типом: {typeof(TNotExpected).Name}"));
            }
        }

        // Проверка что метод/действие выбрасывает исключение
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

        // Проверка что метод/действие выбрасывает любое исключение
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

        // Проверка что метод/действие НЕ выбрасывает исключение
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

        // Принудительное падение теста
        public static void Fail(string message = "")
        {
            throw new AssertionException(FormatMessage(message, "Тест намеренно провален"));
        }

        // Успешное завершение теста
        public static void Pass(string message = "")
        {
            throw new SuccessException(FormatMessage(message, "Тест успешно завершен"));
        }

        // Проверка на вхождение в коллекцию
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

        // Проверка на пустую коллекцию
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

        // Проверка что коллекция не пуста
        public static void IsNotEmpty(System.Collections.ICollection collection, string message = "")
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));

            if (collection.Count == 0)
            {
                throw new AssertionException(FormatMessage(message, "Коллекция пуста"));
            }
        }

        // Вспомогательный метод для форматирования сообщения
        private static string FormatMessage(string userMessage, string defaultMessage)
        {
            return string.IsNullOrEmpty(userMessage) ? defaultMessage : $"{userMessage}: {defaultMessage}";
        }
    }
}