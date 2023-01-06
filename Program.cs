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
                KeepAspectRatio = true,
            };

            Processor worker = new(opts);

            bool exit = false;

            while(!exit)
            {
                Thread.Sleep(1000);
                if(worker.Working)
                {
                    Console.Clear();
                    Console.WriteLine($"{worker.CurrentCount}/{worker.TaskCount}");
                } else
                {
                    Console.Write("?");
                }
            }
        }
    }
}
