namespace ImageResizer
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public record Options
    {
        /// <summary>
        /// Source directory to look for images.
        /// </summary>
        public string SourceDirectory { get; init; } = string.Empty;

        /// <summary>
        /// Directory to move original images to.
        /// </summary>
        public string DestinationDirectory { get; init; } = string.Empty;

        /// <summary>
        /// Dirctory to move resized image to.
        /// </summary>
        public string MovedDirectory { get; init; } = string.Empty;

        /// <summary>
        /// Width of resized picture.
        /// </summary>
        public int Width { get; init; }

        /// <summary>
        /// Height of resized picture.
        /// </summary>
        public int Height { get; init; }

        /// <summary>
        /// Wether to keep aspect ratio (biggest dimension).
        /// </summary>
        public bool KeepAspectRatio { get; init; }

    }
}
