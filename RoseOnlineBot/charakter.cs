//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Diagnostics;
//using System.Reflection;
//using System.Runtime.InteropServices;
//using RoseOnlineBot.Win32;
//using System.Collections;

//namespace RoseOnlineBot
//{
//    public class Charakter
//    {
//        [DllImport("kernel32.dll")]
//        static extern IntPtr OpenProcess(UInt32 dwDesiredAccess, Boolean bInheritHandle, UInt32 dwProcessId);

//        IntPtr TRoseAddress;
//        IntPtr ProcessHandle;
//        IntPtr charBase;
//        Process process;
//        public Target target;
//        public Charakter(Process p)
//        {
//            process = p;
//            TRoseAddress = p.MainModule.BaseAddress;
//            ProcessHandle = OpenProcess((int)ProcessAccessFlags.AllAccess, false, (uint)process.Id);
//            //ProcessHandle = OpenProcess(0x1F0FFF, true, (uint)process.Id);
//            getCharBase();
//            target = new Target(TRoseAddress, ProcessHandle);
//        }


        
//        public Single hp
//        { 
//            get {
//                byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0 }, charBase + 0x18, ProcessHandle);
//                int converted = BitConverter.ToInt16(bytes, 0);
//                return Convert.ToSingle(converted); 
//            } 
//        }
//        public Single maxhp { 
//            get {
//                byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0 }, charBase + 0x293C, ProcessHandle);
//               int converted = BitConverter.ToInt16(bytes, 0);
//               return Convert.ToSingle(converted); 
//            }
//        }
//        public int mp {
//            get
//            {
//                byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0 }, charBase + 0x1C, ProcessHandle);
//                int converted = BitConverter.ToInt16(bytes, 0);
//                return converted;
//            } 
//        }
//        public int maxmp {
//            get {
//                byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0 }, charBase + 0x2940, ProcessHandle);
//                int converted = BitConverter.ToInt16(bytes, 0);
//                return converted;
//            }
//        }
//        public int current_Xp  {
//            get
//            {
//                byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0 }, charBase + 0x20, ProcessHandle);
//                int converted = BitConverter.ToInt32(bytes, 0);
//                return converted;
//            }
//        }

        
//        public int current_lvl  {
//            get
//            {
//                byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0 }, charBase + 0x24, ProcessHandle);
//                int converted = BitConverter.ToInt16(bytes, 0);
//                return converted;
//            }
//        }
//        public int weight
//        {
//            get
//            {
//                byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0 }, charBase + 0x3396, ProcessHandle);
//                int converted = BitConverter.ToInt16(bytes, 0);
//                return converted;
//            }
//        }
//        public string ID;
//        public int max_weight
//        {
//            get
//            {
//                byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0 }, charBase + 0x2952, ProcessHandle);
//                int converted = BitConverter.ToInt16(bytes, 0);
//                return converted;
//            }
//        }
//        public int action {
//            get
//            {
//                byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0x00, 0x36 }, TRoseAddress + 0x3900E8, ProcessHandle);
//                int converted = Convert.ToInt32(bytes[0]);
//                return converted;
//            }
//        }
        
//        public string charName
//        {
//            get
//            {
//                byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0x00 }, TRoseAddress + 0x38FFA4, ProcessHandle);
//                string converted = BitConverter.ToString(bytes, 0);
//                return converted;
//            }
//        }
//        public int killCounter
//        {
//            get
//            {
//                byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0x00, 0x5D4, 0x7A0, 0x77C}, TRoseAddress + 0x2DCB10, ProcessHandle);
//                int converted = BitConverter.ToInt32(bytes, 0);
//                return converted; 
//            }
//        }
//        public int aggroID
//        {
//            get
//            {
//                byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0x00, 0x1EC, 0x04 }, TRoseAddress + 0x3900E8, ProcessHandle);
//                int converted = BitConverter.ToInt32(bytes, 0);
//                return converted;
//            }
//        }
//        public Single x_pos { get {
//            byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0x00 }, TRoseAddress + 0x3901B8, ProcessHandle);
//            Single converted = BitConverter.ToSingle(bytes, 0);
//            return converted;
//        } }
//        public Single y_pos
//        {
//            get
//            {
//                byte[] bytes = memFunc.getValueOfAddress(new uint[] { 0x00 }, TRoseAddress + 0x3901BC, ProcessHandle);
//                Single converted = BitConverter.ToSingle(bytes, 0);
//                return converted;
//        }}
        

        
//        private void getCharBase()
//        {
//            byte[] charPT1 = memFunc.getValueOfAddress(new uint[] { 0, 0x544 + 0 + 0 + 0, 0 }, TRoseAddress + 0x3900E8, ProcessHandle, 4);
//            int address = BitConverter.ToInt32(charPT1, 0);
//            address += 296;

//            charBase = new IntPtr(address);
//        }
        
//        public ArrayList droppedItems = new ArrayList();

//        internal void checkPacketForDrops(byte[] buffer2)
//        {
//            if (buffer2.Length == 28)
//            {
//                int dropISForCharakterID = Convert.ToInt32(BitConverter.ToString(buffer2).Replace("-", "").Substring(48, 4), 16);
//                // Drop ist für mich
//                if (dropISForCharakterID == int.Parse(ID) || dropISForCharakterID == 0)
//                {
//                    int itemDropId = Convert.ToInt32(BitConverter.ToString(buffer2).Replace("-", "").Substring(44, 4), 16);
//                    droppedItems.Add(itemDropId);
//                }
//            }
//        }
//    }
//}
