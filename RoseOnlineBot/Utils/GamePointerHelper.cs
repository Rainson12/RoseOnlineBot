using RoseOnlineBot.Win32;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RoseOnlineBot.Utils
{
    internal class GamePointerHelper
    {
        internal static IntPtr GetCharacterBaseOffsetFromPatternResult(Memory memoryHandle, nint baseAddress, nint patternValue)
        {
            int readLength = 7;
            var buffer = memoryHandle.ReadMemoryArray<byte>(baseAddress + patternValue, readLength);
            var offsets = buffer.TakeLast(4).ToArray();
            int offsetAsInt = BitConverter.ToInt32(offsets, 0);

            return patternValue + offsetAsInt + readLength;
        }

        internal static IntPtr GetEngineBaseOffsetFromPatternResult(Memory memoryHandle, nint baseAddress, nint patternValue)
        {
            int readLength = 7;
            var buffer = memoryHandle.ReadMemoryArray<byte>(baseAddress + patternValue - 6, readLength);
            var offsets = buffer.TakeLast(4).ToArray();
            int offsetAsInt = BitConverter.ToInt32(offsets, 0);

            return patternValue - 6 + offsetAsInt + readLength;
        }

        internal static IntPtr GetCurrentTargetOffsetFromPatternResult(Memory memoryHandle, nint baseAddress, nint patternValue)
        {
            int readLength = 7;
            var buffer = memoryHandle.ReadMemoryArray<byte>(baseAddress + patternValue, readLength);
            var offsets = buffer.TakeLast(4).ToArray();
            int offsetAsInt = BitConverter.ToInt32(offsets, 0);

            return patternValue + offsetAsInt + readLength;
        }

        internal static IntPtr GetInventoryRendererOffsetFromPatternResult(Memory memoryHandle, nint baseAddress, int patternValue)
        {
            int readLength = 7;
            var buffer = memoryHandle.ReadMemoryArray<byte>(baseAddress + patternValue, readLength);
            var offsets = buffer.TakeLast(4).ToArray();
            int offsetAsInt = BitConverter.ToInt32(offsets, 0);

            return patternValue + offsetAsInt + readLength;
        }

        internal static IntPtr GetInventoryUIOffsetFromPatternResult(Memory memoryHandle, nint baseAddress, int patternValue)
        {
            int readLength = 7;
            var buffer = memoryHandle.ReadMemoryArray<byte>(baseAddress + patternValue, readLength);
            var offsets = buffer.TakeLast(4).ToArray();
            int offsetAsInt = BitConverter.ToInt32(offsets, 0);

            return patternValue + offsetAsInt + readLength;
        }

        internal static IntPtr EnableNoClip(Memory memoryHandle, nint baseAddress, int patternValue)
        {
            int readLength = 4;
            var buffer = memoryHandle.ReadMemoryArray<byte>(baseAddress + patternValue+1, readLength);

            var data = buffer.TakeLast(4).ToArray()!;
            data[0] = 0x90;
            data[1] = 0x90;
            data[2] = 0xeb;
            data[3] = data[3];
            memoryHandle.WriteMemoryArray(baseAddress + patternValue+1, data);
            return patternValue;
        }
    }
}
