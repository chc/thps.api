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

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            WriteSymbolList(writer, (List<SymbolEntry>)value, serializer);
        }
        private void WriteSymbolList(JsonWriter writer, List<SymbolEntry> value, JsonSerializer serializer) {
            writer.WriteStartObject();
            foreach(var item in value) {
                WriteSymbolEntry(writer, item, serializer);
            }
            writer.WriteEndObject();
        }
        private void WriteSymbolEntry(JsonWriter writer, SymbolEntry value, JsonSerializer serializer) {
            var propertyName = value.name.ToString();
            writer.WritePropertyName(propertyName);
            if(CanConvert(value.GetType())) {
                WriteJson(writer, value, serializer);
            } else if (value.type == ESymbolType.ESYMBOLTYPE_ARRAY) {
                WriteSymbolArray(writer, value.value, serializer, value);
            } else if(value.type == ESymbolType.ESYMBOLTYPE_NAME) {
                WriteNameValue(writer, value.value, serializer);
            } else if(value.type == ESymbolType.ESYMBOLTYPE_STRUCTURE) {
                WriteSymbolList(writer, (List<SymbolEntry>)value.value, serializer);
            } else if(value.type == ESymbolType.ESYMBOLTYPE_VECTOR || value.type == ESymbolType.ESYMBOLTYPE_PAIR) {
                WriteVector(writer, value.value, serializer, value);
            } else {
                writer.WriteValue(value.value);
            }
        }
        private void WriteVector(JsonWriter writer, object value, JsonSerializer serializer, SymbolEntry parent) {
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
        private void WriteSymbolArray(JsonWriter writer, object value, JsonSerializer serializer, SymbolEntry parent) {
            writer.WriteStartArray();
            if(parent.subType == ESymbolType.ESYMBOLTYPE_STRUCTURE) {
                foreach(var symbolList in (List<object>)value) {
                    WriteSymbolList(writer, (List<SymbolEntry>)symbolList, serializer);
                }
                
            } else {            
                foreach(var arrayItem in (List<object>)value) {
                    writer.WriteValue(arrayItem);
                }
            }
            writer.WriteEndArray();
        }
        private void WriteNameValue(JsonWriter writer, object value, JsonSerializer serializer) {
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
    }
}
