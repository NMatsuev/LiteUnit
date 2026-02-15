using System;
using Task10.Models;

namespace TestedProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //Пределы интегрирования
            const double startX = 1;
            const double finishX = 9;

            //Точность вычисления
            const double accuracy = 0.00001;

            //Подынтегральная функция
            Func<double, double> f = x => Math.Sqrt(x);

            LeftRectangleIntegralCalculator leftRectangleIntegralCalculator = new LeftRectangleIntegralCalculator();
            TrapezoidIntegralCalculator trapezoidIntegralCalculator = new TrapezoidIntegralCalculator();
            Console.WriteLine($"Интеграл в пределах от {startX} до {finishX} равен:\n" +
                              $"Метод левых прямоугольников: {leftRectangleIntegralCalculator.Calculate(f, startX, finishX, accuracy):F3}\n" +
                              $"Метод трапеций: {trapezoidIntegralCalculator.Calculate(f, startX, finishX, accuracy):F3}");
        }
    }
}
