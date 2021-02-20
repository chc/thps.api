using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace QScript
{
    public class SymbolBufferWriter
    {
        private Crc32 crc32;
        public class BufferWriterOptions
        {
            public bool useCompressedTable;
            public GamePlatform platform;
            public GameVersion version;
        }
        public class CompressedKeyInfo
        {
            public int byteSize;
            public int index;
        }

        public SymbolBufferWriter()
        {
            crc32 = new Crc32();
        }

        public void SerializeBuffer(BinaryWriter bw, List<SymbolEntry> symbols)
        {
            foreach (var symbol in symbols)
            {
                WriteSymbol((SymbolEntry)symbol, bw);
            }
            bw.Write((byte)ESymbolType.ESYMBOLTYPE_NONE);
        }
        private void WriteSymbol(SymbolEntry symbol, BinaryWriter bw)
        {
            byte type = (byte)symbol.type;
            if(symbol.compressedByteSize == 1)
            {
                type |= (byte)BIT_NAME_LOOKUP.MASK_8_BIT_NAME_LOOKUP;
            } else if(symbol.compressedByteSize == 2)
            {
                type |= (byte)BIT_NAME_LOOKUP.MASK_16_BIT_NAME_LOOKUP;
            }
            bw.Write(type);
            System.UInt32 checksum;
            if(System.UInt32.TryParse(symbol.name.ToString(), out checksum))
            {
                if(symbol.compressedByteSize == 1)
                {
                    bw.Write((byte)checksum);
                } else if(symbol.compressedByteSize == 2)
                {
                    bw.Write((System.UInt16)checksum);
                } else
                {
                    bw.Write(checksum);
                }
            } else
            {
                checksum = crc32.Get(Encoding.ASCII.GetBytes(symbol.name.ToString().ToLower()));
                bw.Write(checksum);
            }
            WriteSymbolValue(symbol, bw);
        }
        private void WriteSymbolValue(SymbolEntry entry, BinaryWriter bw)
        {
            List<object> list = null;
            switch (entry.type)
            {
                case ESymbolType.ESYMBOLTYPE_NONE:
                case ESymbolType.ESYMBOLTYPE_ZERO_FLOAT:
                case ESymbolType.ESYMBOLTYPE_ZERO_INTEGER:
                    break;
                case ESymbolType.ESYMBOLTYPE_INTEGER_ONE_BYTE:
                    bw.Write(byte.Parse(entry.value.ToString()));
                    break;
                case ESymbolType.ESYMBOLTYPE_UNSIGNED_INTEGER_ONE_BYTE:
                    bw.Write(byte.Parse(entry.value.ToString()));
                    break;
                case ESymbolType.ESYMBOLTYPE_INTEGER_TWO_BYTES:
                    System.Int16 _sVal;
                    if (System.Int16.TryParse(entry.value.ToString(), out _sVal))
                    {
                        bw.Write(_sVal);
                    }
                    break;
                case ESymbolType.ESYMBOLTYPE_UNSIGNED_INTEGER_TWO_BYTES:
                    System.UInt16 _usVal;
                    if(System.UInt16.TryParse(entry.value.ToString(), out _usVal))
                    {
                        bw.Write(_usVal);
                    }
                    break;
                case ESymbolType.ESYMBOLTYPE_FLOAT:
                    bw.Write(float.Parse(entry.value.ToString()));
                    break;
                case ESymbolType.ESYMBOLTYPE_VECTOR:
                    list = (List<object>)entry.value;
                    for(int i=0;i<3;i++)
                    {
                        if(list[i].GetType() != typeof(System.Single))
                        {
                            bw.Write(float.Parse(list[i].ToString()));
                        } else
                        {
                            bw.Write((float)list[i]);
                        }
                        
                    }
                    break;
                case ESymbolType.ESYMBOLTYPE_PAIR:
                    list = (List<object>)entry.value;
                    for (int i = 0; i < 2; i++)
                    {
                        if (list[i].GetType() != typeof(System.Single))
                        {
                            bw.Write(float.Parse(list[i].ToString()));
                        }
                        else
                        {
                            bw.Write((float)list[i]);
                        }

                    }
                    break;
                case ESymbolType.ESYMBOLTYPE_STRING:
                    bw.Write(Encoding.ASCII.GetBytes(entry.value.ToString()));
                    bw.Write((byte)0);
                    break;
                case ESymbolType.ESYMBOLTYPE_STRUCTURE:
                    var lst = (IEnumerable<object>)entry.value;
                    var structList = new List<SymbolEntry>();
                    foreach (var structItem in lst)
                    {
                        structList.Add((SymbolEntry)structItem);
                    }
                    SerializeBuffer(bw, structList);
                    break;
                case ESymbolType.ESYMBOLTYPE_ARRAY:
                    WriteArray(entry, bw);
                    break;
                case ESymbolType.ESYMBOLTYPE_NAME:
                    bw.Write(System.UInt32.Parse(entry.value.ToString()));
                    break;
                case ESymbolType.ESYMBOLTYPE_INTEGER:
                    bw.Write(System.Int32.Parse(entry.value.ToString()));
                    break;
                default:
                    throw new ArgumentException("Got unhandled symbol", entry.type.ToString());
            }
        }
        private void WriteArray(SymbolEntry entry, BinaryWriter bw)
        {
            List<object> items = (List < object > )entry.value;
            bw.Write((byte)entry.subType.Value);
            bw.Write((System.UInt16)items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                var subSymbol = new SymbolEntry();
                subSymbol.type = entry.subType.Value;
                subSymbol.value = items[i];
                WriteSymbolValue(subSymbol, bw);
            }
        }
    }
}
