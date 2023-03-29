namespace ImageResizer
{
    using LanguageExt;

    /// <summary>
    /// Tools to move the files.
    /// </summary>
    public static class Mover
    {
        /// <summary>
        /// Get a unique file path, that exists. Adds a suffix if file already exists.
        /// </summary>
        /// <param name="filePath">File path to check.</param>
        /// <param name="suffix">Added to file path if file exists.</param>
        static Option<string> GetUniqueFilePath(string filePath)
        {
            if(string.IsNullOrEmpty(Path.GetDirectoryName(filePath)))
            {
                return Option<string>.None;
            }

            int suffix = 0;
            while(!System.IO.File.Exists(filePath))
            {
                suffix += 1;
                filePath = $"{filePath}_{suffix}";
            }

            return Option<string>.Some(filePath);
        }
    }

}