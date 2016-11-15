using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace IntelligentEditing.Models
{
    public class TextFileModel
    {
        [Required]
        public HttpPostedFileBase TextFile { get; set; }
    }

    public class RepeatedPhrase
    {
        public string Phrase { get; set; }
        public int Count { get; set; }
        public string Description { get {return $"[{Phrase} (x {Count})]"; } }
    }

    public class TextFile
    {
        public DateTime Date { get; set; }
        public string FileName { get; set; }
        public string Content { get; set; }
        
        /// <summary>
        /// Returns whether the text content only contains English characters
        /// </summary>
        public bool IsEnglish()
        {
            Regex regex = new Regex("[A-Za-z0-9 .,-=+%£(){}\\[\\]\\\'\\s\"@]");
            MatchCollection matches = regex.Matches(Content);
        
            return matches.Count.Equals(Content.Length);
        }

        /// <summary>
        /// Returns number of words in file
        /// </summary>
        public int GetWordCount()
        {
            return Content.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }
        
        /// <summary>
        /// Gets string description of repeated phrases
        /// </summary>
        public string GetRepeatedPhraseDescription()
        {
            return string.Join("\n", FindRepeatedPhrases().Select(r => r.Description));
        }

        /// <summary>
        /// Gets minimum word count from web.config
        /// </summary>
        public static int GetMinimumWordCount()
        {
            return Convert.ToInt32(ConfigurationManager.AppSettings["Minimum.Word.Count"]);
        }
        /// <summary>
        /// Gets phrase length from web.config
        /// </summary>
        private int GetMinimumPhraseLength()
        {
            return Convert.ToInt32(ConfigurationManager.AppSettings["Repeated.Phrase.Length"]);
        }

        /// <summary>
        /// Returns list of repeated phrases 
        /// </summary>
        private List<RepeatedPhrase> FindRepeatedPhrases()
        {
            var split = Content.Split(' ');
            var PhraseLength = GetMinimumPhraseLength();

            List<string> words = new List<string>();
            
            words.AddRange(split
                .Select((x, i) => new { Value = x, Index = i })
                .Select(x => Combine(split, x.Index, x.Index + PhraseLength - 1)));
            
            return words
                .GroupBy(x => x)
                .Where(x => x.Count() > 1)
                .Select(x => new RepeatedPhrase{ Phrase = x.Key, Count = x.Count() }).ToList();
        }

        private string Combine(IEnumerable<string> source, int startIndex, int endIndex)
        {
            return string.Join(" ", source.Where((x, i) => i >= startIndex && i <= endIndex));
        }
        
    }
}