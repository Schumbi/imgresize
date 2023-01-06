namespace ImageResizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var opts = new Options()
            {
                DestinationDirectory = "original",
                MovedDirectory = "resized",
                SourceDirectory = "images",
                Width = 100,
                Height = 100,
                KeepAspectRatio = true,
            };

            Processor worker = new(opts);

            bool exit = false;

            while(!exit)
            {
                Thread.Sleep(1000);
                if(worker.Working)
                {
                    do { Console.Write("\b \b"); } while (Console.CursorLeft > 0);
                    Console.Write($"{worker.CurrentCount}/{worker.TaskCount}");
                } else
                {
                    do { Console.Write("\b \b"); } while (Console.CursorLeft > 0);
                    Console.Write("Waiting...");
                }
            }
        }
    }
}
