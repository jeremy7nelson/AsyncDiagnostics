using Nito.AsyncEx.AsyncDiagnostics;
using System;
using System.Threading;
using System.Threading.Tasks;

[assembly: AsyncDiagnosticAspect(AttributePriority = 1)]

namespace ConsoleApplication1
{
    public static class Test
    {
        static Task cached;

        public static Task MainAsync()
        {
            return MainAsync(CancellationToken.None);
        }

        static async Task MainAsync(CancellationToken token)
        {
            cached = Level2Async();
            Task another = Level2Async();
            var third = Task.Run(() => { throw new InvalidOperationException("third"); });
            var task = Task.WhenAll(cached, another, third);
            try
            {
                await task;
            }
            catch (Exception)
            {
                throw task.Exception;
            }
        }

        static Task Level2Async()
        {
            return Level2Async(CancellationToken.None);
        }

        static async Task Level2Async(CancellationToken token)
        {
            await Task.Delay(1000);
            SynchronousMethodsToo();
        }

        static void SynchronousMethodsToo()
        {
            throw new Exception("test");
        }
    }

    class Program
    {
        static Program()
        {
        }

        public Program()
        {
        }

        private static void Main(string[] args)
        {
            try
            {
                using (AsyncDiagnosticStack.Enter("Hi"))
                {
                    Test.MainAsync().Wait();
                }
            }
            catch (AggregateException ex)
            {
                foreach (var exception in ((AggregateException)ex.InnerException).InnerExceptions)
                    Console.WriteLine(exception.ToAsyncDiagnosticString());
            }

            Console.ReadKey();
        }
    }
}
