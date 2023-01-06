namespace imageResizer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using LanguageExt;

    using static LanguageExt.Prelude;

    using Extensions;

    public class ImageResizer
    {

        /// <summary>
        /// Items to process.
        /// </summary>
        private readonly Queue<TaskItem> processingQueue = new();

        /// <summary>
        /// State of processing.
        /// </summary>
        private readonly Atom<Unit> shouldWork = Atom(Unit.Default);

        public ImageResizer()
        {
            shouldWork.Change += (_) => workOnQueue();
        }

        /// <summary>
        /// Process the queue.
        /// </summary>
        /// <returns></returns>
        private Unit workOnQueue()
        {

            return Unit.Default;
        }

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
                return shouldWork.Swap((_) => Unit.Default);
            }).AsUnit();
    }
}
