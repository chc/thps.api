using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QScript
{
    public class SymbolEntryConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<SymbolEntry>);
        }
        #region Reader
        private ESymbolType GetSymbolTypeFromToken(JsonReader reader, bool zeroes = true, bool optimize = true) {
            ESymbolType result = ESymbolType.ESYMBOLTYPE_NONE;
            switch(reader.TokenType) {
                case JsonToken.Boolean:
                    result = ESymbolType.ESYMBOLTYPE_NAME;
                    break;
                case JsonToken.String:
                    result = ESymbolType.ESYMBOLTYPE_STRING;
                break;   
                case JsonToken.Integer:
                    result = ESymbolType.ESYMBOLTYPE_INTEGER;
                    if(optimize) {
                        long v = (long)reader.Value;
                        if(v > -127 && v < 127) {
                            result = ESymbolType.ESYMBOLTYPE_INTEGER_ONE_BYTE;
                        } else if(v > 255 && v > -32767 && v < 32767) {
                            result = ESymbolType.ESYMBOLTYPE_INTEGER_TWO_BYTES;
                        }

                        if(result == ESymbolType.ESYMBOLTYPE_INTEGER_ONE_BYTE || result == ESymbolType.ESYMBOLTYPE_INTEGER) {
                            if(v > 127 && v < 255) {
                                result = ESymbolType.ESYMBOLTYPE_UNSIGNED_INTEGER_ONE_BYTE;
                            }
                        }

                        if(result == ESymbolType.ESYMBOLTYPE_INTEGER_TWO_BYTES || result == ESymbolType.ESYMBOLTYPE_INTEGER) {
                            if(v > 255 && v < 65535) {
                                result = ESymbolType.ESYMBOLTYPE_UNSIGNED_INTEGER_TWO_BYTES;
                            }
                        }

                        if(zeroes && (long)v == 0) {
                            result = ESymbolType.ESYMBOLTYPE_ZERO_INTEGER;
                        }
                    }                    
                break;
                case JsonToken.Float:
                    result = ESymbolType.ESYMBOLTYPE_FLOAT;
                    if(zeroes && (double)reader.Value == 0.0) {
                        result = ESymbolType.ESYMBOLTYPE_ZERO_FLOAT;
                    }
                break;
                case JsonToken.EndObject:
                break;
                case JsonToken.StartObject:
                    result = ESymbolType.ESYMBOLTYPE_STRUCTURE;
                break;
                case JsonToken.StartArray:
                    result = ESymbolType.ESYMBOLTYPE_ARRAY;
                break;
                case JsonToken.Null:
                result = ESymbolType.ESYMBOLTYPE_NONE;
                break;
                default:
                throw new NotImplementedException();
            }
            return result;
        }
        SymbolEntry CondenseStructureToSymbol(List<SymbolEntry> list) {
            SymbolEntry entry = new SymbolEntry();
            var typeProperty = list.Where(s => s.name.ToString().Equals("type")).FirstOrDefault();
            if(typeProperty != null) {
                var typeName = typeProperty.value;
                if(typeName.ToString().Equals("name")) { //handle name
                    var nameValue = list.Where(s => s.name.ToString().Equals("name") || s.name.ToString().Equals("checksum")).FirstOrDefault();
                    entry.value = nameValue?.value ?? 0;
                    entry.type = ESymbolType.ESYMBOLTYPE_NAME;                            
                } else if(typeName.ToString().Equals("vec2") || typeName.ToString().Equals("vec3")) {
                    var vecList = (SymbolEntry)list.Where(s => s.name.ToString().Equals("value")).FirstOrDefault();
                    entry = vecList;
                    List<object> children = (List<object>)vecList.value;
                    if(children.Count == 2) {
                        entry.type = ESymbolType.ESYMBOLTYPE_PAIR;
                    } else {
                        entry.type = ESymbolType.ESYMBOLTYPE_VECTOR;
                    }
                } else {
                    return null;
                }
                return entry;
            }
            return null;
        }
        void PerformTypeConversions(List<SymbolEntry> symbols) {

           List<SymbolEntry> outputSymbols = new List<SymbolEntry>();
           foreach(var item in symbols) {
                if(item.type == ESymbolType.ESYMBOLTYPE_STRUCTURE) {
                    List<SymbolEntry> children = (List<SymbolEntry>)(item.value);
                    SymbolEntry result = CondenseStructureToSymbol(children);
                    if(result != null) {
                        item.value = result.value;
                        item.type = result.type;
                        item.subType = result.subType;
                    }
                    PerformTypeConversions(children);
                } else if (item.type == ESymbolType.ESYMBOLTYPE_ARRAY && item.subType == ESymbolType.ESYMBOLTYPE_STRUCTURE) {
                    List<object> arrayHead = (List<object>)item.value;

                    List<object> condensedChildren = new List<object>();
                    
                    foreach(var child in arrayHead) {
                        List<SymbolEntry> symbolList = (List<SymbolEntry>)child;
                        SymbolEntry result = CondenseStructureToSymbol(symbolList);
                        if(result != null) {
                            condensedChildren.Add(result);
                        } else {
                           PerformTypeConversions(symbolList);
                        }
                    }

                    if(condensedChildren.Count > 0) {
                        var itemList = new List<object>();
                        item.value = itemList;
                        foreach(var child in (List<object>)condensedChildren) {
                            var childSymbol = (SymbolEntry)child;
                            itemList.Add(childSymbol.value);
                        }
                        item.subType = ((SymbolEntry)(condensedChildren.First())).type;
                    }
                    
                }
            }
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {   
            List<SymbolEntry> result = (List<SymbolEntry>) ReadSymbolList(reader);
            PerformTypeConversions(result);
            return result;
            ///XXX: "second stage" conversions
            ///     convert name types to names
            ///     convert vec2/vec3
        }
        private object ReadSymbolList(JsonReader reader, int exitDepth = 1) {
            List<SymbolEntry> result = new List<SymbolEntry>();
            string lastPropertyName = null;
            while(reader.Read()) {
                if(reader.TokenType == JsonToken.PropertyName) {
                    lastPropertyName = (string)reader.Value;
                    
                } else if(GetSymbolTypeFromToken(reader) != ESymbolType.ESYMBOLTYPE_NONE) {
                    System.Diagnostics.Debug.Assert(!string.IsNullOrWhiteSpace(lastPropertyName));
                    result.Add(ReadSymbolEntry(reader, lastPropertyName));
                } else if(reader.TokenType == JsonToken.EndObject && reader.Depth == exitDepth) {
                    break;
                }
            }
            return result;
        }
        private int GetSymbolTypePrecedence(ESymbolType type) {
             if(type == ESymbolType.ESYMBOLTYPE_ZERO_INTEGER || type == ESymbolType.ESYMBOLTYPE_ZERO_FLOAT) {
                return 1;
            } else if(type == ESymbolType.ESYMBOLTYPE_INTEGER || type == ESymbolType.ESYMBOLTYPE_FLOAT || type == ESymbolType.ESYMBOLTYPE_STRING || type == ESymbolType.ESYMBOLTYPE_STRUCTURE) {
                return 6;
            } else if(type == ESymbolType.ESYMBOLTYPE_INTEGER_ONE_BYTE) {
                return 2;
            } else if(type == ESymbolType.ESYMBOLTYPE_UNSIGNED_INTEGER_ONE_BYTE) {
                return 3;
            } else if(type == ESymbolType.ESYMBOLTYPE_INTEGER_TWO_BYTES) {
                return 4;
            } else if(type == ESymbolType.ESYMBOLTYPE_UNSIGNED_INTEGER_TWO_BYTES) {
                return 5;
            }
            return 0;
        }
        private SymbolEntry ReadSymbolEntry(JsonReader reader, string propertyName) {
            SymbolEntry result = new SymbolEntry();
            result.name = propertyName;
            if(System.Int64.TryParse(propertyName, out System.Int64 checksum)) {
                result.name = checksum;
            }
            result.compressedByteSize = null;

            result.type = GetSymbolTypeFromToken(reader);
            result.subType = ESymbolType.ESYMBOLTYPE_NONE;
            int depth = reader.Depth;
            if(result.type == ESymbolType.ESYMBOLTYPE_ARRAY) {
                var list = new List<object>();
                int lastPrecedence = 0;
                do {
                    reader.Read();
                    if(reader.TokenType == JsonToken.StartObject) {
                        var symbolList = ReadSymbolList(reader, reader.Depth);
                        list.Add(symbolList);
                        result.subType = ESymbolType.ESYMBOLTYPE_STRUCTURE;
                        lastPrecedence = 4;
                    }
                    else if(reader.TokenType != JsonToken.EndArray) {
                        list.Add(reader.Value);
                        ESymbolType type = GetSymbolTypeFromToken(reader, false, false);

                        int precedence = GetSymbolTypePrecedence(type);                        

                        if(precedence > lastPrecedence) {
                            result.subType = type;
                            lastPrecedence = precedence;
                        }                        
                    }                        
                } while(reader.TokenType != JsonToken.EndArray || reader.Depth > depth);

                
                result.value = list;
                
            } else if(result.type == ESymbolType.ESYMBOLTYPE_STRUCTURE) {
                result.value = ReadSymbolList(reader, depth);
            } else if(result.type == ESymbolType.ESYMBOLTYPE_NAME && reader.ValueType == typeof(bool))  {
                result.value = result.name;
                result.name = (System.Int64)0;
            } else {
                result.value = reader.Value;
            }
            

            return result;
        }
        #endregion
        #region Writer
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            WriteSymbolList(writer, (List<SymbolEntry>)value);
        }
        private void WriteSymbolList(JsonWriter writer, List<SymbolEntry> value) {
            writer.WriteStartObject();
            foreach(var item in value) {
                WriteSymbolEntry(writer, item);
            }
            writer.WriteEndObject();
        }
        private void WriteSymbolEntry(JsonWriter writer, SymbolEntry value) {
            if(value.type == ESymbolType.ESYMBOLTYPE_NAME) { //write as flag
                if(value.name.ToString().Equals("0")) {
                    writer.WritePropertyName(value.value.ToString());
                    writer.WriteValue(true);
                    return;
                }
            } 
            var propertyName = value.name.ToString();
            writer.WritePropertyName(propertyName);
            if(CanConvert(value.GetType())) {
                WriteJson(writer, value, null);
            } else if (value.type == ESymbolType.ESYMBOLTYPE_ARRAY) {
                WriteSymbolArray(writer, value.value, value);
            } else if(value.type == ESymbolType.ESYMBOLTYPE_NAME) {
                WriteNameValue(writer, value.value);
            } else if(value.type == ESymbolType.ESYMBOLTYPE_STRUCTURE) {
                WriteSymbolList(writer, (List<SymbolEntry>)value.value);
            } else if(value.type == ESymbolType.ESYMBOLTYPE_VECTOR || value.type == ESymbolType.ESYMBOLTYPE_PAIR) {
                WriteVector(writer, value.value, value);
            } else {
                writer.WriteValue(value.value);
            }
        }
        private void WriteVector(JsonWriter writer, object value, SymbolEntry parent) {
            writer.WriteStartObject();
            
            writer.WritePropertyName("value");
            writer.WriteStartArray();
            foreach(var arrayItem in (List<System.Single>)value) {
                writer.WriteValue(arrayItem);
            }
            writer.WriteEndArray();

            writer.WritePropertyName("type");
            if(parent.type == ESymbolType.ESYMBOLTYPE_VECTOR) {
                writer.WriteValue("vec3");
            } else if(parent.type == ESymbolType.ESYMBOLTYPE_PAIR) {
                writer.WriteValue("vec2");
            } else {
                throw new NotImplementedException();
            }

            writer.WriteEndObject();
            
        }
        private void WriteSymbolArray(JsonWriter writer, object value, SymbolEntry parent) {
            writer.WriteStartArray();    
            foreach(var arrayItem in (List<object>)value) {
                switch(parent.subType) {
                    case ESymbolType.ESYMBOLTYPE_NAME:
                        WriteNameValue(writer, arrayItem);
                    break;
                    case ESymbolType.ESYMBOLTYPE_STRUCTURE:
                        WriteSymbolList(writer, (List<SymbolEntry>)arrayItem);
                    break;
                    case ESymbolType.ESYMBOLTYPE_VECTOR:
                    case ESymbolType.ESYMBOLTYPE_PAIR:
                    throw new NotImplementedException();
                    break;
                    default:
                        writer.WriteValue(arrayItem);
                    break;
                }
                
            }
            writer.WriteEndArray();
        }
        private void WriteNameValue(JsonWriter writer, object value) {
            writer.WriteStartObject();
            if(value.GetType() == typeof(System.UInt32) || value.GetType() == typeof(System.UInt64)) {
                writer.WritePropertyName("checksum");
                writer.WriteValue(value);
            } else {
                writer.WritePropertyName("name");
                writer.WriteValue(value.ToString());
            }
            writer.WritePropertyName("type");
            writer.WriteValue("name");
            writer.WriteEndObject();
        }
        #endregion
    }
}
