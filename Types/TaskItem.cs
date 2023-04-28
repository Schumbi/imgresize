namespace ImageResizer.Types
{
    using LanguageExt;

    public class TaskItem : NewType<TaskItem, string>
    {
        public TaskItem(string value) : base(value)
        {
        }

        /// <summary>
        /// Gets existance.
        /// </summary>
        public virtual bool FileExists => File.Exists(Value);

        /// <summary>
        /// Gets the name of the file without parent directory and extension.
        /// </summary>
        public Option<string> Name
        {
            get
            {
                if (FileExists)
                {
                    var name = Path.GetFileNameWithoutExtension(Value);
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        return name;
                    }
                }
                return Option<string>.None;
            }
        }

        /// <summary>
        /// Gets the extension with period (e.g. ".ini").
        /// </summary>
        public Option<string> Extension
        {
            get
            {
                var ext = Path.GetExtension(Value);
                if (!string.IsNullOrWhiteSpace(ext))
                {
                    return ext;
                }
                return Option<string>.None;
            }
        }

        /// <summary>
        /// Gets directory of file.
        /// </summary>
        public Option<DirectoryPath> Directory
        {
            get
            {
                var dir = Path.GetDirectoryName(Value);
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    return new DirectoryPath(dir);
                }

                return Option<DirectoryPath>.None;
            }
        }

        /// <summary>
        /// Combine directory name, file name, and extension to a file path.
        /// </summary>
        /// <param name="directory">Directory path.</param>
        /// <param name="name">File name without extension.</param>
        /// <param name="ext">Extension starting with ".".</param>
        /// <returns></returns>
        public static Option<TaskItem> Combine(DirectoryPath directory, string name, string ext)
        {
            if (directory.DirectoryExists &&
                !string.IsNullOrWhiteSpace(name) &&
                !string.IsNullOrWhiteSpace(ext) &&
                ext.StartsWith('.') &&
                ext.Length > 1)
            {

                return new TaskItem(Path.TrimEndingDirectorySeparator(directory.Value) + Path.DirectorySeparatorChar + name + ext);
            }

            return Option<TaskItem>.None;
        }

        /// <summary>
        /// A default empty file path.
        /// </summary>
        /// <returns>empty.</returns>
        public static TaskItem Empty() => new(string.Empty);

        /// <summary>
        /// Create a File Path.
        /// </summary>
        /// <param name="path">Path to use.</param>
        /// <returns>Filepath.</returns>
        public static TaskItem Create(string path) => new(path);
    }
}
