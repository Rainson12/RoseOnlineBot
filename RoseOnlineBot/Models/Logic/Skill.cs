using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoseOnlineBot.Models.Logic
{
    internal class Skill
    {
        public Int16[] Ids { get; set; }
        public float Range { get; set; }
        public bool Enabled { get; set; } = true;
        public bool IsAOE { get; set; } = false;
        public Int16 Slot { get; set; }
        public int ManaCost { get; set; }
        public int CooldownInMilliseconds { get; set; }
        public DateTime? LastExecution { get; set; }
        public bool IsOnCooldown => LastExecution != null ? LastExecution.Value.AddMilliseconds(CooldownInMilliseconds) > DateTime.Now : false;
    }
}
