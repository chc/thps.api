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
        [HttpPost("DeserializeCAS/{platform}/{version}")]
        public async Task<Dictionary<string, object>> PostDownloadCAS(GamePlatform platform, GameVersion version)
        {
            IChecksumResolver checksumResolver = new THPS.API.Utils.ChecksumResolver(scriptKeyRepository, platform, version); ;
            QScript.Save.CAS.ISerializationProvider deserializer = new QScript.Save.CAS.Games.THUG2PC_SerializationProvider(checksumResolver);
            var formData = HttpContext.Request.Form;
            var files = HttpContext.Request.Form.Files;
            using (MemoryStream ms = new MemoryStream())
            {
                var file = files.First();
                using (BinaryReader bs = new BinaryReader(ms))
                {
                    await file.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    return await deserializer.DeserializeCAS(bs);
                }
            }
        }
        [HttpPost("SerializeCAS/{platform}/{version}")]
        public async Task<FileStreamResult> PostCreateCAS(GamePlatform platform, GameVersion version, [FromBody] Dictionary<string, List<SymbolEntry>> input)
        {
            IChecksumResolver checksumResolver = new THPS.API.Utils.ChecksumResolver(scriptKeyRepository, platform, version); ;
            QScript.Save.CAS.ISerializationProvider serializer = new QScript.Save.CAS.Games.THUG2PC_SerializationProvider(checksumResolver);
            var ms = await serializer.SerializeCAS(input);
            ms.Seek(0, SeekOrigin.Begin);
            return new FileStreamResult(ms, "application/octet-stream")
            {
                FileDownloadName = "skater.SKA"
            };
        }
                
    }
}