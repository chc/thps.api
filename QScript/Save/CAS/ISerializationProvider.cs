using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace QScript.Save.CAS
{
    public interface ISerializationProvider
    {
        Task<Dictionary<string, object>> DeserializeCAS(BinaryReader bs);
        Task<MemoryStream> SerializeCAS(Dictionary<String, List<SymbolEntry>> saveData);
    }
}
