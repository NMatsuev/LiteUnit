using System.Diagnostics;
using TestsRunner.Models;
using TestsRunner.Models.Enums;

namespace TestsRunner.Services
{
    public static class TestRunnerService
    {
        public static void RunTest(TestMethodModel testMethod, TestClassModel classModel)
        {
            try
            {
                testMethod.Status = TestStatus.Running;

                //Создаем экземпляр класса
                var instance = Activator.CreateInstance(classModel.ClassType);

                var stopwatch = Stopwatch.StartNew();

                try
                {
                    //Выполняем FixtureSetUp если есть
                    classModel.FixtureSetUpMethod?.Invoke(instance, null);

                    //Выполняем SetUp если есть
                    classModel.SetUpMethod?.Invoke(instance, null);

                    //Выполняем тест
                    testMethod.MethodInfo.Invoke(instance, null);

                    testMethod.Status = TestStatus.Passed;
                }
                catch (Exception ex)
                {
                    testMethod.Status = TestStatus.Failed;
                    testMethod.ErrorMessage = ex.InnerException?.Message ?? ex.Message;
                }
                finally
                {
                    //Выполняем TearDown если есть
                    try { classModel.TearDownMethod?.Invoke(instance, null); } catch { }

                    //Выполняем FixtureTearDown если есть
                    try { classModel.FixtureTearDownMethod?.Invoke(instance, null); } catch { }

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

        public static void RunTests(IEnumerable<TestMethodModel> testMethods, TestClassModel classModel)
        {
            foreach (var method in testMethods)
            {
                RunTest(method, classModel);
            }
        }

        public static void RunClassTests(TestClassModel classModel)
        {
            //Сначала запускаем методы текущего класса
            RunTests(classModel.Methods, classModel);

            //Затем запускаем методы вложенных классов
            foreach (var nestedClass in classModel.NestedClasses)
            {
                RunClassTests(nestedClass);
            }
        }
    }
}