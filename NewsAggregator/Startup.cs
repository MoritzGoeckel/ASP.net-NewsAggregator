using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using NewsAggregator.Util;
using NewsAggregator.BackgroundWorkers;
using System.IO;

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
            GlobalDataManager.getInstance().init(new MongoDBImplementation(path + @"Database\mongo\mongod.exe", path + @"Database\data"));

            scheduler = NewsAggregatorScheduler.getInstance();
            scheduler.Start();
        }
    }
}
