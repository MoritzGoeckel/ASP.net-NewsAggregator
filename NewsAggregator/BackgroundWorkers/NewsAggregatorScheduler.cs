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

            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();

            scheduler.Start();
            
            //DoUpdate
            IJobDetail doUpdateJob = JobBuilder.Create<doUpdateJob>()
                .WithIdentity("doUpdate", "defaultGroup")
                .Build();

            ITrigger doUpdateTrigger = TriggerBuilder.Create()
              .WithIdentity("doUpdateTrigger", "defaultGroup")
              .StartNow()
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

            ITrigger saveCurrentWordsToHistoryTrigger = TriggerBuilder.Create()
              .WithIdentity("saveCurrentWordsToHistoryTrigger", "defaultGroup")
              .StartNow()
              .WithCronSchedule("0 30 23 * * ?") //Immer um 23:30. Kp ob es geht Todo
              .ForJob(saveCurrentWordsToHistoryJob)
              .Build();

            scheduler.ScheduleJob(saveCurrentWordsToHistoryJob, saveCurrentWordsToHistoryTrigger);
            
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