using RoseOnlineBot.Models.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RoseOnlineBot
{
    public class DataWrapper
    {
        public List<System.Timers.Timer> Timers = new List<System.Timers.Timer>();
        public List<Thread> Threads = new List<Thread>();

        public List<System.Timers.Timer> WayTimer = new List<System.Timers.Timer>();
        public List<Thread> WayThread = new List<Thread>();

        public List<WayPoint> RecordedWaypoints;
    }
}
