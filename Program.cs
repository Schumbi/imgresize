namespace ImageResizer
{
    using System.Reactive.Linq;

    using static LanguageExt.Prelude;

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
                MaxConcurrent = 4,
                CheckDelay = 500 * ms,
            };

            CancellationTokenSource cts = new();
            Console.CancelKeyPress += (o, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            var statusObservable = Processor.RunAsync(opts, cts.Token);

            using var _ = statusObservable.Subscribe(workingState =>
            {
                var msg = workingState.Match(
                    Some: state => $"{state.CurrentCount}/{state.TaskCount}",
                    None: () => "Waiting...");

                do { Console.Write("\b \b"); } while (Console.CursorLeft > 0);
                Console.Write(msg);
            });

            await statusObservable.LastOrDefaultAsync();
        }
    }
}
