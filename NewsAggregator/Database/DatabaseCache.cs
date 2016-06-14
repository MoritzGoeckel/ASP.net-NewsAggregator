using NewsAggregator.BackgroundWorkers;
using NewsAggregator.BackgroundWorkers.Model;
using NewsAggregator.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace NewsAggregator.Database
{
    public class DatabaseCache : IRegisteredObject
    {
        private Dictionary<string, List<Article>> articles = new Dictionary<string, List<Article>>();
        private List<WordData> words = new List<WordData>();

        private Dictionary<string, List<string>> WordImageUrls = new Dictionary<string, List<string>>();

        private Dictionary<string, List<DateCountPair>> wordStatistics = new Dictionary<string, List<DateCountPair>>();

        public DatabaseCache()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public void Stop(bool immediate)
        {
            HostingEnvironment.UnregisterObject(this);
        }

        private void setArticles(string word, List<Article> articles)
        {
            if (this.articles.ContainsKey(word) == false)
                this.articles.Add(word, articles);
            else
                this.articles[word] = articles;
        }

        private void setStatistic(string word, List<DateCountPair> statistics)
        {
            if (this.wordStatistics.ContainsKey(word) == false)
                this.wordStatistics.Add(word, statistics);
            else
                this.wordStatistics[word] = statistics;
        }

        public List<DateCountPair> GetWordStatistic(string word)
        {
            if (wordStatistics.ContainsKey(word) == false)
                return null;

            return this.wordStatistics[word];
        }

        public IEnumerable<Article> getArticlesToWord(string word)
        {
            if (articles.ContainsKey(word) == false)
                return null;

            return this.articles[word];
        }

        public IEnumerable<WordData> getCurrentWords()
        {
            return words;
        }

        private void setCurrentWords(List<WordData> words)
        {
            this.words = words;
        }

        public void clearCache()
        {
            this.words.Clear();
            this.articles.Clear();
        }

        public void UpdateCache(INewsDatabase database, int wordsCount = 100)
        {
            clearCache();

            List<WordData> words = database.GetCurrentWords(wordsCount);
            setCurrentWords(words);

            foreach (WordData pair in words)
            {
                new Thread(delegate ()
                {
                    setArticles(pair.Word, database.GetArticles(pair.Word, DateTime.Now, 50));
                }).Start();

                new Thread(delegate ()
                {
                    setStatistic(pair.Word, database.GetWordStatistic(pair.Word));
                }).Start();
            }

            new Thread(delegate ()
            {
                UpdateWordImageUrls();
            }).Start();
        }

        public void UpdateWordImageUrls()
        {
            foreach (WordData word in words)
            {
                try {
                    if (WordImageUrls.ContainsKey(word.Word) == false)
                    {
                        List<string> urls = MoritzImageSearchAPI.getGoogleImageSearchResult(word.Word);
                        WordImageUrls.Add(word.Word, new List<string>(urls));
                    }

                    word.imgUrls = WordImageUrls[word.Word];
                }
                catch { }
            }
        }
    }
}