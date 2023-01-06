namespace ImageResizer.Components
{
    using System.Threading.Tasks;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Processing;

    internal class Resizer
    {
        //int newHeight = (int)(image.Height * ((float)newWidth / image.Width));
        public static Task<Image<Rgba32>> Resize(TaskItem taskItem, int newWidth, int newHeight) =>
            Task.Run(() =>
                {
                    using var stream = File.OpenRead(taskItem.Value);
                    var image = Image.Load<Rgba32>(stream, out IImageFormat format);
                    image.Mutate(x => x.Resize(newWidth, newHeight));
                    return image;
                });
    }
}
