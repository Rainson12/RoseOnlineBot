using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoseOnlineBot.Models.Metadata
{
    internal class UsableItem
    {
        public ushort ItemId { get; set; }
        public string Name { get; set; }
        public ItemType ItemType { get; set; }
        public uint RestoreAmount { get; set; }
        public byte RequiredLevel { get; set; }

    }
    internal enum ItemType
    {
        Food,
        Drink,
        HPPotion,
        MPPotion,
        Repair,
        Unkown
    }
}
