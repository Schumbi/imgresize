namespace imageResizer
{
    using LanguageExt;

    /// <summary>
    /// Task item to processingQueue with. Currently, path to image.
    /// </summary>
    public class TaskItem : NewType<TaskItem, string>
    {
        public TaskItem(string value) : base(value)
        {
        }
    }
}
