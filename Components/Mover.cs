namespace ImageResizer.Components
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using ImageResizer.Types;

    using LanguageExt;

    public static class Mover
    {
        /// <summary>
        /// Move a file to a file location.
        /// </summary>
        /// <param name="source">Source file.</param>
        /// <param name="destination">Destination file.</param>
        /// <returns>Success.</returns>
        public static bool Move(FilePath source, FilePath destination)
        {
            if(source.FileExists && !destination.FileExists)
            {
                File.Move(source.Value, destination.Value);
                return true;
            }
            return false;
        }

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

        public static Option<FilePath> GuardDestination(FilePath original, int count = 0)
        {
            if(!original.FileExists)
                return original;

            var fname = Path.GetFileNameWithoutExtension(original.Value);
            return Option<FilePath>.None;

        }
    }
}
