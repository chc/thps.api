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
        public System.Numerics.Vector3 normal { get; set; }
        public System.UInt32 weight { get; set; }
        public System.Numerics.Vector4 bone_indices { get; set; }
        public System.Numerics.Vector2[] texture_uvs { get; set; }
        public System.UInt32 vertex_colour { get; set; }
        public System.Byte vc_wibble_index { get; set; }
    }
    public class Sector
    {
        public System.UInt32 sector_checksum;
        public System.Int32 bone_idx;

        public System.Int32 flags;

        public System.Numerics.Matrix3x2 bbox;

        public System.Numerics.Vector4 bsphere;

        public System.UInt32 billboard_type;
        public System.Numerics.Vector3 billboard_origin;
        public System.Numerics.Vector3 billboard_pivot_pos;
        public System.Numerics.Vector3 billboard_pivot_axis;

        //[JsonIgnore]
        public ICollection<SectorPerVertexData> vertices { get; set; }

        public ICollection<Mesh> meshes;
        public static void WriteSector(Sector sector, System.IO.BinaryWriter bw)
        {
            int flags = 0; ///XXX: calculate flags
            bw.Write(sector.sector_checksum);
            bw.Write(sector.bone_idx);
            bw.Write(sector.flags);

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

            if ((flags & (int)0x00800000UL) != 0)
            {
                bw.Write(sector.billboard_type);

                bw.Write(sector.billboard_origin.X);
                bw.Write(sector.billboard_origin.Y);
                bw.Write(sector.billboard_origin.Z);

                bw.Write(sector.billboard_pivot_pos.X);
                bw.Write(sector.billboard_pivot_pos.Y);
                bw.Write(sector.billboard_pivot_pos.Z);

                bw.Write(sector.billboard_pivot_axis.X);
                bw.Write(sector.billboard_pivot_axis.Y);
                bw.Write(sector.billboard_pivot_axis.Z);
            }

            bw.Write(sector.vertices.Count);

            int num_tc_sets = 0;

            bw.Write((System.UInt32)0); //XXX: calculate stride
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
                    bw.Write(vertex.normal.X);
                    bw.Write(vertex.normal.Y);
                    bw.Write(vertex.normal.Z);
                }
            }

            if ((flags & (int)0x10) != 0)
            {
                foreach (var vertex in sector.vertices)
                {
                    bw.Write(vertex.weight);
                }
                foreach (var vertex in sector.vertices)
                {
                    bw.Write(vertex.bone_indices.X);
                    bw.Write(vertex.bone_indices.Y);
                    bw.Write(vertex.bone_indices.Z);
                    bw.Write(vertex.bone_indices.W);
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
                    bw.Write(vertex.vertex_colour);
                }
            }

            if ((flags & (int)0x800) != 0)
            {
                foreach (var vertex in sector.vertices)
                {
                    bw.Write(vertex.vc_wibble_index);
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

            result.flags = bs.ReadInt32();

            uint num_meshes = bs.ReadUInt32();

            result.bbox = new System.Numerics.Matrix3x2(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());

            result.bsphere = new System.Numerics.Vector4(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());

            if ((result.flags & (int)0x00800000UL) != 0)
            {
                result.billboard_type = bs.ReadUInt32();
                result.billboard_origin = new System.Numerics.Vector3(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());
                result.billboard_pivot_pos = new System.Numerics.Vector3(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());
                result.billboard_pivot_axis = new System.Numerics.Vector3(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());
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
