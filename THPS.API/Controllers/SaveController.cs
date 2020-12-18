using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QScript;
using THPS.API.Repository;

namespace THPS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "SaveAccess")]
    public class SaveController : Controller
    {
        private IScriptKeyRepository scriptKeyRepository;
        public SaveController(IScriptKeyRepository scriptKeyRepository)
        {
            this.scriptKeyRepository = scriptKeyRepository;
        }
        [HttpPost("Deserialize/{platform}/{version}")]
        public async Task<Dictionary<string, object>> PostDownloadCAS(GamePlatform platform, GameVersion version)
        {
            IChecksumResolver checksumResolver = new THPS.API.Utils.ChecksumResolver(scriptKeyRepository, platform, version);
            QScript.Save.CAS.ISerializationProvider deserializer = new QScript.Save.CAS.Games.THPS4Common_SerializationProvider(checksumResolver, 1, 1);
            var formData = HttpContext.Request.Form;
            var files = HttpContext.Request.Form.Files;
            using (MemoryStream ms = new MemoryStream())
            {
                var file = files.First();
                using (BinaryReader bs = new BinaryReader(ms))
                {
                    await file.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var results = await deserializer.DeserializeCAS(bs);
                    results.Remove("headerInfo");
                    return results;
                }
            }
        }
        [HttpPost("Serialize/{platform}/{version}/{friendlyName}")]
        public async Task<FileStreamResult> PostCreateCAS(GamePlatform platform, GameVersion version, string friendlyName, [FromBody] Dictionary<string, List<SymbolEntry>> input)
        {
            SaveFileTypeRecord record = await scriptKeyRepository.GetFileInfo(friendlyName, version, platform);
            if (record == null) throw new NotImplementedException();
            //GetFileInfo(string name, GameVersion version, GamePlatform platform);
            IChecksumResolver checksumResolver = new THPS.API.Utils.ChecksumResolver(scriptKeyRepository, platform, version);
            
            QScript.Save.CAS.ISerializationProvider serializer = new QScript.Save.CAS.Games.THPS4Common_SerializationProvider(checksumResolver, (int)record.fileVersion, record.fixedFileSize);
            var ms = await serializer.SerializeCAS(input);
            ms.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(ms, "application/octet-stream")
            {
                FileDownloadName = "save." + friendlyName
            };
        }
        [Authorize(Policy = "Admin")]
        [HttpPost("RegisterFile/{platform}/{version}/{friendlyName}")]
        public async Task<SaveFileTypeRecord> RegisterFile(GamePlatform platform, GameVersion version, string friendlyName)
        {
            IChecksumResolver checksumResolver = new THPS.API.Utils.ChecksumResolver(scriptKeyRepository, platform, version);
            QScript.Save.CAS.ISerializationProvider deserializer = new QScript.Save.CAS.Games.THPS4Common_SerializationProvider(checksumResolver, 0, 0);
            var formData = HttpContext.Request.Form;
            var files = HttpContext.Request.Form.Files;
            using (MemoryStream ms = new MemoryStream())
            {
                var file = files.First();
                using (BinaryReader bs = new BinaryReader(ms))
                {
                    await file.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var results = await deserializer.DeserializeCAS(bs);
                    var record = new SaveFileTypeRecord();
                    var headerInfo = (Dictionary<String,object>)results["headerInfo"];
                    record.fileVersion = System.UInt32.Parse(headerInfo["version"].ToString());
                    record.fixedFileSize = System.UInt32.Parse(headerInfo["fixedFileSize"].ToString());
                    record.name = friendlyName;
                    record.platform = platform;
                    record.version = version;
                    var dbRecord = await scriptKeyRepository.GetFileInfo(friendlyName, version, platform);
                    if (dbRecord != null) return null;
                    record = await this.scriptKeyRepository.SaveFileInfo(record);
                    return record;
                }
            }
        }

    }
}