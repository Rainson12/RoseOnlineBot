using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoseOnlineBot.Models.Logic
{
    internal class Inventory
    {
        public List<InventoryItem> EquipmentItems { get; set; }
        public List<InventoryItem> Consumables { get; set; }
        public List<InventoryItem> Materials { get; set; }
    }
    internal class InventoryItem
    {
        public ulong DBId { get; set; }
        public int Amount { get; set; }
        public int ItemId { get; set; }
        public int Slot { get; set; }
    }
}
