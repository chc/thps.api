using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace QScript.Save.CAS.Games
{
    public class THUG2PC_SerializationProvider : THPS4PC_SerializationProvider
    {
        public THUG2PC_SerializationProvider(IChecksumResolver checksumResolver) : base(checksumResolver, 37, 90112)
        {

        }
    }
}
