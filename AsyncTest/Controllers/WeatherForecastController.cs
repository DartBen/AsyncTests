using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;

namespace AsyncTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }


        [HttpGet]
        [Route("TestAsyncGet")]
        public async Task<string> TestAsyncGet()
        {
            string result = null;
            result = "Было";

            CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(10000));
            CancellationToken cancellationToken = cts.Token;

            var tokenTask = Task.Run(() =>
            {
                while (true)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return new OperationCanceledException();
                }
            });
            Task<string> tmp2 = null;

            var task = new Task(() => tmp2 = LogicTask(15000,
                cancellationToken), cancellationToken,
                TaskCreationOptions.AttachedToParent);

            task.Start();


            Task.WaitAny(task, tokenTask);

            if (tokenTask.IsCompleted)
            {
                result = "сработала отмена по токену";
            }

            if (task.IsCompleted)
                return tmp2.Result;

            return result;
        }

        private async Task<string> LogicTask(int sleepTime, CancellationToken token)
        {

            token.ThrowIfCancellationRequested();
            if (token.IsCancellationRequested)
                return null;

            string result = "LogicTask начало";

            Thread.Sleep(sleepTime);

            result = "LogicTask конец - отмена не прошла";

            return result;
        }

        [HttpGet]
        [Route("Main")]
        public async Task Main()
        {
            using (var procces = new Process())
            {
                string path = Path.Combine(@"C:\prj\buff\AsyncTest\AsyncTest\ConsoleApp1\bin\Release\net7.0\publish\win-x64\ConsoleApp1.exe");

                procces.StartInfo.FileName = path;
                int dalay1 = 2000;
                int dalay2 = 4000;
                procces.StartInfo.Arguments = string.Format($"{dalay1.ToString()} {dalay2.ToString()}");

                procces.Start();

            }
        }

        private async Task task1()
        {
            var parentTask = Task.Run(() =>
            {
                Console.WriteLine("Родительский Task начало");
                Task.Delay(2000).Wait(); // Родительский Task с задержкой 2 секунды.
                Console.WriteLine("Родительский Task конец");
            });

            var childTask = parentTask.ContinueWith(t =>
            {
                Console.WriteLine("Дочерний Task начало");
                Task.Delay(4000).Wait(); // Дочерний Task с задержкой 4 секунды.
                Console.WriteLine("Дочерний Task конец");
            }, TaskContinuationOptions.AttachedToParent);

            await parentTask; // Дожидаемся завершения родительского Task.

            Console.WriteLine("Весь процесс завершен.");
        }

        static void test()
        {
            Task sleeper = Task.Factory.StartNew(() => Thread.Sleep(100000));

            int index = Task.WaitAny(new[] { sleeper },
                                     TimeSpan.FromSeconds(0.5));
            Console.WriteLine(index); // Prints -1, timeout

            var cts = new CancellationTokenSource();

            // Just a simple wait of getting a cancellable task
            Task cancellable = sleeper.ContinueWith(ignored => { }, cts.Token);

            // It doesn't matter that we cancel before the wait
            cts.Cancel();

            index = Task.WaitAny(new[] { cancellable },
                                 TimeSpan.FromSeconds(0.5));
            Console.WriteLine(index); // 0 - task 0  has completed (ish :)
            Console.WriteLine(cancellable.Status); // Cancelled

            //var config = new HttpConfiguration();
            //var dispatcher = new HttpRoutingDispatcher(config);
            //var invoker = new HttpMessageInvoker(dispatcher);


            //invoker.Send(new HttpRequestMessage(), cts.Token);



            //HttpClient client = new HttpClient();

            //client.SendAsync(new HttpRequestMessage(), cts.Token);
        }

    }
}