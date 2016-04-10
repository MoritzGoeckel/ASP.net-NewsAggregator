using NewsAggregator.BackgroundWorkers;
using NewsAggregator.BackgroundWorkers.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace NewsAggregator.Util
{
    public class Importer
    {
        public static void ImportCommonWordsFile(INewsDatabase database, string path)
        {
            string[] lines = File.ReadAllText(path).Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                database.InsertCommonWords(line);
            }
        }

        public static void ImportSourcesFromXML(string path, INewsDatabase database)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path);

                XmlNode root = doc.SelectSingleNode("root");
                foreach (XmlNode node in root.ChildNodes)
                {
                    database.InsertSource(CreateTextsourceFromXML(node));
                }
            }
            catch (Exception e) { throw new Exception("Cant import Sources!", e); }
        }

        private static TextSource CreateTextsourceFromXML(XmlNode xml)
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
    }
}