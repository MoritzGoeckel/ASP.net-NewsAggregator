using NewsAggregator.BackgroundWorkers.Model;
using NewsAggregator.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml;

namespace NewsAggregator.BackgroundWorkers
{
    public class DataDownloader
    {
        public List<TextSource> Sources = new List<TextSource>();

        public DataDownloader()
        {
            loadSources("BackgroundWorkers/Content/Sources.xml"); //Todo... richtig?
        }

        public void loadSources(string path)
        {
            try
            {
                Sources.Clear();

                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                XmlNode root = doc.SelectSingleNode("root");
                foreach (XmlNode node in root.ChildNodes)
                {
                    Sources.Add(new TextSource(node));
                }
            }
            catch(Exception e) { throw new Exception("Cant load Sources!", e); }
        }

        public List<ProcessedArticle> DownloadArticles()
        {
            List<ProcessedArticle> articles = new List<ProcessedArticle>();
            foreach (TextSource source in Sources)
            {
                try
                {
                    articles.AddRange(source.Download());
                }
                catch (Exception e)
                {
                    Logger.Error("Error downloading articles: " + source.getID() + " " + e.Message);
                }
            }

            return articles;
        }
    }
}