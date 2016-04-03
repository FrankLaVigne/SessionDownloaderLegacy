using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;

namespace SessionDownloader
{
    public enum MediaType
    {
        Mp4Low,
        Mp4High,
        Mp3,
        Slides
    }

    public class Downloader
    {

        private Uri _feedUri;
        private SyndicationFeed _feed;
        private char[] _invalidChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
        private List<SessionInfo> _sessionInfoList;

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

        public void DownloadMedia()
        {
            DownloadAtom();
            ProcessAtomFeed();
            DownloadFiles();
        }

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
                var title = item.Title.Text;
                var uri = item.Links[1].Uri;
                var fileSize = item.Links[1].Length;
                var code = uri.ToString().Split('/').Last().Split('.').First();

                this._sessionInfoList.Add(new SessionInfo
                {
                    Title = title,
                    MediaUri = uri,
                    FileSize = fileSize,
                    Code = code
                });

            }
        }

        private void DownloadAtom()
        {
            XmlReader reader = XmlReader.Create(this._feedUri.ToString());
            this._feed = SyndicationFeed.Load(reader);
        }

        private void Initialize()
        {
            this._sessionInfoList = new List<SessionInfo>();

            SetFeedUri();
        }

        private void SetFeedUri()
        {
            string targetUrl = string.Empty;

            switch (MediaType)
            {
                case MediaType.Mp4High:
                    targetUrl = this.BaseUrl + "/mp4high";
                    break;
                case MediaType.Mp3:
                    targetUrl = this.BaseUrl + "/mp3";
                    break;
                case MediaType.Slides:
                    targetUrl = this.BaseUrl + "/slides";
                    break;
                default:
                    targetUrl = this.BaseUrl + "/mp4";
                    break;
            }

            this._feedUri = new Uri(targetUrl);
        }

    }
}
