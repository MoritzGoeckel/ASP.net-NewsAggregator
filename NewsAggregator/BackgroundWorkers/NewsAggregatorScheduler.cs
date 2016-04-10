using Hangfire;
using NewsAggregator.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace NewsAggregator.BackgroundWorkers
{
    public class NewsAggregatorScheduler
    {
        private DataDownloader downloader;
        private INewsDatabase database;

        private static NewsAggregatorScheduler instance;
        public static NewsAggregatorScheduler getInstance()
        {
            if (instance == null)
                instance = new NewsAggregatorScheduler();

            return instance;
        }

        private NewsAggregatorScheduler()
        {
            database = GlobalDataManager.getInstance().getDatabase();
            //FirstTimeStart();
            downloader = new DataDownloader(database);
        }

        public void FirstTimeStart() //Do only once!
        {
            database.PrepareDB();
            Importer.ImportSourcesFromXML(@"E:\Programmieren\C# Neu\NewsAggregator\NewsAggregator\Content\Sources.xml", database);
            Importer.ImportCommonWordsFile(database, @"E:\Programmieren\C# Neu\NewsAggregator\NewsAggregator\Content\common_words.txt");
        }

        public void Start()
        {
            RecurringJob.AddOrUpdate("UpdateJob", () => doUpdate(), "0 * * * *"); //Jede Stunde um X:00
            RecurringJob.AddOrUpdate("SaveCurrentWordsToHistoryJob", () => saveCurrentWordsToHistory(), "0 23 * * *"); //Jeden Tag um 23:30
        }

        public void doUpdate()
        {
            try {
                List<Article> articles = downloader.DownloadArticles();
                database.InsertArticles(articles);
                Logger.Log("Done saving the articles (doUpdate)");

                database.UpdateCurrentWords();
                Logger.Log("Done updating words");
            }
            catch (Exception e)
            {
                throw new Exception("Fatal Exception in upper doUpdate!", e);
            }
        }

        public void saveCurrentWordsToHistory()
        {
            try
            {
                database.SaveCurrentWordsForHistory();
                Logger.Log("Done saving words for history");
            }
            catch (Exception e)
            {
                throw new Exception("Fatal Exception in upper saveCurrentWordsToHistory!", e);
            }
        }
    }
}