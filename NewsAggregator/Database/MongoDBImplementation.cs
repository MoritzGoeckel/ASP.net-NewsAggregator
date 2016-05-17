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
    public class MongoDBImplementation : INewsDatabase
    {
        //Todo: Idee Doubletten-Erkennung

        Dictionary<string, TextSource> cachedSources;

        Dictionary<string, bool> cachedCommonWords; 

        MongoClient client;
        MongoServer server;
        MongoDatabase database;
        Process serverProcess;

        MongoCollection articles, words, sources, commonWords;

        public MongoDBImplementation(string mongodPath, string databasePath)
        {
            serverProcess = new Process();
            serverProcess.StartInfo.FileName = mongodPath;
            serverProcess.StartInfo.Arguments = "--dbpath \""+databasePath+"\"";

            serverProcess.Start();

            client = new MongoClient();
            server = client.GetServer();
            database = server.GetDatabase("testDB"); //Todo: Real name

            articles = database.GetCollection("articles");
            words = database.GetCollection("words");
            commonWords = database.GetCollection("commonWords");
            sources = database.GetCollection("sources");

            LoadSources();
            LoadCommonWords();
        }

        private string getSearchString(string word)
        {
            return ".*[ -]" + word + ".*" + "|" +
                   ".*" + word + "[ -].*" + "|" +

                   ".*[ -]" + word.Substring(0, 1).ToUpper() + word.Substring(1) +".*" + "|" +
                   ".*" + word.Substring(0, 1).ToUpper() + word.Substring(1) + "[ -].*" + "|" +

                   ".*[ -]" + word.ToUpper() + ".*" + "|" +
                   ".*" + word.ToUpper() + "[ -].*";
        }

        void INewsDatabase.PrepareDB()
        {
            articles.CreateIndex(IndexKeys.Ascending("id"), IndexOptions.SetUnique(true));
            commonWords.CreateIndex(IndexKeys.Ascending("word"), IndexOptions.SetUnique(true));
        }

        List<Article> INewsDatabase.GetArticles(DateTime to, int count)
        {
            List<Article> returnArticles = new List<Article>();
            var docs = articles.FindAs<BsonDocument>(
                    Query.LTE("downloaded", DateTimeHelper.DateTimeToUnixTimestamp(to))
                ).SetLimit(count);

            foreach (var d in docs)
                returnArticles.Add(new Article(d, this));

            return returnArticles;
        }

        List<Article> INewsDatabase.GetArticles(DateTime from, DateTime to, int count)
        {
            List<Article> returnArticles = new List<Article>();
            var docs = articles.FindAs<BsonDocument>(
                Query.And(
                    Query.GT("downloaded", DateTimeHelper.DateTimeToUnixTimestamp(from)),
                    Query.LTE("downloaded", DateTimeHelper.DateTimeToUnixTimestamp(to))
                    )
                ).SetLimit(count);

            foreach (var d in docs)
                returnArticles.Add(new Article(d, this));

            return returnArticles;
        }

        List<Article> INewsDatabase.GetArticles(DateTime from, DateTime to)
        {
            List<Article> returnArticles = new List<Article>();
            var docs = articles.FindAs<BsonDocument>(
                Query.And(
                    Query.GT("downloaded", DateTimeHelper.DateTimeToUnixTimestamp(from)),
                    Query.LTE("downloaded", DateTimeHelper.DateTimeToUnixTimestamp(to))
                    )
                );

            foreach (var d in docs)
                returnArticles.Add(new Article(d, this));

            return returnArticles;
        }

        List<Article> INewsDatabase.GetArticles(string word, DateTime to, int count) //Geht nicht TODO: ???
        {
            List<Article> returnArticles = new List<Article>();
            var docs = articles.FindAs<BsonDocument>(
                Query.And(
                    Query.LTE("downloaded", DateTimeHelper.DateTimeToUnixTimestamp(to)),
                    Query.Or(Query.Matches("headline", getSearchString(word)), Query.Matches("summery", getSearchString(word)))
                    )
                ).SetLimit(count);

            foreach (var d in docs)
                returnArticles.Add(new Article(d, this));

            return returnArticles;
        }

        List<Article> INewsDatabase.GetArticles(string word, DateTime from, DateTime to, int count)
        {
            List<Article> returnArticles = new List<Article>();
            var docs = articles.FindAs<BsonDocument>(
                Query.And(
                    Query.GT("downloaded", DateTimeHelper.DateTimeToUnixTimestamp(from)),
                    Query.LTE("downloaded", DateTimeHelper.DateTimeToUnixTimestamp(to)),
                    Query.Or(Query.Matches("headline", getSearchString(word)), Query.Matches("summery", getSearchString(word)))
                    )
                ).SetLimit(count);

            foreach (var d in docs)
                returnArticles.Add(new Article(d, this));

            return returnArticles;
        }

        List<DateCountPair> INewsDatabase.GetWordStatistic(string word)
        {
            List<DateCountPair> wordCountPairs = new List<DateCountPair>();

            var results = words.FindAs<BsonDocument>(
                Query.And(
                    Query.GTE("date", DateTimeHelper.DateTimeToUnixTimestamp(DateTime.Now.Subtract(TimeSpan.FromDays(100)))),
                    Query.Not(Query.EQ("date", "current")),
                    Query.EQ("word", word)
                    )
                ).SetSortOrder(SortBy.Ascending("date"));

            foreach (var wordDate in results)
                wordCountPairs.Add(new DateCountPair(DateTimeHelper.UnixTimeStampToDateTime(wordDate["date"].AsDouble), wordDate["count"].AsInt32));

            return wordCountPairs;
        }

        List<WordCountPair> INewsDatabase.GetCurrentWords(int count)
        {
            List<WordCountPair> returnWords = new List<WordCountPair>();
            var docs = words.FindAs<BsonDocument>(Query.EQ("date", "current"))
                .SetSortOrder(SortBy.Descending("count"))
                .SetLimit(count);

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
                ).SetSortOrder(SortBy.Descending("count"))
                .SetLimit(count);

            foreach (var d in docs)
                returnWords.Add(new WordCountPair(d["word"].AsString, d["count"].AsInt32));

            return returnWords;
        }

        void INewsDatabase.InsertArticle(Article article)
        {
            articles.Insert(article.toBson(), new MongoInsertOptions { Flags = InsertFlags.ContinueOnError, WriteConcern = WriteConcern.Unacknowledged });
        }

        void INewsDatabase.InsertArticles(List<Article> articles)
        {
            List<BsonDocument> docs = new List<BsonDocument>();
            foreach (Article a in articles)
                docs.Add(a.toBson());

            this.articles.InsertBatch(docs, new MongoInsertOptions { Flags = InsertFlags.ContinueOnError, WriteConcern = WriteConcern.Unacknowledged });
        }

        void INewsDatabase.SaveCurrentWordsForHistory() //Todo: code doublication to UpdateCurrentWords
        {
            Dictionary<string, int> words = new Dictionary<string, int>();

            DateTime hoursAgo24 = DateTime.Now;
            hoursAgo24 = hoursAgo24.Subtract(new TimeSpan(24, 0, 0));

            List<Article> articles = (this as INewsDatabase).GetArticles(hoursAgo24, DateTime.Now);
            foreach (Article a in articles)
                ArticleProcessor.getArticleWords(a, ref words);

            ArticleProcessor.removeCommonWords((this as INewsDatabase).GetCommonWords(), ref words);

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
            hoursAgo24 = hoursAgo24.Subtract(new TimeSpan(24, 0, 0));

            List<Article> articles = (this as INewsDatabase).GetArticles(hoursAgo24, DateTime.Now);
            foreach(Article a in articles)
                ArticleProcessor.getArticleWords(a, ref words);

            ArticleProcessor.removeCommonWords((this as INewsDatabase).GetCommonWords(), ref words);

            List<BsonDocument> docs = new List<BsonDocument>();
            foreach(KeyValuePair<string, int> word in words)
            {
                docs.Add(new BsonDocument(new List<BsonElement>() {
                    new BsonElement("date", "current"),
                    new BsonElement("word", word.Key),
                    new BsonElement("count", word.Value)
                }));
            }

            this.words.Remove(Query.EQ("date", "current"));
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
            commonWords.Insert(new BsonDocument(new BsonElement("word", word)), new MongoInsertOptions { Flags = InsertFlags.ContinueOnError, WriteConcern = WriteConcern.Unacknowledged });
        }

        void INewsDatabase.AddDownloadErrorToSource(TextSource source)
        {
            sources.Update(Query.EQ("id", source.getID()), Update.Inc("error", 1)); //UpdateFlags.Multi
        }

        //Not implemented
        List<WordCountPair> INewsDatabase.GetWords(int count, string search)
        {
            //Todo: Complicated because I need to add up all double accuring words ????
            throw new NotImplementedException();
        }
    }
}