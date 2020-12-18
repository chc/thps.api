using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace QScript.Save.CAS
{
    public interface IFileHeaderProvider
    {
        void Read(BinaryReader br);
        void Write(BinaryWriter bw);
    }
}
