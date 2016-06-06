using NewsAggregator.BackgroundWorkers;
using NewsAggregator.BackgroundWorkers.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace NewsAggregator.Database
{
    public class DatabaseCache : IRegisteredObject
    {
        private Dictionary<string, IEnumerable<Article>> articles = new Dictionary<string, IEnumerable<Article>>();
        private List<WordCountPair> words = new List<WordCountPair>();

        public DatabaseCache()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public void Stop(bool immediate)
        {
            HostingEnvironment.UnregisterObject(this);
        }

        public IEnumerable<Article> getArticlesToWord(string word)
        {
            if (articles.ContainsKey(word) == false)
                return null;

            return this.articles[word];
        }

        private void setArticles(string word, IEnumerable<Article> articles)
        {
            if (this.articles.ContainsKey(word) == false)
                this.articles.Add(word, articles);
            else
                this.articles[word] = articles;
        }

        public IEnumerable<WordCountPair> getCurrentWords()
        {
            return words;
        }

        private void setCurrentWords(List<WordCountPair> words)
        {
            this.words = words;
        }

        public void clearCache()
        {
            this.words.Clear();
            this.articles.Clear();
        }

        public void UpdateCache(INewsDatabase database)
        {
            clearCache();

            List<WordCountPair> words = database.GetCurrentWords(100);
            setCurrentWords(words);

            foreach (WordCountPair pair in words)
                setArticles(pair.Word, database.GetArticles(pair.Word, DateTime.Now, 50));
        }
    }
}