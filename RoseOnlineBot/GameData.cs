using ProcessMemoryUtilities.Managed;
using RoseOnlineBot.Business;
using RoseOnlineBot.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static RoseOnlineBot.Models.Communication.SendMessageLibraryRequest;

namespace RoseOnlineBot
{
    internal static class GameData
    {
        public static IntPtr BaseAddress { get; set; }
        public static IntPtr CurrentCharacterBaseOffset { get; set; }

        public static IntPtr InventoryRendererOffset { get; set; }
        public static IntPtr CurrentTargetBaseOffset { get; set; }
        public static IntPtr EngineBaseOffset { get; set; }
        public static IntPtr InventoryUIOffset { get; set; }

        public static Communication Pipe { get; set; }
        public static Memory Handle { get; set; }
        public static Injector Injector { get; set; }
        public static Player Player { get; set; }

        public static List<Int16> MobIdIgnoreList { get; set; } = new List<Int16>();
        public static bool IsInitialized { get; set; } = false;

        public static void Init(Process process, Communication pipe)
        {
            BaseAddress = process.MainModule.BaseAddress;
            if (BaseAddress == IntPtr.Zero)
            {
                throw new Exception("No MainModule address found!");
            }
            Pipe = pipe;
            Injector = new Injector(process);
            EjectIfAlreadyInjected(process);
            process = Process.GetProcessById(process.Id);
            Injector = new Injector(process);
            Injector.InjectLibrary(@"C:\develop\rBotMagic\x64\Debug\rBotMagic.dll");
            Handle = new Memory(process.Id);

            var procSize = process.MainModule.ModuleMemorySize;
            var patternsInfo = PatternFinder.Find(Handle, BaseAddress, procSize);
            if (patternsInfo.FirstOrDefault(x => x.Key == "Character Base Address Function") is KeyValuePair<string, int> characterPattern)
            {
                CurrentCharacterBaseOffset= GamePointerHelper.GetCharacterBaseOffsetFromPatternResult(Handle, BaseAddress, characterPattern.Value);
            }

            if (patternsInfo.FirstOrDefault(x => x.Key == "Engine Base") is KeyValuePair<string, int> entityHelperPattern)
            {
                EngineBaseOffset = GamePointerHelper.GetEngineBaseOffsetFromPatternResult(Handle, BaseAddress, entityHelperPattern.Value);
            }
            if (patternsInfo.FirstOrDefault(x => x.Key == "Current Target Id") is KeyValuePair<string, int> currentTargetIdPattern)
            {
                CurrentTargetBaseOffset = GamePointerHelper.GetCurrentTargetOffsetFromPatternResult(Handle, BaseAddress, currentTargetIdPattern.Value);
            }
            if (patternsInfo.FirstOrDefault(x => x.Key == "InventoryRenderer") is KeyValuePair<string, int> inventoryRendererPattern)
            {
                InventoryRendererOffset = GamePointerHelper.GetInventoryRendererOffsetFromPatternResult(Handle, BaseAddress, inventoryRendererPattern.Value);
            }
            if (patternsInfo.FirstOrDefault(x => x.Key == "Inventory UI Base") is KeyValuePair<string, int> InventoryUIBasePattern)
            {
                InventoryUIOffset = GamePointerHelper.GetInventoryUIOffsetFromPatternResult(Handle, BaseAddress, InventoryUIBasePattern.Value);
            }
            if (patternsInfo.FirstOrDefault(x => x.Key == "NoClip") is KeyValuePair<string, int> NoCLipBasePattern)
            {
                GamePointerHelper.EnableNoClip(Handle, BaseAddress, NoCLipBasePattern.Value);
            }
            

            Player = new Player();
            IsInitialized = true;
        }

        private static object lockObj = new object();
        public static void SendMessage(byte[] message)
        {
            lock(lockObj)
            {
                MessageStruct messageData = new MessageStruct() { data = Encoding.Unicode.GetString(message), length = message.Length };
                Injector.CallExport("rBotMagic.dll", "sendData", messageData, message);
            }
        }


        private static void EjectIfAlreadyInjected(Process myProcess)
        {
            foreach (ProcessModule mod in myProcess.Modules)
            {
                if (mod.ModuleName == "rBotMagic.dll")
                {
                    //Injector.EjectLibrary(@"C:\develop\rBotMagic\x64\Debug\rBotMagic.dll", mod);
                    break;
                }
            }
        }
    }
}
