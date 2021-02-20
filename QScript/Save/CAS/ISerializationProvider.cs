using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace QScript.Save.CAS
{
    public class CASData {
        public List<SymbolEntry> summary {get; set;}
        public List<SymbolEntry> save_data {get; set;}
    }
    public interface ISerializationProvider
    {
        Task<CASData> DeserializeCAS(BinaryReader bs);
        Task<MemoryStream> SerializeCAS(CASData saveData);
    }
}
