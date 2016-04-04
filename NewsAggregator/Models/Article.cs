using NewsAggregator.BackgroundWorkers.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsAggregator.BackgroundWorkers
{
    public class Article
    {
        public string Headline, Summery, Url;
        public DateTime PublishDate, DownloadDate;
        public TextSource Source;
        
        public Article(string Headline, string Summery, string Url, DateTime PublishDate, DateTime DownloadDate, TextSource Source)
        {
            this.Headline = Headline;
            this.Summery = Summery;
            this.Url = Url;
            this.PublishDate = PublishDate;
            this.Source = Source;
            this.DownloadDate = DownloadDate;
        }
    }
}