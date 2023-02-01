namespace ImageResizer.Types
{
    using LanguageExt;

    using static LanguageExt.Prelude;

    public class DirectoryPath : NewType<DirectoryPath, string>
    {
        public DirectoryPath(string value) : base(value)
        {
        }

        public bool DirectoryExists => Directory.Exists(Value);
    }
}
