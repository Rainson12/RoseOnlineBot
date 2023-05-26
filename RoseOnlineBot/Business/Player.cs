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
            var firstPtr = GameData.Handle.ReadMemory<IntPtr>(GameData.BaseAddress + GameData.EngineBase);
            DBId = GameData.Handle.ReadMemory<UInt16>(firstPtr + Id * 2 + 0x0002000A);
            NPCBase = GameData.Handle.ReadMemory<IntPtr>(firstPtr + Id * 8 + 0x00022078);

        }
        private IntPtr Base
        {
            get
            {
                return GameData.Handle.ReadMemory<IntPtr>(GameData.BaseAddress + GameData.CurrentCharacterBase);
            }
        }

        private IntPtr NPCBase { get; }
        public UInt16 DBId { get; set; }
        public Int16 Id => GameData.Handle.ReadMemory<Int16>(Base + 0x1c);
        public Int32 HP => GameData.Handle.ReadMemory<Int16>(NPCBase + 0x28 + 0x3ac0);
        public Int32 MAXHP => GameData.Handle.ReadMemory<Int16>(NPCBase + 0x4620);
        public Int32 MP => GameData.Handle.ReadMemory<Int16>(NPCBase + 0x3AEC);
        public Int32 MAXMP => GameData.Handle.ReadMemory<Int16>(NPCBase + 0x4624);
        public Single PosX => GameData.Handle.ReadMemory<Single>(Base + 0x10);
        public Single PosY => GameData.Handle.ReadMemory<Single>(Base + 0x14);
        public Single PosZ => GameData.Handle.ReadMemory<Single>(Base + 0x18);

        public List<int> Targets = new List<int>();

        public Int16 TargetId
        {
            get
            {
                var firstPointer = GameData.Handle.ReadMemory<IntPtr>(GameData.BaseAddress + GameData.CurrentTargetBase);
                return GameData.Handle.ReadMemory<Int16>(firstPointer + 0x8);
            }
            set
            {
                var firstPointer = GameData.Handle.ReadMemory<IntPtr>(GameData.BaseAddress + GameData.CurrentTargetBase);
                GameData.Handle.WriteMemory<Int16>(firstPointer + 0x8, value);
            }
        }

        public Int16 AmountTargetsAround
        {
            get
            {
                var firstPointer = GameData.Handle.ReadMemory<IntPtr>(GameData.BaseAddress + GameData.EngineBase);
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
            var firstPointer = GameData.Handle.ReadMemory<IntPtr>(GameData.BaseAddress + GameData.EngineBase);
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
            var currentZ = Convert.ToUInt16(PosZ);
            byte[] bCoordZ =  BitConverter.GetBytes(currentZ);
            GameData.SendMessage(new byte[] { 0x12, 0x00, 0x9a, 0x07, 0xd1, 0x58, 0x00, 0x00, bCoordX[0], bCoordX[1], bCoordX[2], bCoordX[3], bCoordY[0], bCoordY[1], bCoordY[2], bCoordY[3], bCoordZ[0], bCoordZ[1] });
        }
        public void CastSpellOnTarget(UInt16 targetId, Int16 skillId)
        {
            Int16 action = 0x7b3;
            var actionAsByte = BitConverter.GetBytes(action);

            byte[] target = BitConverter.GetBytes(targetId);
            var skillTest = BitConverter.ToInt16(new byte[]{ 0x10, 0x02});

            var skillBytes = BitConverter.GetBytes(skillId);
            GameData.SendMessage(new byte[] { 0xa, 00, actionAsByte[0], actionAsByte[1], 0xd1, 0x58, target[0], target[1], skillBytes[0], skillBytes[1] });
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

    }
}
