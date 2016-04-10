using MongoDB.Bson;
using NewsAggregator.BackgroundWorkers.Model;
using NewsAggregator.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace NewsAggregator.BackgroundWorkers
{
    [Serializable]
    public class Article
    {
        public string Headline, Summery, Url;

        [NonSerialized]
        public DateTime PublishDate, DownloadDate;

        public TextSource Source;

        public double date;

        public Article(string Headline, string Summery, string Url, DateTime PublishDate, DateTime DownloadDate, TextSource Source)
        {
            this.Headline = Headline;
            this.Summery = Summery;
            this.Url = Url;
            this.PublishDate = PublishDate;
            this.Source = Source;
            this.DownloadDate = DownloadDate;

            this.date = DateTimeHelper.DateTimeToUnixTimestamp(DownloadDate);
        }

        public Article(BsonDocument bson, INewsDatabase database)
        {
            this.Headline = bson["headline"].AsString;
            this.Source = database.GetSources()[bson["sourceid"].AsString];
            this.Summery = bson["summery"].AsString;
            this.PublishDate = DateTimeHelper.UnixTimeStampToDateTime(bson["published"].AsDouble);
            this.DownloadDate = DateTimeHelper.UnixTimeStampToDateTime(bson["downloaded"].AsDouble);
            this.Url = bson["url"].AsString;

            this.date = DateTimeHelper.DateTimeToUnixTimestamp(DownloadDate);
        }

        public string getID()
        {
            return Headline + "_" + DownloadDate + "_" + Source.getID();
        }

        public BsonDocument toBson()
        {
            BsonDocument articleDoc = new BsonDocument();
            articleDoc.Add(new BsonElement("headline", Headline));
            articleDoc.Add(new BsonElement("sourceid", Source.getID()));
            articleDoc.Add(new BsonElement("summery", Summery));
            articleDoc.Add(new BsonElement("published", DateTimeHelper.DateTimeToUnixTimestamp(PublishDate)));
            articleDoc.Add(new BsonElement("downloaded", DateTimeHelper.DateTimeToUnixTimestamp(DownloadDate)));
            articleDoc.Add(new BsonElement("url", Url));
            articleDoc.Add(new BsonElement("id", getID()));

            return articleDoc;
        }
    }
}