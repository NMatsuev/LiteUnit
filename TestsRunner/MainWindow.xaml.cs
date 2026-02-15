using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;

namespace TestsRunner
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            try
            {
                // Загружаем сборку с тестами по пути
                string testAssemblyPath = @"TestingProject.dll";
                Assembly testAssembly = Assembly.LoadFrom(testAssemblyPath);

                // Загружаем сборку с атрибутами (если она отдельно)
                string libraryPath = @"TestingLibrary.dll";
                Assembly libraryAssembly = Assembly.LoadFrom(libraryPath);

                // Получаем тип TestAttribute из загруженной сборки
                Type testAttributeType = libraryAssembly.GetType("TestingLibrary.TestAttribute");

                List<MethodInfo> methods = GetTestMethods(testAssembly, testAttributeType);

                foreach (MethodInfo method in methods)
                {
                    RunTestMethod(method);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private List<MethodInfo> GetTestMethods(Assembly assembly, Type testAttributeType)
        {
            List<MethodInfo> testMethods = new List<MethodInfo>();

            foreach (Type type in assembly.GetExportedTypes())
            {
                foreach (MethodInfo method in type.GetMethods(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                    BindingFlags.Instance)) // Добавил Instance на всякий случай
                {
                    // Проверяем наличие атрибута через рефлексию
                    var attributes = method.GetCustomAttributes(testAttributeType, false);
                    if (attributes != null && attributes.Length > 0)
                    {
                        testMethods.Add(method);
                    }
                }
            }

            return testMethods;
        }

        private void RunTestMethod(MethodInfo method)
        {
            try
            {
                object instance = null;

                // Если метод не статический, создаем экземпляр класса
                if (!method.IsStatic)
                {
                    instance = Activator.CreateInstance(method.DeclaringType);
                }

                // Вызываем метод
                method.Invoke(instance, null);

                // Можно добавить логирование успеха
                MessageBox.Show($"✓ {method.DeclaringType.Name}.{method.Name} - OK");
            }
            catch (Exception ex)
            {
                // Логируем ошибку
                MessageBox.Show($"✗ {method.DeclaringType.Name}.{method.Name} - FAILED: {ex.InnerException?.Message}");
            }
        }
    }
}