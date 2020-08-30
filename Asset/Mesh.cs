using Asset.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Asset
{
    public enum EMeshFlags
    {
        MESH_FLAG_IS_INSTANCE = 0x01,
        MESH_FLAG_NO_SKATER_SHADOW = 0x02,
        MESH_FLAG_MATERIAL_COLOR_OVERRIDE = 0x04,
        MESH_FLAG_VERTEX_COLOR_WIBBLE = 0x08,
        MESH_FLAG_BILLBOARD = 0x10,     // This mesh is a billboard.
        MESH_FLAG_HAS_TRANSFORM = 0x20,
        MESH_FLAG_ACTIVE = 0x40,
        MESH_FLAG_NO_ANISOTROPIC = 0x80,        // No texture 0 anisotropic filtering for this mesh.
        MESH_FLAG_NO_ZWRITE = 0x100,    // No zwrite for this mesh.
        MESH_FLAG_SHADOW_VOLUME = 0x200,    // This mesh represents a single shadow volume.
        MESH_FLAG_BUMPED_WATER = 0x400,
        MESH_FLAG_UNLIT = 0x20000   // This corresponds to the material unlit flag during the scene conversion process.
    };
    public class Mesh
    {
        public System.Numerics.Vector3 center;
        public float radius;
        public System.Numerics.Vector3 inf;
        public System.Numerics.Vector3 sup;

        [JsonConverter(typeof(MeshFlagConverter))]
        public System.UInt32 flags;

        public System.UInt32 material_checksum;

        public ICollection<System.UInt16>[] indices;
        public static void WriteMesh(Mesh mesh, System.IO.BinaryWriter bw)
        {
            bw.Write(mesh.center.X);
            bw.Write(mesh.center.Y);
            bw.Write(mesh.center.Z);

            bw.Write(mesh.radius);

            bw.Write(mesh.inf.X);
            bw.Write(mesh.inf.Y);
            bw.Write(mesh.inf.Z);

            bw.Write(mesh.sup.X);
            bw.Write(mesh.sup.Y);
            bw.Write(mesh.sup.Z);


            bw.Write(mesh.flags);

            bw.Write(mesh.material_checksum);

            bw.Write((System.UInt32)mesh.indices.Length); 
            foreach(var indices in mesh.indices)
            {
                bw.Write((System.UInt32)indices.Count);
                foreach(var index in indices)
                {
                    bw.Write(index);
                }
            }
        }
        public static Mesh ReadMesh(System.IO.BinaryReader bs, System.UInt32 sector_flags)
        {
            var result = new Mesh();


            result.center = new System.Numerics.Vector3(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());
            result.radius = bs.ReadSingle();
            result.inf = new System.Numerics.Vector3(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());
            result.sup = new System.Numerics.Vector3(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());

            result.flags = bs.ReadUInt32();

            /*if ((sector_flags & (uint)0x400) != 0)
            {
                result.flags |= (uint)EMeshFlags.MESH_FLAG_NO_SKATER_SHADOW;
            }

            if ((sector_flags & (uint)EMeshFlags.MESH_FLAG_UNLIT) != 0)
            {
                result.flags |= (uint)EMeshFlags.MESH_FLAG_UNLIT;
            }*/

            result.material_checksum = bs.ReadUInt32();

            var num_lod_index_levels = bs.ReadUInt32();
            result.indices = new ICollection<System.UInt16>[num_lod_index_levels];
            if (num_lod_index_levels > 0)
            {
                for (int lod_level = 0; lod_level < num_lod_index_levels; ++lod_level)
                {
                    var num_indices = bs.ReadUInt32();

                    var indices = new List<System.UInt16>();
                    for (var i = 0; i < num_indices; i++)
                    {
                        indices.Add(bs.ReadUInt16());
                    }
                    result.indices[lod_level] = indices;
                }
            }

            /*if ((sector_flags & (uint)0x200000) != 0)
            {
                result.flags |= (uint)EMeshFlags.MESH_FLAG_SHADOW_VOLUME;
            }*/
            return result;
        }
    }
}
