using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SessionDownloader
{
    public class Arguments 
    {
        public string DestinationPath { get; set; }

        public string BaseUrl { get; set; }

        public MediaType MediaType { get; set; }

        public bool TitlesHasCode { get; set; }
    }

    public class Program
    {
        private static int DESTINATION_PATH_ARG_INDEX = 0;
        private static int BASE_URL_ARG_INDEX = 1;
        private static int MEDIA_TYPE_ARG_INDEX = 2;


        static void Main(string[] args)
        {
            // TODO: Parameterize the event portion

            //------------------------------------------------------------------------------------
            // Reference
            //------------------------------------------------------------------------------------
            // Build 2015
            // https://channel9.msdn.com/Events/Build/2015/RSS
            // Azure Con     
            // https://channel9.msdn.com/Events/Microsoft-Azure/AzureCon-2015/RSS
            // Cortana Analytics
            // https://channel9.msdn.com/Events/Cortana-Analytics-Suite/CA-Suite-Workshop-10-11SEP15/RSS
            //------------------------------------------------------------------------------------

            Arguments arguments = ParseArgs(args);

            if (arguments == null)
            {
                return;
            }

            Downloader d = new Downloader(
                                            arguments.DestinationPath, 
                                            arguments.BaseUrl, 
                                            arguments.MediaType,
                                            arguments.TitlesHasCode);

            var startTime = DateTime.Now;

            Console.WriteLine(string.Format("Starting download of {0} files \nfrom {1}\nSaving to {2}",
                arguments.MediaType,
                arguments.BaseUrl, 
                arguments.DestinationPath
               ));
            Console.WriteLine(startTime);

            d.DownloadMedia();

            Console.WriteLine("Done!");
            Console.WriteLine("Started: " + startTime);
            Console.WriteLine("Finished: " + DateTime.Now);

            Console.ReadLine();
        }

        private static Arguments ParseArgs(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Please enter a destination path and base RSS feed URL!");
                return null;
            }

            var destinationPath = args[DESTINATION_PATH_ARG_INDEX];

            if (destinationPath.Last() != '\\')
            {
                destinationPath = destinationPath + '\\';
            }

            var baseUrl = args[BASE_URL_ARG_INDEX];

            MediaType mediaType = ReadMediaTypeArg(args);

            return new Arguments()
            {
                MediaType = mediaType,
                DestinationPath = destinationPath,
                BaseUrl = baseUrl,
                TitlesHasCode = args.FirstOrDefault(a => a.Contains("code")) != default(string)
            };
        }

        private static MediaType ReadMediaTypeArg(string[] args)
        {

            MediaType returnValue = MediaType.Mp4Low;

            try
            {

                var argInputString = args[MEDIA_TYPE_ARG_INDEX].ToLower();

                if (argInputString.StartsWith("h"))
                {
                    returnValue = MediaType.Mp4High;
                }
                else if (argInputString.Contains("mp3"))
                {
                    returnValue = MediaType.Mp3;
                }
                else if (argInputString.Contains("s"))
                {
                    returnValue = MediaType.Slides;
                }
                else
                {
                    returnValue = MediaType.Mp4Low;
                }


            }
            catch (Exception)
            {

                returnValue = MediaType.Mp4Low;


            }

            return returnValue;

        }
    }
}
