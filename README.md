# SessionDownloader

A no frills downloader CLI app that pulls down all the video and slides from Build 2018 sessions and other events on MSDN's Channel9.

## History
Here's a blog post on the Session Downloader: http://www.franksworld.com/2015/10/07/session-downloader-reloaded/
The above blog post I wrote was deleted due to ClearDB's Epic SNAFU. Listen to this for details http://datadriven.tv/special-5pm-know-data-is/ 

The blog post talked about the history of this code. It began humbly as a WPF app to capture all the sessions from PDC 2009. 2009 was a simpler time, mobile broadband was expensive and not widely available and I wanted a way to watch PDC videos at home on my TV (sad, right?) or on the go on my laptop. The answer was to grab all the content using the RSS/ATOM feed Channel9 made available.

In 2015, I updated it to download the sessions from Build 2015. In 2016, I added parameters to make it grab all items on Channel9 with an Event RSS feed. In 2018, I encountered some issues: first with the RSS feed then with some other oddities stemming from the previous approach, which had more or less in place since 2009. 

## Thanks
At this time, I'd like to thank those who contributed code to make the downloader better!

# Instructions

## First parameter: 
Path to directory where images will be saved.

## Second parameter: 
Download high or low quality videos (h or high for high) omit for low quality

## Third parameter: 
File format. Options are: MP4High, Low, MP3, or Slides. (Low is the default)

### Get all the session videos from Build 2018
SessionDownloader.exe C:\Downloads\ https://s.ch9.ms/Events/Build/2018/RSS


### Get all the session videos from Build 2017
SessionDownloader.exe C:\Downloads\ https://s.ch9.ms/Events/Build/2017/RSS
## Example Uses


### Get all the session videos from Build 2016
SessionDownloader.exe C:\Downloads\ https://s.ch9.ms/Events/Build/2016/RSS 

### Get all the sessions from Build 2016 in audio format only
SessionDownloader.exe C:\Downloads\Audio\ https://s.ch9.ms/Events/Build/2016/RSS mp3

### Get all the slides from Build 2016
SessionDownloader.exe C:\Downloads\Slides\ https://s.ch9.ms/Events/Build/2016/RSS slides



