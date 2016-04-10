using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NewsAggregator.BackgroundWorkers.Model;
using System.Diagnostics;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using NewsAggregator.Util;

namespace NewsAggregator.BackgroundWorkers
{
    public class MongoFacade : INewsDatabase
    {
        //Todo: Idee Doubletten-Erkennung

        Dictionary<string, TextSource> cachedSources;

        Dictionary<string, bool> cachedCommonWords; 

        MongoClient client;
        MongoServer server;
        MongoDatabase database;
        Process serverProcess;

        MongoCollection articles, words, sources, commonWords;

        public MongoFacade(string mongodPath, string databasePath)
        {
            serverProcess = new Process();
            serverProcess.StartInfo.FileName = mongodPath;
            serverProcess.StartInfo.Arguments = "--dbpath \""+databasePath+"\"";

            serverProcess.Start();

            client = new MongoClient();
            server = client.GetServer();
            database = server.GetDatabase("tester");

            articles = database.GetCollection("articles");
            words = database.GetCollection("words");
            commonWords = database.GetCollection("commonWords");
            sources = database.GetCollection("sources");

            LoadSources();
            LoadCommonWords();
        }

        private string getSearchString(string word)
        {
            return "[^ ]*" + word + "[^ ]*";
        }

        void INewsDatabase.PrepareDB()
        {
            /*
            
            var docsDarunter = collection.Find(Query.LT("nr", 4)).SetSortOrder(SortBy.Descending("nr")).SetLimit(1);
            BsonDocument darunter = docsDarunter.ToList<BsonDocument>()[0];

            var docsDarüber = collection.Find(Query.GT("nr", 4)).SetSortOrder(SortBy.Ascending("nr")).SetLimit(1);
            BsonDocument darüber = docsDarüber.ToList<BsonDocument>()[0];

            Console.WriteLine(darunter["str"] + " " + darüber["str"]);



            //Bereich auslesen
            var docsBerreich = collection.Find(Query.And(Query.GT("nr", 1), Query.LT("nr", 5))).SetSortOrder(SortBy.Ascending("nr"));
            foreach (var d in docsBerreich)
                Console.WriteLine(d["str"]);

            */
        }

        List<Article> INewsDatabase.GetArticles(DateTime to, int count)
        {
            List<Article> returnArticles = new List<Article>();
            var docs = articles.FindAs<BsonDocument>(
                    Query.LTE("date", DateTimeHelper.DateTimeToUnixTimestamp(to))
                ).SetLimit(count);

            foreach (var d in docs)
                returnArticles.Add(new Article(d, this)); //Todo: Bson to Article

            return returnArticles;
        }

        List<Article> INewsDatabase.GetArticles(DateTime from, DateTime to, int count)
        {
            List<Article> returnArticles = new List<Article>();
            var docs = articles.FindAs<BsonDocument>(
                Query.And(
                    Query.GT("date", DateTimeHelper.DateTimeToUnixTimestamp(from)),
                    Query.LTE("date", DateTimeHelper.DateTimeToUnixTimestamp(to))
                    )
                ).SetLimit(count);

            foreach (var d in docs)
                returnArticles.Add(new Article(d, this)); //Todo: Bson to Article

            return returnArticles;
        }

        List<Article> INewsDatabase.GetArticles(DateTime from, DateTime to)
        {
            List<Article> returnArticles = new List<Article>();
            var docs = articles.FindAs<BsonDocument>(
                Query.And(
                    Query.GT("date", DateTimeHelper.DateTimeToUnixTimestamp(from)),
                    Query.LTE("date", DateTimeHelper.DateTimeToUnixTimestamp(to))
                    )
                );

            foreach (var d in docs)
                returnArticles.Add(new Article(d, this)); //Todo: Bson to Article

            return returnArticles;
        }

        List<Article> INewsDatabase.GetArticles(string word, DateTime to, int count)
        {
            List<Article> returnArticles = new List<Article>();
            var docs = articles.FindAs<BsonDocument>(
                Query.And(
                    Query.LTE("date", DateTimeHelper.DateTimeToUnixTimestamp(to)),
                    Query.Or(Query.Matches("headline", getSearchString(word)), Query.Matches("summery", getSearchString(word)))
                    )
                ).SetLimit(count);

            foreach (var d in docs)
                returnArticles.Add(new Article(d, this)); //Todo: Bson to Article

            return returnArticles;
        }

        List<Article> INewsDatabase.GetArticles(string word, DateTime from, DateTime to, int count)
        {
            List<Article> returnArticles = new List<Article>();
            var docs = articles.FindAs<BsonDocument>(
                Query.And(
                    Query.GT("date", DateTimeHelper.DateTimeToUnixTimestamp(from)),
                    Query.LTE("date", DateTimeHelper.DateTimeToUnixTimestamp(to)),
                    Query.Or(Query.Matches("headline", getSearchString(word)), Query.Matches("summery", getSearchString(word)))
                    )
                ).SetLimit(count);

            foreach (var d in docs)
                returnArticles.Add(new Article(d, this)); //Todo: Bson to Article

            return returnArticles;
        }

        List<WordCountPair> INewsDatabase.GetWords(int count, string search)
        {
            //Todo: Complicated because I need to add up all double accuring words
            throw new NotImplementedException();
        }

