using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Xml;

namespace SessionDownloader
{

    public class Downloader
      {
        // TODO: parameterize this
        private const string ROOT_PATH = "N:\\Build\\2015\\";

        // TODO: parameterize this
        private const string MP4_FEED_URL = "https://channel9.msdn.com/Events/Build/2015/RSS/mp4";

        //private const string WMV_FEED_URL = "http://channel9.msdn.com/Events/Build/2013/RSS/wmv";
        //private const string WMVHIGH_FEED_URL = "http//channel9.msdn.com/Events/Build/2013/RSS/wmvhigh";

        //private MediaType _mediaType;
        private Uri _feedUri;

        private char[] _invalidChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

        private List<SessionInfo> _sessionInfoList;
   
        SyndicationFeed _feed;
   
        public Downloader()
        {
            //this._mediaType = mediaType;
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
                    webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;

                    webClient.DownloadFile(sessionItem.MediaUri, destinationFileName);




                  }
              }
              
   
          }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            Console.WriteLine(e.ProgressPercentage);
        }

        private string ComputeFileName(string sessionTitle)
          {
              var fileName = string.Empty;
   
              string fileNameTitle = ScrubSessionTitle(sessionTitle);
   
              fileName = ROOT_PATH + fileNameTitle + ".mp4";
   
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

            //switch (this._mediaType)
            //{
            //  case MediaType.wmvhigh:
            //        targetUrl = WMVHIGH_FEED_URL;
            //        break;
            //  default:
            //        targetUrl = WMV_FEED_URL;
            //        break;
            //}

            targetUrl = MP4_FEED_URL;   
            this._feedUri = new Uri(targetUrl);
          }
   
      }
  }
