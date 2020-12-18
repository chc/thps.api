﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text;

namespace QScript
{
    public class SymbolEntry
    {
        public object name;
        [JsonConverter(typeof(StringEnumConverter))]
        public ESymbolType type;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public ESymbolType? subType;
        [JsonConverter(typeof(QValueConverter))]
        public object value;
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        //[JsonIgnore]
        public int? compressedByteSize;
    }
    public enum ESymbolType
    {
        ESYMBOLTYPE_NONE = 0,
        ESYMBOLTYPE_INTEGER,
        ESYMBOLTYPE_FLOAT,
        ESYMBOLTYPE_STRING,
        ESYMBOLTYPE_LOCALSTRING,
        ESYMBOLTYPE_PAIR,
        ESYMBOLTYPE_VECTOR,
        ESYMBOLTYPE_QSCRIPT,
        ESYMBOLTYPE_CFUNCTION,
        ESYMBOLTYPE_MEMBERFUNCTION,
        ESYMBOLTYPE_STRUCTURE,
        // ESYMBOLTYPE_STRUCTUREPOINTER is not really used any more. It is only supported as a valid
        // type that can be sent to AddComponent, which is an old CStruct member function that is
        // still supported for back compatibility. mType will never be ESYMBOLTYPE_STRUCTUREPOINTER
        ESYMBOLTYPE_STRUCTUREPOINTER,
        ESYMBOLTYPE_ARRAY,
        ESYMBOLTYPE_NAME,

        // These symbols are just used for memory optimization by the
        // CScriptStructure WriteToBuffer and ReadFromBuffer functions.
        ESYMBOLTYPE_INTEGER_ONE_BYTE,
        ESYMBOLTYPE_INTEGER_TWO_BYTES,
        ESYMBOLTYPE_UNSIGNED_INTEGER_ONE_BYTE,
        ESYMBOLTYPE_UNSIGNED_INTEGER_TWO_BYTES,
        ESYMBOLTYPE_ZERO_INTEGER,
        ESYMBOLTYPE_ZERO_FLOAT,

        // Warning! Don't exceed 256 entries, since Type is a uint8 in SSymbolTableEntry
        // New warning! Don't exceed 64 entries, because the top two bits of the symbol
        // type are used to indicate whether the name checksum has been compressed to
        // a 8 or 16 bit index, when WriteToBuffer writes out parameter names.
    };

    // These get masked onto the symbol type in CScriptStructure::WriteToBuffer if
    // the following name checksum matches one in the lookup table. (Defined in compress.q)
    enum BIT_NAME_LOOKUP
    {
        MASK_8_BIT_NAME_LOOKUP = 128,
        MASK_16_BIT_NAME_LOOKUP = 64
    };
}
