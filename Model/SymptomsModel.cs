using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot.Model
{
    public class SymptomsModel
    {
        public Question question { get; set; }
        public List<Condition> conditions { get; set; }
        public Extras2 extras { get; set; }
    }
    public class Choice
    {
        public string id { get; set; }
        public string label { get; set; }
    }

    public class Item
    {
        public string id { get; set; }
        public string name { get; set; }
        public List<Choice> choices { get; set; }
    }

    public class Extras
    {
    }

    public class Question
    {
        public string type { get; set; }
        public string text { get; set; }
        public List<Item> items { get; set; }
        public Extras extras { get; set; }
    }

    public class Condition
    {
        public string id { get; set; }
        public string name { get; set; }
        public string common_name { get; set; }
        public double probability { get; set; }
    }

    public class Extras2
    {
    }
}