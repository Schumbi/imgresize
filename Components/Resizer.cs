namespace ImageResizer.Components
{
    using System.Threading.Tasks;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    /// <summary>
    /// Resize an image.
    /// </summary>
    internal class Resizer
    {
        //int newHeight = (int)(image.Height * ((float)newWidth / image.Width));
        public static Image Resize(TaskItem taskItem, int newWidth, int newHeight, bool keepRation)
        {
            using var stream = File.OpenRead(taskItem.Value);
            var image = Image.Load(stream, out IImageFormat format);

            if (keepRation)
            {
                double ratio = image.Width / image.Height;
                if (ratio > 1.0)
                {
                    // landscape
                    newHeight = (int)Math.Round(newWidth / ratio);
                }
                else
                {
                    // protrait
                    newWidth = (int)Math.Round(newHeight / ratio);
                }
            }

            image.Mutate(x => x.Resize(newWidth, newHeight));
            return image;
        }
    }
}
