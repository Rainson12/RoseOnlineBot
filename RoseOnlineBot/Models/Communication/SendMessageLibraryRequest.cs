using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoseOnlineBot.Models.Communication
{
    internal class SendMessageLibraryRequest
    {
        public struct MessageStruct
        {
            [CustomMarshalAs(CustomUnmanagedType.LPWStr)]
            public string data;
            public int length;
        }
    }
}
