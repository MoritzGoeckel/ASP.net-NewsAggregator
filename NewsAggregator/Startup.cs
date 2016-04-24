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
        public void Configuration(IAppBuilder app)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            GlobalDataManager.getInstance().init(new MongoFacade(path + @"Database\mongo\mongod.exe", path+ @"Database\data"));
            
            NewsAggregatorScheduler scheduler = NewsAggregatorScheduler.getInstance();
            scheduler.Start();
        }
    }
}
