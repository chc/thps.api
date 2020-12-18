using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QScript;

namespace THPS.API.Repository
{
    public class ScriptKeyRecord
    {
        public System.Int32? checksum;
        public String name;
        public GameVersion? version;
        public GamePlatform? platform;
        public int? compressedByteSize;
    };
    public class SaveFileTypeRecord
    {
        public String name;
        public System.UInt32 fileVersion;
        public System.UInt32 fixedFileSize;
        public GameVersion? version;
        public GamePlatform? platform;
    }
    public interface IScriptKeyRepository
    {
        Task<ScriptKeyRecord> GetRecord(ScriptKeyRecord record);
        Task<List<ScriptKeyRecord>> GetCompressTables(ScriptKeyRecord lookup);
        Task<SaveFileTypeRecord> SaveFileInfo(SaveFileTypeRecord lookup);
        Task<SaveFileTypeRecord> GetFileInfo(string name, GameVersion version, GamePlatform platform);
    }
}
