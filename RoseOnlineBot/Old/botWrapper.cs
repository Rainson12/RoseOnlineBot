//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Timers;
//using System.Windows.Forms;
//using System.Diagnostics;
//using System.Threading;

//namespace RoseOnlineBot
//{
//    class botWrapper
//    {

//        Process ROSEON;
//        IntPtr ProcessHandle;
//        IntPtr AggroActionPtr = IntPtr.Zero;
//        IntPtr BaseAddress;
//        Communication pipeServer;
//        DataWrapper data;                
//        Int32 lastMobKilled = 0;
//        String WaypointPath;
//        DateTime lastattack = new DateTime();
//        List<Waypoint> waypoints;
//        List<int> monsterIds = new List<int>();
//        memFunc memFunc = new memFunc();


//        public botWrapper(Communication _pipeServer, Process _ROSEON, IntPtr _ProcessHandle, DataWrapper _data, string _WaypointPath)
//        {
//            pipeServer = _pipeServer;
//            ROSEON = _ROSEON;
//            data = _data;
//            ProcessHandle = _ProcessHandle;
//            WaypointPath = _WaypointPath;
//        }
//        public void start()
//        {
//            waypoints = null; // memFunc.readWaypoints(WaypointPath);      
//            BaseAddress = ROSEON.MainModule.BaseAddress;
//            AggroActionPtr = BaseAddress + 3303360;
//            Thread HPMonitor = new Thread(new ThreadStart(monitorHP));
//            HPMonitor.Start();
//            data.Threads.Add(HPMonitor);
//            Thread AggroMonitor = new Thread(new ThreadStart(getAggro));
//            AggroMonitor.Start();
//            data.Threads.Add(AggroMonitor);
//            Stopwatch watch1 = new Stopwatch();
//            Stopwatch buff = new Stopwatch();
//            buff.Start();
//            watch1.Start();
//            buff.Reset();
//            memFunc.sendKey(Keys.F6, ROSEON.MainWindowHandle);
//            memFunc.sendKey(Keys.F6, ROSEON.MainWindowHandle);
//            buff.Start();
//            while (true && (watch1.Elapsed.Hours * 60 + watch1.Elapsed.Minutes) < 500)
//            {
//                foreach (Waypoint w in waypoints)
//                {
//                    byte[] packet = null;//  memFunc.calcPacketForMove(w.coordX, w.coordY);
//                    this.pipeServer.SendMessage(packet);
//                    Thread.Sleep(650);
//                    bool reSentPacket = false;
//                    while (reachedWaypoint(w.coordX, w.coordY) == false)
//                    {
//                        if (buff.Elapsed.TotalSeconds > (60 * 3))
//                        {
//                            buff.Reset();
//                            memFunc.sendKey(Keys.F6, ROSEON.MainWindowHandle);
//                            Thread.Sleep(1500);
//                            buff.Start();
//                        }
//                        int action = getAction();
//                        if (reSentPacket == true && action == 0) // stuck
//                        {
//                            Thread.Sleep(1000);
//                            break;
//                        }
//                        if (monsterIds.Count <= 0)
//                        {
//                            memFunc.sendKey(Keys.F2, ROSEON.MainWindowHandle);
//                            Thread.Sleep(175);
//                            int target = getTarget();
//                            if (target != 0 && target != lastMobKilled)
//                            {
//                                if (!monsterIds.Contains(target))
//                                    monsterIds.Add(target);
//                            }
//                        }
//                        if (monsterIds.Count > 0)  // wenn in der liste ein monster ist
//                        {

