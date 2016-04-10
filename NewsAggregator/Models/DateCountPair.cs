using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsAggregator.BackgroundWorkers.Model
{
    [Serializable]
    public class DateCountPair
    {
        public DateTime date;
        public int count;

        public DateCountPair(DateTime date, int count)
        {
            this.date = date;
            this.count = count;
        }
    }
}