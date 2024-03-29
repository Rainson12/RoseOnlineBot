﻿using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoseOnlineBot.Business
{
    internal class NpcEntity
    {
        public float PosX => GameData.Handle.ReadMemory<Single>(BaseAddress + 0x10);
        public float PosY => GameData.Handle.ReadMemory<Single>(BaseAddress + 0x14);
        public float PosZ => GameData.Handle.ReadMemory<Single>(BaseAddress + 0x18);
        public byte Type { get; }
        public Int16 NpcStbId { get; set; }
        public bool Exists
        {
            get
            {
                var firstPtr = GameData.Handle.ReadMemory<IntPtr>(GameData.BaseAddress + GameData.EngineBaseOffset);
                var idNotNull =  GameData.Handle.ReadMemory<IntPtr>(firstPtr + Id * 8 + 0x00022078) != 0;
                if (idNotNull == false)
                    return false;
                else
                    return HP > 0;
            }
        }

        public short Id { get; } = 0;
        public int HP => GameData.Handle.ReadMemory<Int32>(BaseAddress + 0xE8);
        public int MaxHP => GameData.Handle.ReadMemory<Int32>(BaseAddress + 0xE8 + 8);

        public UInt16 DBId { get; set; }

        public NpcEntity(short npcId)
        {
            Id = npcId;
            var firstPtr = GameData.Handle.ReadMemory<IntPtr>(GameData.BaseAddress + GameData.EngineBaseOffset);
            BaseAddress = GameData.Handle.ReadMemory<IntPtr>(firstPtr + npcId * 8 + 0x00022078);
            // db id
            DBId = GameData.Handle.ReadMemory<UInt16>(firstPtr + npcId * 2 + 0x0002000A);
            NpcStbId = GameData.Handle.ReadMemory<Int16>(BaseAddress + 0x2c0);
            if (BaseAddress != 0x00)
            {
                Type = GameData.Handle.ReadMemory<byte>(BaseAddress);

                if (Type == 0x40)
                {
                    int mobId = GameData.Handle.ReadMemory<Int32>(BaseAddress + 0x1c);
                    if (mobId == npcId)
                    {
                        int hp = GameData.Handle.ReadMemory<Int32>(BaseAddress + 0xE8);
                        int max_hp = GameData.Handle.ReadMemory<Int32>(BaseAddress + 0xE8 + 8);
                    }

                }
                else if (Type == 0xa0)
                {
                    // player or myself?
                }
                else if (Type == 0x50)
                {
                    // friendly npc?
                }
                else if (Type == 0x60)
                {
                    // friendly npc?
                }
                else
                {
                    //var buffer = GameData.Handle.ReadMemoryArray<byte>(DataBaseAddress, 200);
                    //string hexString = BitConverter.ToString(buffer).Replace("-", " ");
                    //File.WriteAllText("other_" + mobId + ".txt", hexString);
                }
            }

        }
        public IntPtr BaseAddress { get; }
    }
}
