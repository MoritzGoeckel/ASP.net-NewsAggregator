using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsAggregator.BackgroundWorkers.Model
{
    public class WordCountPair
    {
        public WordCountPair(string Word, int Count)
        {
            this.Word = Word;
            this.Count = Count;
        }

        public string Word;
        public int Count;
    }
}