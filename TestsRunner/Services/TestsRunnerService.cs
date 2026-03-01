using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using TestsRunner.Models;
using TestsRunner.Models.Enums;
using TestingLibrary.Exceptions;

namespace TestsRunner.Services
{
    public static class TestRunnerService
    {
        private static async Task InvokeMethodAsync(MethodInfo method, object instance, object[] parameters = null)
        {
            if (method == null) return;

            try
            {
                var result = method.Invoke(instance, parameters ?? Array.Empty<object>());

                //Обработка асинхронных методов
                if (result != null)
                {
                    //Task / Task<T>
                    if (result is Task task)
                    {
                        await task;
                    }
                }
                //Для async void методов
                else if (method.ReturnType == typeof(void) &&
                         method.GetCustomAttribute<AsyncStateMachineAttribute>() != null)
                {
                    Debug.WriteLine($"Внимание: метод {method.Name} - async void, " +
                                   "рекомендуется использовать async Task");
                }
            }
            catch (TargetInvocationException ex)
            {
                //Разворачиваем исключение, чтобы получить оригинальное
                if (ex.InnerException != null)
                    throw ex.InnerException;
                throw;
            }
        }

        //Запуск одного тест-кейса (синхронно)
        public static void RunTestCase(TestCaseData testCase, MethodInfo method, TestClassModel classModel)
        {
            RunTestCaseAsync(testCase, method, classModel).GetAwaiter().GetResult();
        }

        //Запуск одного тест-кейса (асинхронно)
        public static async Task RunTestCaseAsync(TestCaseData testCase, MethodInfo method, TestClassModel classModel)
        {
            object instance = null;

            try
            {
                testCase.Status = TestStatus.Running;

                //Создаем экземпляр класса для теста
                instance = Activator.CreateInstance(classModel.ClassType);

                var stopwatch = Stopwatch.StartNew();

                try
                {
                    //Выполняем SetUp перед тестом
                    await InvokeMethodAsync(classModel.SetUpMethod, instance);

                    //Выполняем тест с параметрами
                    await InvokeMethodAsync(method, instance, testCase.Arguments);

                    testCase.Status = TestStatus.Passed;
                }
                catch (SuccessException)
                {
                    //Assert.Pass был вызван - тест успешен
                    testCase.Status = TestStatus.Passed;
                    Debug.WriteLine($"Тест {method.Name} успешно завершен через Assert.Pass");
                }
                catch (Exception ex)
                {
                    testCase.Status = TestStatus.Failed;
                    testCase.ErrorMessage = ex.InnerException?.Message ?? ex.Message;
                }
                finally
                {
                    //Выполняем TearDown после теста
                    try { await InvokeMethodAsync(classModel.TearDownMethod, instance); } catch { }

                    stopwatch.Stop();
                    testCase.Duration = stopwatch.Elapsed;
                }
            }
            catch (Exception ex)
            {
                testCase.Status = TestStatus.Failed;
                testCase.ErrorMessage = ex.Message;
            }
        }

        //Запуск параметризованного метода (асинхронно)
        public static async Task RunParameterizedMethodAsync(TestMethodModel testMethod, TestClassModel classModel)
        {
            foreach (var testCase in testMethod.TestCases)
            {
                await RunTestCaseAsync(testCase, testMethod.MethodInfo, classModel);
            }
        }

        //Запуск одного тестового метода (синхронно)
        public static void RunTest(TestMethodModel testMethod, TestClassModel classModel)
        {
            RunTestAsync(testMethod, classModel).GetAwaiter().GetResult();
        }

        //Запуск одного тестового метода (асинхронно)
        public static async Task RunTestAsync(TestMethodModel testMethod, TestClassModel classModel)
        {
            object instance = null;

            try
            {
                testMethod.Status = TestStatus.Running;

                //Создаем экземпляр класса для теста
                instance = Activator.CreateInstance(classModel.ClassType);

                var stopwatch = Stopwatch.StartNew();

                try
                {
                    //Выполняем SetUp перед тестом
                    await InvokeMethodAsync(classModel.SetUpMethod, instance);

                    // Выполняем тест
                    await InvokeMethodAsync(testMethod.MethodInfo, instance);

                    testMethod.Status = TestStatus.Passed;
                }
                catch (SuccessException)
                {
                    //Assert.Pass был вызван - тест успешен
                    testMethod.Status = TestStatus.Passed;
                    Debug.WriteLine($"Тест {testMethod.MethodName} успешно завершен через Assert.Pass");
                }
                catch (Exception ex)
                {
                    testMethod.Status = TestStatus.Failed;
                    testMethod.ErrorMessage = ex.InnerException?.Message ?? ex.Message;
                }
                finally
                {
                    //Выполняем TearDown после теста
                    try { await InvokeMethodAsync(classModel.TearDownMethod, instance); } catch { }

                    stopwatch.Stop();
                    testMethod.Duration = stopwatch.Elapsed;
                }
            }
            catch (Exception ex)
            {
                testMethod.Status = TestStatus.Failed;
                testMethod.ErrorMessage = ex.Message;
            }
        }

        public static void RunClassTests(TestClassModel classModel)
        {
            RunClassTestsAsync(classModel).GetAwaiter().GetResult();
        }

        public static async Task RunClassTestsAsync(TestClassModel classModel)
        {
            object fixtureInstance = null;

            try
            {
                // TestFixtureSetUp
                if (classModel.FixtureSetUpMethod != null)
                {
                    fixtureInstance = classModel.FixtureSetUpMethod.IsStatic
                        ? null
                        : Activator.CreateInstance(classModel.ClassType);

                    try
                    {
                        await InvokeMethodAsync(classModel.FixtureSetUpMethod, fixtureInstance);
                    }
                    catch (SuccessException)
                    {
                        // TestFixtureSetUp может тоже содержать Assert.Pass
                        Debug.WriteLine("TestFixtureSetUp успешно завершен через Assert.Pass");
                    }
                }

                // Запускаем методы
                foreach (var method in classModel.Methods)
                {
                    if (method.IsParameterized)
                    {
                        await RunParameterizedMethodAsync(method, classModel);
                    }
                    else
                    {
                        await RunTestAsync(method, classModel);
                    }
                }

                // Вложенные классы
                foreach (var nestedClass in classModel.NestedClasses)
                {
                    await RunClassTestsAsync(nestedClass);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка в TestFixtureSetUp: {ex.Message}");
            }
            finally
            {
                // TestFixtureTearDown
                try
                {
                    if (classModel.FixtureTearDownMethod != null)
                    {
                        var tearDownInstance = classModel.FixtureTearDownMethod.IsStatic ? null : fixtureInstance;

                        await InvokeMethodAsync(classModel.FixtureTearDownMethod, tearDownInstance);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка в TestFixtureTearDown: {ex.Message}");
                }
            }
        }

        public static void RunAssemblyTests(TestAssemblyModel assemblyModel)
        {
            RunAssemblyTestsAsync(assemblyModel).GetAwaiter().GetResult();
        }

        public static async Task RunAssemblyTestsAsync(TestAssemblyModel assemblyModel)
        {
            foreach (var classModel in assemblyModel.Classes)
            {
                await RunClassTestsAsync(classModel);
            }
        }
    }
}