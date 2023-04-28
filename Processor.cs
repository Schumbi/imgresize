namespace ImageResizer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Threading.Tasks;

    using LanguageExt;

    using static LanguageExt.Prelude;

    using SixLabors.ImageSharp.Formats.Jpeg;
    using ImageResizer.Components;
    using ImageResizer.Types;

    public static class Processor
    {

        public static Task Process(
            Queue<TaskItem> workload,
            Options opts,
            Mutex mutex,
            CancellationToken cancellationToken)
            => Task.Run(() =>
        {
            ThreadPool.SetMaxThreads(opts.MaxConcurrent, opts.MaxConcurrent);
            do
            {
                Console.WriteLine("Run");
                if (workload.Any() && ThreadPool.PendingWorkItemCount == 0)
                {
                    if (mutex.WaitOne((int)opts.CheckDelay.Milliseconds))
                    {
                        Console.WriteLine("Got mutex");
                        foreach (int i in Range(0, opts.MaxConcurrent))
                        {
                            if (workload.Any())
                            {
                                ThreadPool.QueueUserWorkItem(
                                    async (p) => await ProcessAsync(p.item, p.opts),
                                    (item: workload.Dequeue(), opts),
                                    false);
                            }
                            else
                            {
                                break;
                            }
                        }
                        mutex.ReleaseMutex();
                        Console.WriteLine("Released mutex");
                    }
                }
                else
                {
                    Thread.Sleep((int)opts.CheckDelay.Milliseconds);
                }
            }
            while (cancellationToken.IsCancellationRequested == false);
        }, cancellationToken);

        /// <summary>
        /// Find all image files in the given directory.
        /// </summary>
        /// <param name="directory">Absolute directory path.</param>
        /// <returns>List of task items.</returns>
        public static IEnumerable<TaskItem> CheckForImageFiles(string directory)
            => new DirectoryInfo(directory)
                .GetFiles()
                .Where(f => f.Extension.Contains("jpg", StringComparison.InvariantCultureIgnoreCase))
                .Select(f => new TaskItem(f.FullName));

        /// <summary>
        /// Process a single task item.
        /// </summary>
        /// <returns>Task.</returns>
        private static async Task<TaskItem> ProcessAsync(TaskItem item, Options options)
        {
            string original = Path.Combine(options.DestinationDirectory, Path.GetFileName(item.Value));

            if (Mover.Move(TaskItem.Create(item.Value), DirectoryPath.Create(options.DestinationDirectory)))
            {
                var img = await Resizer.ResizeAsync(
                    TaskItem.Create(original),
                    options.Width,
                    options.Height,
                    options.KeepAspectRatio);

                if (img != null)
                {
                    using var writeStream = File.OpenWrite(Path.Combine(options.MovedDirectory, Path.GetFileName(item.Value)));
                    await img.SaveAsync(writeStream, new JpegEncoder { ColorType = JpegColorType.Rgb, Quality = 85 });
                }
            }

            return item;
        }
    }
}
