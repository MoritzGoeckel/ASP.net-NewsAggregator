using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using NewsAggregator.Util;
using NewsAggregator.BackgroundWorkers;
using System.IO;
using NewsAggregator.Database;
using System.Web.Hosting;

[assembly: OwinStartup(typeof(NewsAggregator.Startup))]

namespace NewsAggregator
{
    public partial class Startup
    {
        //Ändere zwei Attribute im Apppool auf 0 (Sekunden / Minuten) um die App nicht zu recyclen. Dadurch funktioniert dann vielleicht Quartz

        NewsAggregatorScheduler scheduler;
        public void Configuration(IAppBuilder app)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            GlobalDataManager.getInstance().setData(new MongoDBImplementation(path + @"Database\mongo\mongod.exe", path + @"Database\data"), new DatabaseCache());

            Logger.Log("Starting up scheduler...");
            scheduler = NewsAggregatorScheduler.getInstance();
            scheduler.Start();
            Logger.Log("Scheduler started");
            
            Logger.Log("Starting cache update...");
            GlobalDataManager.getInstance().getCache().UpdateCache(GlobalDataManager.getInstance().getDatabase());
            Logger.Log("Done cache update");
        }
    }
}
