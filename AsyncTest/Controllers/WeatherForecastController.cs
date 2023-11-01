using Microsoft.AspNetCore.Mvc;
using System;


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

            var task =new Task(() => tmp2=LogicTask(15000, 
                cancellationToken),cancellationToken, 
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