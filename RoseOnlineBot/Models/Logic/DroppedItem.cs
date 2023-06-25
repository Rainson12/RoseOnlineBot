using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoseOnlineBot.Models.Logic
{
    internal class DroppedItem
    {
        public UInt32 ItemNo { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public UInt16 ObjectId { get; set; }
    }
}
