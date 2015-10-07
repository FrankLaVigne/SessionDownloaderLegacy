using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionDownloader
{
    public class Program
    {
        static void Main(string[] args)
        {

            if (args.Length == 0)
            {
                Console.WriteLine("Please enter a destination path!");
                return;
            }

            var destinationPath = args[0];
            
            if (destinationPath.Last() != '\\' )
            {
                destinationPath = destinationPath + '\\';
            }


            Downloader d = new Downloader(destinationPath);

            var startTime = DateTime.Now;

            Console.WriteLine("Starting");
            Console.WriteLine(startTime);

            d.DownloadMedia();

            Console.WriteLine("Done!");
            Console.WriteLine("Started: " + startTime);
            Console.WriteLine("Finished: " + DateTime.Now);


            Console.ReadLine();

        }
    }
}
