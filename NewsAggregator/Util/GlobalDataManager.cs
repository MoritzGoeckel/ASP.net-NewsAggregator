using NewsAggregator.BackgroundWorkers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsAggregator.Util
{
    public class GlobalDataManager
    {
        private static GlobalDataManager instance;
        public static GlobalDataManager getInstance()
        {
            if (instance == null)
                instance = new GlobalDataManager();

            return instance;
        }

        private INewsDatabase database;

        public INewsDatabase getDatabase()
        {
            return database;
        }

        private GlobalDataManager()
        {

        }

        public void init(INewsDatabase database)
        {
            this.database = database;
        }
    }
}