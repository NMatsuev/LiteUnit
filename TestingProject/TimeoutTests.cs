using System.Threading;
using System.Threading.Tasks;
using TestingLibrary;
using TestingLibrary.Attributes;

namespace TestingProject
{
    [TestFixture]
    [CancelAfter(5000)] 
    public class TimeoutTests
    {
        [Test]
        [CancelAfter(1000)] // Этот тест будет отменен через 1 секунду
        public void TestWithTimeout(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // Долгий цикл
                Thread.Sleep(100);
            }
        }

        [Test]
        public async Task AsyncTestWithClassTimeout_PassesSuccessfully(CancellationToken token)
        {
            // Использует таймаут класса (5 секунд)
            await Task.Delay(3000, token);
        }

        [Test]
        public async Task AsyncTestWithClassTimeout_FailsWithException(CancellationToken token)
        {
            // Использует таймаут класса (5 секунд)
            await Task.Delay(10000, token);
        }

        [TestCase(1)]
        [TestCase(2)]
        [CancelAfter(2000)] // Каждый тест-кейс будет иметь свой таймаут
        public void ParameterizedTest(int value, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(100);
            }
        }
    }
}
