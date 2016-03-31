using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NewsAggregator.BackgroundWorkers.Model;

namespace NewsAggregator.BackgroundWorkers
{
    public class MongoFacade : INewsDatabase
    {
        List<Article> INewsDatabase.GetArticles(DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }

        List<Article> INewsDatabase.GetArticles(string topic, DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }

        List<WordCountPair> INewsDatabase.GetCurrentWords(int count)
        {
            throw new NotImplementedException();
        }

        List<WordCountPair> INewsDatabase.GetWords(int count, string search)
        {
            throw new NotImplementedException();
        }

        List<WordCountPair> INewsDatabase.GetWords(int count, DateTime date)
        {
            throw new NotImplementedException();
        }

        List<DateCountPair> INewsDatabase.GetWordStatistic(DateTime from, DateTime to)
        {
            throw new NotImplementedException();
        }

        void INewsDatabase.InsertArticle(Article article)
        {
            throw new NotImplementedException();
        }

        void INewsDatabase.InsertArticles(List<Article> articles)
        {
            throw new NotImplementedException();
        }

        void INewsDatabase.PrepareDB()
        {
            throw new NotImplementedException();
        }

        void INewsDatabase.SaveCurrentWordsForHistory()
        {
            throw new NotImplementedException();
        }

        void INewsDatabase.UpdateCurrentWords()
        {
            throw new NotImplementedException();
        }
    }
}