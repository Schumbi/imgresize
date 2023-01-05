namespace ImageResizer
{
    using System;

    using SixLabors.ImageSharp;
    using SixLabors.ImageSharp.Formats.Jpeg;
    using SixLabors.ImageSharp.Processing;
    using SixLabors.ImageSharp.PixelFormats;
    using SixLabors.ImageSharp.Formats;

    class Program
    {
        static readonly string sourceDirectory = "images";

        static readonly string destinatinDirectory = "resized";

        static readonly string movedDirectory = "original";



        static void Main(string[] args)
        {
            var watcher = new FileSystemWatcher(sourceDirectory)
            {
                EnableRaisingEvents = true,
                Filters = { "*.jpg", "*.JPG" },
                IncludeSubdirectories = true,
            };
            watcher.Created += OnFileCreated;

            while(true)
            {
                Thread.Sleep(1000);
                Console.Write(".");
            }
        }

        static void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            Console.WriteLine(e.FullPath);
            using (var stream = File.OpenRead(e.FullPath))
            {

                var image = Image.Load<Rgba32>(stream, out IImageFormat format);

                int newWidth = image.Width / 2;
                int newHeight = (int)(image.Height * ((float)newWidth / image.Width));

                image.Mutate(x => x.Resize(newWidth, newHeight));

                using var writeStream = File.OpenWrite(Path.Combine(destinatinDirectory, e.Name));

                image.Save(writeStream, new JpegEncoder { ColorType = JpegColorType.Rgb, Quality = 85 });
            }

            File.Move(e.FullPath, Path.Combine(movedDirectory, e.Name));
        }
    }
}
