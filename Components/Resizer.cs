namespace ImageResizer.Components
{
    using System.Threading.Tasks;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Processing;

    using static LanguageExt.Prelude;

    /// <summary>
    /// Resize an image.
    /// </summary>
    internal class Resizer
    {
        //int newHeight = (int)(image.Height * ((float)newWidth / image.Width));
        public static async Task<Image> ResizeAsync(TaskItem taskItem, int newWidth, int newHeight, bool keepRation)
        {
            var image = await Image.LoadAsync(taskItem.Value);

            if (keepRation)
            {
                double ratio = (double)image.Width / (double)image.Height;
                if (ratio > 1.0)
                {
                    // landscape
                    newHeight = (int)Math.Round(newWidth / ratio);
                }
                else
                {
                    // protrait
                    newWidth = (int)Math.Round(newHeight * ratio);
                }
            }

            image.Mutate(x => x.Resize(newWidth, newHeight));

            return image;
        }
    }
}
