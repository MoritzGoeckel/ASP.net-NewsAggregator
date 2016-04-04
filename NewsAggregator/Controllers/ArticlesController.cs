using NewsAggregator.BackgroundWorkers;
using NewsAggregator.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace NewsAggregator.Controllers
{
    public class ArticlesController : ApiController
    {
        private INewsDatabase database = GlobalDataManager.getInstance().getDatabase();

        public IEnumerable<Article> GetAllArticles() //api/articles
        {
            return database.GetArticles(DateTime.Now, 20);
        }

        public IEnumerable<Article> GetArticle(int startDate, int endDate) //api/articles/start/end
        {
            return database.GetArticles(DateTimeHelper.UnixTimeStampToDateTime(startDate), DateTimeHelper.UnixTimeStampToDateTime(endDate));
        }

        public IEnumerable<Article> GetArticle(string topic) //api/articles/topic
        {
            return database.GetArticles(topic, DateTime.Now, 20);
        }
    }
}
