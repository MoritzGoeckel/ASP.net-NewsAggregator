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

        [Route("api/words")]
        public IEnumerable<WordCountPair> GetAllWords() //api/words
        {
            return database.GetCurrentWords(100);
        }

        [Route("api/words/{date}")]
        public IEnumerable<WordCountPair> GetWords(int date) //api/words/date
        {
            return database.GetWords(100, DateTimeHelper.UnixTimeStampToDateTime(date));
        }

        [Route("api/words/statistic/{topic}")]
        public IEnumerable<DateCountPair> GetWords(string topic) //api/words/topic
        {
            return database.GetWordStatistic(topic); //Todo: implement
        }
    }
}
