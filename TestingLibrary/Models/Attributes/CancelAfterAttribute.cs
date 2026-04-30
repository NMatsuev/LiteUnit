using System;

namespace TestingLibrary.Attributes
{
    /// <summary>
    /// Атрибут для установки таймаута выполнения теста (в миллисекундах)
    /// После истечения времени тест будет отменен
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class CancelAfterAttribute : Attribute
    {
        /// <summary>
        /// Таймаут в миллисекундах
        /// </summary>
        public int Timeout { get; }

        /// <summary>
        /// Конструктор атрибута
        /// </summary>
        /// <param name="timeout">Таймаут в миллисекундах</param>
        public CancelAfterAttribute(int timeout)
        {
            Timeout = timeout;
        }
    }
}