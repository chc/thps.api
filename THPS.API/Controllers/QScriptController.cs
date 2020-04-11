using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QScript;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using THPS.API.Repository;

namespace THPS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "SaveAccess")]
    public class QScriptController : Controller
    {
        private IScriptKeyRepository scriptKeyRepository;
        public QScriptController(IScriptKeyRepository scriptKeyRepository)
        {
            this.scriptKeyRepository = scriptKeyRepository;
        }
        [HttpPost("Deserialize/{platform}/{version}")]
        public async Task<List<TokenEntry>> PostDownloadCAS(GamePlatform platform, GameVersion version)
        {
            IChecksumResolver checksumResolver = new THPS.API.Utils.ChecksumResolver(scriptKeyRepository, platform, version);
            var formData = HttpContext.Request.Form;
            var files = HttpContext.Request.Form.Files;
            using (MemoryStream ms = new MemoryStream())
            {
                var file = files.First();
                using (BinaryReader bs = new BinaryReader(ms))
                {
                    var reader = new TokenBufferReader(checksumResolver, bs);
                    await file.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var results = await reader.ReadBuffer();
                    return results;
                }
            }
        }
    }
}
