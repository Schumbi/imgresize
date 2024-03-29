﻿namespace ImageResizer.Components
{
    using Types;
    using Extensions;

    public static class Mover
    {
        /// <summary>
        /// Move a file to a file location.
        /// </summary>
        /// <param name="source">Source file.</param>
        /// <param name="destination">Destination file.</param>
        /// <returns>Success.</returns>
        public static bool Move(FilePath source, FilePath destination) => source
            .DoIf<FilePath, bool>(
                s => s.FileExists,
                src => GuardDestination(destination)
                    .Match(
                        Some: fp =>
                        {
                            File.Move(source.Value, fp.Value);
                            return true;
                        },
                        None: () => false))
            .Match(b => b, () => false);

        /// <summary>
        /// Move file to directory.
        /// </summary>
        /// <param name="source">Source file.</param>
        /// <param name="destination">Destination directory.</param>
        /// <returns>Success.</returns>
        public static bool Move(FilePath source, DirectoryPath destination)
        {
            if (source.FileExists && destination.DirectoryExists)
            {
                var fileDestination = new FilePath(Path.Combine(destination.Value, Path.GetFileName(source.Value)));
                return Move(source, fileDestination);
            }
            return false;
        }

        /// <summary>
        /// Searches for a new filename if file exists.
        /// </summary>
        /// <param name="original">Original file path.</param>
        /// <param name="count">Counter to increase.</param>
        /// <param name="error">Error.</param>
        /// <returns></returns>
        public static LanguageExt.Option<FilePath> GuardDestination(FilePath original, int count = 0, bool error = false)
        {
            if (!original.FileExists)
                return original;

            if (error)
                return LanguageExt.Option<FilePath>.None;

            // next count
            count += 1;

            var nextFilePath = original.Bind(o => FilePath
                .Combine(
                original.Directory.Match(p => p, new DirectoryPath(string.Empty)),
                    original.Name.Match(name =>
                        name.EndsWith($"_{count - 1}") ? name.Replace($"_{count - 1}", $"_{count}") : $"{name}_{count}", $"{count}"),
                     original.Extension.Match(e => e, string.Empty))
                .IfNone(FilePath.Empty));

            error = nextFilePath == FilePath.Empty() || nextFilePath.Directory.IsNone || nextFilePath.Extension.IsNone;

            return GuardDestination(nextFilePath, count, error);
        }
    }
}
