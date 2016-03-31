using NewsAggregator.BackgroundWorkers.Model;
using NewsAggregator.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Web;
using System.Xml;

namespace NewsAggregator.BackgroundWorkers
{
    public class DataDownloader
    {
        public List<TextSource> Sources = new List<TextSource>();

        public DataDownloader()
        {
            LoadSources("BackgroundWorkers/Content/Sources.xml"); //Todo... richtig?
        }

        public void LoadSources(string path)
        {
            try
            {
                Sources.Clear();

                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                XmlNode root = doc.SelectSingleNode("root");
                foreach (XmlNode node in root.ChildNodes)
                {
                    Sources.Add(CreateTextsourceFromXML(node));
                }
            }
            catch(Exception e) { throw new Exception("Cant load Sources!", e); }
        }

        public List<Article> DownloadArticles()
        {
            List<Article> articles = new List<Article>();
            foreach (TextSource source in Sources)
            {
                try
                {
                    articles.AddRange(DownloadArticlesForSource(source));
                }
                catch (Exception e)
                {
                    Logger.Error("Error downloading articles: " + source.getID() + " " + e.Message);
                }
            }

            return articles;
        }

        public TextSource CreateTextsourceFromXML(XmlNode xml)
        {
            try
            {
                List<string> urls = new List<string>();
                string Name = xml.SelectSingleNode("name").InnerText;
                string Typ = xml.SelectSingleNode("typ").InnerText;
                string Country = xml.SelectSingleNode("country").InnerText;

                //Hat URLs
                if (xml.SelectSingleNode("rss").ChildNodes.Count != 0)
                {
                    //Iterate urls
                    foreach (XmlNode urlNode in xml.SelectSingleNode("rss").ChildNodes)
                        urls.Add(urlNode.InnerText);
                }

                return new TextSource(Name, Typ, Country, urls);
            }
            catch (Exception e)
            {
                throw new Exception("Cant parse XML for TextSource!", e);
            }
        }

        public List<Article> DownloadArticlesForSource(TextSource source)
        {
            List<Article> articles = new List<Article>();
            foreach (string url in source.urls)
            {
                try
                {
                    XmlReader reader = XmlReader.Create(url);
                    SyndicationFeed feed = SyndicationFeed.Load(reader);
                    reader.Close();

                    foreach (SyndicationItem item in feed.Items)
                    {
                        try
                        {
                            articles.Add(
                                ArticleProcessor.processArticle(
                                        item.Title.Text,
                                        item.Summary.Text,
                                        (item.Links.Count > 0 ? item.Links[0].Uri.ToString() : ""),
                                        DateTime.Now,
                                        item.PublishDate.DateTime,
                                        source)
                                    );
                        }
                        catch (Exception e)
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
    }
}