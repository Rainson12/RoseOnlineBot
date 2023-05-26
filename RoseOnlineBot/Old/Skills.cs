using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RoseOnlineBot
{
    public class PlayerClass
    {
        public string ClassName;
        public List<Skill> skills = new List<Skill>();
        public List<Skill> buffs = new List<Skill>();
    }
    public class Skill
    {
        public Keys Key;
        public int Interval;
    }
}
