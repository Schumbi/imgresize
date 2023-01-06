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

        public Processor(Options options)
        {
            Options = options;
            shouldWork.Change += (shouldWork) => WorkOnQueue(shouldWork);
            // initial check
            CheckForFiles();

            // ongoing check
            _ = Task.Run(() =>
            {
                while (true)
                {
                    _ = shouldWork.Value.IfFalse(() => CheckForFiles());
                    Thread.Sleep(1000);
                }
            });

            void CheckForFiles()
            {
                var tasks = new DirectoryInfo(Options.SourceDirectory).GetFiles()
                    .Where(f => f.Extension.Contains("jpg", StringComparison.InvariantCultureIgnoreCase));
                foreach (var item in tasks)
                {
                    _ = AddTask(new TaskItem(item.FullName));
                }
                _ = shouldWork.Swap((_) => tasks.Any());
            }
        }

        /// <summary>
        /// Process the queue.
        /// </summary>
        /// <returns></returns>
        private Unit WorkOnQueue(bool shouldWork) => shouldWork
            .IfTrue(() =>
            {
                _ = Task.Run(() =>
                {
                    TaskCount = processingQueue.Count;
                    while (processingQueue.Count > 0)
                    {
                        TaskItem item = processingQueue.Dequeue();
                        string original = Path.Combine(Options.DestinationDirectory, Path.GetFileName(item.Value));
                        File.Move(item.Value, original);

                        var img = Resizer.Resize(
                            new TaskItem(original), 
                            Options.Width, 
                            Options.Height, 
                            Options.KeepAspectRatio);
                        
                        if (img != null)
                        {
                            using var writeStream = File.OpenWrite(Path.Combine(Options.MovedDirectory, Path.GetFileName(item.Value)));
                            img.Save(writeStream, new JpegEncoder { ColorType = JpegColorType.Rgb, Quality = 85 });
                            CurrentCount = processingQueue.Count;
                        }
                    }
                    _ = this.shouldWork.Swap((_) => false);
                });
            }).AsUnit();

        /// <summary>
        /// Working state.
        /// </summary>
        public bool Working => shouldWork.Value;

        /// <summary>
        /// Get total workload.
        /// </summary>
        public int TaskCount { get; private set; }

        /// <summary>
        /// Get current workload.
        /// </summary>
        public int CurrentCount { get; private set; }

        /// <summary> 
        /// Options set during start up.
        /// </summary>
        public Options Options { get; }

        /// <summary>
        /// AddTask an item to the processing state.
        /// </summary>
        /// <param name="item">Work item to add.</param>
        /// <returns>Unit</returns>
        public Unit AddTask(TaskItem item) => item
            .DoIf<TaskItem,bool>(
            pred: (i) => !string.IsNullOrWhiteSpace(i.Value) && Path.Exists(i.Value),
            func: (i) =>
            {
                processingQueue.Enqueue(i);
                return true;
            })
            .AsUnit();
    }
}
