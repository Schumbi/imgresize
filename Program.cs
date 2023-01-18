namespace ImageResizer
{
    using System.Reactive.Linq;
    using System.CommandLine;

    using static LanguageExt.Prelude;

    class Program
    {
        static int Main(string[] args)
        {
            var sourceArg = new System.CommandLine.Option<DirectoryInfo>(
                aliases: new string[] {"--source", "-s"},
                description: "The directory watched for new files.") 
            {
                IsRequired = true,
                Arity = ArgumentArity.ExactlyOne
            };

            sourceArg.AddValidator(result => {
                var a = result.GetValueForOption(sourceArg);
            });

            var destArg = new System.CommandLine.Option<DirectoryInfo>(
                aliases: new string[] {"--destination", "-d"},
                description: "The directory the files should be moved to.")
            {
                IsRequired = true,
                Arity = ArgumentArity.ExactlyOne
            };

            var moveArg = new System.CommandLine.Option<DirectoryInfo>(
                aliases: new string[] {"--move", "-m"},
                description: "The directory for the resized images!") 
            {
                IsRequired = true,
                Arity = ArgumentArity.ExactlyOne
            };

            var rootCommand = new RootCommand("Watches a directory, resizes JPG-Images and moves images.")
            {
                sourceArg,
                destArg,
                moveArg
            };

            rootCommand.SetHandler(async (source, destination, move) =>
            {
                await Resize(source, destination, move);
            }, destArg, sourceArg, moveArg);

            return rootCommand.InvokeAsync(args).Result;
        }

        internal static async Task Resize(DirectoryInfo source, DirectoryInfo destination, DirectoryInfo move)
        {
            var opts = new Options()
            {
                DestinationDirectory = destination.FullName,
                MovedDirectory = move.FullName,
                SourceDirectory = source.FullName,
                Width = 100,
                Height = 100,
                KeepAspectRatio = true,
                MaxConcurrent = 4,
                CheckDelay = 500 * ms,
            };
            await ImgResize(opts);
        }

        static async Task ImgResize(Options opts)
        {
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
