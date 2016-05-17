using NewsAggregator.Util;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;

namespace NewsAggregator.BackgroundWorkers
{
    public class NewsAggregatorScheduler
    {
        private DataDownloader downloader;
        private INewsDatabase database;

        private IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();

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
            Importer.ImportSourcesFromXML(AppDomain.CurrentDomain.BaseDirectory + @"Content\Sources.xml", database);
            Importer.ImportCommonWordsFile(database, AppDomain.CurrentDomain.BaseDirectory +@"Content\common_words.txt");
        }

        public void Start()
        {
            //RecurringJob.AddOrUpdate("UpdateJob", () => doUpdate(), "0 * * * *"); //Jede Stunde um X:00
            //RecurringJob.AddOrUpdate("SaveCurrentWordsToHistoryJob", () => saveCurrentWordsToHistory(), "0 23 * * *"); //Jeden Tag um 23:30

            Logger.Log("Starting up scheduler...");
            
            //DoUpdate
            IJobDetail doUpdateJob = JobBuilder.Create<doUpdateJob>()
                .WithIdentity("doUpdate", "defaultGroup")
                .Build();

            DateTime n = DateTime.Now;
            DateTime nextFullHour = new DateTime(n.Year, n.Month, n.Day, n.Hour + 1, 0, 0);

            ITrigger doUpdateTrigger = TriggerBuilder.Create()
              .WithIdentity("doUpdateTrigger", "defaultGroup")
              .StartAt(nextFullHour)
              .WithSimpleSchedule(x => x
                .WithIntervalInHours(1)
                .RepeatForever()) //Jede Stunde
              .ForJob(doUpdateJob)
              .Build();

            scheduler.ScheduleJob(doUpdateJob, doUpdateTrigger);
            
            //SaveCurrentWordsToHistory
            IJobDetail saveCurrentWordsToHistoryJob = JobBuilder.Create<saveCurrentWordsToHistoryJob>()
                .WithIdentity("saveCurrentWordsToHistory", "defaultGroup")
                .Build();

            DateTime todayAt2330 = new DateTime(n.Year, n.Month, n.Day, 23, 30, 0);

            ITrigger saveCurrentWordsToHistoryTrigger = TriggerBuilder.Create()
              .WithIdentity("saveCurrentWordsToHistoryTrigger", "defaultGroup")
              .StartAt(todayAt2330)
              //.WithCronSchedule("0 30 23 * * ?") //Geht nicht
              .WithSimpleSchedule(x => x
                .WithIntervalInHours(24)
                .RepeatForever())
              .ForJob(saveCurrentWordsToHistoryJob)
              .Build();

            scheduler.ScheduleJob(saveCurrentWordsToHistoryJob, saveCurrentWordsToHistoryTrigger);

            scheduler.Start();
            Logger.Log("Scheduler started");
        }

        public void doUpdate()
        {
            try
            {
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

        class doUpdateJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                new Thread(delegate () {
                    Logger.Log("Start doUpdate");
                    getInstance().doUpdate();
                    Logger.Log("End doUpdate");
                }).Start();
            }
        }

        class saveCurrentWordsToHistoryJob : IJob
        {
            public void Execute(IJobExecutionContext context)
            {
                new Thread(delegate () {
                    Logger.Log("Start saveCurrentWordsToHistory");
                    getInstance().saveCurrentWordsToHistory();
                    Logger.Log("End saveCurrentWordsToHistory");
                }).Start();
            }
        }
    }
}