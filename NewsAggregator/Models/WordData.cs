using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsAggregator.BackgroundWorkers.Model
{
    [Serializable]
    public class WordData
    {
        public WordData(string Word, int Count, string imgUrl)
        {
            this.Word = Word;
            this.Count = Count;
            this.imgUrl = imgUrl;
        }

        public string Word;
        public int Count;
        public string imgUrl;
    }
}