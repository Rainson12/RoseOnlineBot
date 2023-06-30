using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoseOnlineBot.Models.Logic
{
    internal class ItemPrice
    {
        public ushort ItemId { get; set; }
        public uint Price { get; set; }
        public Single FoundCoordX { get; set; }
        public Single FoundCoordY { get; set; }
    }
}
