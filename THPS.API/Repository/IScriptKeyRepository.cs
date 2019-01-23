using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QScript;

namespace THPS.API.Repository
{
    public class ScriptKeyRecord
    {
        public System.UInt32? checksum;
        public String name;
        public GameVersion? version;
        public GamePlatform? platform;
        public int? compressedByteSize;
    };
    public interface IScriptKeyRepository
    {
        Task<ScriptKeyRecord> GetRecord(ScriptKeyRecord record);
        Task<List<ScriptKeyRecord>> GetCompressTables(ScriptKeyRecord lookup);
    }
}
