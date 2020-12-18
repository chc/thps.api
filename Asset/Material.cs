using Asset.JsonConverters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Asset
{
    public enum EBlendModes
    {
        vBLEND_MODE_DIFFUSE,                                // ( 0 - 0 ) * 0 + Src
        vBLEND_MODE_ADD,                                    // ( Src - 0 ) * Src + Dst
        vBLEND_MODE_ADD_FIXED,                              // ( Src - 0 ) * Fixed + Dst
        vBLEND_MODE_SUBTRACT,                               // ( 0 - Src ) * Src + Dst
        vBLEND_MODE_SUB_FIXED,                              // ( 0 - Src ) * Fixed + Dst
        vBLEND_MODE_BLEND,                                  // ( Src * Dst ) * Src + Dst	
        vBLEND_MODE_BLEND_FIXED,                            // ( Src * Dst ) * Fixed + Dst	
        vBLEND_MODE_MODULATE,                               // ( Dst - 0 ) * Src + 0
        vBLEND_MODE_MODULATE_FIXED,                         // ( Dst - 0 ) * Fixed + 0	
        vBLEND_MODE_BRIGHTEN,                               // ( Dst - 0 ) * Src + Dst
        vBLEND_MODE_BRIGHTEN_FIXED,                         // ( Dst - 0 ) * Fixed + Dst	
        vBLEND_MODE_GLOSS_MAP,                              // Specular = Specular * Src	- special mode for gloss mapping
        vBLEND_MODE_BLEND_PREVIOUS_MASK,                    // ( Src - Dst ) * Dst + Dst
        vBLEND_MODE_BLEND_INVERSE_PREVIOUS_MASK,            // ( Dst - Src ) * Dst + Src

        vBLEND_MODE_MODULATE_COLOR = 15,    // ( Dst - 0 ) * Src(col) + 0	- special mode for the shadow.
        vBLEND_MODE_ONE_INV_SRC_ALPHA = 17, //								- special mode for imposter rendering.
    }

    public enum EMaterialFlag
    {
        MATFLAG_UV_WIBBLE = (1 << 0),
        MATFLAG_VC_WIBBLE = (1 << 1),
        MATFLAG_TEXTURED = (1 << 2),
        MATFLAG_ENVIRONMENT = (1 << 3),
        MATFLAG_DECAL = (1 << 4),
        MATFLAG_SMOOTH = (1 << 5),
        MATFLAG_TRANSPARENT = (1 << 6),
        MATFLAG_PASS_COLOR_LOCKED = (1 << 7),
        MATFLAG_SPECULAR = (1 << 8),        // Specular lighting is enabled on this material = (Pass0).
        MATFLAG_BUMP_SIGNED_TEXTURE = (1 << 9),     // This pass uses an offset texture which needs to be treated as signed data.
        MATFLAG_BUMP_LOAD_MATRIX = (1 << 10),       // This pass requires the bump mapping matrix elements to be set up.
        MATFLAG_PASS_TEXTURE_ANIMATES = (1 << 11),      // This pass has a texture which animates.
        MATFLAG_PASS_IGNORE_VERTEX_ALPHA = (1 << 12),       // This pass should not have the texel alpha modulated by the vertex alpha.
        MATFLAG_EXPLICIT_UV_WIBBLE = (1 << 14),     // Uses explicit uv wibble = (set via script) rather than calculated.
        MATFLAG_WATER_EFFECT = (1 << 27),       // This material should be processed to provide the water effect.
        MATFLAG_NO_MAT_COL_MOD = (1 << 28)		// No material color modulation required = (all passes have m.rgb = 0.5).
    }
    public class UVWibbleParams
    {
        public float UVel { get; set; }
        public float VVel { get; set; }
        public float UFrequency { get; set; }
        public float VFrequency { get; set; }
        public float UAmplitude { get; set; }
        public float VAmplitude { get; set; }
        public float UPhase { get; set; }
        public float VPhase { get; set; }
    };
    public class VCWibbleKeyframe
    {
        public System.Int32 time { get; set; }
        public System.Numerics.Vector4 colour { get; set; }
    }

    public class VCWibbleParams
    {
        public int phase { get; set; }
        public ICollection<VCWibbleKeyframe> keyframes { get; set; }
    };

    public class sTextureWibbleKeyframe
    {
        public System.UInt32 time { get; set; }
        public System.UInt32 texture { get; set; }
    };
    public class sTextureWibbleParams
    {
        public int num_keyframes { get; set; }
        public int period { get; set; }
        public int num_iterations { get; set; }
        public int phase { get; set; }
        
        public ICollection<sTextureWibbleKeyframe> keyframes { get; set; }
    };

    public class MaterialTexture
    {
        //MaterialFlagConverter
        public System.UInt32 Checksum { get; set; }
        [JsonConverter(typeof(MaterialFlagConverter))]
        public System.UInt32 flags { get; set; }
        public bool has_colour { get; set; }
        public System.Numerics.Vector3 colour { get; set; }
        [JsonConverter(typeof(RegAlphaConverter))]
        public System.UInt64 reg_alpha { get; set; }
        public System.UInt32 u_addressing { get; set; }
        public System.UInt32 v_addressing { get; set; }
        public System.Numerics.Vector2 envmap_tiling { get; set; }
        public System.UInt32 filtering_mode { get; set; }
        public UVWibbleParams uvWibbleParams { get; set; }
        public ICollection<VCWibbleParams> vcWibbleParams { get; set; }
        public sTextureWibbleParams textureWibbleParams { get; set; }
        public System.UInt32 MMAG { get; set; }
        public System.UInt32 MMIN { get; set; }
        public System.UInt32 K { get; set; }
        public System.UInt32 L { get; set; }
    }
    public class Material
    {
        public System.UInt32 checksum { get; set; }
        public System.UInt32 name_checksum { get; set; }
        //public System.UInt32 passes { get; set; }
        public System.UInt32 AlphaCutoff { get; set; }
        public bool sorted { get; set; }
        public float draw_order { get; set; }
        public bool single_sided { get; set; }
        public bool no_backface_culling { get; set; }
        public System.Int32 zbias { get; set; }
        public bool grassify { get; set; }
        public float grass_height { get; set; }
        public int grass_layers { get; set; }
        public System.Numerics.Vector4 specular_colour { get; set; }
        public ICollection<MaterialTexture> materialTextures { get; set; }
        public static void SerializeMaterial(Material material, System.IO.BinaryWriter bs)
        {
            bs.Write(material.checksum);
            bs.Write(material.name_checksum);

            System.UInt32 total_textures = 0;
            foreach(var item in material.materialTextures)
            {
                total_textures++;
            }
            bs.Write(total_textures);

            bs.Write(material.AlphaCutoff);
            bs.Write(material.sorted);
            bs.Write(material.draw_order);
            bs.Write(material.single_sided);
            bs.Write(material.no_backface_culling);
            bs.Write(material.zbias);
            bs.Write(material.grassify);
            if(material.grassify)
            {
                bs.Write(material.grass_height);
                bs.Write(material.grass_layers);
            }
            bs.Write(material.specular_colour.W);
            if(material.specular_colour.W > 0)
            {
                bs.Write(material.specular_colour.X);
                bs.Write(material.specular_colour.Y);
                bs.Write(material.specular_colour.Z);
            }
            int texture_index = 0;
            foreach (var texture in material.materialTextures)
            {
                bs.Write(texture.Checksum);
                bs.Write(texture.flags); //XXX: calculate flags
                

                bs.Write(texture.has_colour);
                bs.Write(texture.colour.X);
                bs.Write(texture.colour.Y);
                bs.Write(texture.colour.Z);

                bs.Write(texture.reg_alpha);

                bs.Write(texture.u_addressing);
                bs.Write(texture.v_addressing);

                bs.Write(texture.envmap_tiling.X);
                bs.Write(texture.envmap_tiling.Y);
                bs.Write(texture.filtering_mode);

                if (texture.uvWibbleParams != null)
                {
                    bs.Write(texture.uvWibbleParams.UVel);
                    bs.Write(texture.uvWibbleParams.VVel);
                    bs.Write(texture.uvWibbleParams.UFrequency);
                    bs.Write(texture.uvWibbleParams.VFrequency);
                    bs.Write(texture.uvWibbleParams.UAmplitude);
                    bs.Write(texture.uvWibbleParams.VAmplitude);
                    bs.Write(texture.uvWibbleParams.UPhase);
                    bs.Write(texture.uvWibbleParams.VPhase);
                }

                if (texture_index == 0 && texture.vcWibbleParams != null)
                {
                    bs.Write(texture.vcWibbleParams.Count);
                    foreach(var item in texture.vcWibbleParams)
                    {
                        bs.Write(item.keyframes.Count);
                        bs.Write(item.phase);
                        foreach(var keyframe in item.keyframes)
                        {
                            bs.Write(keyframe.time);
                            bs.Write(keyframe.colour.X);
                            bs.Write(keyframe.colour.Y);
                            bs.Write(keyframe.colour.Z);
                            bs.Write(keyframe.colour.W);
                        }
                    }
                }

                if (texture.textureWibbleParams != null)
                {
                    bs.Write(texture.textureWibbleParams.num_keyframes);
                    bs.Write(texture.textureWibbleParams.period);
                    bs.Write(texture.textureWibbleParams.num_iterations);
                    bs.Write(texture.textureWibbleParams.phase);
                    foreach (var item in texture.textureWibbleParams.keyframes)
                    {
                        bs.Write(item.time);
                        bs.Write(item.texture);
                    }

                }

                bs.Write(texture.MMAG);
                bs.Write(texture.MMIN);
                bs.Write(texture.K);
                bs.Write(texture.L);


                texture_index++;
            }
        }
        public static void WriteMaterial(Material material, System.IO.BinaryWriter bw)
        {
            bw.Write(material.checksum);
            bw.Write(material.name_checksum);
            bw.Write(material.materialTextures.Count);

            bw.Write(material.AlphaCutoff);

            bw.Write(material.sorted);
            bw.Write(material.draw_order);
            bw.Write(material.single_sided);
            bw.Write(material.no_backface_culling);
            bw.Write(material.zbias);

            bw.Write(material.grassify);
            if(material.grassify)
            {
                bw.Write(material.grass_height);
                bw.Write(material.grass_layers);
            }

            bw.Write(material.specular_colour.W);
            if (material.specular_colour.W > 0)
            {
                bw.Write(material.specular_colour.X);
                bw.Write(material.specular_colour.Y);
                bw.Write(material.specular_colour.Z);
            }

            bool first = true;
            foreach (var materialTexture in material.materialTextures)
            {

                bw.Write(materialTexture.Checksum);
                bw.Write(materialTexture.flags);
                bw.Write(materialTexture.has_colour);
                bw.Write(materialTexture.colour.X);
                bw.Write(materialTexture.colour.Y);
                bw.Write(materialTexture.colour.Z);
                bw.Write(materialTexture.reg_alpha);
                bw.Write(materialTexture.u_addressing);
                bw.Write(materialTexture.v_addressing);

                bw.Write(materialTexture.envmap_tiling.X);
                bw.Write(materialTexture.envmap_tiling.Y);

                bw.Write(materialTexture.filtering_mode);

                if ((materialTexture.flags & (uint)EMaterialFlag.MATFLAG_UV_WIBBLE) != 0)
                {
                    bw.Write(materialTexture.uvWibbleParams.UVel);
                    bw.Write(materialTexture.uvWibbleParams.VVel);
                    bw.Write(materialTexture.uvWibbleParams.UFrequency);
                    bw.Write(materialTexture.uvWibbleParams.VFrequency);
                    bw.Write(materialTexture.uvWibbleParams.UAmplitude);
                    bw.Write(materialTexture.uvWibbleParams.VAmplitude);
                    bw.Write(materialTexture.uvWibbleParams.UPhase);
                    bw.Write(materialTexture.uvWibbleParams.VPhase);
                }

                if (first && (materialTexture.flags & (uint)EMaterialFlag.MATFLAG_VC_WIBBLE) != 0)
                {
                    bw.Write((System.UInt32)materialTexture.vcWibbleParams.Count);
                    foreach(var item in materialTexture.vcWibbleParams)
                    {
                        bw.Write((System.UInt32)item.keyframes.Count);
                        bw.Write(item.phase);
                        foreach(var frame in item.keyframes)
                        {
                            bw.Write((System.UInt32)frame.time);
                            bw.Write((Byte)frame.colour.X);
                            bw.Write((Byte)frame.colour.Y);
                            bw.Write((Byte)frame.colour.Z);
                            bw.Write((Byte)frame.colour.W);
                        }
                    }
                }

                if ((materialTexture.flags & (uint)EMaterialFlag.MATFLAG_PASS_TEXTURE_ANIMATES) != 0)
                {
                    bw.Write((System.Int32)materialTexture.textureWibbleParams.keyframes.Count);
                    bw.Write(materialTexture.textureWibbleParams.period);
                    bw.Write(materialTexture.textureWibbleParams.num_iterations);
                    bw.Write(materialTexture.textureWibbleParams.phase);
                    foreach(var frame in materialTexture.textureWibbleParams.keyframes)
                    {
                        bw.Write(frame.time);
                        bw.Write(frame.texture);
                    }
                }

                bw.Write(materialTexture.MMAG);
                bw.Write(materialTexture.MMIN);
                bw.Write(materialTexture.K);
                bw.Write(materialTexture.L);
                first = false;
            }

        }
         public static Material LoadMaterial(System.IO.BinaryReader bs)
        {
            var result = new Material();

            bool neutral_material_color = true;

            result.checksum = bs.ReadUInt32();
            result.name_checksum = bs.ReadUInt32();
            var passes = bs.ReadUInt32();

            result.AlphaCutoff = bs.ReadUInt32();

            result.sorted = bs.ReadBoolean();
            result.draw_order = bs.ReadSingle();
            result.single_sided = bs.ReadBoolean();
            result.no_backface_culling = bs.ReadBoolean();

            result.zbias = bs.ReadInt32();

            result.grassify = bs.ReadBoolean();
            if(result.grassify)
            {
                result.grass_height = bs.ReadSingle();
                result.grass_layers = bs.ReadInt32();
            }

            float w = bs.ReadSingle();
            if(w > 0)
            {
                result.specular_colour = new System.Numerics.Vector4(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle(), w);
            } else
            {
                result.specular_colour = new System.Numerics.Vector4(0f, 0f, 0f, w);
            }

            var materials = new List<MaterialTexture>();
            for(var i=0;i<passes;i++)
            {
                System.UInt32 TextureChecksum = 0;
                var texture = new MaterialTexture();
                TextureChecksum = bs.ReadUInt32();
                texture.flags = bs.ReadUInt32();
                texture.has_colour = bs.ReadBoolean();
                texture.colour = new System.Numerics.Vector3(bs.ReadSingle(), bs.ReadSingle(), bs.ReadSingle());
                texture.reg_alpha = bs.ReadUInt64();

                if(neutral_material_color)
                {
                    if(texture.colour.X != 0.5 && texture.colour.Y != 0.5 && texture.colour.Z != 0.5)
                    {
                        neutral_material_color = false;
                    }
                }


                texture.u_addressing = bs.ReadUInt32();
                texture.v_addressing = bs.ReadUInt32();

                texture.envmap_tiling = new System.Numerics.Vector2(bs.ReadSingle(), bs.ReadSingle());
                texture.filtering_mode = bs.ReadUInt32();

                //read uv wibble, vc wibble, colour wibble...
                if ((texture.flags & (uint)EMaterialFlag.MATFLAG_UV_WIBBLE) != 0)
                {
                    texture.uvWibbleParams = new UVWibbleParams();

                    texture.uvWibbleParams.UVel = bs.ReadSingle();
                    texture.uvWibbleParams.VVel = bs.ReadSingle();
                    texture.uvWibbleParams.UFrequency = bs.ReadSingle();
                    texture.uvWibbleParams.VFrequency = bs.ReadSingle();
                    texture.uvWibbleParams.UAmplitude = bs.ReadSingle();
                    texture.uvWibbleParams.VAmplitude = bs.ReadSingle();
                    texture.uvWibbleParams.UPhase = bs.ReadSingle();
                    texture.uvWibbleParams.VPhase = bs.ReadSingle();
                }

                if (i == 0 && (texture.flags & (uint)EMaterialFlag.MATFLAG_VC_WIBBLE) != 0)
                {
                    texture.vcWibbleParams = new List<VCWibbleParams>();
                    System.UInt32 num_sequences = bs.ReadUInt32();
                    for(var x=0;x<num_sequences;x++)
                    {
                        var vcWibbleParam = new VCWibbleParams();
                        System.UInt32 num_keys = bs.ReadUInt32();
                        System.Int32 phase = bs.ReadInt32();
                        vcWibbleParam.phase = phase;

                        List<VCWibbleKeyframe> keyframes = new List<VCWibbleKeyframe>();
                        for(var j=0;j<num_keys;j++)
                        {
                            VCWibbleKeyframe frame = new VCWibbleKeyframe();
                            frame.time = bs.ReadInt32();
                            frame.colour = new System.Numerics.Vector4(bs.ReadByte(), bs.ReadByte(), bs.ReadByte(), bs.ReadByte());
                            keyframes.Add(frame);
                        }
                        vcWibbleParam.keyframes = keyframes;
                        texture.vcWibbleParams.Add(vcWibbleParam);
                    }
                }

                if ((texture.flags & (uint)EMaterialFlag.MATFLAG_PASS_TEXTURE_ANIMATES) != 0)
                {
                    texture.textureWibbleParams = new sTextureWibbleParams();
                    texture.textureWibbleParams.num_keyframes = bs.ReadInt32();
                    texture.textureWibbleParams.period = bs.ReadInt32();
                    texture.textureWibbleParams.num_iterations= bs.ReadInt32();
                    texture.textureWibbleParams.phase = bs.ReadInt32();

                    var keyframes = new List<sTextureWibbleKeyframe>();
                    for(var x=0;x< texture.textureWibbleParams.num_keyframes;++x)
                    {
                        var frame = new sTextureWibbleKeyframe();
                        frame.time = bs.ReadUInt32();
                        frame.texture = bs.ReadUInt32();

                        if(x == 0)
                        {
                            TextureChecksum = frame.texture;
                        }
                        keyframes.Add(frame);
                    }

                    texture.textureWibbleParams.keyframes = keyframes;
                }



                texture.MMAG = bs.ReadUInt32();
                texture.MMIN = bs.ReadUInt32();
                texture.K = bs.ReadUInt32();
                texture.L = bs.ReadUInt32();

                texture.Checksum = TextureChecksum;
                materials.Add(texture);
            }

            /*if (neutral_material_color && materials.Count > 0)
            {
                materials[0].flags |= (uint)EMaterialFlag.MATFLAG_NO_MAT_COL_MOD;
            }
            else if (neutral_material_color)
            {
                System.Diagnostics.Debugger.Break();
            }

            if (result.specular_colour.W > 0.0f && materials.Count > 0)
            {
                materials[0].flags |= (uint)EMaterialFlag.MATFLAG_SPECULAR;
            } else if(result.specular_colour.W > 0.0f)
            {
                System.Diagnostics.Debugger.Break();
            }*/

            result.materialTextures = materials;
            
            
            return result;
        }
    }
}
