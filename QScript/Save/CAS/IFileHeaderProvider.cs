using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QScript.Save.CAS
{
    public interface IFileHeaderProvider
    {
        void Read(BinaryReader br);
        void Write(BinaryWriter bw);
    }
}
