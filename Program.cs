namespace ImageResizer
{
    class Program
    {
        static void Main(string[] args)
        {
            var opts = new Options()
            {
                DestinationDirectory = "original",
                MovedDirectory = "resized",
                SourceDirectory = "images",
                Width = 100,
                Height = 100,
            };

            Processor worker = new(opts);

            bool exit = false;

            while(!exit)
            {
                Thread.Sleep(1000);
                if(worker.Working)
                {
                    Console.Write("!");
                } else
                {
                    Console.Write("?");
                }
            }
        }

        static void OnFileCreated(object sender, FileSystemEventArgs e)
        {

            //Console.WriteLine(e.FullPath);
            //using (var stream = File.OpenRead(e.FullPath))
            //{

            //    var image = Image.Load<Rgba32>(stream, out IImageFormat format);

            //    int newWidth = image.Width / 2;
            //    int newHeight = (int)(image.Height * ((float)newWidth / image.Width));

            //    image.Mutate(x => x.Resize(newWidth, newHeight));

            //    using var writeStream = File.OpenWrite(Path.Combine(destinatinDirectory, e.Name));

            //    image.Save(writeStream, new JpegEncoder { ColorType = JpegColorType.Rgb, Quality = 85 });
            //}

            //File.Move(e.FullPath, Path.Combine(movedDirectory, e.Name));
        }
    }
}
