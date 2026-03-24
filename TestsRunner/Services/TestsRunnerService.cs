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
        // Универсальный метод для вызова метода с поддержкой отмены
        private static async Task InvokeMethodAsync(MethodInfo method, object instance, object[] parameters, CancellationToken cancellationToken)
        {
            if (method == null) return;

            try
            {
                // Подготавливаем параметры с учетом CancellationToken
                var preparedParams = PrepareMethodParameters(method, parameters, cancellationToken);
                var result = method.Invoke(instance, preparedParams);

                //Проверка для синхронных методов
                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException(
                        $"Метод {method.Name} отменен по таймауту (синхронный метод)",
                        cancellationToken);
                }

                // Обработка асинхронных методов
                if (result != null)
                {
                    if (result is Task task)
                    {
                        // Используем Task.WhenAny для отслеживания отмены
                        var completedTask = await Task.WhenAny(task,
                            Task.Delay(-1, cancellationToken).ContinueWith(t => { }, cancellationToken));

                        if (completedTask == task)
                        {
                            await task;
                        }
                        else
                        {
                            throw new OperationCanceledException(
                                $"Метод {method.Name} отменен по таймауту", cancellationToken);
                        }
                    }
                    else if (result.GetType().IsGenericType &&
                             result.GetType().GetGenericTypeDefinition() == typeof(ValueTask<>))
                    {
                        dynamic dynamicResult = result;
                        await dynamicResult.AsTask();
                    }
                    else if (result is ValueTask valueTask)
                    {
                        await valueTask.AsTask();
                    }
                }
                else if (method.ReturnType == typeof(void) &&
                         method.GetCustomAttribute<AsyncStateMachineAttribute>() != null)
                {
                    Debug.WriteLine($"Внимание: метод {method.Name} - async void, отмена не поддерживается");
                }
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null) throw ex.InnerException;
                throw;
            }
        }

        private static object[] PrepareMethodParameters(MethodInfo method, object[] providedParams, CancellationToken token)
        {
            if (method == null) return null;

            var parameters = method.GetParameters();
            if (parameters.Length == 0) return null;

            var result = new object[parameters.Length];
            int providedIndex = 0;

            for (int i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].ParameterType == typeof(CancellationToken))
                {
                    result[i] = token;
                }
                else if (providedParams != null && providedIndex < providedParams.Length)
                {
                    result[i] = providedParams[providedIndex++];
                }
                else
                {
                    result[i] = GetDefaultValue(parameters[i].ParameterType);
                }
            }

            return result;
        }

        private static object GetDefaultValue(Type type) => type.IsValueType ? Activator.CreateInstance(type) : null;

        // Запуск тест-кейса с поддержкой отмены
        public static async Task RunTestCaseAsync(TestCaseModel testCase, MethodInfo method, TestClassModel classModel)
        {
            object instance = null;

            // Создаем CancellationTokenSource для этого тест-кейса
            using var cts = testCase.HasCancelAfter
                ? new CancellationTokenSource(testCase.CancelAfterTimeout.Value)
                : null;

            testCase.CancellationTokenSource = cts;

            try
            {
                testCase.Status = TestStatus.Running;
                instance = Activator.CreateInstance(classModel.ClassType);
                var stopwatch = Stopwatch.StartNew();
                var token = cts?.Token ?? CancellationToken.None;

                try
                {
                    await InvokeMethodAsync(classModel.SetUpMethod, instance, null, token);
                    await InvokeMethodAsync(method, instance, testCase.Arguments, token);
                    testCase.Status = TestStatus.Passed;
                }
                catch (OperationCanceledException)
                {
                    testCase.Status = TestStatus.Failed;
                    testCase.ErrorMessage = $"Тест отменен по таймауту ({testCase.CancelAfterTimeout} мс)";
                    Debug.WriteLine($"Тест {method.Name} отменен по таймауту");
                }
                catch (SuccessException)
                {
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
                    try { await InvokeMethodAsync(classModel.TearDownMethod, instance, null, CancellationToken.None); } catch { }
                    stopwatch.Stop();
                    testCase.Duration = stopwatch.Elapsed;
                }
            }
            catch (Exception ex)
            {
                testCase.Status = TestStatus.Failed;
                testCase.ErrorMessage = ex.Message;
            }
            finally
            {
                testCase.CancellationTokenSource = null;
            }
        }

        // Запуск параметризованного метода
        public static async Task RunParameterizedMethodAsync(TestMethodModel testMethod, TestClassModel classModel)
        {
            foreach (var testCase in testMethod.TestCases)
            {
                await RunTestCaseAsync(testCase, testMethod.MethodInfo, classModel);
            }
        }

        // Запуск одного тестового метода с поддержкой отмены
        public static async Task RunTestAsync(TestMethodModel testMethod, TestClassModel classModel)
        {
            object instance = null;

            // Создаем CancellationTokenSource для этого теста
            using var cts = testMethod.HasCancelAfter
                ? new CancellationTokenSource(testMethod.CancelAfterTimeout.Value)
                : null;

            testMethod.CancellationTokenSource = cts;

            try
            {
                testMethod.Status = TestStatus.Running;
                instance = Activator.CreateInstance(classModel.ClassType);
                var stopwatch = Stopwatch.StartNew();
                var token = cts?.Token ?? CancellationToken.None;

                try
                {
                    await InvokeMethodAsync(classModel.SetUpMethod, instance, null, token);
                    await InvokeMethodAsync(testMethod.MethodInfo, instance, null, token);
                    testMethod.Status = TestStatus.Passed;
                }
                catch (OperationCanceledException)
                {
                    testMethod.Status = TestStatus.Failed;
                    testMethod.ErrorMessage = $"Тест отменен по таймауту ({testMethod.CancelAfterTimeout} мс)";
                    Debug.WriteLine($"Тест {testMethod.MethodName} отменен по таймауту");
                }
                catch (SuccessException)
                {
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
                    try { await InvokeMethodAsync(classModel.TearDownMethod, instance, null, CancellationToken.None); } catch { }
                    stopwatch.Stop();
                    testMethod.Duration = stopwatch.Elapsed;
                }
            }
            catch (Exception ex)
            {
                testMethod.Status = TestStatus.Failed;
                testMethod.ErrorMessage = ex.Message;
            }
            finally
            {
                testMethod.CancellationTokenSource = null;
            }
        }

        // Запуск всех тестов класса с поддержкой отмены на уровне класса
        public static async Task RunClassTestsAsync(TestClassModel classModel)
        {
            object fixtureInstance = null;

            // Создаем CTS для класса, если есть CancelAfter
            using var classCts = classModel.HasCancelAfter
                ? new CancellationTokenSource(classModel.CancelAfterTimeout.Value)
                : null;

            var classToken = classCts?.Token ?? CancellationToken.None;

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
                        await InvokeMethodAsync(classModel.FixtureSetUpMethod, fixtureInstance, null, classToken);
                    }
                    catch (OperationCanceledException)
                    {
                        Debug.WriteLine("TestFixtureSetUp отменен по таймауту");
                        throw;
                    }
                    catch (SuccessException)
                    {
                        Debug.WriteLine("TestFixtureSetUp успешно завершен через Assert.Pass");
                    }
                }

                // Запускаем методы
                foreach (var method in classModel.Methods)
                {
                    // Если у метода нет своего CancelAfter, но есть у класса, устанавливаем
                    if (!method.HasCancelAfter && classModel.HasCancelAfter)
                    {
                        method.CancelAfterTimeout = classModel.CancelAfterTimeout;
                    }

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
            finally
            {
                // TestFixtureTearDown
                try
                {
                    if (classModel.FixtureTearDownMethod != null)
                    {
                        var tearDownInstance = classModel.FixtureTearDownMethod.IsStatic ? null : fixtureInstance;
                        await InvokeMethodAsync(classModel.FixtureTearDownMethod, tearDownInstance, null, CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка в TestFixtureTearDown: {ex.Message}");
                }
            }
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