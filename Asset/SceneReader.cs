using QScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Asset
{
    public class SceneSerializer
    {
        IChecksumResolver _resolver;
        public SceneSerializer(IChecksumResolver resolver)
        {
            _resolver = resolver;
        }
        public async Task<MemoryStream> WriteScene(Scene scene)
        {
            using (var ms = new MemoryStream())
            {
                using (var bs = new BinaryWriter(ms))
                {
                    var retMs = new MemoryStream();

                    bs.Write((System.UInt32)scene.mat_version);
                    bs.Write((System.UInt32)scene.mesh_version);
                    bs.Write((System.UInt32)scene.vert_version);

                    bs.Write((System.UInt32)scene.materials.Count);
                    //write materials
                    foreach(var material in scene.materials)
                    {
                        Material.WriteMaterial(material, bs);
                    }


                    bs.Write((System.UInt32)scene.sectors.Count);
                    //write sectors
                    foreach(var sector in scene.sectors)
                    {

                    }

                    bs.Write((System.UInt32)scene.hierachy_objects.Count);
                    //write hierachy
                    foreach(var hierarchy_object in scene.hierachy_objects)
                    {
                        bs.Write(hierarchy_object.checksum);
                        bs.Write(hierarchy_object.parent_checksum);
                        bs.Write(hierarchy_object.parent_index);
                        bs.Write(hierarchy_object.bone_index);

                        //padding
                        bs.Write((Byte)0);
                        bs.Write((UInt32)0);

                        bs.Write(hierarchy_object.setup_matrix.M11);
                        bs.Write(hierarchy_object.setup_matrix.M12);
                        bs.Write(hierarchy_object.setup_matrix.M13);
                        bs.Write(hierarchy_object.setup_matrix.M14);

                        bs.Write(hierarchy_object.setup_matrix.M21);
                        bs.Write(hierarchy_object.setup_matrix.M22);
                        bs.Write(hierarchy_object.setup_matrix.M23);
                        bs.Write(hierarchy_object.setup_matrix.M24);

                        bs.Write(hierarchy_object.setup_matrix.M31);
                        bs.Write(hierarchy_object.setup_matrix.M32);
                        bs.Write(hierarchy_object.setup_matrix.M33);
                        bs.Write(hierarchy_object.setup_matrix.M34);

                        bs.Write(hierarchy_object.setup_matrix.M41);
                        bs.Write(hierarchy_object.setup_matrix.M42);
                        bs.Write(hierarchy_object.setup_matrix.M43);
                        bs.Write(hierarchy_object.setup_matrix.M44);
                    }


                    ms.Seek(0, SeekOrigin.Begin);
                    await ms.CopyToAsync(retMs);
                    retMs.Seek(0, SeekOrigin.Begin);
                    return retMs;
                }
            }

        }
        public Task<Scene> ReadBuffer(BinaryReader bs)
        {
            return Task.Run(() =>
            {
                int num_sectors;

                var result = new Scene();
                result.mat_version = bs.ReadInt32();
                result.mesh_version = bs.ReadInt32();
                result.vert_version = bs.ReadInt32();

                var materials = new List<Material>();

                int num_materials = bs.ReadInt32();
                for (var i = 0; i < num_materials; i++)
                {
                    materials.Add(Material.LoadMaterial(bs));
                }

                result.materials = materials;

                num_sectors = bs.ReadInt32();


                var sectors = new List<Sector>();
                for (var i = 0; i < num_sectors; i++)
                {
                    var sector = Sector.LoadSector(bs);
                    sectors.Add(sector);
                }
                result.sectors = sectors;

                var num_hierarchy_objects = bs.ReadUInt32();
                var hierachy_objects = new List<HierarchyObject>();
                if (num_hierarchy_objects > 0)
                {
                    for (var i = 0; i < num_hierarchy_objects; i++)
                    {
                        var heiarchy_object = new HierarchyObject();
                        heiarchy_object.checksum = bs.ReadUInt32();
                        heiarchy_object.parent_checksum = bs.ReadUInt32();
                        heiarchy_object.parent_index = bs.ReadInt16();
                        heiarchy_object.bone_index = bs.ReadByte();
                        bs.ReadByte();
                        bs.ReadUInt32();
                        heiarchy_object.setup_matrix = new System.Numerics.Matrix4x4(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());
                        hierachy_objects.Add(heiarchy_object);
                    }
                }

                result.hierachy_objects = hierachy_objects;
                return result;
            });
        }
    }
}
