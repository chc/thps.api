﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace QScript
{
    public class ScriptKeyRecord
    {
        public System.UInt32? checksum;
        public String name;
        public int? compressedByteSize;
    };
    public interface IChecksumResolver
    {
        Task<string> ResolveChecksum(System.UInt32 checksum, int? compressedByteSize = null);
        Task<QScript.ScriptKeyRecord> ResolveCompressedKey(System.Int64 key, int compressedByteSize);
        Task<QScript.ScriptKeyRecord> GetCompressedKey(string key);
        Task<System.UInt32> GenerateChecksum(string message);
        Task<System.UInt32> GenerateChecksum(byte[] message);
    }
}