//                            while (monsterIds.Count != 0)// nächstes monster in der liste killn + Adds killen
//                            {
//                                setTarget(monsterIds[0]);
//                                if (lastattack.Year == 1 && lastattack.Day == 1)
//                                {
//                                    lastattack = DateTime.Now;
//                                }
//                                memFunc.sendKey(Keys.F1, ROSEON.MainWindowHandle);
//                                Thread.Sleep(1200);
//                                int actiona = getAction();
//                                while (actiona != 0)
//                                {
//                                    if (DateTime.Now.Subtract(lastattack).Seconds > 2 && actiona != 0)
//                                    {
//                                        memFunc.sendKey(Keys.F3, ROSEON.MainWindowHandle); // attack
//                                        Thread.Sleep(1000);
//                                        lastattack = DateTime.Now;
//                                    }
//                                    if (buff.Elapsed.TotalSeconds > (60 * 3))
//                                    {
//                                        buff.Reset();
//                                        memFunc.sendKey(Keys.F6, ROSEON.MainWindowHandle);
//                                        Thread.Sleep(1500);
//                                        buff.Start();
//                                    }
//                                    actiona = getAction();
//                                }
//                                lastMobKilled = monsterIds[0];
//                                monsterIds.RemoveAt(0);
//                                lastattack = new DateTime();
//                                setTarget(0);
//                                Thread.Sleep(150);
//                                memFunc.sendKey(Keys.F8, ROSEON.MainWindowHandle);
//                                Thread.Sleep(1200);
//                                memFunc.sendKey(Keys.F8, ROSEON.MainWindowHandle);
//                                Thread.Sleep(1200);
//                            }
//                        }
//                        Thread.Sleep(500);
//                        this.pipeServer.SendMessage(packet);
//                        reSentPacket = true;
//                    }

//                }
//            }
//            ROSEON.Kill();
//        }
//        #region methods
//        private void monitorHP()
//        {
//            while (true)
//            {
//                Int16 HP = getHP();
//                if (HP < 4500)
//                {
//                    memFunc.sendKey(Keys.F4, ROSEON.MainWindowHandle); // ATTACK
//                    memFunc.sendKey(Keys.F5, ROSEON.MainWindowHandle); // ATTACK
//                    Thread.Sleep(10 * 1000);
//                }
//                Thread.Sleep(500);
//            }
//        }
//        private Int16 getHP()
//        {
//            IntPtr baseOfGame = BaseAddress + 3303360;
//            byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0x00, 0x684 }, baseOfGame, ProcessHandle);
//            return BitConverter.ToInt16(bytes, 0);
//        }
//        private int getAction()
//        {
//            byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0x00, 0x36 }, AggroActionPtr, ProcessHandle);
//            return BitConverter.ToInt16(bytes, 0);
//        }
//        private int getTarget()
//        {
//            IntPtr baseOfGame = BaseAddress + 3162756;
//            byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0x00, 0x14 }, baseOfGame, ProcessHandle);
//            int targetAddress = BitConverter.ToInt32(bytes, 0); // + 20;
//            return targetAddress;
//        }
//        private void setTarget(int mobID)
//        {
//            IntPtr baseOfGame = BaseAddress + 3162756;
//            byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0x00 }, baseOfGame, ProcessHandle);
//            uint targetAddress = BitConverter.ToUInt32(bytes, 0) + 20;
//            bool succsess = memFunc.writeValueToAddress((IntPtr)targetAddress, BitConverter.GetBytes(mobID), ProcessHandle);
//        }
//        private void getAggro()
//        {
//            while (true)
//            {
//                byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0x00, 0x1EC, 0x04 }, AggroActionPtr, ProcessHandle);
//                int MobId = BitConverter.ToInt32(bytes, 0);
//                if (lastMobKilled == MobId || MobId > 15000 || MobId < 0)
//                {
//                    MobId = 0;
//                }
//                if (!monsterIds.Contains(MobId) && MobId != 0)
//                {
//                    monsterIds.Add(MobId);
//                }
//                Thread.Sleep(500);
//            }
//        } 
//        private Single getCoordX()
//        {
//            IntPtr baseOfGame = BaseAddress + 3303568;
//            byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0x00 }, baseOfGame, ProcessHandle);
//            return BitConverter.ToSingle(bytes, 0);
//        }
//        private Single getCoordY()
//        {
//            IntPtr baseOfGame = BaseAddress + 3303572;
//            byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0x00 }, baseOfGame, ProcessHandle);
//            return BitConverter.ToSingle(bytes, 0);
//        }
//        private bool reachedWaypoint(Single coordX, Single coordY)
//        {
//            Single currCoordX = getCoordX();
//            Single currCoordY = getCoordY();
//            //double distance = memFunc.getDistance(currCoordX, currCoordY, coordX, coordY);
//            int distance = 0;
//            if (distance > 150)
//            {
//                return false;
//            }
//            return true;
//        }
//        #endregion
        
//    }
    
//}
