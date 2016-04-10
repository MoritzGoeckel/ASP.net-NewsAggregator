using NewsAggregator.BackgroundWorkers.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NewsAggregator.BackgroundWorkers
{
    public class ArticleProcessor
    {
        public static Article processArticle(string Headline, string Summery, string Url, DateTime PublishDate, DateTime DownloadDate, TextSource Source)
        {
            return new Article(replaceSpecialChars(removeHTML(Headline)),
                replaceSpecialChars(removeHTML(Summery)),
                Url,
                PublishDate,
                DownloadDate,
                Source);
        }

        public static void getArticleWords(Article article, ref Dictionary<string, int> wordsListToUpdate)
        {
            string[] words = article.Headline.Split(' ');

            List<string> wordsOnlyOnce = new List<string>();
            foreach (string w in words)
                if (wordsOnlyOnce.Contains(w) == false)
                    wordsOnlyOnce.Add(w);

            foreach(string w in wordsOnlyOnce)
            {
                if(w != null && w != "" && w != " ")
                {
                    if (wordsListToUpdate.ContainsKey(w) == false)
                        wordsListToUpdate.Add(w, 1);
                    else
                        wordsListToUpdate[w]++;
                }
            }
        }

        public static void removeCommonWords(Dictionary<string, bool> commonWords, ref Dictionary<string, int> wordsListToUpdate)
        {
            foreach(KeyValuePair<string, bool> commonWordPair in commonWords)
            {
                if (wordsListToUpdate.ContainsKey(commonWordPair.Key))
                    wordsListToUpdate.Remove(commonWordPair.Key);
            }
        }

        public static string removeHTML(string text)
        {
            //Tags entfernen
            char[] array = new char[text.Length];
            int arrayIndex = 0;
            bool inside = false;

            for (int i = 0; i < text.Length; i++)
            {
                char let = text[i];
                if (let == '<')
                {
                    inside = true;
                    continue;
                }
                if (let == '>')
                {
                    inside = false;
                    continue;
                }

                if (!inside)
                {
                    array[arrayIndex] = let;
                    arrayIndex++;
                }
            }
            return new string(array, 0, arrayIndex);

            //Regex.Replace(inputHTML, @"<[^>]+>|&nbsp;", "").Trim();
            //string noHTMLNormalised = Regex.Replace(noHTML, @"\s{2,}", " ");
        }

        public static string replaceSpecialChars(string text)
        {
            Dictionary<string, string> specialHtmlChars = new Dictionary<string, string>();
            specialHtmlChars.Add("&nbsp;", " ");
            specialHtmlChars.Add("&auml;", "ä");
            specialHtmlChars.Add("&ouml;", "ö");
            specialHtmlChars.Add("&uuml;", "ü");
            specialHtmlChars.Add("&Auml;", "Ä");
            specialHtmlChars.Add("&Ouml;", "Ö");
            specialHtmlChars.Add("&Uuml;;", "Ü");
            specialHtmlChars.Add("&szlig;", "ß");

            //Umlaute ersetzen
            foreach (KeyValuePair<string, string> pair in specialHtmlChars)
                text = text.Replace(pair.Key, pair.Value);

            return text;
        }

        public static string removeSpecialChars(string text)
        {
            string newText = "";
            int i = 0;
            while (i < text.Length)
            {
                if (i == 0 || text[i] != ' ' || text[i - 1] != ' ')
                {
                    if (",.:!?/()[]{}#'*+\"-_\n".Contains(text[i]) == false)
                        newText += text[i];
                    else newText += " ";
                }
                i++;
            }

            return newText;
        }

        public static List<string> getWordList(string text)
        {
            List<string> words = new List<string>();
            string[] ws = removeSpecialChars(text).Split(' ');
            foreach (string word in ws)
            {
                if (word != null && word != "" && word != " " && words.Contains(word) == false)
                    words.Add(word);
            }

            return words;
        }
    }
}