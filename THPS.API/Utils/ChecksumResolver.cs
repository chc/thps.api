using QScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using THPS.API.Repository;

namespace THPS.API.Utils
{
    public class ChecksumResolver : IChecksumResolver
    {
        private Crc32 crc32;
        private IScriptKeyRepository scriptKeyRepository;
        private GamePlatform platform;
        private GameVersion version;
        List<Repository.ScriptKeyRecord> compressedKeys;
        public ChecksumResolver(IScriptKeyRepository scriptKeyRepository, GamePlatform platform, GameVersion version)
        {
            this.version = version;
            this.platform = platform;
            this.scriptKeyRepository = scriptKeyRepository;
            crc32 = new Crc32();
        }
        public Task<System.UInt32> GenerateChecksum(string message)
        {
            return Task.Run(() =>
            {
                return crc32.Get(Encoding.ASCII.GetBytes(message.ToLower()));
            });
        }

        public Task<System.UInt32> GenerateChecksum(byte[] message)
        {
            return Task.Run(() =>
            {
                return crc32.Get(message, true);
            });
        }
        private async Task<List<QScript.ScriptKeyRecord>> GetCompressedKeys()
        {
            var ret = new List<QScript.ScriptKeyRecord>();
            var record = new Repository.ScriptKeyRecord();
            record.platform = platform;
            record.version = version;
            var entries = await scriptKeyRepository.GetCompressTables(record);

            foreach(var entry in entries)
            {
                var qrec = new QScript.ScriptKeyRecord();
                qrec.checksum = (System.UInt32)entry.checksum;
                qrec.compressedByteSize = entry.compressedByteSize;
                qrec.name = entry.name;
                ret.Add(qrec);
            }
            return ret;
        }

        public async Task<string> ResolveChecksum(System.UInt32 checksum, int? compressedByteSize = null)
        {
            if(compressedByteSize.HasValue)
            {
                var rec = await ResolveCompressedKey(checksum, compressedByteSize.Value);
                if(rec != null && rec.name != null)
                {
                    return rec.name;
                }
            } else
            {
                var record = new Repository.ScriptKeyRecord();
                record.checksum = (System.Int32)checksum;
                record = await scriptKeyRepository.GetRecord(record);
                return record?.name;
            }
            return null;
        }

        public async Task<QScript.ScriptKeyRecord> ResolveCompressedKey(System.Int64 key, int compressedByteSize)
        {
            await LoadCompressedKeys();
            var record = compressedKeys.Where(s => s.checksum == key && s.compressedByteSize == compressedByteSize).FirstOrDefault();
            if(record != null)
            {
                var qrec = new QScript.ScriptKeyRecord();
                qrec.name = record.name.ToLower();
                qrec.checksum = (System.UInt32)record.checksum;
                qrec.compressedByteSize = record.compressedByteSize;
                return qrec;
            }
            return null;
        }
        public async Task<QScript.ScriptKeyRecord> GetCompressedKey(string keyName)
        {
            await LoadCompressedKeys();
            var record = compressedKeys.Where(s => s.name.ToLower() == keyName.ToLower()).FirstOrDefault();
            if(record != null)
            {
                var qrec = new QScript.ScriptKeyRecord();
                qrec.name = keyName.ToLower();
                qrec.checksum = (System.UInt32)record.checksum;
                qrec.compressedByteSize = record.compressedByteSize;
                return qrec;
            }
            return null;
        }
        private async Task LoadCompressedKeys()
        {
            if (compressedKeys == null)
            {
                var lookup = new Repository.ScriptKeyRecord();
                lookup.platform = platform;
                lookup.version = version;
                compressedKeys = await scriptKeyRepository.GetCompressTables(lookup);
            }
        }
    }
}