        List<DateCountPair> INewsDatabase.GetWordStatistic(string word)
        {
            //Todo: Generated Grahpic, cache it. Make it fast
            throw new NotImplementedException();
        }

        List<WordCountPair> INewsDatabase.GetCurrentWords(int count)
        {
            List<WordCountPair> returnWords = new List<WordCountPair>();
            var docs = words.FindAs<BsonDocument>(Query.EQ("date", "current")).SetLimit(count);
            foreach(var d in docs)
                returnWords.Add(new WordCountPair(d["word"].AsString, d["count"].AsInt32));

            return returnWords;
        }

        List<WordCountPair> INewsDatabase.GetWords(int count, DateTime date)
        {
            List <WordCountPair> returnWords = new List<WordCountPair>();
            var docs = words.FindAs<BsonDocument>(
                Query.And(
                    Query.GT("date", DateTimeHelper.DateTimeToUnixTimestamp(date.Subtract(new TimeSpan(24, 0, 0)))),
                    Query.LTE("date", DateTimeHelper.DateTimeToUnixTimestamp(date))
                    )
                ).SetLimit(count);

            foreach (var d in docs)
                returnWords.Add(new WordCountPair(d["word"].AsString, d["count"].AsInt32));

            return returnWords;
        }

        void INewsDatabase.InsertArticle(Article article)
        {
            articles.Insert(article.toBson());
        }

        void INewsDatabase.InsertArticles(List<Article> articles)
        {
            List<BsonDocument> docs = new List<BsonDocument>();
            foreach (Article a in articles)
                docs.Add(a.toBson());

            this.articles.InsertBatch(docs);
        }

        void INewsDatabase.SaveCurrentWordsForHistory() //Todo: code doublication to UpdateCurrentWords
        {
            Dictionary<string, int> words = new Dictionary<string, int>();

            DateTime hoursAgo24 = DateTime.Now;
            hoursAgo24.Subtract(new TimeSpan(24, 0, 0));

            List<Article> articles = (this as INewsDatabase).GetArticles(hoursAgo24, DateTime.Now);
            foreach (Article a in articles)
                ArticleProcessor.getArticleWords(a, ref words);

            ArticleProcessor.removeCommonWords(cachedCommonWords, ref words);

            List<BsonDocument> docs = new List<BsonDocument>();
            foreach (KeyValuePair<string, int> word in words)
            {
                docs.Add(new BsonDocument(new List<BsonElement>() {
                    new BsonElement("date", DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now)),
                    new BsonElement("word", word.Key),
                    new BsonElement("count", word.Value)
                }));
            }

            this.words.InsertBatch(docs);
        }

        void INewsDatabase.UpdateCurrentWords()
        {
            Dictionary<string, int> words = new Dictionary<string, int>();

            DateTime hoursAgo24 = DateTime.Now;
            hoursAgo24.Subtract(new TimeSpan(24, 0, 0));

            List<Article> articles = (this as INewsDatabase).GetArticles(hoursAgo24, DateTime.Now);
            foreach(Article a in articles)
                ArticleProcessor.getArticleWords(a, ref words);

            ArticleProcessor.removeCommonWords(cachedCommonWords, ref words);

            this.words.Remove(Query.EQ("date", "current"));

            List<BsonDocument> docs = new List<BsonDocument>();
            foreach(KeyValuePair<string, int> word in words)
            {
                docs.Add(new BsonDocument(new List<BsonElement>() {
                    new BsonElement("date", "current"),
                    new BsonElement("word", word.Key),
                    new BsonElement("count", word.Value)
                }));
            }

            this.words.InsertBatch(docs);
        }

        void INewsDatabase.Shutdown()
        {
            server.Shutdown();

            while (serverProcess.HasExited == false)
            {
                Console.Write(".");
                System.Threading.Thread.Sleep(300);
            }
        }

        Dictionary<string, TextSource> INewsDatabase.GetSources()
        {
            if (cachedSources == null)
                LoadSources();

            return cachedSources;
        }

        void LoadSources()
        {
            cachedSources = new Dictionary<string, TextSource>();

            var docs = sources.FindAllAs<BsonDocument>();

            foreach (var d in docs)
            {
                TextSource source = new TextSource(d);
                cachedSources.Add(source.getID(), source);
            }
        }

        void INewsDatabase.InsertSource(TextSource source)
        {
            var docs = sources.FindAs<BsonDocument>(Query.EQ("id", source.getID()));
            if (docs.Count() != 0)
                throw new Exception("ID already taken!");

            sources.Insert(source.toBson());
        }

        Dictionary<string, bool> INewsDatabase.GetCommonWords()
        {
            if (cachedCommonWords == null)
                LoadCommonWords();

            return cachedCommonWords;
        }

        void LoadCommonWords()
        {
            cachedCommonWords = new Dictionary<string, bool>();

            var docs = commonWords.FindAs<BsonDocument>(Query.Null);

            foreach (var e in docs)
                cachedCommonWords.Add(e["word"].AsString, true);
        }

        void INewsDatabase.InsertCommonWords(string word)
        {
            commonWords.Insert(new BsonDocument(new BsonElement("word", word)));
        }
    }
}