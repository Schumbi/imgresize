namespace ImageResizer.Extensions
{
    using System;

    using LanguageExt;

    public static class FileInfoExtensions
    {
        public static Unit PrintFileInfo(this string fileName, Action<string ?> printer)
                {
                    FileInfo fi = new FileInfo(fileName);
                    printer(
                        $"Name: {fi.FullName} \n" +
                        $"Length: {fi.Length} \t" +
                        $"Creation: {fi.CreationTime} " +
                        $"LastWrite: {fi.LastWriteTime} " +
                        $"LastAccess: {fi.LastAccessTime} " +
                        $"Attributes: {fi.Attributes} " +
                        $"{fi.UnixFileMode}");
                    return Unit.Default;
                }
    }
}