using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QScript
{
    public class QValueConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if(reader.TokenType == JsonToken.StartArray)
            {
                var list = new List<object>();
                JArray jarray = JArray.Load(reader);
                foreach (var item in jarray)
                {
                    switch (item.Type)
                    {
                        case JTokenType.Array:
                            var subList = new List<SymbolEntry>();
                            foreach (var subItem in item)
                            {
                                subList.Add(subItem.ToObject<SymbolEntry>());
                            }
                            list.Add(subList);
                            break;
                        case JTokenType.Object:
                            list.Add(item.ToObject<SymbolEntry>());
                            break;
                        default:
                            switch(item.Type)
                            {
                                case JTokenType.Integer:
                                    list.Add((System.Int64)item);
                                    break;
                                case JTokenType.Float:
                                    list.Add((float)item);
                                    break;
                                case JTokenType.String:
                                    list.Add(item.ToString());
                                    break;
                            }
                            break;
                    }
                }
                return list;
            } if(reader.TokenType == JsonToken.String)
            {
                return (string)reader.Value;
            } else if(reader.TokenType == JsonToken.Float)
            {
                return float.Parse(reader.Value.ToString());
            } else if(reader.TokenType == JsonToken.Integer)
            {
                return System.Int64.Parse(reader.Value.ToString());
            }
            return reader.Value;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
