using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;

using Hangfire;
using NewsAggregator.Util;
using NewsAggregator.BackgroundWorkers;

[assembly: OwinStartup(typeof(NewsAggregator.Startup))]

namespace NewsAggregator
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //Hangfire
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(@"Server=(localdb)\MSSQLLocalDB; Database=hangfireDb;");

            app.UseHangfireDashboard("/hangfire");
            app.UseHangfireServer();

            GlobalDataManager.getInstance().init(new MongoFacade(@"E:\Programmieren\C# Neu\NewsAggregator\Database\mongo\mongod.exe", @"E:\Programmieren\C# Neu\NewsAggregator\Database\data"));
            
            NewsAggregatorScheduler scheduler = NewsAggregatorScheduler.getInstance();
            scheduler.Start();
        }
    }
}
