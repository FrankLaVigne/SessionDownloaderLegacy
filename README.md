# SessionDownloader

A no frills downloader CLI app that pulls down all the video and slides from Build 2016 sessions and other events on MSDN's Channel9.

##First parameter: 
Path to directory where images will be saved.

##Second parameter: 
Download high or low quality videos (h or high for high) omit for low quality

##Third parameter: 
File format. Options are: MP4High, Low, MP3, or Slides. (Low is the default)


##Example Uses

###Get all the session videos from Build 2016
SessionDownloader.exe C:\Downloads\ https://s.ch9.ms/Events/Build/2016/RSS 

###Get all the slides from Build 2016
SessionDownloader.exe C:\Downloads\Slides\ https://s.ch9.ms/Events/Build/2016/RSS slides

##More Info
Here's a blog post on the Session Downloader: http://www.franksworld.com/2015/10/07/session-downloader-reloaded/
