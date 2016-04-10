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
        public Dictionary<string, TextSource> Sources;
        private INewsDatabase database;

        public DataDownloader(INewsDatabase database)
        {
            Sources = database.GetSources();
            this.database = database;
        }

        public List<Article> DownloadArticles()
        {
            List<Article> articles = new List<Article>();
            foreach (KeyValuePair<string,TextSource> source in Sources)
            {
                try
                {
                    articles.AddRange(DownloadArticlesForSource(source.Value, database));
                }
                catch (Exception e)
                {
                    Logger.Error("Error downloading articles: " + source.Key + " " + e.Message);
                }
            }

            return articles;
        }

        public List<Article> DownloadArticlesForSource(TextSource source, INewsDatabase database)
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
                                        item.PublishDate.DateTime,
                                        DateTime.Now,
                                        source)
                                    );
                        }
                        catch (Exception e)
                        {
                            Logger.Error("Error creating article object: " + e.Message);
                            database.AddDownloadErrorToSource(source);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("Error downloading from URL: " + url + " " + e.Message);
                    database.AddDownloadErrorToSource(source);
                }
            }
            return articles;
        }
    }
}