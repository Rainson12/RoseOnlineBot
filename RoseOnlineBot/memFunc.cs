//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Runtime.InteropServices;
//using System.Xml.Serialization;
//using System.IO;
//using System.Windows.Forms;
//using System.Threading;
//using System.Diagnostics;
//using RoseOnlineBot.Models.Logic;

//namespace RoseOnlineBot
//{
//    class memFunc
//    {
//        [DllImport("kernel32.dll")]
//        static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, ref uint lpNumberOfBytesWritten);
//        [DllImport("kernel32.dll")]
//        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, UIntPtr nSize, ref uint lpNumberOfBytesWritten);



//        public byte[] StringToByteArray(string str)
//        {
//            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
//            return enc.GetBytes(str);
//        }
//        public string ByteArrayToString(byte[] arr)
//        {
//            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
//            return enc.GetString(arr);
//        }


//        public static byte[] calcPacketForMove(Single coordX, Single coordY)
//        {
//            byte[] bCoordX = BitConverter.GetBytes(coordX);
//            byte[] bCoordY = BitConverter.GetBytes(coordY);

//            byte[] calc1 = new byte[14];
//            calc1[0] = 0x9A;
//            calc1[1] = 0x07;
//            //calc1[2] = 0x04;
//            calc1[2] = 0x3A;
//            //calc1[3] = 0x26;
//            calc1[3] = 0x2E;
//            calc1[4] = 0x00; ;
//            calc1[5] = 0x00; ;
//            calc1[6] = bCoordX[0];
//            calc1[7] = bCoordX[1];
//            calc1[8] = bCoordX[2];
//            calc1[9] = bCoordX[3];
//            calc1[10] = bCoordY[0];
//            calc1[11] = bCoordY[1];
//            calc1[12] = bCoordY[2];
//            calc1[13] = bCoordY[3];
//            calc1 = XorEncrypt(calc1, 97);
//            byte[] calc2 = new byte[calc1.Length + 4];
//            calc2[0] = Convert.ToByte(calc1.Length + 4);
//            calc2[1] = 0;
//            for (int cnt = 0; cnt < calc1.Length; cnt++)
//            {
//                calc2[cnt + 2] = calc1[cnt];
//            }
//            calc2[calc2.Length - 2] = 0xFF;
//            calc2[calc2.Length - 1] = 0xFF;


//            return calc2;
//        }
//        public static byte[] calcPacketForCollectItem(int itemId)
//        {
//            byte[] calc1 = new byte[8];
//            calc1[0] = 0x08;
//            calc1[1] = 0x00;
//            calc1[2] = 0xC6;
//            calc1[3] = 0x66;
//            calc1[4] = 0x5B;
//            calc1[5] = 0x4F;

//            byte[] itemPacket = BitConverter.GetBytes(itemId);
//            calc1[6] = itemPacket[1];
//            calc1[7] = itemPacket[0];

//            return calc1;
//        }
//        public static byte[] calcPacketForSit()
//        {
//            byte[] calc1 = new byte[7];
//            calc1[0] = 0x07;
//            calc1[1] = 0x00;
//            calc1[2] = 0xE3;
//            calc1[3] = 0x66;
//            calc1[4] = 0x5B;
//            calc1[5] = 0x2E;
//            calc1[6] = 0x01;

//            return calc1;
//        }
//        private static byte[] XorEncrypt(byte[] bytes, int key)
//        {
//            byte[] encryptedBytes = new byte[bytes.Length];
//            for (int i = 0; i < bytes.Length; i++)
//            {
//                int charValue = Convert.ToInt32(bytes[i]); //get the ASCII value of the character
//                charValue ^= key; //xor the value
//                encryptedBytes[i] = Convert.ToByte(charValue);

//                //newText += char.ConvertFromUtf32(charValue); //convert back to string
//            }

//            return encryptedBytes;
//        }


