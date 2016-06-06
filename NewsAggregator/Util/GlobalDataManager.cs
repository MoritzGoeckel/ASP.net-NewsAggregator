using NewsAggregator.BackgroundWorkers;
using NewsAggregator.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace NewsAggregator.Util
{
    public class GlobalDataManager : IRegisteredObject
    {
        private static GlobalDataManager instace;

        public static GlobalDataManager getInstance()
        {
            if (instace == null)
                instace = new GlobalDataManager();

            return instace;
        }

        private GlobalDataManager()
        {
            HostingEnvironment.RegisterObject(this);
        }

        private INewsDatabase database;
        private DatabaseCache cache;

        public void setData(INewsDatabase database, DatabaseCache cache)
        {
            this.database = database;
            this.cache = cache;
        }

        public INewsDatabase getDatabase()
        {
            return database;
        }

        public DatabaseCache getCache()
        {
            return cache;
        }

        public void Stop(bool immediate)
        {
            HostingEnvironment.UnregisterObject(this);
        }
    }
}