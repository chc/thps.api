using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Asset.JsonConverters
{
    public class MeshFlagConverter : JsonConverter<System.UInt32>
    {
        public override System.UInt32 ReadJson(JsonReader reader, Type objectType, [AllowNull] System.UInt32 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            int result = 0;
            if (reader.TokenType == JsonToken.StartArray)
            {
                JArray jarray = JArray.Load(reader);
                foreach (var item in jarray)
                {
                    if (item.Type == JTokenType.String)
                    {
                        var flag_name = item.ToObject<string>();
                        EMeshFlags flag = Enum.Parse<EMeshFlags>(flag_name);
                        result |= (int)flag;
                    }
                }
            }
            return (uint)result;
        }

        public override void WriteJson(JsonWriter writer, [AllowNull] System.UInt32 value, JsonSerializer serializer)
        {
            writer.WriteStartArray();
            if ((value & (int)EMeshFlags.MESH_FLAG_IS_INSTANCE) != 0)
            {
                writer.WriteValue(EMeshFlags.MESH_FLAG_IS_INSTANCE.ToString());
            }
            if ((value & (int)EMeshFlags.MESH_FLAG_NO_SKATER_SHADOW) != 0)
            {
                writer.WriteValue(EMeshFlags.MESH_FLAG_NO_SKATER_SHADOW.ToString());
            }
            if ((value & (int)EMeshFlags.MESH_FLAG_MATERIAL_COLOR_OVERRIDE) != 0)
            {
                writer.WriteValue(EMeshFlags.MESH_FLAG_MATERIAL_COLOR_OVERRIDE.ToString());
            }
            if ((value & (int)EMeshFlags.MESH_FLAG_VERTEX_COLOR_WIBBLE) != 0)
            {
                writer.WriteValue(EMeshFlags.MESH_FLAG_VERTEX_COLOR_WIBBLE.ToString());
            }
            if ((value & (int)EMeshFlags.MESH_FLAG_BILLBOARD) != 0)
            {
                writer.WriteValue(EMeshFlags.MESH_FLAG_BILLBOARD.ToString());
            }
            if ((value & (int)EMeshFlags.MESH_FLAG_HAS_TRANSFORM) != 0)
            {
                writer.WriteValue(EMeshFlags.MESH_FLAG_HAS_TRANSFORM.ToString());
            }
            if ((value & (int)EMeshFlags.MESH_FLAG_ACTIVE) != 0)
            {
                writer.WriteValue(EMeshFlags.MESH_FLAG_ACTIVE.ToString());
            }
            if ((value & (int)EMeshFlags.MESH_FLAG_NO_ANISOTROPIC) != 0)
            {
                writer.WriteValue(EMeshFlags.MESH_FLAG_NO_ANISOTROPIC.ToString());
            }
            if ((value & (int)EMeshFlags.MESH_FLAG_NO_ZWRITE) != 0)
            {
                writer.WriteValue(EMeshFlags.MESH_FLAG_NO_ZWRITE.ToString());
            }
            if ((value & (int)EMeshFlags.MESH_FLAG_SHADOW_VOLUME) != 0)
            {
                writer.WriteValue(EMeshFlags.MESH_FLAG_SHADOW_VOLUME.ToString());
            }
            if ((value & (int)EMeshFlags.MESH_FLAG_BUMPED_WATER) != 0)
            {
                writer.WriteValue(EMeshFlags.MESH_FLAG_BUMPED_WATER.ToString());
            }
            if ((value & (int)EMeshFlags.MESH_FLAG_UNLIT) != 0)
            {
                writer.WriteValue(EMeshFlags.MESH_FLAG_UNLIT.ToString());
            }
            writer.WriteEndArray();
        }
    }
}
