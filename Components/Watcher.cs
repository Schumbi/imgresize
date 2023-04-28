namespace ImageResizer.Components
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    using LanguageExt;
    using ImageResizer.Types;
    using ImageResizer.Extensions;

    internal class Watcher
    {
        public static Task RunWatcher(
            DirectoryInfo watchedDir,
            Option<Action<FilePath>> created,
            Option<Action<FilePath>> deleted,
            Option<Action<Exception>> errorHandler,
            Option<Func<NotifyFilters>> notifyFilters,
            Option<Func<string>> fileFilter,
            bool recursive,
            CancellationToken cancellationToken)

             => Task.Run(async () =>
             {
                
                 using var watcher = new FileSystemWatcher(watchedDir.FullName);

                 created.IfSome(f => watcher.Created += (object sender, FileSystemEventArgs e) =>
                    {
                        Console.WriteLine($"{e.ChangeType}");
                        f.Invoke(FilePath.Create(e.FullPath));
                        e.FullPath.PrintFileInfo(Console.WriteLine);
                    });

                 deleted.IfSome(f => watcher.Deleted += (object sender, FileSystemEventArgs e) =>
                 {
                    Console.WriteLine($"{e.ChangeType}");
                     f.Invoke(FilePath.Create(e.FullPath));
                 });

                 errorHandler.IfSome(f => watcher.Error += (object sender, ErrorEventArgs e) => f.Invoke(e.GetException()));

                 notifyFilters.IfSome(f => watcher.NotifyFilter = f.Invoke());

                 fileFilter.IfSome(f => watcher.Filter = f.Invoke());

                 watcher.IncludeSubdirectories = recursive;
                 watcher.EnableRaisingEvents = true;

                 await Task.Delay(-1, cancellationToken);

             }, 
                 cancellationToken);
    }
}
