using System;

namespace Task10.Models
{
    /// <summary>
    /// Базовый класс для вычисления интеграла с заданной точностью
    /// </summary>
    public abstract class IntegralCalculatorBase
    {
        //Начальное количество сегментов
        private const int INITIAL_SEGMENTS = 4;

        /// <summary>
        /// Вычислить интеграл с заданной точностью (шаблонный метод)
        /// </summary>
        /// <param name="function">Подынтегральная функция</param>
        /// <param name="startX">Нижний предел интегрирования</param>
        /// <param name="finishX">Верхний предел интегрирования</param>
        /// <param name="accuracy">Точность вычисления</param>
        /// <returns>Численное значение интеграла</returns>
        public double Calculate(Func<double, double> function, double startX, double finishX, double accuracy)
        {
            //Точность вычислений должна быть положительной
            if (accuracy <= 0)
            {
                Console.WriteLine("Точность вычислений должна быть положительной.");

                return double.NaN;
            }

            try
            {
                //Получаем начальное количество сегментов
                int segmentsAmount = GetInitialSegments();

                //Инициализация переменных для предыдущего и текущего значений интегралов
                double previousResult = 0;
                double currentResult = ComputeIntegral(function, startX, finishX, segmentsAmount);

                //Цикл вычисления интеграла до определенной точности
                do
                {
                    previousResult = currentResult;

                    //Увеличиваем число сегментов и считаем новое значение интеграла
                    segmentsAmount = UpdateSegments(segmentsAmount);
                    currentResult = ComputeIntegral(function, startX, finishX, segmentsAmount);

                } while (Math.Abs(currentResult - previousResult) > accuracy);

                return currentResult;
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine(ex.Message);

                return double.NaN;
            }
        }

        /// <summary>
        /// Изменить количество сегментов интегрирования
        /// </summary>
        /// <param name="currentSegments">Текущее количество сегментов</param>
        /// <returns>Новое количество сегментов</returns>
        protected abstract int UpdateSegments(int currentSegments);

        /// <summary>
        /// Вычислить интеграл определенным методом
        /// </summary>
        /// <param name="function">Подынтегральная функция</param>
        /// <param name="startX">Нижний предел интегрирования</param>
        /// <param name="finishX">Верхний предел интегрирования</param>
        /// <param name="segmentsAmount">Количество сегментов интегрирования</param>
        /// <returns>Численное значение интеграла</returns>
        protected abstract double ComputeIntegral(Func<double, double> function, double startX, double finishX, int segmentsAmount);

        /// <summary>
        /// Получить начальное количество сегментов интегрирования
        /// </summary>
        /// <returns>Количество сегментов интегрирования</returns>
        protected virtual int GetInitialSegments()
        {
            return INITIAL_SEGMENTS;
        }
    }
}