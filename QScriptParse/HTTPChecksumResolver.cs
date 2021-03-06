using QScript;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Linq;

namespace QScriptParse
{
    public class ScriptKeyResult
    {
        public System.Int32? checksum { get; set; }
        public String name { get; set; }
        public GameVersion? version { get; set; }
        public GamePlatform? platform { get; set; }
        public int? compressedByteSize { get; set; }
    };
    public class HTTPChecksumResolver : IChecksumResolver
    {
        public const string HTTPClientFactoryName = "THPSAPI";
        private IHttpClientFactory _clientFactory;
        private List<ScriptKeyRecord> compressedScriptKeys;
        private QScript.GamePlatform platform;
        private QScript.GameVersion version;
        public HTTPChecksumResolver(IHttpClientFactory clientFactory, QScript.GamePlatform platform, QScript.GameVersion version)
        {
            _clientFactory = clientFactory;
            this.version = version;
            this.platform = platform;
            this.compressedScriptKeys = null;
        }
        public async Task<uint> GenerateChecksum(string message)
        {
            using (var client = _clientFactory.CreateClient(HTTPClientFactoryName))
            {
                var path = "/api/ScriptKey";
                var jsonString = JsonSerializer.Serialize<string>(message);
                var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var result = await client.PutAsync(path, content);
                result.EnsureSuccessStatusCode();
                var resultString = await result.Content.ReadAsStringAsync();
                var record = JsonSerializer.Deserialize<ScriptKeyResult>(resultString);

                var checksum = record?.checksum;
                if (!checksum.HasValue)
                    throw new ArgumentException();

                return (uint)checksum.Value;
                
            }
        }
        public async Task<string> ResolveChecksum(uint checksum, int? compressedByteSize = null)
        {
            if(checksum == 0) {
                return null;
            }
            if(compressedByteSize.HasValue) {
                var compressedKey = await ResolveCompressedKey(checksum, compressedByteSize.Value);
                if(compressedKey != null) {
                    return compressedKey.name;
                }
            }
            using(var client = _clientFactory.CreateClient(HTTPClientFactoryName))
            {
                var path = "/api/ScriptKey/getByChecksum/" + ((System.Int32)checksum).ToString();
                var result = await client.GetAsync(path);
                result.EnsureSuccessStatusCode();
                var resultString = await result.Content.ReadAsStringAsync();
                var record = JsonSerializer.Deserialize<ScriptKeyResult>(resultString);
                return record?.name ?? throw new ArgumentException();
            }
        }

        public async Task<ScriptKeyRecord> ResolveCompressedKey(long key, int compressedByteSize)
        {
            if(compressedScriptKeys == null) {
                using (var client = _clientFactory.CreateClient(HTTPClientFactoryName))
                {
                    var path = "/api/ScriptKey/GetCompressedTable/" + platform + "/" + version;
                    var result = await client.GetAsync(path);
                    result.EnsureSuccessStatusCode();
                    var resultString = await result.Content.ReadAsStringAsync();
                    compressedScriptKeys = JsonSerializer.Deserialize<List<ScriptKeyRecord>>(resultString);
                }
            }

            return compressedScriptKeys.Where(g => g.checksum == key && g.compressedByteSize == compressedByteSize).FirstOrDefault();
        }

        public Task<uint> GenerateChecksum(byte[] message)
        {
            throw new NotImplementedException();
        }

        public Task<ScriptKeyRecord> GetCompressedKey(string key)
        {
            throw new NotImplementedException();
        }
    }
}
