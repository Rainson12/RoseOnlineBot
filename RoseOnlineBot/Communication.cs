using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Collections;
using System.Windows.Forms;

namespace RoseOnlineBot
{
    public class Communication
    {
        public const int BUFFER_SIZE = 4096;
        private string pipeName { get; set; }
        private Thread listenThread;

        public Communication(string pipeName)
        {
            this.pipeName = pipeName;
        }

        public void Start()
        {
            //start the listening thread
            this.listenThread = new Thread(new ThreadStart(ListenForClients));
            this.listenThread.Start();
        }

        private void ListenForClients()
        {
            while (true)
            {
                var pipeSecurity = new PipeSecurity();
                var usersPipeAccessRule = new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), PipeAccessRights.FullControl | PipeAccessRights.Synchronize, AccessControlType.Allow);
                var usersPipeAccessRule2 = new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.AnonymousSid, null), PipeAccessRights.FullControl | PipeAccessRights.Synchronize, AccessControlType.Allow);
                var usersPipeAccessRule3 = new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.WorldSid, null), PipeAccessRights.FullControl | PipeAccessRights.Synchronize, AccessControlType.Allow);
                pipeSecurity.AddAccessRule(usersPipeAccessRule);
                pipeSecurity.AddAccessRule(usersPipeAccessRule2);
                pipeSecurity.AddAccessRule(usersPipeAccessRule3);

                var pipeServer = NamedPipeServerStreamConstructors.New(
                    pipeName,
                    PipeDirection.InOut,
                    5,
                    PipeTransmissionMode.Message,
                    PipeOptions.Asynchronous,
                    4096,
                    4096, pipeSecurity);

                pipeServer.WaitForConnection();

                Thread readThread = new Thread(new ParameterizedThreadStart(Read));
                readThread.Start(pipeServer);
            }
        }

        /// <summary>
        /// Reads incoming data from connected clients
        /// </summary>
        /// <param name="clientObj"></param>
        private void Read(object clientObj)
        {
            NamedPipeServerStream pipeServer = (NamedPipeServerStream)clientObj;
            var cnt = 0;
            var logs = new string[10];

            var hasHeader = false;
            var nextPacketLen = 0;
            var nextPacketCmd = 0;
            var actionsList = new List<uint>();
            var lastActions = new List<int>();
            var lastPackages = new List<byte[]>();
            while (true)
            {

                byte[] _buffer = new byte[BUFFER_SIZE];
                var bytesRead = pipeServer.Read(_buffer, 0, _buffer.Length);
                byte[] subsetArray = new byte[bytesRead];
                Array.Copy(_buffer, 0, subsetArray, 0, bytesRead);

                string hexString = BitConverter.ToString(subsetArray).Replace("-", " ");
                //File.AppendAllLines("intercepted.log", new[] { hexString });

                if (!GameData.IsInitialized)
                {
                    continue;
                }
                if (bytesRead == 6)
                {
                    hasHeader = true;
                    nextPacketLen = BitConverter.ToInt16(subsetArray.Take(2).ToArray());
                    nextPacketCmd = BitConverter.ToInt16(subsetArray.Skip(2).Take(2).ToArray());
                    if (!actionsList.Contains((uint)nextPacketCmd))
                        actionsList.Add((uint)nextPacketCmd);

                    if (nextPacketCmd == 0x716)
                    {

                    }
                    if (nextPacketCmd == 0x718)
                    {

                    }
                }
                else if (hasHeader == true)
                {
                    hasHeader = false;
                    lastPackages.Insert(0, subsetArray);
                    lastActions.Insert(0, nextPacketCmd);
                    if (lastActions.Count > 20)
                    {
                        lastActions = new List<int>();
                        lastPackages = new List<byte[]>();

                    }
                    else if (nextPacketCmd == 0x799)
                    {
                        // someone is getting attacked
                        var attackerObjectIdx = BitConverter.ToUInt16(subsetArray.Skip(0).Take(2).ToArray());
                        var defenderObjectIdx = BitConverter.ToUInt16(subsetArray.Skip(2).Take(2).ToArray());
                        var amount = BitConverter.ToInt32(subsetArray.Skip(4).Take(4).ToArray());
                        var flags = BitConverter.ToSingle(subsetArray.Skip(8).Take(4).ToArray());
                        if (attackerObjectIdx == GameData.Player.DBId)
                        {
                            // i am attacker
                            var mob = GameData.Player.GetMobs().FirstOrDefault(x => x.DBId == defenderObjectIdx);
                            if (mob != null && !GameData.Player.Targets.Any(x => x.Id == mob.Id))
                            {
                                GameData.Player.Targets.Add(mob);
                            }
                        }
                        else if (defenderObjectIdx == GameData.Player.DBId)
                        {
                            // i am defender
                            var mob = GameData.Player.GetMobs().FirstOrDefault(x => x.DBId == attackerObjectIdx);
                            if (mob != null && !GameData.Player.Targets.Any(x => x.Id == mob.Id))
                            {
                                GameData.Player.Targets.Add(mob);
                            }
                        }
                        continue;
                    }
                    else if (nextPacketCmd == 0x798)
                    {
                        // someone is getting attacked
                    }
                    else if (nextPacketCmd == 0x79a)
                    {
                        // current character moves
                        var objectIdx = BitConverter.ToInt16(subsetArray.Skip(0).Take(2).ToArray());
                        var targetObjectIdx = BitConverter.ToInt16(subsetArray.Skip(2).Take(2).ToArray());
                        var serverDist = BitConverter.ToInt16(subsetArray.Skip(4).Take(2).ToArray());
                        var posX = BitConverter.ToSingle(subsetArray.Skip(6).Take(4).ToArray());
                        var posY = BitConverter.ToSingle(subsetArray.Skip(10).Take(4).ToArray());
                        var posZ = BitConverter.ToInt16(subsetArray.Skip(14).Take(2).ToArray());
                    }
                    else if (nextPacketCmd == 0x7b8)
                    {
                        // item dropped by character?
                    }
                    else if (nextPacketCmd == 0x7a6)
                    {
                        // item dropped
                        var posX = BitConverter.ToSingle(subsetArray.Skip(0).Take(4).ToArray());
                        var posY = BitConverter.ToSingle(subsetArray.Skip(4).Take(4).ToArray());
                        var itemType = BitConverter.ToUInt16(subsetArray.Skip(8).Take(2).ToArray());
                        var itemNo = BitConverter.ToUInt32(subsetArray.Skip(10).Take(4).ToArray());
                        var charDbId = BitConverter.ToUInt32(subsetArray.Skip(14).Take(4).ToArray());
                        /* reserved */
                        //this.readUint32();
                        var color = BitConverter.ToUInt32(subsetArray.Skip(22).Take(4).ToArray());

                        var itemKey = BitConverter.ToUInt64(subsetArray.Skip(26).Take(8).ToArray());
                        var isCrafted = subsetArray.Skip(34).Take(1);
                        var gemOption1 = BitConverter.ToUInt16(subsetArray.Skip(35).Take(2).ToArray());
                        var gemOption2 = BitConverter.ToUInt16(subsetArray.Skip(37).Take(2).ToArray());
                        var gemOption3 = BitConverter.ToUInt16(subsetArray.Skip(39).Take(2).ToArray());
                        var durability = BitConverter.ToUInt16(subsetArray.Skip(41).Take(2).ToArray());
                        var itemLife = BitConverter.ToUInt16(subsetArray.Skip(43).Take(2).ToArray());
                        var socketCount = subsetArray.Skip(46).Take(1);
                        var isAppraised = subsetArray.Skip(47).Take(1);
                        var refineGrade = subsetArray.Skip(48).Take(1);
                        var quantity = BitConverter.ToUInt16(subsetArray.Skip(53).Take(2).ToArray()); // safe
                        var location = subsetArray.Skip(55).Take(1);
                        var slotNo = BitConverter.ToUInt32(subsetArray.Skip(56).Take(4).ToArray());
                        /*pickupTime*/
                        //this.skip(14);
                        var timeRemaining = BitConverter.ToInt32(subsetArray.Skip(72).Take(4).ToArray());
                        var moveLimits = BitConverter.ToUInt16(subsetArray.Skip(76).Take(2).ToArray());
                        var bindOnAcquire = subsetArray.Skip(78).Take(1);
                        var bindOnEquipUse = subsetArray.Skip(79).Take(1);
                        var money = BitConverter.ToUInt32(subsetArray.Skip(80).Take(4).ToArray());
                        var objectIdx = BitConverter.ToUInt16(subsetArray.Skip(84).Take(2).ToArray());
                        var ownerObjectIdx = BitConverter.ToUInt16(subsetArray.Skip(86).Take(2).ToArray());

                        if (GameData.Player.PartyMode)
                            GameData.Player.ItemsToPickup.Add(new Models.Logic.DroppedItem() { ItemNo = itemNo, PosX = posX, PosY= posY, ObjectId = objectIdx  });
                        else if (ownerObjectIdx == GameData.Player.DBId || ownerObjectIdx == 0x00)
                        {
                            GameData.Player.ItemsToPickup.Add(new Models.Logic.DroppedItem() { ItemNo = itemNo, PosX = posX, PosY = posY, ObjectId = objectIdx });
                        }
                    }
                    else if (nextPacketCmd == 0x7b5)
                    {
                    }
                    else if (nextPacketCmd == 0x7bb)
                    {
                    }
                    else if (nextPacketCmd == 0x865)
                    {
                        var skillId = BitConverter.ToUInt16(subsetArray.Skip(0).Take(2).ToArray());
                        var matchingSkill = GameData.Player.Skills.FirstOrDefault(x => x.Ids.Any(y => y == skillId));
                        if (matchingSkill != null)
                        {
                            matchingSkill.LastExecution = DateTime.Now;
                        }
                    }
                    else if (nextPacketCmd == 0x7b2)
                    {
                        var characterDbId = BitConverter.ToUInt16(subsetArray.Skip(0).Take(2).ToArray());
                        if(GameData.Player.DBId == characterDbId)
                        {
                            var skillId = BitConverter.ToUInt16(subsetArray.Skip(2).Take(2).ToArray());
                            var matchingSkill = GameData.Player.Skills.FirstOrDefault(x => x.Ids.Any(y => y == skillId));
                            if (matchingSkill != null)
                            {
                                matchingSkill.LastExecution = DateTime.Now;
                            }
                        }
                    }
                    else if (nextPacketCmd == 0x794)
                    {
                        // object remove
                    }
                    else if (nextPacketCmd == 0x873)
                    {
                        // ?? something with coordinates 
                    }
                    else if (nextPacketCmd == 0x797)
                    {
                        //object move to
                    }
                    else if (nextPacketCmd == 0x792)
                    {
                        //spawn mob
                    }
                    else if (nextPacketCmd == 0x862)
                    {
                        //???
                    }
                    else if (nextPacketCmd == 0x793)
                    {
                        // spawn char
                    }
                    else if (nextPacketCmd == 0x7b9)
                    {
                        // skill result
                    }
                    else if (nextPacketCmd == 0x716)
                    {
                        // inventory data
                    }
                    else if (nextPacketCmd == 0x7b7)
                    {
                        //GSV_CLEAR_STATUS
                    }
                    else if (nextPacketCmd == 0x796)
                    {
                        //GSV_STOP
                    }
                    else if (nextPacketCmd == 0x7a5)
                    {
                        // char equip item
                    }
                    else if (nextPacketCmd == 0x7b3)
                    {
                        // target skill
                    }
                    else if (nextPacketCmd == 0x770)
                    {
                        // GSV_ADJUST_POS
                    }
                    else if (nextPacketCmd == 0x7b6)
                    {
                        //handle skill effect
                    }
                    else if (nextPacketCmd == 0x700)
                    {
                        //SVR_ERROR
                    }
                    else if (nextPacketCmd == 0x79e)
                    {
                        //levelup
                    }
                    else if (nextPacketCmd == 0x7cd)
                    {

                        // GSV_REPAIRED_FROM_NPC
                    }
                    else if (nextPacketCmd == 0x7ec)
                    {

                        // hp update
                    }
                    else if (nextPacketCmd == 0x7ce)
                    {

                        var itemKey = BitConverter.ToUInt64(subsetArray.Skip(0).Take(8).ToArray());
                        var remainingDurabilityHits = BitConverter.ToUInt16(subsetArray.Skip(8).Take(2).ToArray());
                        // durability update
                        if (hexString.Contains("FD F4 07") || hexString.Contains("07 F4 FD"))
                        {
                            
                            
                        }
                        // GSV_SET_ITEM_LIFE
                    }
                    else if (nextPacketCmd == 0x79b)
                    {
                        // set xp
                    }
                    else if (nextPacketCmd == 0x71b)
                    {
                        // quest stuff
                    }
                    else if (nextPacketCmd == 0x723)
                    {
                        // quest stuff
                    }
                    else if (nextPacketCmd == 0x730)
                    {
                        // quest stuff
                    }
                    else if (nextPacketCmd == 0x774)
                    {
                        // GSV_CHANGE_NPC
                    }
                    else if (nextPacketCmd == 0x829)
                    {
                        // ???
                    }
                    else if (nextPacketCmd == 0x791)
                    {
                        // spawn npc GSV_NPC_CHAR
                    }
                    else if (nextPacketCmd == 0x7a7)
                    {
                        // GSV_GET_FIELDITEM_REPLY
                    }
                    else if (nextPacketCmd == 0x782)
                    {
                        // sitting
                    }
                    else if (nextPacketCmd == 0x7fc)
                    {
                        // ???
                    }
                    else if (nextPacketCmd == 0x7d5)
                    {
                        // GSV_CHANGE_OBJIDX
                    }
                    else if (nextPacketCmd == 0x7f5)
                    {
                        // ???
                    }
                    else if (nextPacketCmd == 0x867)
                    {
                        // ???
                    }
                    else if (nextPacketCmd == 0x7ed)
                    {
                        // ???
                    }
                    else if (nextPacketCmd == 0x783)
                    {
                        // GSV_CHAT
                    }
                    else if (nextPacketCmd == 0x7d4)
                    {
                        // GSV_PARTY_LEVnEXP
                    }
                    else if (nextPacketCmd == 0x7f9)
                    {
                        // ???
                    }
                    else if (nextPacketCmd == 0x7d2)
                    {
                        // GSV_PARTY_MEMBER
                    }
                    else if (nextPacketCmd == 0x7c4)
                    {
                        //GSV_P_STORE_LIST_REPLY
                        var abc = bytesRead;
                        var abc2 = nextPacketLen;
                        var test = hexString;

                        bool sellingData = subsetArray[0] == 1;
                        bool buyingData = subsetArray[0] == 2;
                        if(buyingData)
                        {
                            int itemCount = subsetArray[1];
                            for(int i = 0; i < itemCount; i++)
                            {
                                var slot = subsetArray.Skip(2 + i * 85).First();
                                var itemId = BitConverter.ToUInt16(subsetArray.Skip(2 + i * 85 + 3).Take(2).ToArray());
                                var price = BitConverter.ToUInt32(subsetArray.Skip(2 + i * 85 + 77).Take(4).ToArray());
                                GameData.StorePrices.Add(new Models.Logic.ItemPrice() {  ItemId = itemId, Price = price, FoundCoordX = GameData.Player.LastOpenedShopXCoord, FoundCoordY = GameData.Player.LastOpenedShopYCoord });
                            }
                        }
                    }
                    else if (nextPacketCmd == 0x7c6)
                    {
                        //GSV_P_STORE_RESULT
                    }
                    else if (nextPacketCmd == 0x7c7)
                    {
                        //GSV_P_STORE_MONEYnINV
                    }
                    else
                    {

                    }

                }
                else
                {
                    // no packet without header should exist?

                }

                //client has disconnected
                if (bytesRead == 0)
                    break;
            }

            //clean up resources
            pipeServer.Close();
        }
    }
}
