using System;

namespace Task10.Models
{
    /// <summary>
    /// Класс для вычисления интеграла с заданной точностью методом трапеций
    /// </summary>
    public class TrapezoidIntegralCalculator:IntegralCalculatorBase
    {
        //Приращение количества сегментов
        private const int SEGMENTS_INCREASE = 10;

        /// <summary>
        /// Изменить количество сегментов интегрирования
        /// </summary>
        /// <param name="currentSegments">Текущее количество сегментов</param>
        /// <returns>Новое количество сегментов</returns>
        protected override int UpdateSegments(int currentSegments)
        {
            return currentSegments + SEGMENTS_INCREASE;
        }

        /// <summary>
        /// Вычислить интеграл определенным методом
        /// </summary>
        /// <param name="function">Подынтегральная функция</param>
        /// <param name="startX">Нижний предел интегрирования</param>
        /// <param name="finishX">Верхний предел интегрирования</param>
        /// <param name="segmentsAmount">Количество сегментов интегрирования</param>
        /// <returns>Численное значение интеграла</returns>
        /// <exception cref="ArgumentException">Неверные пределы интегрирования или неверное количество сегментов</exception>
        protected override double ComputeIntegral(Func<double, double> function, 
                                                  double startX, 
                                                  double finishX, 
                                                  int segmentsAmount)
        {
            //Нижний предел интегрирования должен быть не больше верхнего
            if (startX > finishX)
                throw new ArgumentException($"Неверный предел интегрирования: [{startX}, {finishX}]");

            //Количество сегментов должно быть положительным
            if (segmentsAmount < 0)
                throw new ArgumentException($"Количество сегментов должно быть положительным");

            //Инициализация шага интегрирования и суммы значений функции
            double step = (finishX - startX) / segmentsAmount;
            double sum = (function(startX) + function(finishX)) / 2;

            //Цикл расчета суммы значений функции
            for (int i = 1; i < segmentsAmount; i++)
            {
                //Находим новый аргумент
                double x = startX + i * step;

                //Если функция в точке не определена
                if (double.IsNaN(function(x)))
                    throw new ArgumentException($"Функция не определена на всем промежутке [{startX}, {finishX}]");

                //Добавляем в общую сумму значений функции
                sum += function(x);
            }

            return sum * step;
        }
    }
}
