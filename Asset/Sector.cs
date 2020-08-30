using Asset.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Asset
{
    public class SectorPerVertexData
    {
        public System.Numerics.Vector3 position { get; set; }
        public System.Numerics.Vector3? normal { get; set; }
        public System.UInt32? weight { get; set; }
        public System.Numerics.Vector4? bone_indices { get; set; }
        public System.Numerics.Vector2[] texture_uvs { get; set; }
        public System.UInt32? vertex_colour { get; set; }
        public System.Byte? vc_wibble_index { get; set; }
    }
    public class Sector
    {
        public System.UInt32 sector_checksum { get; set; }
        public System.Int32 bone_idx { get; set; }

        public System.UInt32 flags { get; set; }

        public System.Numerics.Matrix3x2 bbox { get; set; }

        public System.Numerics.Vector4 bsphere { get; set; }

        public System.UInt32 billboard_type { get; set; }
        public System.Numerics.Vector3? billboard_origin { get; set; }
        public System.Numerics.Vector3? billboard_pivot_pos { get; set; }
        public System.Numerics.Vector3? billboard_pivot_axis { get; set; }

        //[JsonIgnore]
        public ICollection<SectorPerVertexData> vertices { get; set; }

        public ICollection<Mesh> meshes { get; set; }
        private static uint CalculateVertexStride(Sector sector)
        {
            var flags = CalculateFlags(sector);
            uint stride = 0;
            stride += 4 * 3; //sizeof(float) * 3 (position)
            if ((flags & (uint)0x04) != 0)
            {
                stride += 4 * 3;
            }

            if ((flags & (uint)0x10) != 0)
            {
                stride += 4 * 5;
            }
            if ((flags & (int)0x02) != 0)
            {
                stride += 4;
            }
            if ((flags & (int)0x01) != 0)
            {
                var enumerator = sector.vertices.GetEnumerator();
                enumerator.MoveNext();
                var first_vertex = enumerator.Current;
                stride += (uint)first_vertex.texture_uvs.Length * 4 * 2;
            }
            if ((flags & (int)0x800) != 0)
            {
                stride++;
            }
            return stride;
        }
        private static uint CalculateFlags(Sector sector)
        {
            uint flags = 0;
            if(sector.billboard_origin.HasValue)
            {
                flags |= (uint)0x00800000UL;
            }

            if(sector.vertices.Count > 0)
            {
                var enumerator = sector.vertices.GetEnumerator();
                enumerator.MoveNext();
                var first_vertex = enumerator.Current;
                if (first_vertex.normal.HasValue)
                {
                    flags |= (uint)0x04;
                }
                if (first_vertex.bone_indices.HasValue)
                {
                    flags |= (uint)0x10;
                }
                if (first_vertex.texture_uvs != null && first_vertex.texture_uvs.Length > 0)
                {
                    flags |= (uint)0x01;
                }
                if (first_vertex.vertex_colour.HasValue)
                {
                    flags |= (uint)0x02;
                }
                if(first_vertex.vc_wibble_index.HasValue)
                {
                    flags |= (uint)0x800;
                }
            }
            
            return flags;
        }
        public static void WriteSector(Sector sector, System.IO.BinaryWriter bw)
        {
            uint flags = CalculateFlags(sector); ///XXX: calculate flags
            bw.Write(sector.sector_checksum);
            bw.Write(sector.bone_idx);
            bw.Write(flags);

            bw.Write((UInt32)sector.meshes.Count);

            bw.Write(sector.bbox.M11);
            bw.Write(sector.bbox.M12);
            bw.Write(sector.bbox.M21);
            bw.Write(sector.bbox.M22);
            bw.Write(sector.bbox.M31);
            bw.Write(sector.bbox.M32);

            bw.Write(sector.bsphere.X);
            bw.Write(sector.bsphere.Y);
            bw.Write(sector.bsphere.Z);
            bw.Write(sector.bsphere.W);

            if ((flags & (uint)0x00800000UL) != 0)
            {
                bw.Write(sector.billboard_type);

                bw.Write(sector.billboard_origin.Value.X);
                bw.Write(sector.billboard_origin.Value.Y);
                bw.Write(sector.billboard_origin.Value.Z);

                bw.Write(sector.billboard_pivot_pos.Value.X);
                bw.Write(sector.billboard_pivot_pos.Value.Y);
                bw.Write(sector.billboard_pivot_pos.Value.Z);

                bw.Write(sector.billboard_pivot_axis.Value.X);
                bw.Write(sector.billboard_pivot_axis.Value.Y);
                bw.Write(sector.billboard_pivot_axis.Value.Z);
            }

            bw.Write((System.UInt32)sector.vertices.Count);

            int num_tc_sets = 0;

            bw.Write((System.UInt32)CalculateVertexStride(sector)); //XXX: calculate stride
            foreach (var vertex in sector.vertices)
            {
                num_tc_sets = vertex.texture_uvs.Length;
                bw.Write(vertex.position.X);
                bw.Write(vertex.position.Y);
                bw.Write(vertex.position.Z);
            }

            if ((flags & (int)0x04) != 0)
            {
                foreach (var vertex in sector.vertices)
                {
                    bw.Write(vertex.normal.Value.X);
                    bw.Write(vertex.normal.Value.Y);
                    bw.Write(vertex.normal.Value.Z);
                }
            }

            if ((flags & (int)0x10) != 0)
            {
                foreach (var vertex in sector.vertices)
                {
                    bw.Write(vertex.weight.Value);
                }
                foreach (var vertex in sector.vertices)
                {
                    bw.Write((System.UInt16)vertex.bone_indices.Value.X);
                    bw.Write((System.UInt16)vertex.bone_indices.Value.Y);
                    bw.Write((System.UInt16)vertex.bone_indices.Value.Z);
                    bw.Write((System.UInt16)vertex.bone_indices.Value.W);
                }
            }
            if ((flags & (int)0x01) != 0)
            {
                bw.Write(num_tc_sets);
                foreach (var vertex in sector.vertices)
                {
                    foreach(var uvs in vertex.texture_uvs)
                    {
                        bw.Write(uvs.X);
                        bw.Write(uvs.Y);
                    }
                }
            }

            if ((flags & (int)0x02) != 0)
            {
                foreach (var vertex in sector.vertices)
                {
                    bw.Write(vertex.vertex_colour.Value);
                }
            }

            if ((flags & (int)0x800) != 0)
            {
                foreach (var vertex in sector.vertices)
                {
                    bw.Write(vertex.vc_wibble_index.Value);
                }
            }
            foreach(var mesh in sector.meshes)
            {
                Mesh.WriteMesh(mesh, bw);
            }
        }
        public static Sector LoadSector(System.IO.BinaryReader bs)
        {
            var result = new Sector();
            result.sector_checksum = bs.ReadUInt32();

            result.bone_idx = bs.ReadInt32();

            result.flags = bs.ReadUInt32();

            uint num_meshes = bs.ReadUInt32();

            result.bbox = new System.Numerics.Matrix3x2(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());

            result.bsphere = new System.Numerics.Vector4(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());

            if ((result.flags & (uint)0x00800000UL) != 0)
            {
                result.billboard_type = bs.ReadUInt32();
                result.billboard_origin = new System.Numerics.Vector3(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());
                result.billboard_pivot_pos = new System.Numerics.Vector3(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());
                result.billboard_pivot_axis = new System.Numerics.Vector3(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());
            } else
            {
                result.billboard_origin = null;
                result.billboard_pivot_pos = null;
                result.billboard_pivot_axis = null;
            }

            var num_verticies = bs.ReadInt32();
            //var vertex_stride = bs.ReadInt32();
            bs.ReadInt32(); //skip vertex stride

            var vertices = new SectorPerVertexData[num_verticies];
            for(var i=0;i<num_verticies;i++)
            {
                vertices[i] = new SectorPerVertexData();
            }

            for (int i=0;i<num_verticies;i++)
            {
                var vert = new System.Numerics.Vector3(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());
                vertices[i].position = vert;

            }

            if ((result.flags & (int)0x04) != 0)
            {
                for (int i = 0; i < num_verticies; i++)
                {
                    var vert = new System.Numerics.Vector3(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());
                    vertices[i].normal = vert;
                }
            }

            if ((result.flags & (int)0x10) != 0)
            {
                
                for (int i = 0; i < num_verticies; i++)
                {
                    vertices[i].weight = bs.ReadUInt32();
                }

                for (int i = 0; i < num_verticies; i++)
                {
                    vertices[i].bone_indices = new System.Numerics.Vector4(bs.ReadUInt16(), bs.ReadUInt16(), bs.ReadUInt16(), bs.ReadUInt16());
                }
            }


            if ((result.flags & (int)0x01) != 0)
            {
                int num_tc_sets = bs.ReadInt32();
                if (num_tc_sets > 0)
                {
                    for (var i = 0; i < num_tc_sets; i++)
                    {
                        for (var x = 0; x < num_verticies; x++)
                        {
                            if (i == 0)
                            {
                                vertices[x].texture_uvs = new System.Numerics.Vector2[num_tc_sets];
                            }
                            vertices[x].texture_uvs[i] = new System.Numerics.Vector2(bs.ReadSingle(), bs.ReadSingle());
                        }
                    }
                }
            }


            if ((result.flags & (int)0x02) != 0)
            {
                for (var i = 0; i < num_verticies; i++)
                {
                    vertices[i].vertex_colour = bs.ReadUInt32();
                }
            }

            if ((result.flags & (int)0x800) != 0)
            {
                for (var i = 0; i < num_verticies; i++)
                {
                    vertices[i].vc_wibble_index = bs.ReadByte();
                }
            }

            result.vertices = vertices;

            var meshes = new List<Mesh>();
            for(int i=0;i<num_meshes;i++)
            {
                var mesh = Mesh.ReadMesh(bs, result.flags);
                meshes.Add(mesh);
            }
            result.meshes = meshes; 

            return result;
        }
    }
}
