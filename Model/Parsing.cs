using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot.Model
{
    public class Parsing
    {
        public bool obvious { get; set; }
        public List<Mention> mentions { get; set; }
    }
    public class Mention
    {
        public string orth { get; set; }
        public string common_name { get; set; }
        public string type { get; set; }
        public string choice_id { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }
}