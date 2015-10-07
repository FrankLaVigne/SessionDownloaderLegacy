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
            Downloader d = new Downloader();

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
