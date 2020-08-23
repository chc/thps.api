using System;
using System.Collections.Generic;

namespace Asset
{
    public class HierarchyObject
    {
        public System.UInt32 checksum { get; set; }          // Object checksum
        public System.UInt32 parent_checksum { get; set; }   // Checksum of parent, or 0 if root object
        public System.Int16 parent_index { get; set; }       // Index of parent in the hierarchy array (or -1 if root object)
        public System.Byte bone_index { get; set; }     // The index of the bone matrix used on this object
        public System.Numerics.Matrix4x4 setup_matrix { get; set; }
    }
    public class Scene
    {
        public int mat_version { get; set; }
        public int mesh_version { get; set; }
        public int vert_version { get; set; }
        public ICollection<Material> materials { get; set; }
        public ICollection<Sector> sectors { get; set; }
        public ICollection<HierarchyObject> hierachy_objects { get; set; }
    }
}
