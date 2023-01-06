using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ImageResizerTests")]
namespace ImageResizer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using LanguageExt;

    using static LanguageExt.Prelude;

    using Extensions;
    using imageResizer.Components;

    public class ImageResizer
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

        public ImageResizer()
        {
            shouldWork.Change += (shouldWork) => WorkOnQueue(shouldWork);
        }

        /// <summary>
        /// Process the queue.
        /// </summary>
        /// <returns></returns>
        protected Unit WorkOnQueue(bool shouldWork) => shouldWork
            .IfTrue(async () =>
            {
                if(processingQueue.Count > 0)
                {
                    TaskItem item = processingQueue.Dequeue();
                    var img = await Resizer.Resize(item, 1024, 768);
                }
                else
                {
                    _ = this.shouldWork.Swap((_) => false);
                }
            })
            .IfFalse(() =>
            {
                Console.WriteLine("Noting to do.");
            }).AsUnit();


        public bool Working => shouldWork.Value;

        /// <summary>
        /// Add an item to the processing state.
        /// </summary>
        /// <param name="item">Work item to add.</param>
        /// <returns>Unit</returns>
        public Unit Add(TaskItem item) => item.DoIf(
            pred: (i) => !string.IsNullOrWhiteSpace(i.Value) && Path.Exists(i.Value),
            func: (i) =>
            {
                processingQueue.Enqueue(i);
                return shouldWork.Swap((_) => true);
            }).AsUnit();
    }
}
