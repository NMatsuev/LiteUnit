using System.Diagnostics;
using System.IO;
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

            //Загружаем специальные методы
            classModel.SetUpMethod = GetMethodByAttribute(type, "SetUpAttribute");
            classModel.TearDownMethod = GetMethodByAttribute(type, "TearDownAttribute");
            classModel.FixtureSetUpMethod = GetMethodByAttribute(type, "TestFixtureSetUpAttribute");
            classModel.FixtureTearDownMethod = GetMethodByAttribute(type, "TestFixtureTearDownAttribute");

            //Загружаем тестовые методы
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

            foreach (var method in methods)
            {
                //Получаем все атрибуты метода
                var attributes = method.GetCustomAttributes().ToList();

                //Проверяем наличие Test атрибута
                var testAttr = attributes.FirstOrDefault(a => a.GetType().Name == "TestAttribute" || a.GetType().Name == "TestCaseAttribute");
                if (testAttr != null)
                {
                    //Проверяем на Ignore
                    var ignoreAttr = attributes.FirstOrDefault(a => a.GetType().Name == "IgnoreAttribute");
                    if (ignoreAttr != null) continue; //Пропускаем игнорируемые методы

                    var methodModel = new TestMethodModel
                    {
                        MethodInfo = method
                    };

                    //Получаем категории
                    var categoryAttrs = attributes
                        .Where(a => a.GetType().Name == "CategoryAttribute")
                        .ToList();

                    if (categoryAttrs.Any())
                    {
                        methodModel.Categories = categoryAttrs
                            .Select(c => c.GetType().GetProperty("Category")?.GetValue(c)?.ToString() ?? "Unknown")
                            .ToArray();
                    }

                    //Получаем все TestCase атрибуты
                    var testCaseAttrs = attributes
                        .Where(a => a.GetType().Name == "TestCaseAttribute")
                        .ToList();

                    if (testCaseAttrs.Any())
                    {
                        foreach (var testCaseAttr in testCaseAttrs)
                        {
                            var testCase = CreateTestCaseFromAttribute(testCaseAttr, method);
                            if (testCase != null)
                            {
                                methodModel.TestCases.Add(testCase);
                            }
                        }
                    }

                    //Для обычных тестов TestCases остается пустым

                    classModel.Methods.Add(methodModel);
                }
            }

            //Загружаем вложенные классы
            var nestedTypes = type.GetNestedTypes(BindingFlags.Public)
                .Where(t => t.GetCustomAttributes().Any(a => a.GetType().Name == "TestFixtureAttribute"));

            foreach (var nestedType in nestedTypes)
            {
                var nestedClassModel = LoadClass(nestedType);
                classModel.NestedClasses.Add(nestedClassModel);
            }

            return classModel;
        }

        private static TestCaseData CreateTestCaseFromAttribute(object attribute, MethodInfo method)
        {
            try
            {
                var testCaseData = new TestCaseData();
                var type = attribute.GetType();

                //Получаем аргументы из конструктора атрибута
                var argumentsField = type.GetField("_testParams", BindingFlags.NonPublic | BindingFlags.Instance);
                if (argumentsField != null)
                {
                    testCaseData.Arguments = argumentsField.GetValue(attribute) as object[] ?? Array.Empty<object>();
                }
                else
                {
                    var argumentsProp = type.GetProperty("TestParams");
                    if (argumentsProp != null)
                    {
                        testCaseData.Arguments = argumentsProp.GetValue(attribute) as object[] ?? Array.Empty<object>();
                    }
                }

                //Получаем TestName если есть
                var testNameProp = type.GetProperty("TestName");
                if (testNameProp != null)
                {
                    testCaseData.DisplayName = testNameProp.GetValue(attribute)?.ToString();
                }

                return testCaseData;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка создания TestCase: {ex.Message}");
                return null;
            }
        }

        private static MethodInfo GetMethodByAttribute(Type type, string attributeName)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .FirstOrDefault(m => m.GetCustomAttributes().Any(a => a.GetType().Name == attributeName));
        }
    }
}