//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Diagnostics;
//using System.Threading;
//using System.Windows.Forms;
//using RoseOnlineBot.Classes;

//namespace RoseOnlineBot
//{
//    public class Main
//    {
//        public Charakter charakter;

//        public Process process;
//        public Communication com;
//        public List<Waypoint> waypoints;
//        public int lastMobKilled;
//        public List<int> aggroingMe = new List<int>();

//        public Main(Process p, Communication _communicate, string waypointFileUrl)
//        {
//            process = p;
//            com = _communicate;
//            charakter = new Charakter(p);
//            com.mainLogic = this;
//            waypoints = memFunc.readWaypoints(waypointFileUrl);
//        }

//        public void start()
//        {
//            // Get CharacterId
            
//            initialize();
//            Combat charakterClass = new Combat(this);            
//            while(true)
//            {
//                foreach (Waypoint w in waypoints)
//                {
//                    com.SendMessage(memFunc.calcPacketForMove(w.coordX, w.coordY));
//                    Thread.Sleep(50);
//                    Single lastXCoord;
//                    Single lastYCoord;
//                    bool cantReachWP = false;
//                    while (reachedWaypoint(w.coordX, w.coordY) == false && !cantReachWP)
//                    {
//                        cantReachWP = false;
//                        lastXCoord = charakter.x_pos;
//                        lastYCoord = charakter.y_pos;
//                        if (aggroingMe.Count == 0) // neues Target suchen
//                        {
//                            if (charakter.hp < ((charakter.maxhp / 100) * 75))
//                            {
//                                com.SendMessage(memFunc.calcPacketForMove(charakter.x_pos, charakter.y_pos));
//                                charakterClass.rest();
//                            }
//                            else
//                            {
//                                // get next target
//                                memFunc.sendKey(Keys.F12, process.MainWindowHandle);
//                                Thread.Sleep(175);
//                                int target = charakter.target.getTarget();
//                                if (target != 0 && target != lastMobKilled)
//                                {
//                                    if (!aggroingMe.Contains(target))
//                                        aggroingMe.Add(target);
//                                }
//                            }
//                        }
//                        if (aggroingMe.Count > 0)  // wenn in der liste ein monster ist
//                        {
//                            while (aggroingMe.Count != 0)// nächstes monster in der liste killn + Adds killen
//                            {
//                                if (charakter.target.getTarget() != aggroingMe[0]) // wenn monster noch nicht im target
//                                    charakter.target.setTarget(aggroingMe[0]);
//                                Thread.Sleep(250);

//                                charakterClass.killUnit();

//                                lastMobKilled = aggroingMe[0];
//                                aggroingMe.RemoveAt(0);
//                            }
//                        }
//                            com.SendMessage(memFunc.calcPacketForMove(w.coordX, w.coordY));
//                        Thread.Sleep(50);
//                        if (memFunc.getDistance(charakter.x_pos, charakter.y_pos, lastXCoord, lastYCoord) < 20)
//                            cantReachWP = true;
//                    }
                    
//                }
//            }
//        }
       
//        private bool reachedWaypoint(Single coordX, Single coordY)
//        {
//            Single currCoordX = charakter.x_pos;
//            Single currCoordY = charakter.y_pos;
//            double distance = memFunc.getDistance(currCoordX, currCoordY, coordX, coordY);
//            if (distance > 150)
//            {
//                return false;
//            }
//            return true;
//        }
//        private void initialize()
//        {
//            charakter.ID = null;
//            Single xCoord = charakter.x_pos;
//            Single yCoord = charakter.y_pos;

//            byte[] arr = memFunc.calcPacketForMove(xCoord, yCoord);
//            com.initCharacterID = true;
//            //Send movementPacket to receive an answer with the characterId
//            com.SendMessage(arr);
//            while (string.IsNullOrEmpty(charakter.ID))
//            {
//                Thread.Sleep(100);
//            }
//            Thread HPMonitor = new Thread(new ThreadStart(monitorHP));
//            HPMonitor.Start();
//            Thread MPMonitor = new Thread(new ThreadStart(monitorMP));
//            MPMonitor.Start();
//            Thread AggroMonitor = new Thread(new ThreadStart(monitorAggro));
//            AggroMonitor.Start();
//            Thread lootThread = new Thread(new ThreadStart(loot));
//            lootThread.Start();
//            Thread buffThread = new Thread(new ThreadStart(buff));
//            buffThread.Start();
//        }
//        private void loot()
//        {
//            while (true)
//            {
//                if (charakter.droppedItems.Count > 0)
//                {
//                    // collect Items
//                    while (charakter.droppedItems.Count > 0)
//                    {
//                        byte[] pCollectItem = memFunc.calcPacketForCollectItem(int.Parse(charakter.droppedItems[0].ToString()));
//                        com.SendMessage(pCollectItem);
//                        Thread.Sleep(100);
//                        charakter.droppedItems.RemoveAt(0);
//                    }
//                }
//                Thread.Sleep(300);
//            }
//        }
//        private void buff()
//        {
//            while (true)
//            {
//                memFunc.sendKey(Keys.F8, process.MainWindowHandle); 
//                Thread.Sleep(1000*60);
//            }
//        }
//        private void monitorHP()
//        {
//            while (true)
//            {
//                if (charakter.hp < ((charakter.maxhp / 100) * 75) && charakter.action != 0 && charakter.action != 1 && charakter.action != 10)
//                {
//                    //memFunc.sendKey(Keys.F4, process.MainWindowHandle); // ATTACK
//                    memFunc.sendKey(Keys.F9, process.MainWindowHandle); // ATTACK
//                    Thread.Sleep(10 * 900);
//                }
//                Thread.Sleep(200);
//            }
//        }
//        private void monitorMP()
//        {
//            while (true)
//            {
//                if (charakter.mp < ((charakter.maxmp / 100) * 30) && charakter.action != 0 && charakter.action != 1 && charakter.action != 10)
//                {
//                    memFunc.sendKey(Keys.F10, process.MainWindowHandle); // ATTACK
//                    Thread.Sleep(10 * 1000);
//                }
//                Thread.Sleep(500);
//            }
//        }
//        private void monitorAggro()
//        {
//            while (true)
//            {
//                int MobId = charakter.aggroID;
//                if (lastMobKilled == MobId || MobId > 15000 || MobId < 0)
//                {
//                    MobId = 0;
//                }
//                if (!aggroingMe.Contains(MobId) && MobId != 0)
//                {
//                    aggroingMe.Add(MobId);
//                }
//                Thread.Sleep(500);
//            }
//        }
//    }
//}
