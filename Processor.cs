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

    public class Processor
    {
        private readonly Atom<Option<WorkingStateInfo>> workingState =
            Atom<Option<WorkingStateInfo>>(None);

        public Processor(Options options)
            => Options = options;

        /// <summary>
        /// Run the processor loop indefinitely.
        /// </summary>
        /// <returns>Observable of optional working state information.
        /// Missing values indicate that there is currently no work to do.</returns>
        public IObservable<Option<WorkingStateInfo>> ProcessAsync()
        {
            var workingStateObservable = Observable.FromEvent<AtomChangedEvent<Option<WorkingStateInfo>>, Option<WorkingStateInfo>>(
                h => workingState.Change += h,
                h => workingState.Change -= h);

            var watchObservable = Task.Run(async () =>
            {
                var concurrencyLimit = new SemaphoreSlim(Options.MaxConcurrent, Options.MaxConcurrent);
                workingState.Swap(_ => None);

                while (true)
                {
                    var tasks = CheckForImageFiles(Options.SourceDirectory);

                    workingState.Swap(_ => new WorkingStateInfo(tasks.Count(), tasks.Count()));

                    var jobs = tasks.Select(item => ProcessAsync(item, concurrencyLimit)).ToList();

                    await Task.WhenAll(jobs);
                    workingState.Swap(_ => None);

                    await Task.Delay(Options.CheckDelay.ToTimeSpan());
                }
            }).ToObservable().Select(_ => Option<WorkingStateInfo>.None);

            return Observable.Merge(workingStateObservable, watchObservable);
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
        private async Task ProcessAsync(TaskItem item, SemaphoreSlim concurrencyLimit)
        {
            string original = Path.Combine(Options.DestinationDirectory, Path.GetFileName(item.Value));
            File.Move(item.Value, original);

            var img = await Resizer.ResizeAsync(
                new TaskItem(original),
                Options.Width,
                Options.Height,
                Options.KeepAspectRatio,
                concurrencyLimit);

            if (img != null)
            {
                using var writeStream = File.OpenWrite(Path.Combine(Options.MovedDirectory, Path.GetFileName(item.Value)));
                await img.SaveAsync(writeStream, new JpegEncoder { ColorType = JpegColorType.Rgb, Quality = 85 });
                workingState.Swap(s => s.Map(ws => ws with { CurrentCount = ws.CurrentCount - 1 }));
            }
        }

        /// <summary> 
        /// Options set during start up.
        /// </summary>
        public Options Options { get; }

        /// <summary>
        /// Working state information.
        /// </summary>
        /// <param name="TaskCount">Get total workload.</param>
        /// <param name="CurrentCount">Get current workload.</param>
        public record WorkingStateInfo(int TaskCount, int CurrentCount);
    }
}
