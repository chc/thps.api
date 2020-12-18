using System;
using System.Collections.Generic;
using System.Text;

namespace QScript.Save
{
    public class SaveFileFormatInfo
    {
        public long fixedFileSize;
        public int fileVersion;
        public GamePlatform platform;
        public GameVersion version;
        public String extension;
        public String friendlyName;
    }
}
