namespace ConsoleApp1
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            Console.WriteLine(args[0]);
            Console.WriteLine(args[1]);

            await Program.TestTask(int.Parse(args[0]), int.Parse(args[1]));
        }


        static async Task TestTask(int delay1,int delay2)
        {
            var parentTask = Task.Run(() =>
            {
                Console.WriteLine("Родительский Task начало");
                Task.Delay(delay1).Wait(); // Родительский Task с задержкой 2 секунды.
                Console.WriteLine("Родительский Task конец");
            });

            var childTask = parentTask.ContinueWith(t =>
            {
                Console.WriteLine("Дочерний Task начало");
                Task.Delay(delay2).Wait(); // Дочерний Task с задержкой 4 секунды.
                Console.WriteLine("Дочерний Task конец");
            }, TaskContinuationOptions.AttachedToParent);

            await parentTask; // Дожидаемся завершения родительского Task.

            Console.WriteLine("Весь процесс завершен.");
        }
    }
}