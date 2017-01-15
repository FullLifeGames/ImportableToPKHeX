using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PKHeX.WinForms
{
    public class Pokemon
    {
        
        public string name = null;
        public string ability;
        public string evs = "";
        public string ivs = null;
        public List<string> moves = new List<string>();
        public string item = "(None)";
        public string nature;
        public string level = null;
        public bool shiny = false;
        public string gender = null;
        public string happiness = null;
        public string nickname = null;

    }
}
