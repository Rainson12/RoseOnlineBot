using RoseOnlineBot.Models.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoseOnlineBot.Business
{
    internal class MetaDataReader
    {
        public struct STBFieldString
        {
            // Token: 0x1700000D RID: 13
            // (get) Token: 0x06000026 RID: 38 RVA: 0x0000348C File Offset: 0x0000248C
            // (set) Token: 0x06000027 RID: 39 RVA: 0x00003494 File Offset: 0x00002494
            public string FieldString
            {
                get
                {
                    return this.m_strFieldString;
                }
                set
                {
                    this.m_strFieldString = value;
                }
            }

            // Token: 0x0400001A RID: 26
            internal ushort m_strFieldStringLength;

            // Token: 0x0400001B RID: 27
            private string m_strFieldString;
        }

        // Token: 0x02000008 RID: 8
        public struct STBFieldStringArray
        {
            // Token: 0x1700000E RID: 14
            // (get) Token: 0x06000028 RID: 40 RVA: 0x0000349D File Offset: 0x0000249D
            // (set) Token: 0x06000029 RID: 41 RVA: 0x000034A5 File Offset: 0x000024A5
            public STBFieldString[] FieldString
            {
                get
                {
                    return this.m_STBFieldStringArray;
                }
                set
                {
                    this.m_STBFieldStringArray = value;
                }
            }

            // Token: 0x0400001C RID: 28
            private STBFieldString[] m_STBFieldStringArray;
        }

        // Token: 0x02000009 RID: 9
        public struct STBFieldComment
        {
            // Token: 0x1700000F RID: 15
            // (get) Token: 0x0600002A RID: 42 RVA: 0x000034AE File Offset: 0x000024AE
            // (set) Token: 0x0600002B RID: 43 RVA: 0x000034B6 File Offset: 0x000024B6
            public string FieldComment
            {
                get
                {
                    return this.m_strFieldComment;
                }
                set
                {
                    this.m_strFieldComment = value;
                }
            }

            // Token: 0x0400001D RID: 29
            internal ushort m_strFieldCommentLength;

            // Token: 0x0400001E RID: 30
            private string m_strFieldComment;
        }
        public List<UsableItem> ReadUsableRecoveryItems()
        {
            try
            {
                var filename = @"C:\Users\Rainson\Downloads\vfs_extractor-v1-win-x86_64\LIST_USEITEM.STB";
                FileStream fileStream = File.OpenRead(filename);
                BinaryReader binaryReader = new BinaryReader(fileStream, Encoding.ASCII);
                var m_strFileName = filename;
                var m_strFileType = new string(binaryReader.ReadChars(4));
                var m_iDataOffset = binaryReader.ReadUInt32();
                var m_iRowCount = binaryReader.ReadUInt32();
                var m_iFieldCount = binaryReader.ReadUInt32();
                var m_iUnknown = binaryReader.ReadUInt32();
                var m_ColumnsWidth = new ushort[m_iFieldCount + 1U];
                for (int i = 0; i < m_ColumnsWidth.Length; i++)
                {
                    m_ColumnsWidth[i] = binaryReader.ReadUInt16();
                }
                var m_ColumnsHeaderText = new string[m_ColumnsWidth.Length];
                m_ColumnsHeaderText[0] = "#";
                for (int j = 1; j < m_ColumnsHeaderText.Length; j++)
                {
                    ushort num = binaryReader.ReadUInt16();
                    string text = Encoding.ASCII.GetString(binaryReader.ReadBytes((int)num));
                    m_ColumnsHeaderText[j] = text;
                }
                var m_iStructNameLength = binaryReader.ReadUInt16();
                var m_strStructName = Encoding.ASCII.GetString(binaryReader.ReadBytes((int)m_iStructNameLength));
                var m_DataTable = new DataTable();
                for (int k = 0; k < m_ColumnsHeaderText.Length; k++)
                {
                    m_DataTable.Columns.Add(string.Empty);
                }
                var m_STBFieldComment = new STBFieldComment[m_iRowCount - 1U];
                for (int l = 0; l < m_STBFieldComment.Length; l++)
                {
                    m_DataTable.Rows.Add(m_DataTable.NewRow());
                }
                for (int m = 0; m < m_STBFieldComment.Length; m++)
                {
                    m_STBFieldComment[m].m_strFieldCommentLength = binaryReader.ReadUInt16();
                    m_STBFieldComment[m].FieldComment = Encoding.ASCII.GetString(binaryReader.ReadBytes((int)m_STBFieldComment[m].m_strFieldCommentLength));
                    m_DataTable.Rows[m][1] = m_STBFieldComment[m].FieldComment;
                }
                var m_STBFieldStringArray = new STBFieldStringArray[m_iRowCount - 1U];
                for (int n = 0; n < m_STBFieldStringArray.Length; n++)
                {
                    m_STBFieldStringArray[n].FieldString = new STBFieldString[m_iFieldCount - 1U];
                    m_DataTable.Rows[n][0] = n.ToString();
                    for (int num2 = 0; num2 < m_STBFieldStringArray[n].FieldString.Length; num2++)
                    {
                        m_STBFieldStringArray[n].FieldString[num2].m_strFieldStringLength = binaryReader.ReadUInt16();
                        m_STBFieldStringArray[n].FieldString[num2].FieldString = Encoding.ASCII.GetString(binaryReader.ReadBytes((int)m_STBFieldStringArray[n].FieldString[num2].m_strFieldStringLength));
                        m_DataTable.Rows[n][num2 + 2] = m_STBFieldStringArray[n].FieldString[num2].FieldString;
                    }
                }

                var usableItems = new List<UsableItem>();
                foreach (DataRow row in m_DataTable.Rows)
                {
                    var itemName = row[2].ToString();
                    if (itemName != "")
                    {
                        var id = row[0].ToString();
                        var @type = uint.Parse(row[6].ToString() != "" ? row[6].ToString() : "0"); // 311 = potion, 312 == food/dring, 320 = item drop like HP +1000

                        var affectsStat = ushort.Parse(row[31].ToString() != "" ? row[31].ToString() : "0"); // 16 = hp, 17 = mp
                        var recoveryValue = uint.Parse(row[32].ToString() != "" ? row[32].ToString() : "0"); // hp or mp value to recover
                        if(@type == 311 || @type == 312 || @type == 315)
                        usableItems.Add(new UsableItem() { ItemId = ushort.Parse(id), Name = itemName, RestoreAmount = recoveryValue,  @ItemType = 
                            @type == 311 && affectsStat == 16 ? ItemType.HPPotion :
                            @type == 311 && affectsStat == 17 ? ItemType.MPPotion :
                            @type == 312 && affectsStat == 16 ? ItemType.Food :
                            @type == 311 && affectsStat == 17 ? ItemType.Drink :
                            @type == 315 ? ItemType.Repair : ItemType.Unkown });
                    }
                }
                binaryReader.Close();
                return usableItems;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
