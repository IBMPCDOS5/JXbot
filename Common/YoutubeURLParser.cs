using System;
using System.Web;
using System.Net;

namespace JXbot.Common
{
    public class YoutubeURLParser
    {   
        public static string TitleParser(string url)
        {
            var YTapi = $"http://youtube.com/get_video_info?video_id={ArgsParser(url, "v", '?')}";
            return ArgsParser(new WebClient().DownloadString(YTapi), "title", '&');
        }

        private static string ArgsParser(string args, string key, char query)
        {
            var inqueryS = args.IndexOf(query);
            return inqueryS == -1 ? string.Empty : HttpUtility.ParseQueryString(inqueryS < args.Length - 1  ? args.Substring(inqueryS + 1) : string.Empty)[key];
        }
    }
}
