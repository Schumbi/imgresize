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

                var msg = processor.WorkingState.Match(
                    Some: state => $"{state.CurrentCount}/{state.TaskCount}",
                    None: () => "Waiting...");

                do { Console.Write("\b \b"); } while (Console.CursorLeft > 0);
                Console.Write(msg);
            }

            await processorTask;
        }
    }
}
