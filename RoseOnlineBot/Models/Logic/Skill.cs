using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoseOnlineBot.Models.Logic
{
    internal class Skill
    {
        public int Id { get; set; }
        public int CooldownInMilliseconds { get; set; }
        public DateTime? LastExecution { get; set; }
        public bool IsOnCooldown => LastExecution != null ? LastExecution.Value.AddMilliseconds(CooldownInMilliseconds) < DateTime.Now : true;
    }
}
