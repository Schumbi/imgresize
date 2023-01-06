using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ImageResizerTests")]
namespace ImageResizer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using LanguageExt;

    using static LanguageExt.Prelude;

    using Extensions;
    using SixLabors.ImageSharp.Formats.Jpeg;
    using ImageResizer.Components;

    public class Processor
    {

        /// <summary>
        /// Items to process.
        /// </summary>
        private readonly Queue<TaskItem> processingQueue = new();

        /// <summary>
        /// State of processing.
        /// </summary>
        private readonly Atom<bool> shouldWork = Atom(false);

        /// <summary>
        /// Maximum tasks to use.
        /// </summary>
        private int maxTasks = 5;

        /// <summary>
        /// Watch directory for created events.
        /// </summary>
        private readonly FileSystemWatcher watcher = new();

        public Processor(Options options)
        {
            Options = options;
            shouldWork.Change += (shouldWork) => WorkOnQueue(shouldWork);

            // search for existing files
            foreach (var item in new DirectoryInfo(Options.SourceDirectory).GetFiles()
                .Where(f => f.Extension.Contains("jpg", StringComparison.InvariantCultureIgnoreCase)))
            {
                _ = AddTask(new TaskItem(item.FullName));
            }

            // start watcher
            watcher = new FileSystemWatcher(Path.GetFullPath(Options.SourceDirectory))
            {
                Filters = { "*.jpg", "*.JPG" },
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
            };
            watcher.Changed += (obj, e) => {
                if (e is not null && Path.Exists(e.FullPath) && File.Exists(e.FullPath)) {
                    _ = AddTask(new TaskItem(e.FullPath));
                }};
        }

        /// <summary>
        /// Destructor. Dispose FilesystemWatcher.
        /// </summary>
        ~Processor() => watcher.Dispose();

        /// <summary>
        /// Process the queue.
        /// </summary>
        /// <returns></returns>
        protected Unit WorkOnQueue(bool shouldWork) => shouldWork
            .IfTrue(async () =>
            {
                await Task.Run(async () =>
                {
                    while (processingQueue.Count > 0)
                    {
                        TaskItem item = processingQueue.Dequeue();
                        var img = await Resizer.Resize(item, Options.Width, Options.Height);
                        if (img != null)
                        {
                            var fileName = Path.GetFileName(item.Value);
                            using var writeStream = File.OpenWrite(Path.Combine(Options.MovedDirectory, fileName));
                            img.Save(writeStream, new JpegEncoder { ColorType = JpegColorType.Rgb, Quality = 85 });
                        }
                    }
                });
            })
            .IfFalse(() =>
            {
                Console.WriteLine("Nothing to do.");
            }).AsUnit();

        /// <summary>
        /// Working state.
        /// </summary>
        public bool Working => shouldWork.Value;

        /// <summary>
        /// Options set during start up.
        /// </summary>
        public Options Options { get; }

        /// <summary>
        /// AddTask an item to the processing state.
        /// </summary>
        /// <param name="item">Work item to add.</param>
        /// <returns>Unit</returns>
        public Unit AddTask(TaskItem item) => item.DoIf(
            pred: (i) => !string.IsNullOrWhiteSpace(i.Value) && Path.Exists(i.Value),
            func: (i) =>
            {
                processingQueue.Enqueue(i);
                if(shouldWork.Value == false)
                {
                    // start working
                    return shouldWork.Swap((_) => true);
                }
                return true;
            }).AsUnit();
    }
}
