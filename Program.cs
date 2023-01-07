namespace ImageResizer
{
    class Program
    {
        static async Task Main(string[] args)
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

            Processor processor = new(opts);
            var processorTask = processor.ProcessAsync();

            while (!processorTask.IsCompleted)
            {
                Thread.Sleep(1000);
                if (processor.Working)
                {
                    do { Console.Write("\b \b"); } while (Console.CursorLeft > 0);
                    Console.Write($"{processor.CurrentCount}/{processor.TaskCount}");
                }
                else
                {
                    do { Console.Write("\b \b"); } while (Console.CursorLeft > 0);
                    Console.Write("Waiting...");
                }
            }

            await processorTask;
        }
    }
}
