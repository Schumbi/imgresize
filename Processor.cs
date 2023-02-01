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
            => Observable.Defer(() => ProcessDirectory(options, cancellationToken))
                .Select(Some)
                .Append(None)
                .Concat(Wait<Option<WorkingStateInfo>>(
                    options.CheckDelay.ToTimeSpan(),
                    cancellationToken))
                .Repeat()
                .OnErrorResumeNext(Observable.Empty<Option<WorkingStateInfo>>())
                .Replay(1)
                .AutoConnect(0);

        /// <summary>
        /// Creates an observable sequence that emits no elements and
        /// completes after the given duration.
        /// If cancellation is requested the sequence immediately
        /// emits an error.
        /// </summary>
        private static IObservable<T> Wait<T>(
            TimeSpan duration,
            CancellationToken cancellationToken)
            => Observable
                .Never<T>()
                .ToTask(cancellationToken)
                .ToObservable()
                .TakeUntil(Observable.Timer(duration));

        /// <summary>
        /// Process all files in the source directory concurrently.
        /// </summary>
        /// <returns>Sequence of processed files/state info.</returns>
        private static IObservable<WorkingStateInfo> ProcessDirectory(
            Options options,
            CancellationToken cancellationToken)
        {
            var taskItems = CheckForImageFiles(options.SourceDirectory);
            int taskItemsCount = taskItems.Count();

            return taskItems
                .Select(item => Observable.Defer(() =>
                    cancellationToken.IsCancellationRequested ?
                        Observable.Empty<TaskItem>() :
                        Observable.FromAsync(() => ProcessAsync(item, options))))
                .Merge(options.MaxConcurrent)
                .Scan(
                    new WorkingStateInfo(taskItemsCount, taskItemsCount),
                    (ws, _) => ws with { CurrentCount = ws.CurrentCount - 1 })
                .StartWith(new WorkingStateInfo(taskItemsCount, taskItemsCount));
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
        private static async Task<TaskItem> ProcessAsync(TaskItem item, Options options)
        {
            string original = Path.Combine(options.DestinationDirectory, Path.GetFileName(item.Value));
            // todo Check if file exists. If so, add increased prefix and check again
            File.Move(item.Value, original);

            var img = await Resizer.ResizeAsync(
                new TaskItem(original),
                options.Width,
                options.Height,
                options.KeepAspectRatio);

            if (img != null)
            {
                using var writeStream = File.OpenWrite(Path.Combine(options.MovedDirectory, Path.GetFileName(item.Value)));
                await img.SaveAsync(writeStream, new JpegEncoder { ColorType = JpegColorType.Rgb, Quality = 85 });
            }

            return item;
        }

        /// <summary>
        /// Working state information.
        /// </summary>
        /// <param name="TaskCount">Get total workload.</param>
        /// <param name="CurrentCount">Get current workload.</param>
        public record WorkingStateInfo(int TaskCount, int CurrentCount);
    }
}
