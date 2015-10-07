using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;

namespace SessionDownloader
{

    public enum Quality
    {
        Low,
        High
    }


    public class Downloader
      {

        // TODO: parameterize this
        private const string MP4_FEED_URL = "https://channel9.msdn.com/Events/Build/2015/RSS/mp4";

        //------------------------------------------------------------------------------------
        // Reference
        //------------------------------------------------------------------------------------
        // High quality
        //https://channel9.msdn.com/Events/Build/2015/RSS/mp4High
        // Low quality
        //https://channel9.msdn.com/Events/Build/2015/RSS/mp4        
        // Azure Con     
        //https://channel9.msdn.com/Events/Microsoft-Azure/AzureCon-2015/RSS/Mp4
        //------------------------------------------------------------------------------------

        private Uri _feedUri;
        private SyndicationFeed _feed;
        private char[] _invalidChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
        private List<SessionInfo> _sessionInfoList;

        /// <summary>
        /// Where files should be saved locally
        /// </summary>
        public string DestinationRootPath { get; set; }

        /// <summary>
        /// Quality of videos to download
        /// </summary>
        public Quality VideoQuality { get; set; }

        /// <summary>
        /// Downloader constuctor class
        /// </summary>
        /// <param name="destinationRootPath">Where files should be saved locally</param>
        /// <param name="quality">Quality of videos to download. Defaults to Low</param>
        public Downloader(string destinationRootPath, Quality quality = Quality.Low)
        {
            this.DestinationRootPath = destinationRootPath;
            this.VideoQuality = quality;

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
              string destinationFileName = ComputeFileName(sessionItem.Title);
              bool fileExists = CheckIfFileExists(destinationFileName);

              if (fileExists)
              {
                    Console.WriteLine("File Exists: " + sessionItem.Title);
              }
              else
              {
                    Console.WriteLine("Downloading " + sessionItem.Title);
                    WebClient webClient = new WebClient();
                    webClient.DownloadFile(sessionItem.MediaUri, destinationFileName);
                  }
              }
          }

        private string ComputeFileName(string sessionTitle)
          {
              var fileName = string.Empty;
   
              string fileNameTitle = ScrubSessionTitle(sessionTitle);
              fileName = this.DestinationRootPath + fileNameTitle + ".mp4";
   
              return fileName;
          }
   
          private bool CheckIfFileExists(string fileName)
          {
              return File.Exists(fileName);
          }
   
          private string ScrubSessionTitle(string sessionTitle)
          {
              var scrubbedString = sessionTitle;
   
              this._invalidChars.ToList().ForEach(x => {
   
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
   
                  this._sessionInfoList.Add(new SessionInfo 
                      {
                          Title = title,
                          MediaUri = uri
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

            switch (VideoQuality)
            {
                case Quality.High:
                    targetUrl = MP4_FEED_URL + "high";
                    break;
                default:
                    targetUrl = MP4_FEED_URL;
                    break;
            }

            this._feedUri = new Uri(targetUrl);
          }
   
      }
  }
