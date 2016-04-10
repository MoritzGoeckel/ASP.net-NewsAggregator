using MongoDB.Bson;
using NewsAggregator.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Xml;

namespace NewsAggregator.BackgroundWorkers.Model
{
    [Serializable]
    public class TextSource
    {
        public string Name, Typ, Country;
        public List<string> urls = new List<string>();

        public TextSource(string Name, string Typ, string Country, List<string> urls)
        {
            this.Name = Name;
            this.Typ = Typ;
            this.Country = Country;
            this.urls.AddRange(urls);
        }

        public TextSource(BsonDocument bson)
        {
            this.Name = bson["name"].AsString;
            this.Typ = bson["typ"].AsString; ;
            this.Country = bson["country"].AsString;

            foreach (BsonValue url in bson["urls"].AsBsonArray)
                this.urls.Add(url.AsString);
        }

        public string getID()
        {
            return (Name + Country).Replace(" ", "");
        }

        public BsonDocument toBson()
        {
            BsonDocument sourceDoc = new BsonDocument();
            sourceDoc.Add(new BsonElement("name", Name));
            sourceDoc.Add(new BsonElement("typ", Typ));
            sourceDoc.Add(new BsonElement("country", Country));
            sourceDoc.Add(new BsonElement("id", getID()));

            sourceDoc.Add(new BsonElement("urls", new BsonArray(urls)));

            return sourceDoc;
        }
    }
}