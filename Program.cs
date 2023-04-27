namespace ImageResizer
{
    using System;
    using System.IO;

    using System.CommandLine.Parsing;
    using System.CommandLine;


    class Program
    {
        private static void RunWatcher(DirectoryInfo watchedDir)
        {
            using var watcher = new FileSystemWatcher(watchedDir.FullName);

            // watcher.NotifyFilter = NotifyFilters.CreationTime
            //                      | NotifyFilters.FileName
            //                      | NotifyFilters.LastAccess
            //                      | NotifyFilters.LastWrite;

            watcher.NotifyFilter = NotifyFilters.Size
                        | NotifyFilters.FileName
                        | NotifyFilters.LastWrite;

            watcher.Changed += OnChanged;
            watcher.Created += OnCreated;
            watcher.Deleted += OnDeleted;
            watcher.Renamed += OnRenamed;
            watcher.Error += OnError;

            watcher.Filter = "*.jpg";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }

        static int Main(string[] args)
        {
            var sourceArg = new System.CommandLine.Option<DirectoryInfo>(
                aliases: new string[] { "--source", "-s" },
                description: "The directory watched for new files.")
            {
                IsRequired = true,
                Arity = ArgumentArity.ExactlyOne,
            };


            var rootCommand = new RootCommand("Watches a directory and prints events happening on the inside.")
            {
                sourceArg,
            };

            rootCommand.SetHandler(RunWatcher, sourceArg);

            return rootCommand.InvokeAsync(args).Result;

        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            Console.WriteLine($"Changed: {e.FullPath}");
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.FullPath}";
            Console.WriteLine(value);
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e) =>
            Console.WriteLine($"Deleted: {e.FullPath}");

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"Renamed:");
            Console.WriteLine($"    Old: {e.OldFullPath}");
            Console.WriteLine($"    New: {e.FullPath}");
        }

        private static void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private static void PrintException(Exception? ex)
        {
            if (ex != null)
            {
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine("Stacktrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                PrintException(ex.InnerException);
            }
        }
    }
}