using Hangfire;
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
        private ReportCreator reporter;

        private static NewsAggregatorScheduler instance;
        public static NewsAggregatorScheduler getInstance()
        {
            if (instance == null)
                instance = new NewsAggregatorScheduler();

            return instance;
        }

        private NewsAggregatorScheduler()
        {
            downloader = new DataDownloader();
            database = new MongoFacade();
            reporter = new ReportCreator();
        }

        public void Start()
        {
            RecurringJob.AddOrUpdate("UpdateJob", () => doUpdate(), Cron.Hourly);
        }

        private void doUpdate()
        {
            try {
                List<ProcessedArticle> articles = downloader.DownloadArticles();

                //Save articles to DB
                //Report stuff?? Analyse??
            }
            catch (Exception e)
            {
                throw new Exception("Fatal Exception in upper doUpdate!", e);
            }
        }
    }
}