using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LuisBot.Model
{
    public class DiaognisModel
    {
        public string sex { get; set; }
        public int age { get; set; }
        public List<Evidence> evidence { get; set; }
    }
    public class Evidence
    {
        public string id { get; set; }
        public string choice_id { get; set; }
    }
}