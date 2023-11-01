namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            Task sleeper = Task.Factory.StartNew(() => { Thread.Sleep(10000); Console.WriteLine("sleeper ended"); });

            int index = Task.WaitAny(new[] { sleeper },
                                     TimeSpan.FromSeconds(2));
            Console.WriteLine(index); // Prints -1, timeout
            Console.WriteLine("sleeper status : " + sleeper.Status);
            var cts = new CancellationTokenSource();

            // Just a simple wait of getting a cancellable task
            Task cancellable = sleeper.ContinueWith(ignored => { }, cts.Token);
            Console.WriteLine("sleeper status : " + sleeper.Status);
            // It doesn't matter that we cancel before the wait
            cts.Cancel();
            Console.WriteLine("sleeper status : " + sleeper.Status);
            index = Task.WaitAny(new[] { cancellable },
                                 TimeSpan.FromSeconds(0.5));
            Console.WriteLine(index); // 0 - task 0  has completed (ish :)
            Console.WriteLine(cancellable.Status); // Cancelled
            Console.WriteLine("sleeper status : " + sleeper.Status);




            Thread.Sleep(15000);
            Console.WriteLine("sleeper status : " + sleeper.Status);
        }
    }
}