using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Asset.JsonConverters
{
    public class MaterialFlagConverter : JsonConverter<System.UInt32>
    {
        public override uint ReadJson(JsonReader reader, Type objectType, [AllowNull] System.UInt32 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            int result = 0;
            if (reader.TokenType == JsonToken.StartArray)
            {
                JArray jarray = JArray.Load(reader);
                foreach (var item in jarray)
                {
                    if(item.Type == JTokenType.String)
                    {
                        var flag_name = item.ToObject<string>();
                        EMaterialFlag flag = Enum.Parse<EMaterialFlag>(flag_name);
                        result |= (int)flag;
                    }
                }
            }
            return (uint)result;
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] System.UInt32 value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            if ((value & (int)EMaterialFlag.MATFLAG_UV_WIBBLE) != 0)
            {
                writer.WriteValue(EMaterialFlag.MATFLAG_UV_WIBBLE.ToString());
            }
            if ((value & (int)EMaterialFlag.MATFLAG_VC_WIBBLE) != 0)
            {
                writer.WriteValue(EMaterialFlag.MATFLAG_VC_WIBBLE.ToString());
            }
            if ((value & (int)EMaterialFlag.MATFLAG_TEXTURED) != 0)
            {
                writer.WriteValue(EMaterialFlag.MATFLAG_TEXTURED.ToString());
            }
            if ((value & (int)EMaterialFlag.MATFLAG_ENVIRONMENT) != 0)
            {
                writer.WriteValue(EMaterialFlag.MATFLAG_ENVIRONMENT.ToString());
            }
            if ((value & (int)EMaterialFlag.MATFLAG_DECAL) != 0)
            {
                writer.WriteValue(EMaterialFlag.MATFLAG_DECAL.ToString());
            }
            if ((value & (int)EMaterialFlag.MATFLAG_SMOOTH) != 0)
            {
                writer.WriteValue(EMaterialFlag.MATFLAG_SMOOTH.ToString());
            }
            if ((value & (int)EMaterialFlag.MATFLAG_TRANSPARENT) != 0)
            {
                writer.WriteValue(EMaterialFlag.MATFLAG_TRANSPARENT.ToString());
            }
            if ((value & (int)EMaterialFlag.MATFLAG_PASS_COLOR_LOCKED) != 0)
            {
                writer.WriteValue(EMaterialFlag.MATFLAG_PASS_COLOR_LOCKED.ToString());
            }
            if ((value & (int)EMaterialFlag.MATFLAG_SPECULAR) != 0)
            {
                writer.WriteValue(EMaterialFlag.MATFLAG_SPECULAR.ToString());
            }
            if ((value & (int)EMaterialFlag.MATFLAG_BUMP_SIGNED_TEXTURE) != 0)
            {
                writer.WriteValue(EMaterialFlag.MATFLAG_BUMP_SIGNED_TEXTURE.ToString());
            }
            if ((value & (int)EMaterialFlag.MATFLAG_BUMP_LOAD_MATRIX) != 0)
            {
                writer.WriteValue(EMaterialFlag.MATFLAG_BUMP_LOAD_MATRIX.ToString());
            }
            if ((value & (int)EMaterialFlag.MATFLAG_PASS_TEXTURE_ANIMATES) != 0)
            {
                writer.WriteValue(EMaterialFlag.MATFLAG_PASS_TEXTURE_ANIMATES.ToString());
            }
            if ((value & (int)EMaterialFlag.MATFLAG_PASS_IGNORE_VERTEX_ALPHA) != 0)
            {
                writer.WriteValue(EMaterialFlag.MATFLAG_PASS_IGNORE_VERTEX_ALPHA.ToString());
            }
            if ((value & (int)EMaterialFlag.MATFLAG_EXPLICIT_UV_WIBBLE) != 0)
            {
                writer.WriteValue(EMaterialFlag.MATFLAG_EXPLICIT_UV_WIBBLE.ToString());
            }
            if ((value & (int)EMaterialFlag.MATFLAG_WATER_EFFECT) != 0)
            {
                writer.WriteValue(EMaterialFlag.MATFLAG_WATER_EFFECT.ToString());
            }
            if ((value & (int)EMaterialFlag.MATFLAG_NO_MAT_COL_MOD) != 0)
            {
                writer.WriteValue(EMaterialFlag.MATFLAG_NO_MAT_COL_MOD.ToString());
            }
            writer.WriteEndArray();
        }
    }
}
