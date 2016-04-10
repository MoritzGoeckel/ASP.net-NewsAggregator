using NewsAggregator.BackgroundWorkers;
using NewsAggregator.BackgroundWorkers.Model;
using NewsAggregator.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NewsAggregator.Controllers
{
    public class WordsController : ApiController
    {
        private INewsDatabase database = GlobalDataManager.getInstance().getDatabase();

        public IEnumerable<WordCountPair> GetAllWords() //api/words
        {
            return database.GetCurrentWords(50);
        }

        public IEnumerable<WordCountPair> GetWords(int date) //api/words/date
        {
            return database.GetWords(50, DateTimeHelper.UnixTimeStampToDateTime(date));
        }

        public IEnumerable<DateCountPair> GetWords(string topic) //api/words/topic
        {
            return database.GetWordStatistic(topic); //Todo: implement
        }
    }
}
