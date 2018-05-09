using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SessionDownloader
{
    public enum MediaType
    {
        Mp4Low,
        Mp4High,
        Mp4Medium,
        Mp3,
        Slides
    }

    public class Downloader
    {

        #region Private Members

        private Uri _feedUri;
        private SyndicationFeed _feed;
        private char[] _invalidChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|', '\n' };
        private List<SessionInfo> _sessionInfoList;

        #endregion
        
        #region Public Properties


        /// <summary>
        /// Where files should be saved locally
        /// </summary>
        public string DestinationRootPath { get; set; }

        public string BaseUrl { get; set; }

        /// <summary>
        /// Quality of videos to download
        /// </summary>
        public MediaType MediaType { get; set; }

        /// <summary>
        /// Whether the Title contains the Session Code or not
        /// </summary>
        public bool TitleHasCode { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Downloader constuctor class
        /// </summary>
        /// <param name="destinationRootPath">Where files should be saved locally</param>
        /// <param name="mediaType">Quality of videos to download. Defaults to Low</param>
        public Downloader(string destinationRootPath, string baseUrl, MediaType mediaType = MediaType.Mp4Low, bool titleHasCode = false)
        {
            this.DestinationRootPath = destinationRootPath;
            this.BaseUrl = baseUrl;
            this.MediaType = mediaType;
            this.TitleHasCode = titleHasCode;

            this.Initialize();
        }

        #endregion

        #region Public Methods
        public void DownloadMedia()
        {
            DownloadAtom();
            ProcessAtomFeed();
            DownloadFiles();
        }

        #endregion

        #region Private Methods
        
        private void DownloadFiles()
        {
            foreach (var sessionItem in this._sessionInfoList)
            {
                string destinationFileName = ComputeFileName(sessionItem.Title, sessionItem.Code);
                bool fileExists = CheckIfFileExists(destinationFileName);

                if (fileExists)
                {
                    Console.WriteLine("File Exists: " + sessionItem.Title);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Downloading " + sessionItem.Title);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(ComputeFileSizeInMb(sessionItem.FileSize) + "MB");
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(sessionItem.MediaUri, destinationFileName);
                }
            }
        }

        private string ComputeFileSizeInMb(long fileSizeInBytes)
        {
            var fileSizeinMB = (fileSizeInBytes / 1024f) / 1024f;

            return fileSizeinMB.ToString();
        }

        private string ComputeFileName(string sessionTitle, string sessionCode)
        {
            var fileName = string.Empty;
            var code = this.TitleHasCode ? $"[{sessionCode}] " : string.Empty;
            var fileNameTitle = code + ScrubSessionTitle(sessionTitle);

            fileName = this.DestinationRootPath + fileNameTitle + DetermineFileExtension();

            return fileName;
        }

        private string DetermineFileExtension()
        {
            string fileExtension;

            if (this.MediaType == MediaType.Mp3)
            {
                fileExtension = ".mp3";
            }
            else if (this.MediaType == MediaType.Slides)
            {
                fileExtension = ".pptx";
            }
            else
            {
                fileExtension = ".mp4";
            }

            return fileExtension;
        }

        private bool CheckIfFileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        private string ScrubSessionTitle(string sessionTitle)
        {
            var scrubbedString = sessionTitle;

            this._invalidChars.ToList().ForEach(x =>
            {

                scrubbedString = scrubbedString.Replace(x, ' ');
            });

            return scrubbedString;
        }

        private void ProcessAtomFeed()
        {
            foreach (var item in this._feed.Items)
            {
                Uri fileUri;
                long fileSize;

                item.AttributeExtensions.Add(new XmlQualifiedName("media", "http://search.yahoo.com/mrss/"), "media");

                var mediaElement = LoadMediaElementGroup(item);

                if (mediaElement == null)
                {
                    continue;
                }

                XElement mediaInfo = DetermineMediaUrl(mediaElement);

                if (mediaInfo == null)
                {
                    continue;
                }

                fileSize = long.Parse(mediaInfo.Attribute("fileSize").Value);
                fileUri = new Uri(mediaInfo.Attribute("url").Value);

                // Sample XML
                // -----------------------------------------------------------------------------------
                //< media:group xmlns:media = "http://search.yahoo.com/mrss/" >
                //< media:content url = "http://ch9northcentralus.blob.core.windows.net/mfupload/55a876d55f4045e89213a5db0029a1c9/C9LIVE032AzureIoT.mp4" expression = "full" duration = "1612" fileSize = "1" type = "video/mp4" medium = "video" ></ media:content >
                //< media:content url = "http://video.ch9.ms/ch9/c0c1/fe7a553e-94a5-4e8d-84e6-7c416e6fc0c1/C9LIVE032AzureIoT.mp3" expression = "full" duration = "1612" fileSize = "25799370" type = "audio/mp3" medium = "audio" ></ media:content >
                //< media:content url = "http://video.ch9.ms/ch9/c0c1/fe7a553e-94a5-4e8d-84e6-7c416e6fc0c1/C9LIVE032AzureIoT.mp4" expression = "full" duration = "1612" fileSize = "158328366" type = "video/mp4" medium = "video" ></ media:content >
                //< media:content url = "http://video.ch9.ms/ch9/c0c1/fe7a553e-94a5-4e8d-84e6-7c416e6fc0c1/C9LIVE032AzureIoT_high.mp4" expression = "full" duration = "1612" fileSize = "1347843378" type = "video/mp4" medium = "video" ></ media:content >
                //< media:content url = "http://video.ch9.ms/ch9/c0c1/fe7a553e-94a5-4e8d-84e6-7c416e6fc0c1/C9LIVE032AzureIoT_mid.mp4" expression = "full" duration = "1612" fileSize = "776583734" type = "video/mp4" medium = "video" ></ media:content >
                //</ media:group >}
                // -----------------------------------------------------------------------------------



                var title = item.Title.Text;


                var code = fileUri.ToString().Split('/').Last().Split('.').First();

                this._sessionInfoList.Add(new SessionInfo
                {
                    Title = title,
                    MediaUri = fileUri,
                    FileSize = fileSize,
                    Code = code
                });

            }
        }

        private XElement DetermineMediaUrl(XElement mediaElement)
        {

            //<media:group>          
            //	<media:content url="http://ch9northcentralus.blob.core.windows.net/mfupload/3945ffdbd51747d19996a8d801823ee3/Build18CloudMigrationApp.mp4" expression="full" duration="1221" fileSize="1" type="video/mp4" medium="video"></media:content>
            //	<media:content url="http://video.ch9.ms/ch9/7c48/a97c5468-4fd2-453a-a0b3-20ab1fa57c48/Build18CloudMigrationApp.mp3" expression="full" duration="1221" fileSize="19550922" type="audio/mp3" medium="audio"></media:content>
            //	<media:content url="http://video.ch9.ms/ch9/7c48/a97c5468-4fd2-453a-a0b3-20ab1fa57c48/Build18CloudMigrationApp.mp4" expression="full" duration="1221" fileSize="85090993" type="video/mp4" medium="video"></media:content>
            //	<media:content url="http://video.ch9.ms/ch9/7c48/a97c5468-4fd2-453a-a0b3-20ab1fa57c48/Build18CloudMigrationApp_high.mp4" expression="full" duration="1221" fileSize="768394670" type="video/mp4" medium="video"></media:content>
            //	<media:content url="http://video.ch9.ms/ch9/7c48/a97c5468-4fd2-453a-a0b3-20ab1fa57c48/Build18CloudMigrationApp_mid.mp4" expression="full" duration="1221" fileSize="456256294" type="video/mp4" medium="video"></media:content>
            //</media:group>

            int mediaIndex = 4;
            string fileEnding = string.Empty;

            switch (this.MediaType)
            {
                case MediaType.Mp4High:
                    mediaIndex = 3;
                    fileEnding = "_high.mp4";
                    break;
                case MediaType.Mp4Medium:
                    mediaIndex = 4;
                    fileEnding = "_mid.mp4";
                    break;
                case MediaType.Mp3:
                    mediaIndex = 1;
                    fileEnding = ".mp3";
                    break;
                default:
                    mediaIndex = 4;
                    fileEnding = ".mp4";
                    break;
            }


            var mediaFormatElement = (
                                        from me in mediaElement.Descendants()
                                        where me.Attribute("url").Value.EndsWith(fileEnding) && me.Attribute("url").Value.Contains("blob.core.windows.net") == false
                                        select me
                                     )
                                      .FirstOrDefault();

            return mediaFormatElement;
        }

        private XElement LoadMediaElementGroup(SyndicationItem item)
        {
            var allMediaExt = item.ElementExtensions;
            var mediaExt = item.ElementExtensions
                .Where(x => x.OuterNamespace == "http://search.yahoo.com/mrss/")
                .LastOrDefault();

            if (mediaExt == null)
            {
                return null;
            }

            var reader = mediaExt.GetReader();
            XNode mediaNode = XElement.ReadFrom(reader);
            XElement mediaElement = mediaNode as XElement;

            return mediaElement;


        }

        private void DownloadAtom()
        {
            XmlReader reader = XmlReader.Create(this._feedUri.ToString());

            try
            {
                this._feed = SyndicationFeed.Load(reader);
            }
            catch (Exception ex)
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.Out.WriteLine("Error has occcured.");
                Console.Out.WriteLine("Details:");
                Console.Out.WriteLine(ex.Message);
                Console.ReadLine();
            }

        }

        private void Initialize()
        {
            if (!Directory.Exists(this.DestinationRootPath))
                Directory.CreateDirectory(this.DestinationRootPath);

            this._sessionInfoList = new List<SessionInfo>();

            SetFeedUri();
        }

        private void SetFeedUri()
        {
            string targetUrl = string.Empty;

            switch (MediaType)
            {
                case MediaType.Slides:
                    targetUrl = this.BaseUrl + "/slides";
                    break;
                default:
                    targetUrl = this.BaseUrl + "/";
                    break;
            }

            this._feedUri = new Uri(targetUrl);
        }

        #endregion

    }
}
