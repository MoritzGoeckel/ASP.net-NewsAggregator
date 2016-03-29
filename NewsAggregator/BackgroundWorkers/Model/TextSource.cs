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

        public TextSource(XmlNode xml)
        {
            try {
                Name = xml.SelectSingleNode("name").InnerText;
                Typ = xml.SelectSingleNode("typ").InnerText;
                Country = xml.SelectSingleNode("country").InnerText;

                //Hat URLs
                if (xml.SelectSingleNode("rss").ChildNodes.Count != 0)
                {
                    //Iterate urls
                    foreach (XmlNode urlNode in xml.SelectSingleNode("rss").ChildNodes)
                        urls.Add(urlNode.InnerText);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Cant parse XML for TextSource!", e);
            }
        }

        public List<ProcessedArticle> Download()
        {
            List<ProcessedArticle> articles = new List<ProcessedArticle>();
            foreach (string url in urls)
            {
                try
                {
                    XmlReader reader = XmlReader.Create(url);
                    SyndicationFeed feed = SyndicationFeed.Load(reader);
                    reader.Close();

                    foreach (SyndicationItem item in feed.Items)
                    {
                        try {
                            articles.Add(new ProcessedArticle(
                                item.Title.Text,
                                item.Summary.Text,
                                (item.Links.Count > 0 ? item.Links[0].Uri.ToString() : ""),
                                item.PublishDate.DateTime,
                                this));
                        }
                        catch(Exception e)
                        {
                            Logger.Error("Error creating article object: " + e.Message);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Error downloading from URL: " + url + " " + e.Message);
                }
            }
            return articles;
        }

        public string getID()
        {
            return (Name + Country).Replace(" ", "");
        }
    }
}