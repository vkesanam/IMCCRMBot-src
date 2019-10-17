using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot.Model
{
    public class QnAAnswer
    {
        public IList<Answer> answers { get; set; }
    }
    public class Metadata
    {
        public string name { get; set; }
        public string value { get; set; }
    }

    public class Answer
    {
        public IList<string> questions { get; set; }
        public string answer { get; set; }
        public double score { get; set; }
        public int id { get; set; }
        public string source { get; set; }
        public IList<object> keywords { get; set; }
        public IList<Metadata> metadata { get; set; }
    }
}