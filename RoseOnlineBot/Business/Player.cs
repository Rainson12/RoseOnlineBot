using RoseOnlineBot.Models.Logic;
using RoseOnlineBot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoseOnlineBot.Business
{
    internal class Player
    {
        public Player()
        {
            Skills.Add(new Skill() { CooldownInMilliseconds = 10000, Slot = 1, ManaCost = 12, Ids = new Int16[] { 440, 441, 442, 443, 444 }, Range = 600, IsAOE = true });
            Skills.Add(new Skill() { CooldownInMilliseconds = 10000, Slot = 2, ManaCost = 7, Ids = new Int16[] { 450, 451, 452, 453, 454 }, Range = 600, Enabled = false, IsAOE = true });
            Skills.Add(new Skill() { CooldownInMilliseconds = 10000, Slot = 3, ManaCost = 7, Ids = new Int16[] { 445, 446, 447, 448, 449 }, Range = 600, Enabled = false, IsAOE = true });
            Skills.Add(new Skill() { CooldownInMilliseconds = 3000, Slot = 0x210, ManaCost = 9, Ids = new Int16[] { 101 }, Range = 300 });
            Skills.Add(new Skill() { CooldownInMilliseconds = 6000, Slot = 0x211, ManaCost = 9, Ids = new Int16[] { 102 }, Range = 1600, Enabled = false });
        }
        private IntPtr Base
        {
            get
            {
                return GameData.Handle.ReadMemory<IntPtr>(GameData.BaseAddress + GameData.CurrentCharacterBaseOffset);
            }
        }

        public bool WaitingForSkillExecution { get; set; } = false;
        public List<Skill> Skills { get; set; } = new List<Skill>();

        public UInt16 DBId
        {
            get
            {
                var firstPtr = GameData.Handle.ReadMemory<IntPtr>(GameData.BaseAddress + GameData.EngineBaseOffset);
                return GameData.Handle.ReadMemory<UInt16>(firstPtr + Id * 2 + 0x0002000A);
            }
        }
        public IntPtr InventoryPtr => Base + 0x3D58;

        public Int16 Id => GameData.Handle.ReadMemory<Int16>(Base + 0x1c);
        public Int32 HP => GameData.Handle.ReadMemory<Int16>(Base + 0x28 + 0x3ac0);
        public Int32 MAXHP => GameData.Handle.ReadMemory<Int16>(Base + 0x4620);
        public Int32 MP => GameData.Handle.ReadMemory<Int16>(Base + 0x3AEC);
        public Int32 MAXMP => GameData.Handle.ReadMemory<Int16>(Base + 0x4624);
        public Single PosX => GameData.Handle.ReadMemory<Single>(Base + 0x10);
        public Single PosY => GameData.Handle.ReadMemory<Single>(Base + 0x14);
        public Single PosZ => GameData.Handle.ReadMemory<Single>(Base + 0x18);
        public Animation CurrentAnimation
        {
            get
            {
                var anim = GameData.Handle.ReadMemory<byte>(Base + 0x58);
                return (Animation)anim;
            }
        }

        public List<int> Targets = new List<int>();

        public Int16 TargetId
        {
            get
            {
                var firstPointer = GameData.Handle.ReadMemory<IntPtr>(GameData.BaseAddress + GameData.CurrentTargetBaseOffset);
                return GameData.Handle.ReadMemory<Int16>(firstPointer + 0x8);
            }
            set
            {
                var firstPointer = GameData.Handle.ReadMemory<IntPtr>(GameData.BaseAddress + GameData.CurrentTargetBaseOffset);
                GameData.Handle.WriteMemory<Int16>(firstPointer + 0x8, value);
            }
        }

        public Int16 AmountTargetsAround
        {
            get
            {
                var firstPointer = GameData.Handle.ReadMemory<IntPtr>(GameData.BaseAddress + GameData.EngineBaseOffset);
                var firstMobAddress = GameData.Handle.ReadMemory<IntPtr>(firstPointer + 0x22050);

                Int16 x = 0;
                while (true)
                {
                    IntPtr npcIdAddress = firstMobAddress + x * 4;
                    var npcId = GameData.Handle.ReadMemory<Int16>(npcIdAddress);
                    NpcEntity npc = new(npcId);
                    if (npc.Type == 0x00)
                        break;
                    x++;
                }
                return x;
            }
        }

        public List<NpcEntity> GetMobs()
        {
            List<NpcEntity> mobs = new List<NpcEntity>();
            var firstPointer = GameData.Handle.ReadMemory<IntPtr>(GameData.BaseAddress + GameData.EngineBaseOffset);
            IntPtr firstMobAddress = GameData.Handle.ReadMemory<IntPtr>(firstPointer + 0x22050);

            int x = 0;
            while (true)
            {
                IntPtr npcIdAddress = firstMobAddress + x * 4;
                var npcId = GameData.Handle.ReadMemory<Int16>(npcIdAddress);
                NpcEntity npc = new(npcId);
                if (npc.Type == 0x40)
                {
                    // only add mobs
                    mobs.Add(npc);
                }
                if (npc.Type == 0x00)
                    break;
                x++;
            }
            return mobs;
        }
        public void Move(float x, float y)
        {
            byte[] bCoordX = BitConverter.GetBytes(x);
            byte[] bCoordY = BitConverter.GetBytes(y);
            var currentZ = Convert.ToInt16(PosZ);
            byte[] bCoordZ = BitConverter.GetBytes(currentZ);
            GameData.SendMessage(new byte[] { 0x12, 0x00, 0x9a, 0x07, 0xd1, 0x58, 0x00, 0x00, bCoordX[0], bCoordX[1], bCoordX[2], bCoordX[3], bCoordY[0], bCoordY[1], bCoordY[2], bCoordY[3], bCoordZ[0], bCoordZ[1] });
        }
        public void CastSpellOnTarget(UInt16 targetId, Int16 slotNo)
        {
            WaitingForSkillExecution = true;
            Int16 action = 0x7b3;
            var actionAsByte = BitConverter.GetBytes(action);

            byte[] target = BitConverter.GetBytes(targetId);
            var skillTest = BitConverter.ToInt16(new byte[] { 0x10, 0x02 });

            var skillBytes = BitConverter.GetBytes(slotNo);
            GameData.SendMessage(new byte[] { 0xa, 00, actionAsByte[0], actionAsByte[1], 0xd1, 0x58, target[0], target[1], skillBytes[0], skillBytes[1] });
        }
        public void CastSpellOnMySelf(Int16 slotNo)
        {
            WaitingForSkillExecution = true;
            Int16 action = 0x7b2;
            var actionAsByte = BitConverter.GetBytes(action);

            var skillBytes = BitConverter.GetBytes(slotNo);
            GameData.SendMessage(new byte[] { 0x8, 00, actionAsByte[0], actionAsByte[1], 0xd1, 0x58, skillBytes[0], skillBytes[1] });
        }

        public void ToggleSit()
        {
            Int16 action = 0x782;
            var actionAsByte = BitConverter.GetBytes(action);

            GameData.SendMessage(new byte[] { 0x7, 00, actionAsByte[0], actionAsByte[1], 0xd1, 0x58, 0x1 });
        }

        public void AttackTarget(UInt16 targetId)
        {
            Int16 action = 0x798;
            var actionAsByte = BitConverter.GetBytes(action);
            byte[] target = BitConverter.GetBytes(targetId);
            GameData.SendMessage(new byte[] { 0x8, 00, actionAsByte[0], actionAsByte[1], 0xd1, 0x58, target[0], target[1] });
        }

        public NpcEntity FindNextTarget()
        {
            var posX = GameData.Player.PosX;
            var posY = GameData.Player.PosY;
            var posZ = GameData.Player.PosZ;

            var mobs = GameData.Player.GetMobs();
            if (mobs.Count > 0)
            {
                var closestMob = mobs.Where(x => x.HP > 0).OrderBy(x => Vector2D.CalculateDistance(posX, posY, x.PosX, x.PosY)).ToList();
                var closest = closestMob[0];
                return closest;
            }
            return null;
        }

        public void PickupItem(UInt16 itemId)
        {
            Int16 action = 0x7a7;
            var actionAsByte = BitConverter.GetBytes(action);

            byte[] itemIdAsBytes = BitConverter.GetBytes(itemId);
            GameData.SendMessage(new byte[] { 0x9, 00, actionAsByte[0], actionAsByte[1], 0xd1, 0x58, 0x00, itemIdAsBytes[0], itemIdAsBytes[1] });
        }

        public void SendJoinZone()
        {
            GameData.SendMessage(new byte[] { 0x50, 0x00, 0x0b, 0x07, 0xd1, 0x58, 0x48, 0x00, 0x08, 0xe5, 0xa1, 0x02, 0x12, 0x20, 0x64, 0x38, 0x62, 0x31, 0x33, 0x61, 0x35, 0x62, 0x39, 0x36, 0x36, 0x34, 0x35, 0x30, 0x62, 0x64, 0x31, 0x35, 0x36, 0x31, 0x35, 0x31, 0x32, 0x30, 0x63, 0x36, 0x35, 0x34, 0x37, 0x38, 0x61, 0x35, 0x1a, 0x20, 0x85, 0x85, 0x2c, 0xa7, 0xcc, 0xf6, 0x1a, 0x1f, 0x51, 0x63, 0xb2, 0x24, 0x51, 0xb0, 0x11, 0x11, 0x16, 0x96, 0x76, 0x02, 0x81, 0xe7, 0x2f, 0x36, 0x88, 0x31, 0xba, 0xfc, 0x0d, 0x60, 0xc0, 0xe3 });



            Int16 action4 = 0x715;
            var actionAsByte4 = BitConverter.GetBytes(action4);
            GameData.SendMessage(new byte[] { 0x11, 00, actionAsByte4[0], actionAsByte4[1], 0xd1, 0x58, 0x00, 0xee, 0x82, 0x52, 0x61, 0x69,0x6e,0x73,0x6f,0x6e,0x00 });



            Int16 action2 = 0x7e5;
            var actionAsByte2 = BitConverter.GetBytes(action2);
            GameData.SendMessage(new byte[] { 0x7, 00, actionAsByte2[0], actionAsByte2[1], 0xd1, 0x58, 0x03 });


            Int16 action = 0x753;
            var actionAsByte = BitConverter.GetBytes(action);
            GameData.SendMessage(new byte[] { 0x8, 00, actionAsByte[0], actionAsByte[1], 0xd1, 0x58, 0x03, 0x00 });

            Int16 action3 = 0x860;
            var actionAsByte3 = BitConverter.GetBytes(action3);
            GameData.SendMessage(new byte[] { 0x8, 00, actionAsByte3[0], actionAsByte3[1], 0xd1, 0x58, 0xe0, 0x86 });
        }

        public void GetInventory()
        {
            var ptr = GameData.Handle.ReadMemory<nint>(GameData.BaseAddress + GameData.InventoryRendererOffset);
            var ptr2 = ptr + 0x28;
            var itemsCnt = 0;

            int characterTab = 1; // 2cnd tab
            var itemIndex = 0;

            var startIndex = 0x1E * characterTab;
            var offset = (startIndex + itemIndex) * 0x168;
            offset = offset + 0x2D78;
            var itemBaseAddress = ptr2 + offset; // this is equal to rcx at trose.exe+216D73 breakpoint

            var ptr4 = GameData.Handle.ReadMemory<nint>(itemBaseAddress + 0x108);
            var ptr5 = GameData.Handle.ReadMemory<nint>(ptr4 + 0xF8);
            var itemId = GameData.Handle.ReadMemory<Int16>(ptr5 + 0x38);
            
            var someId = GameData.Handle.ReadMemory<IntPtr>(InventoryPtr + itemId * 0x8 + 0x48);


            // now get the item base address or so at trose.exe + 10A0F0 (ptr5 = rbx,  rax = 1, rcx = InventoryPtr, some id = rdx, rdi = startIndex?)


            // missing part
            var amount = GameData.Handle.ReadMemory<Int16>(itemBaseAddress + 0x20);

            //while (true)
            //{
            //    var a =  GameData.Handle.ReadMemory<UInt64>(InventoryPtr + itemsCnt * 8 + 0x48);
            //    if(a == 0xc3d7a0)
            //    {
            //        break;
            //    }
            //    itemsCnt++;
            //}

        }

    }
}
