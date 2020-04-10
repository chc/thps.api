using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using THPS.API.Repository;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace THPS.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScriptKeyController : Controller
    {
        //private IChecksumDBContext checksumDbContext;
        private IScriptKeyRepository scriptKeyRepository;
        public ScriptKeyController(IScriptKeyRepository scriptKeyRepository)
        {
            this.scriptKeyRepository = scriptKeyRepository;
        }
        // GET: api/<controller>
        [HttpGet("getByChecksum/{checksum}")]
        public async Task<ScriptKeyRecord> GetByChecksum(System.Int32 checksum)
        {
            ScriptKeyRecord lookup = new ScriptKeyRecord();
            lookup.checksum = checksum;
            var result = await scriptKeyRepository.GetRecord(lookup);
            return result;
        }

        [HttpGet("GetCompressedTable/{platform}/{version}")]
        public async Task<List<ScriptKeyRecord>> GetCompressedKey(QScript.GamePlatform platform, QScript.GameVersion version)
        {
            ScriptKeyRecord lookup = new ScriptKeyRecord();
            lookup.platform = platform;
            lookup.version = version;
            return await scriptKeyRepository.GetCompressTables(lookup);
        }
        [HttpPut]
        public Task<ScriptKeyRecord> Post([FromBody] String scriptKey)
        {
            ScriptKeyRecord lookup = new ScriptKeyRecord();
            lookup.name = scriptKey;
            return scriptKeyRepository.GetRecord(lookup);
        }
    }
}
