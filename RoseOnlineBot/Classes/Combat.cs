//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Windows.Forms;

//namespace RoseOnlineBot.Classes
//{
//    public class Combat
//    {
//        int F2cooldown = 5000;
//        int F3cooldown = 13000;
//        int F4cooldown = 5000;
//        DateTime F2lastPressed = DateTime.Today;
//        DateTime F3lastPressed = DateTime.Today;
//        DateTime F4lastPressed = DateTime.Today;

//        bool F2isReady
//        {
//            get {
//                if (F2lastPressed < F2lastPressed.AddMilliseconds(F2cooldown))
//                    return true;
//                else
//                    return false;
//            }
//        }
//        bool F3isReady
//        {
//            get
//            {
//                if (F3lastPressed < F3lastPressed.AddMilliseconds(F3cooldown))
//                    return true;
//                else
//                    return false;
//            }
//        }
//        bool F4isReady
//        {
//            get
//            {
//                if (F4lastPressed < F4lastPressed.AddMilliseconds(F4cooldown))
//                    return true;
//                else
//                    return false;
//            }
//        }
//        private Main main;
//        public Combat(Main _main) {
//            main = _main;
//        }
//        public void killUnit()
//        {
//            double origDistance = Math.Sqrt(Math.Pow(main.charakter.x_pos - main.charakter.target.getXCoord(), 2) + Math.Pow(main.charakter.y_pos - main.charakter.target.getYCoord(), 2));
//            double distance = origDistance;
//            double enemyHP = main.charakter.target.getHP();
            
//            memFunc.sendKey(Keys.F1, main.process.MainWindowHandle); // monster angreiffen
//            Thread.Sleep(1500);
//            do
//            {
//                origDistance = distance;
//                distance = Math.Sqrt(Math.Pow(main.charakter.x_pos - main.charakter.target.getXCoord(), 2) + Math.Pow(main.charakter.y_pos - main.charakter.target.getYCoord(), 2));

//                if (F2isReady && enemyHP > 0)
//                {
//                    memFunc.sendKey(Keys.F2, main.process.MainWindowHandle); // monster angreiffen
//                    Thread.Sleep(500); // Latenz
//                    F2lastPressed = DateTime.Now;
//                    Thread.Sleep(500);
//                }
//                if (F3isReady && enemyHP > 0)
//                {
//                    memFunc.sendKey(Keys.F3, main.process.MainWindowHandle); // monster angreiffen
//                    Thread.Sleep(500); // Latenz
//                    F3lastPressed = DateTime.Now;
//                    Thread.Sleep(1500);
//                }
//                if (F4isReady && enemyHP > 0)
//                {
//                    memFunc.sendKey(Keys.F4, main.process.MainWindowHandle); // monster angreiffen
//                    Thread.Sleep(500); // Latenz
//                    F4lastPressed = DateTime.Now;
//                    Thread.Sleep(1500);
//                }
//            }
//            while ((main.charakter.action != 0 && main.charakter.action != 10) || (main.charakter.target.getHP() != 0 && (distance < origDistance || distance < 500)));
            
//            // solange action != 0 ist der charackter im kampf
//        }

//        public void rest()
//        {
//            while (main.charakter.action != 0 && main.aggroingMe.Count == 0)
//            {
//                Thread.Sleep(50);
//            }
//            if (main.aggroingMe.Count == 0)
//            {
//                Thread.Sleep(500);
//                byte[] sitPacket = memFunc.calcPacketForSit();
//                //memFunc.sendKey(Keys.F9, main.process.MainWindowHandle); // monster angreiffen
//                main.com.SendMessage(sitPacket);
//                while (main.charakter.hp < main.charakter.maxhp && main.aggroingMe.Count == 0)
//                {
//                    if (main.charakter.action != 10)
//                    {
//                        main.com.SendMessage(sitPacket);
//                        Thread.Sleep(600);
//                    }

//                    Thread.Sleep(10);
                    
//                }
//            }
//        }
        
//    }
//}