//        public static void saveWaypoints(List<WayPoint> Waypoints, string fileName)
//        {
//            XmlSerializer serializer = new XmlSerializer(typeof(List<Waypoint>));
//            TextWriter textWriter = new StreamWriter(fileName);
//            serializer.Serialize(textWriter, Waypoints);
//            textWriter.Close();
//        }
//        public static List<Waypoint> readWaypoints(string fileName)
//        {
//            XmlSerializer deserializer = new XmlSerializer(typeof(List<Waypoint>));
//            TextReader textReader = new StreamReader(fileName);
//            List<Waypoint> Waypoints;
//            Waypoints = (List<Waypoint>)deserializer.Deserialize(textReader);
//            textReader.Close();

//            return Waypoints;
//        }

//        bool getValueOfAddressIsUsed = false;

//        /// <summary>
//        /// Returns the content of a multi level pointer.
//        /// </summary>
//        /// <param name="offsets">An Array which contains the offsets</param>
//        /// <param name="basePtr">The BaseAddress of the Process</param>
//        /// <param name="readHandle">The reader Handle of the process</param>
//        /// <returns>Returns the Value of an Address</returns>
//        public static byte[] getValueOfAddress(uint[] offsets, IntPtr basePtr, IntPtr readHandle, int length = 8)
//        {
//            byte[] bytes = new byte[length];
//            uint size = sizeof(int);
//            uint rw = 0;

//            uint baseAddress = (uint)basePtr;
//            for (int i = 0; i < offsets.Length; i++)
//            {
//                uint address = baseAddress + offsets[i]; // Set base/base + offset
//                ReadProcessMemory(readHandle, (IntPtr)(baseAddress + offsets[i]), bytes, (UIntPtr)size, ref rw); // get value of the pointer
//                baseAddress = BitConverter.ToUInt32(bytes, 0);
//            }
//            return bytes;
//        }
//        public static byte[] getValueOfAddress(uint[] offsets, UIntPtr basePtr, IntPtr readHandle, int length = 8)
//        {
//            byte[] bytes = new byte[length];
//            uint size = sizeof(int);
//            uint rw = 0;

//            uint baseAddress = (uint)basePtr;
//            foreach (uint s in offsets)
//            {
//                uint address = baseAddress + s; // Set base/base + offset
//                ReadProcessMemory(readHandle, (IntPtr)(baseAddress + s), bytes, (UIntPtr)size, ref rw); // get value of the pointer
//                baseAddress = BitConverter.ToUInt32(bytes, 0);
//            }
//            return bytes;
//        }

//        public static bool writeValueToAddress(IntPtr address, byte[] value, IntPtr readHandle)
//        {
//            uint rw = 0;
//            return WriteProcessMemory(readHandle, address, value, (UIntPtr)value.Length, ref rw);
//        }
//        public static string getCharacterId(byte[] values)
//        {
//            if (values.Length == 16)
//            {
//                if (BitConverter.ToString(values, 0).EndsWith("FF-FF"))
//                {
//                    string ts = BitConverter.ToString(values);
//                    int i = Convert.ToInt32(ts.Replace("-", "").Substring(0, 4), 16);
//                    return i.ToString();
//                }
//            }
//            return null;
//        }


//        // Keyboard
//        [DllImport("user32.dll")]
//        static extern uint MapVirtualKey(uint uCode, uint uMapType);
//        [DllImport("User32.dll", EntryPoint = "PostMessage")]
//        private static extern int PostMessage(IntPtr hWnd, uint Msg, uint wParam, uint lParam);

//        Keys keySpace;
//        const uint WM_KEYDOWN = 0x100;
//        const uint WM_KEYUP = 0x101;
//        const uint WM_CHAR = 0x102;
//        static bool BsendKey = false;
//        public static void sendKey(Keys key, IntPtr ProcessHandle)
//        {
//            while (BsendKey == true)
//            {
//                Thread.Sleep(20);
//            }
//            BsendKey = true;
//            PostMessage(ProcessHandle, WM_KEYUP, (uint)key, MakeLParam((uint)key));
//            BsendKey = false;
//        }
//        private static uint MakeLParam(uint key)
//        {
//            return (MapVirtualKey(key, 0) << 16);
//        }
//    }
//}
