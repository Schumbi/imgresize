namespace ImageResizer.Types
{
    using LanguageExt;

    using static LanguageExt.Prelude;

    public class DirectoryPath : NewType<DirectoryPath, string>
    {
        public DirectoryPath(string value) : base(value)
        {
        }

        public virtual bool DirectoryExists => Directory.Exists(Value);

        public static DirectoryPath Create(string path) => new DirectoryPath(path);
    }
}
