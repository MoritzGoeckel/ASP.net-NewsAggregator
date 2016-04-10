using Hangfire;
using NewsAggregator.Util;
using System;
using System.Collections.Generic;
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
            downloader = new DataDownloader(database);
        }

        public void Start()
        {
            RecurringJob.AddOrUpdate("UpdateJob", () => doUpdate(), "0 0 * * * ?");
            RecurringJob.AddOrUpdate("SaveCurrentWordsToHistoryJob", () => doUpdate(), "0 55 23 * * ?");
        }

        private void doUpdate()
        {
            try {
                List<Article> articles = downloader.DownloadArticles();
                database.InsertArticles(articles);
                database.UpdateCurrentWords();
            }
            catch (Exception e)
            {
                throw new Exception("Fatal Exception in upper doUpdate!", e);
            }
        }

        private void saveCurrentWordsToHistory()
        {
            try
            {
                database.SaveCurrentWordsForHistory();
            }
            catch (Exception e)
            {
                throw new Exception("Fatal Exception in upper saveCurrentWordsToHistory!", e);
            }
        }
    }
}