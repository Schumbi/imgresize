namespace ImageResizer
{
    using System.Reactive.Linq;
    using System.CommandLine;

    using static LanguageExt.Prelude;

    class Program
    {
        static int Main(string[] args)
        {
            var sourceArg = new System.CommandLine.Option<DirectoryInfo?>(
                name: "--source",
                description: "The directory with source files!"
            );
            sourceArg.Arity = ArgumentArity.ExactlyOne;

            var destArg = new System.CommandLine.Option<DirectoryInfo?>(
                name:"--destination",
                getDefaultValue: () => new DirectoryInfo( $"{Path.GetTempPath()}{Path.DirectorySeparatorChar}imgResizer_originals" ),
                description: "The directory the files should be moved to."
            );

            var moveArg = new System.CommandLine.Option<DirectoryInfo?>(
                name: "--move",
                getDefaultValue: () => new DirectoryInfo( $"{Path.GetTempPath()}{Path.DirectorySeparatorChar}imgResizer_resized" ),
                description: "The directory for the resized images!"
            );
            var rootCommand = new RootCommand("Resizes JPG-Images and moves images.")
            {
                sourceArg,
                destArg,
                moveArg
            };

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
