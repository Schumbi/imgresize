namespace ImageResizer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Threading.Tasks;
    using System.Reactive.Linq;
    using System.Threading.Tasks;

    using LanguageExt;

    using static LanguageExt.Prelude;

    using SixLabors.ImageSharp.Formats.Jpeg;
    using ImageResizer.Components;

    public static class Processor
    {
        /// <summary>
        /// Run the processor loop indefinitely.
        /// </summary>
        /// <returns>Observable of optional working state information.
        /// Missing values indicate that there is currently no work to do.</returns>
        public static IObservable<Option<WorkingStateInfo>> RunAsync(
            Options options,
            CancellationToken cancellationToken = default)
        {
            var workingState = Atom<Option<WorkingStateInfo>>(None);

            var workingStateObservable = Observable.FromEvent<AtomChangedEvent<Option<WorkingStateInfo>>, Option<WorkingStateInfo>>(
                h => workingState.Change += h,
                h => workingState.Change -= h);

            var watchObservable = Task.Run(async () =>
            {
                var concurrencyLimit = new SemaphoreSlim(options.MaxConcurrent, options.MaxConcurrent);
                workingState.Swap(_ => None);

                while (!cancellationToken.IsCancellationRequested)
                {
                    var tasks = CheckForImageFiles(options.SourceDirectory);
                    int taskCount = tasks.Count();

                    if (taskCount > 0)
                    {
                        workingState.Swap(_ => new WorkingStateInfo(taskCount, taskCount));

                        var jobs = tasks
                            .Select(item => ProcessAsync(item, options, concurrencyLimit)
                                .ContinueWith(t => workingState
                                    .Swap(s => s.Map(ws => ws with { CurrentCount = ws.CurrentCount - 1 }))))
                            .ToList();

                        await Task.WhenAll(jobs);
                        workingState.Swap(_ => None);
                    }

                    await Task.Delay(options.CheckDelay.ToTimeSpan());
                }
            }).ToObservable();

            return workingStateObservable.TakeUntil(watchObservable);
        }

        /// <summary>
        /// Find all image files in the given directory.
        /// </summary>
        /// <param name="directory">Absolute directory path.</param>
        /// <returns>List of task items.</returns>
        private static IEnumerable<TaskItem> CheckForImageFiles(string directory)
            => new DirectoryInfo(directory)
                .GetFiles()
                .Where(f => f.Extension.Contains("jpg", StringComparison.InvariantCultureIgnoreCase))
                .Select(f => new TaskItem(f.FullName));

        /// <summary>
        /// Process a single task item.
        /// </summary>
        /// <returns>Task.</returns>
        private static async Task ProcessAsync(TaskItem item, Options options, SemaphoreSlim concurrencyLimit)
        {
            string original = Path.Combine(options.DestinationDirectory, Path.GetFileName(item.Value));
            File.Move(item.Value, original);

            var img = await Resizer.ResizeAsync(
                new TaskItem(original),
                options.Width,
                options.Height,
                options.KeepAspectRatio,
                concurrencyLimit);

            if (img != null)
            {
                using var writeStream = File.OpenWrite(Path.Combine(options.MovedDirectory, Path.GetFileName(item.Value)));
                await img.SaveAsync(writeStream, new JpegEncoder { ColorType = JpegColorType.Rgb, Quality = 85 });
            }
        }

        /// <summary>
        /// Working state information.
        /// </summary>
        /// <param name="TaskCount">Get total workload.</param>
        /// <param name="CurrentCount">Get current workload.</param>
        public record WorkingStateInfo(int TaskCount, int CurrentCount);
    }
}
