using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsAggregator.BackgroundWorkers.Model
{
    [Serializable]
    public class WordData
    {
        public WordData(string Word, int Count)
        {
            this.Word = Word;
            this.Count = Count;
        }

        public string Word;
        public int Count;
        public List<string> imgUrls;
    }
}