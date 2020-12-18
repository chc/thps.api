using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace QScript
{
    public class SymbolBufferReader
    {
        BinaryReader _bs;
        public SymbolBufferReader(BinaryReader bs)
        {
            _bs = bs;
        }

        public void Dispose()
        {
            _bs.Dispose();
        }

        public List<SymbolEntry> ReadBuffer()
        {
            List<SymbolEntry> objects = new List<SymbolEntry>();
            while (true)
            {
                int skipSize;
                ESymbolType symbol = GetNextSymbol(out skipSize);
                ESymbolType? subType;
                if ((ESymbolType)symbol == ESymbolType.ESYMBOLTYPE_NONE) break;
                System.UInt32 checksum = GetNextChecksum(skipSize);
                object value = GetSymbolValue(symbol, out subType);
                SymbolEntry entry = new SymbolEntry();
                entry.name = checksum;
                entry.value = value;
                entry.type = symbol;
                entry.subType = subType;
                if (skipSize != 4)
                    entry.compressedByteSize = skipSize;
                objects.Add(entry);
            }

            return objects;
        }
        private System.UInt32 GetNextChecksum(int skipSize)
        {
            if(skipSize == 1)
            {
                return (System.UInt32)_bs.ReadByte();
            } else if(skipSize == 2)
            {
                return (System.UInt32)_bs.ReadUInt16();
            }
            return _bs.ReadUInt32();
        }
        private ESymbolType GetNextSymbol(out int skipSize)
        {
            System.UInt32 b = (System.UInt32)_bs.ReadByte();
            skipSize = 4;
            if((b & (System.UInt32)BIT_NAME_LOOKUP.MASK_8_BIT_NAME_LOOKUP) != 0)
            {
                skipSize = 1;
                b &= ~(System.UInt32)BIT_NAME_LOOKUP.MASK_8_BIT_NAME_LOOKUP;
            }
            if ((b & (System.UInt32)BIT_NAME_LOOKUP.MASK_16_BIT_NAME_LOOKUP) != 0)
            {
                skipSize = 2;
                b &= ~(System.UInt32)BIT_NAME_LOOKUP.MASK_16_BIT_NAME_LOOKUP;
            }
            return (ESymbolType)(b);
        }
        private object ReadArray(out ESymbolType? subType)
        {
            int skipSize;
            subType = GetNextSymbol(out skipSize);
            System.UInt16 size = _bs.ReadUInt16();
            List<object> array = new List<object>();
            for(int i=0;i<size;i++) {
                ESymbolType? dropType;
                array.Add(GetSymbolValue(subType.Value, out dropType));
            }
            return array;
        }
        private object GetSymbolValue(ESymbolType type, out ESymbolType? subType)
        {
            subType = null;
            var float_list = new List<float>();
            switch (type)
            {
                case ESymbolType.ESYMBOLTYPE_ZERO_INTEGER:
                    return (System.Int32)(0);
                case ESymbolType.ESYMBOLTYPE_ZERO_FLOAT:
                    return 0.0f;
                case ESymbolType.ESYMBOLTYPE_INTEGER_ONE_BYTE:
                    return (System.Int32)_bs.ReadByte();
                case ESymbolType.ESYMBOLTYPE_INTEGER_TWO_BYTES:
                    return (short)_bs.ReadInt16();
                case ESymbolType.ESYMBOLTYPE_UNSIGNED_INTEGER_ONE_BYTE:
                    return (System.UInt32)(_bs.ReadByte());
                case ESymbolType.ESYMBOLTYPE_UNSIGNED_INTEGER_TWO_BYTES:
                    return (System.UInt16)(_bs.ReadUInt16());
                case ESymbolType.ESYMBOLTYPE_FLOAT:
                    return (float)_bs.ReadSingle();
                case ESymbolType.ESYMBOLTYPE_INTEGER:
                    return _bs.ReadInt32();
                case ESymbolType.ESYMBOLTYPE_NONE:
                    return ESymbolType.ESYMBOLTYPE_NONE;
                case ESymbolType.ESYMBOLTYPE_NAME:
                    return _bs.ReadUInt32();
                case ESymbolType.ESYMBOLTYPE_ARRAY:
                    return ReadArray(out subType);
                case ESymbolType.ESYMBOLTYPE_STRUCTURE:
                    SymbolBufferReader reader = new SymbolBufferReader(_bs);
                    return reader.ReadBuffer();
                case ESymbolType.ESYMBOLTYPE_PAIR:
                    float_list.Add(_bs.ReadSingle());
                    float_list.Add(_bs.ReadSingle());
                    return float_list;
                case ESymbolType.ESYMBOLTYPE_VECTOR:
                    float_list.Add(_bs.ReadSingle());
                    float_list.Add(_bs.ReadSingle());
                    float_list.Add(_bs.ReadSingle());
                    return float_list;
                case ESymbolType.ESYMBOLTYPE_LOCALSTRING:
                case ESymbolType.ESYMBOLTYPE_STRING:
                    return readString();
                default:
                    throw new ArgumentException("Got unhandled symbol", type.ToString());
            }
        }
        private String readString()
        {
            StringBuilder sb = new StringBuilder();
            byte ch;
            do
            {
                ch = _bs.ReadByte();
                sb.Append((char)ch);
            } while (ch != 0);
            String s = sb.ToString();
            s = s.Substring(0, s.Length - 1);
            return s;
        }
    }
}
