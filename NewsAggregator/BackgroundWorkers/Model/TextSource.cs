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

        public string getID()
        {
            return (Name + Country).Replace(" ", "");
        }
    }
}