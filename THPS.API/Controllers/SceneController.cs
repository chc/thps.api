using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Asset;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QScript;
using THPS.API.Repository;

namespace THPS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "SceneAccess")]
    public class SceneController : Controller
    {
        private IScriptKeyRepository scriptKeyRepository;
        public SceneController(IScriptKeyRepository scriptKeyRepository)
        {
            this.scriptKeyRepository = scriptKeyRepository;
        }
        [HttpPost("Deserialize/{platform}/{version}")]
        public async Task<Scene> PostUploadScene(GamePlatform platform, GameVersion version)
        {
            if (platform != GamePlatform.PlatformType_PC || version != GameVersion.GameVersion_THUG)
            {
                throw new System.NotImplementedException();
            }
            IChecksumResolver checksumResolver = new THPS.API.Utils.ChecksumResolver(scriptKeyRepository, platform, version);
            var files = HttpContext.Request.Form.Files;
            using (MemoryStream ms = new MemoryStream())
            {
                var file = files.First();
                using (BinaryReader bs = new BinaryReader(ms))
                {
                    var reader = new SceneSerializer(checksumResolver);
                    await file.CopyToAsync(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var results = await reader.ReadBuffer(bs);
                    return results;
                }
            }
        }

        [HttpPost("Serialize/{platform}/{version}")]
        public async Task<FileStreamResult> PostDownloadScene(GamePlatform platform, GameVersion version, [FromBody] Scene sceneData)
        {
            if (platform != GamePlatform.PlatformType_PC || version != GameVersion.GameVersion_THUG)
            {
                throw new System.NotImplementedException();
            }
            IChecksumResolver checksumResolver = new THPS.API.Utils.ChecksumResolver(scriptKeyRepository, platform, version);
            var writer = new SceneSerializer(checksumResolver);
            var ms = await writer.WriteScene(sceneData);
            return new FileStreamResult(ms, "application/octet-stream")
            {
                FileDownloadName = "scene.mdl.xbx"
            };

        }
    }
}