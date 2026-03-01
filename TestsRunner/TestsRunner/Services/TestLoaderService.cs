using System.Reflection;
using TestsRunner.Models;

namespace TestsRunner.Services
{
    public static class TestLoaderService
    {
        public static TestAssemblyModel LoadAssembly(string assemblyPath)
        {
            try
            {
                var assembly = Assembly.LoadFrom(assemblyPath);
                var assemblyModel = new TestAssemblyModel(assemblyPath);

                var types = assembly.GetTypes()
                    .Where(t => t.GetCustomAttributes().Any(a => a.GetType().Name == "TestFixtureAttribute"))
                    .ToList();

                foreach (var type in types)
                {
                    var classModel = LoadClass(type);
                    assemblyModel.Classes.Add(classModel);
                }

                return assemblyModel;
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка загрузки сборки: {ex.Message}");
            }
        }

        private static TestClassModel LoadClass(Type type)
        {
            var classModel = new TestClassModel
            {
                ClassType = type
            };

            // Загружаем специальные методы
            classModel.SetUpMethod = GetMethodByAttribute(type, "SetUpAttribute");
            classModel.TearDownMethod = GetMethodByAttribute(type, "TearDownAttribute");
            classModel.FixtureSetUpMethod = GetMethodByAttribute(type, "TestFixtureSetUpAttribute");
            classModel.FixtureTearDownMethod = GetMethodByAttribute(type, "TestFixtureTearDownAttribute");

            // Загружаем тестовые методы
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var method in methods)
            {
                // Проверяем атрибут Test
                var testAttr = method.GetCustomAttributes().FirstOrDefault(a => a.GetType().Name == "TestAttribute");
                if (testAttr != null)
                {
                    // Проверяем атрибут Ignore
                    var ignoreAttr = method.GetCustomAttributes().FirstOrDefault(a => a.GetType().Name == "IgnoreAttribute");

                    if (ignoreAttr == null) // Не добавляем игнорируемые методы
                    {
                        var methodModel = new TestMethodModel
                        {
                            MethodInfo = method
                        };

                        // Загружаем категории
                        var categoryAttrs = method.GetCustomAttributes()
                            .Where(a => a.GetType().Name == "CategoryAttribute" || a.GetType().Name == "TestCategoryAttribute")
                            .ToList();

                        if (categoryAttrs.Any())
                        {
                            methodModel.Categories = categoryAttrs
                                .Select(c => c.GetType().GetProperty("Name")?.GetValue(c)?.ToString() ?? "Unknown")
                                .ToArray();
                        }

                        classModel.Methods.Add(methodModel);
                    }
                }
            }

            // Загружаем вложенные классы с атрибутом TestFixture
            var nestedTypes = type.GetNestedTypes(BindingFlags.Public)
                .Where(t => t.GetCustomAttributes().Any(a => a.GetType().Name == "TestFixtureAttribute"));

            foreach (var nestedType in nestedTypes)
            {
                var nestedClassModel = LoadClass(nestedType);
                classModel.NestedClasses.Add(nestedClassModel);
            }

            return classModel;
        }

        private static MethodInfo GetMethodByAttribute(Type type, string attributeName)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .FirstOrDefault(m => m.GetCustomAttributes().Any(a => a.GetType().Name == attributeName));
        }
    }
}