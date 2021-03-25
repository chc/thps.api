using QScript;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using System.Text.Json;
using System.Text.Json.Serialization;


namespace QScript.External
{
    public class ScriptKeyResult
    {
        public System.Int32? checksum { get; set; }
        public String name { get; set; }
        public int? version { get; set; }
        public int? platform { get; set; }
        public int? compressedByteSize { get; set; }
    };
    public class HTTPChecksumResolver
    {
        public const string HTTPClientFactoryName = "THPSAPI";
        private string _httpClientFactoryName;
        private IHttpClientFactory _clientFactory;
        private List<ScriptKeyResult> compressedScriptKeys;
        private int platform;
        private int version;
        public HTTPChecksumResolver(IHttpClientFactory clientFactory, int platform, int version, string httpChecksumResolver = null)
        {
            _clientFactory = clientFactory;
            this.version = version;
            this.platform = platform;
            this.compressedScriptKeys = null;
            _httpClientFactoryName = httpChecksumResolver ?? HTTPClientFactoryName;
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

        public async Task<ScriptKeyResult> ResolveCompressedKey(long key, int compressedByteSize)
        {
            if(compressedScriptKeys == null) {
                using (var client = _clientFactory.CreateClient(HTTPClientFactoryName))
                {
                    var path = "/api/ScriptKey/GetCompressedTable/" + platform + "/" + version;
                    var result = await client.GetAsync(path);
                    result.EnsureSuccessStatusCode();
                    var resultString = await result.Content.ReadAsStringAsync();
                    compressedScriptKeys = JsonSerializer.Deserialize<List<ScriptKeyResult>>(resultString);
                }
            }

            return compressedScriptKeys.Where(g => g.checksum == key && g.compressedByteSize == compressedByteSize).FirstOrDefault();
        }
    }
}
