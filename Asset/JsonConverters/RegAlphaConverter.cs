using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Asset.JsonConverters
{
    public class RegAlphaConverter : JsonConverter<System.UInt64>
    {
        public override System.UInt64 ReadJson(JsonReader reader, Type objectType, [AllowNull] System.UInt64 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            System.UInt64 result = 0;

            EBlendModes blend_mode = EBlendModes.vBLEND_MODE_ADD; ;
            ulong fixed_alpha = 0;
            if (reader.TokenType == JsonToken.StartObject)
            {
                reader.Read();
                var values = new Dictionary<string, object>();
                while (reader.TokenType != JsonToken.EndObject)
                {
                    if (reader.TokenType != JsonToken.PropertyName)
                        throw new FormatException("Expected a property name, got: " + reader.TokenType);
                    string propertyName = reader.Value.ToString();
                    reader.Read();

                    values.Add(propertyName, reader.Value);
                    reader.Read();
                }
                if (values.ContainsKey("blend_mode"))
                {
                    blend_mode = Enum.Parse<EBlendModes>(values["blend_mode"].ToString());
                }
                if (values.ContainsKey("fixed_alpha"))
                {
                    fixed_alpha = ulong.Parse(values["fixed_alpha"].ToString());
                }
            }

            result = ((System.UInt64)blend_mode & 0x00FFFFFFUL) | (fixed_alpha & 0xFF000000UL);
            return result;
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] System.UInt64 value, JsonSerializer serializer)
        {
            System.UInt64 blend_mode = value & 0x00FFFFFFUL;
            System.UInt64 fixed_alpha = value & 0xFF000000UL;

            var blend_mode_enum = (EBlendModes)blend_mode;

            writer.WriteStartObject();

            writer.WritePropertyName("blend_mode");
            writer.WriteValue(blend_mode_enum.ToString());

            writer.WritePropertyName("fixed_alpha");
            writer.WriteValue(fixed_alpha);

            writer.WriteEndObject();

        }
    }
}
