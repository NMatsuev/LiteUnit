using System;
using Task10.Models;
using TestedProject.Parser;

namespace TestedProject
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Программа вычисления определенного интеграла ===");
            Console.WriteLine();

            try
            {
                // Ввод функции
                Console.WriteLine("Введите функцию от x (доступны: +, -, *, /, sin, cos, tg, ctg, pow, скобки)");
                Console.WriteLine("Примеры: 2*x + sin(x), pow(x,2) + cos(x), x*tg(x)");
                Console.Write("f(x) = ");
                string functionInput = Console.ReadLine();

                // Парсинг функции
                var parser = new MathExpressionParser(functionInput);
                Func<double, double> function = parser.Parse();
                Console.WriteLine("Функция успешно распознана!");
                Console.WriteLine();

                // Ввод пределов интегрирования
                Console.Write("Введите нижний предел интегрирования a = ");
                double startX = double.Parse(Console.ReadLine());

                Console.Write("Введите верхний предел интегрирования b = ");
                double finishX = double.Parse(Console.ReadLine());

                // Ввод точности
                Console.Write("Введите точность вычисления (например, 0.00001) = ");
                double accuracy = double.Parse(Console.ReadLine());

                Console.WriteLine();
                Console.WriteLine($"Вычисление интеграла ∫ от {startX} до {finishX} функции:");
                Console.WriteLine($"f(x) = {functionInput}");
                Console.WriteLine($"с точностью {accuracy}");
                Console.WriteLine(new string('-', 50));

                // Вычисление интегралов
                var leftRectangleCalculator = new LeftRectangleIntegralCalculator();
                var trapezoidCalculator = new TrapezoidIntegralCalculator();

                double leftResult = leftRectangleCalculator.Calculate(function, startX, finishX, accuracy);
                double trapezoidResult = trapezoidCalculator.Calculate(function, startX, finishX, accuracy);

                Console.WriteLine($"Метод левых прямоугольников: {leftResult:F10}");
                Console.WriteLine($"Метод трапеций: {trapezoidResult:F10}");
                Console.WriteLine($"Разность методов: {Math.Abs(leftResult - trapezoidResult):F10}");

                // Проверка на возможную сходимость
                if (Math.Abs(leftResult - trapezoidResult) < accuracy)
                {
                    Console.WriteLine("Результаты сошлись с заданной точностью!");
                }
                else
                {
                    Console.WriteLine("Результаты отличаются больше заданной точности. Возможно, требуется увеличить количество итераций.");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Ошибка: Неверный формат числа!");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Ошибка в выражении: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Неожиданная ошибка: {ex.Message}");
            }

            Console.WriteLine();
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}