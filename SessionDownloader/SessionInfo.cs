using System;

namespace SessionDownloader
{
    public class SessionInfo
    {
        public string Title { get; set; }

        public Uri MediaUri { get; set; }

        public long FileSize { get; set; }

        public string Code { get; set; }
    }

}