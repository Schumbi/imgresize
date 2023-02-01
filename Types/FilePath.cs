namespace ImageResizer.Types
{
    using LanguageExt;

    public class FilePath : NewType<FilePath, string>
    {
        public FilePath(string value) : base(value)
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
                if(FileExists)
                {
                    var name = Path.GetFileNameWithoutExtension(Value);
                    if(!string.IsNullOrWhiteSpace(name))
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
                if(FileExists)
                {
                    var ext = Path.GetExtension(Value);
                    if (!string.IsNullOrWhiteSpace(ext))
                    {
                        return ext;
                    }
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
                if(FileExists)
                {
                    var dir = Path.GetDirectoryName(Value);
                    if(!string.IsNullOrWhiteSpace(dir))
                    {
                        return new DirectoryPath(dir);
                    }
                }
                return Option<DirectoryPath>.None;
            }
        }
    }
}
