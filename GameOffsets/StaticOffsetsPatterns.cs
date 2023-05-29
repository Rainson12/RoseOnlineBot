namespace GameOffsets
{
    public struct StaticOffsetsPatterns
    {
        public static readonly Pattern[] Patterns =
        {
            // <HowToFindIt>
            // 1: Open CheatEngine and Attach to POE Game
            // 2: Search for String: "InGameState", type: UTF-16 (use case insensitive if you don't find anything in the first try)
            // 3: On all of them, "Find out what accesses this address"
            // 4: Highlight one of the instruction
            // 5: Go through all the registers (e.g. RAX, RBX, RCX) and one of them would be the HashNode Address which points to the InGameState Address.
            // 5.1: To know that this is a HashNode Address make sure that Address + 0x20 points to the "InGameState" key (string)
            // 6: Open HashNode Address in the "Dissect data/structure" window of CheatEngine program.
            // 7: @ 0x08 is the Root HashNode. Copy that value (copy xxxx in i.e. p-> xxxxxx)
            // 7.1: To validate that it's a real Root, 0x019 (byte) would be 1.
            // 8: Pointer scan the value ( which is an address ) you got from step-7 with following configs
            //     Maximum Offset Value: 1024 is enough
            //     Maximum Level: 2 is enough, if you don't find anything increase to 3.
            //     "Allow stack addresses of the first threads": false/unchecked
            //     Rest: doesn't matter
            // 9: Copy the base address and put it in your "Add address manually" button this is your InGameState Address.
            // 10: Do "Find out what accesses this address" and make a pattern out of that function. (pick the one which has smallest offset)
            // </HowToFindIt>
            new(
                "Character Base Address Function",
                "0f b7 8c 48 0a 00 02 00 66 89 4a 06 8b 07 48 8b 4b 28 89 41 08 48 8b 4b 28 8b 47 04 89 41 0c ^"
                //"28 89 41 08 48 8b 4b 28 8b 47 04 89 41 0c ^"
            ),
            new(
                "Engine Base",
                "48 63 ca 48 8b 9c c8 78 20 02 00 48 85 db"
            ),
            new(
                "Current Target Id",
                "?? ?? ?? ?? ?? ?? ?? ?? ?? ?? ?? 3b c3 0f 94 c3 48 8b 07 48 8b cf ff 50 40 83 e8 06"
            ),
            new(
                "InventoryRenderer",
                "48 8b 51 08 48 8b 01 48 89 02 48 8b 11 48 8b 41 08 48 89 42 08 49 ff 88 08 04 00 00 ba 18 00 00 00 ?? ?? ?? ?? ?? 48 8b 37 48 b8 aa aa aa aa aa aa aa 0a 48 39 47 08 ?? ?? 48 89 7c 24 20 48 c7 44 24 28 00 00 00 00 b9 18 00 00 00 ?? ?? ?? ?? ff 48 89 58 10 48 ff 47 08 48 8b 4e 08 48 89 30 48 89 48 08 48 89 46 08 48 89 01 ^"
                
                // shorter alternative
                //"48 8b 51 08 48 8b 01 48 89 02 48 8b 11 48 8b 41 08 48 89 42 08 49 ff 88 08 04 00 00 ba 18 00 00 00"
            ),

            new(
                "Inventory UI Base",
                "48 8b 5c 24 30 b0 01 48 83 c4 38 c3 41 b9 ff ff ff ff c7 44 24 20 ff ff ff ff 41 b0 01 ^ ?? ?? ?? ?? ?? ?? ?? 41 8d 51 1d"
            ),

        };
    }
}
