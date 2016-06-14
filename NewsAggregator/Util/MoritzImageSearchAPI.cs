using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;

namespace NewsAggregator.Util
{
    public class MoritzImageSearchAPI
    {
        public static List<string> getGoogleImageSearchResult(string word)
        {
            List<string> urls = new List<string>();

            WebClient wc = new WebClient();
            string result = wc.DownloadString("https://www.google.de/search?q=" + word + "&safe=off&tbm=isch&tbs=qdr:d");

            Regex r = new Regex("src=\"(https:\\/\\/encrypted-tbn3\\.gstatic\\.com\\/images\\?q=[^\"]*)\"");
            foreach (Match match in r.Matches(result))
                urls.Add(match.Groups[1].Value);

            return urls;
        }
    }
}