﻿using ProcessMemoryUtilities.Managed;
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
        public static IntPtr CurrentCharacterBase { get; set; }

        public static IntPtr CurrentTargetBase { get; set; }
        public static IntPtr EngineBase { get; set; }
        public static Communication Pipe { get; set; }
        public static Memory Handle { get; set; }
        public static Injector Injector { get; set; }
        public static Player Player { get; set; }
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
                CurrentCharacterBase= GamePointerHelper.GetCharacterBaseFromPatternResult(Handle, BaseAddress, characterPattern.Value);
            }

            if (patternsInfo.FirstOrDefault(x => x.Key == "Engine Base") is KeyValuePair<string, int> entityHelperPattern)
            {
                EngineBase = GamePointerHelper.GetEngineBaseFromPatternResult(Handle, BaseAddress, entityHelperPattern.Value);
            }
            if (patternsInfo.FirstOrDefault(x => x.Key == "Current Target Id") is KeyValuePair<string, int> currentTargetIdPattern)
            {
                CurrentTargetBase = GamePointerHelper.GetCurrentTargetFromPatternResult(Handle, BaseAddress, currentTargetIdPattern.Value);
            }
            Player = new Player();
            IsInitialized = true;
        }


        public static void SendMessage(byte[] message)
        {
            
            MessageStruct messageData = new MessageStruct() { data = Encoding.Unicode.GetString(message), length = message.Length };
            Injector.CallExport("rBotMagic.dll", "sendData", messageData, message);
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