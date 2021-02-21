using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QScript.Save.CAS.Games
{
    public class THPS4Common_SerializationProvider : ISerializationProvider
    {
        private class FileHeader
        {
            public System.UInt32 checksum;
            public System.UInt32 summaryInfoChecksum;
            public System.Int32 summaryInfoSize;
            public System.Int32 dataSize;
            public System.Int32 version;
            public int SIZE_IN_BYTES = 20;

            public void Read(BinaryReader bs)
            {
                checksum = bs.ReadUInt32();
                summaryInfoChecksum = bs.ReadUInt32();
                summaryInfoSize = bs.ReadInt32();
                dataSize = bs.ReadInt32();
                version = bs.ReadInt32();
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(checksum);
                bw.Write(summaryInfoChecksum);
                bw.Write(summaryInfoSize);
                bw.Write(dataSize);
                bw.Write(version);
            }
        }
        private IChecksumResolver checksumResolver;
        private System.Int32 version;
        private long fixedFileSize;
        public THPS4Common_SerializationProvider(IChecksumResolver checksumResolver, System.Int32 version, long fixedFileSize)
        {
            this.checksumResolver = checksumResolver;
            this.version = version;
            this.fixedFileSize = fixedFileSize;
        }
        public async Task<List<SymbolEntry>> ResolveChecksums(List<SymbolEntry> input)
        {
            ScriptKeyRecord record = null;
            System.UInt32 checksum;
            foreach (var item in input)
            {
                if (System.UInt32.TryParse(item.name.ToString(), out checksum))
                {
                    String name = await checksumResolver.ResolveChecksum(checksum, item.compressedByteSize);
                    
                    if (name != null)
                    {
                        item.compressedByteSize = null;
                        item.name = name;
                    }
                    else
                    {
                        item.name = checksum;
                    }
                }
                switch (item.type)
                {
                    case QScript.ESymbolType.ESYMBOLTYPE_NAME:
                        if (System.UInt32.TryParse(item.value.ToString(), out checksum))
                        {
                            var name = await checksumResolver.ResolveChecksum(checksum);
                            if (name != null)
                            {
                                item.value = name;
                            }
                            else
                            {
                                item.value = checksum;
                            }
                        }
                        break;
                    case QScript.ESymbolType.ESYMBOLTYPE_STRUCTURE:
                        item.value = await ResolveChecksums((List<SymbolEntry>)item.value);
                        break;
                    case QScript.ESymbolType.ESYMBOLTYPE_ARRAY:
                        var array = (List<object>)item.value;
                        for (int i = 0; i < ((List<object>)item.value).Count; i++)
                        {
                            switch (item.subType)
                            {
                                case QScript.ESymbolType.ESYMBOLTYPE_NAME:
                                    record = new ScriptKeyRecord();
                                    checksum = (System.UInt32)((List<object>)item.value)[i];
                                    var name = await checksumResolver.ResolveChecksum(checksum);
                                    if (name != null)
                                    {
                                        ((List<object>)item.value)[i] = name;
                                    }
                                    else
                                    {
                                        ((List<object>)item.value)[i] = checksum;
                                    }
                                    break;
                                case QScript.ESymbolType.ESYMBOLTYPE_STRUCTURE:
                                    ((List<object>)item.value)[i] = await ResolveChecksums((List<SymbolEntry>)((List<object>)item.value)[i]);
                                    break;

                            }
                        }
                        break;
                }
            }
            return input;

        }
        private bool ValidateChecksums(FileHeader header, BinaryReader bs)
        {
            var crc = new Crc32();
            var position = bs.BaseStream.Position;

            var summary_bytes = bs.ReadBytes(header.summaryInfoSize);
            if (crc.Get(summary_bytes, true) != header.summaryInfoChecksum)
            {
                return false;
            }

            bs.BaseStream.Seek(4, SeekOrigin.Begin); //skip checksum (4 = sizeof uint32)

            System.UInt32 initialCrc = 3736805603; //initial CRC accumulator for null checksum (4 null bytes)

            var save_bytes = bs.ReadBytes(header.dataSize - 4);
            var checksum = crc.Get(save_bytes, true, initialCrc);
            if (checksum != header.checksum)
            {
                return false;
            }

            bs.BaseStream.Seek(position, SeekOrigin.Begin);
            return true;
        }
        public async Task<CASData> DeserializeCAS(BinaryReader bs)
        {
            QScript.SymbolBufferReader qReader = new QScript.SymbolBufferReader(bs);
            FileHeader header = new FileHeader();
            header.Read(bs);
            /*if (!ValidateChecksums(header, bs))
            {
                return null;
            }*/
            var result = new CASData();
            result.summary = (qReader.ReadBuffer());
            result.summary = await ResolveChecksums(result.summary);
            result.save_data = (qReader.ReadBuffer());
            result.save_data = await ResolveChecksums(result.save_data);
            
            return result;
        }

        public async Task<MemoryStream> SerializeCAS(CASData saveData)
        {
            var qw = new SymbolBufferWriter();
            using (MemoryStream ms = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(ms))
                {
                    FileHeader header = new FileHeader();
                    header.Write(bw); //write place holder

                    int header_size = (int)ms.Position;

                    var retMs = new MemoryStream();


                    var summary_info = (List<SymbolEntry>)saveData.summary;
                    summary_info = await GenerateChecksums(summary_info);
                    qw.SerializeBuffer(bw, summary_info);
                    var summary_size = ms.Position - header_size;

                    var save_data = (List<SymbolEntry>)saveData.save_data;
                    save_data = await GenerateChecksums(save_data);
                    qw.SerializeBuffer(bw, save_data);
                    var save_size = ms.Position - summary_size - header_size;

                    byte[] summary_buffer = summary_buffer = ms.GetBuffer().Skip(header_size).Take((int)summary_size).ToArray();

                    header.summaryInfoChecksum = await checksumResolver.GenerateChecksum(summary_buffer);
                    header.checksum = 0;
                    header.dataSize = (int)(summary_size + header_size + save_size);
                    header.summaryInfoSize = (int)summary_size;
                    header.version = version;

                    bw.BaseStream.Seek(0, SeekOrigin.Begin);
                    header.Write(bw);

                    byte[] buff = ms.GetBuffer().Take(header.dataSize).ToArray();
                    header.checksum = await checksumResolver.GenerateChecksum(buff);

                    bw.BaseStream.Seek(0, SeekOrigin.Begin);
                    header.Write(bw);
                    buff = ms.GetBuffer();

                    bw.BaseStream.Seek(0, SeekOrigin.End);
                    var desired_size = fixedFileSize - header.dataSize;
                    for (int i = 0; i < desired_size; i++)
                    {
                        bw.Write((byte)'i');
                    }

                    ms.WriteTo(retMs);




                    return retMs;
                }
            }
        }
        private async Task<List<SymbolEntry>> GenerateChecksums(List<SymbolEntry> input)
        {
            List<SymbolEntry> list = new List<SymbolEntry>();
            foreach (var item in input)
            {
                var shortKey = await checksumResolver.GetCompressedKey(item.name.ToString());
                if (shortKey != null)
                {
                    item.name = shortKey.checksum.ToString();
                    item.compressedByteSize = shortKey.compressedByteSize;
                }
                else
                {
                    if (item.name.GetType() != typeof(System.Int64))
                    {
                        var crc = await checksumResolver.GenerateChecksum(item.name.ToString());
                        item.name = (System.UInt32)crc;
                    }

                }
                switch (item.type)
                {
                    case QScript.ESymbolType.ESYMBOLTYPE_NAME:
                        if (item.value.GetType() != typeof(System.Int64))
                        {
                            item.value = (System.UInt32)await checksumResolver.GenerateChecksum(item.value.ToString());
                        }
                        break;
                    case QScript.ESymbolType.ESYMBOLTYPE_STRUCTURE:
                        item.value = await GenerateChecksums((List<SymbolEntry>)item.value);
                        break;
                    case QScript.ESymbolType.ESYMBOLTYPE_ARRAY:
                        item.value = await GenerateArrayChecksums(item);
                        break;
                }
                list.Add(item);
            }
            return list;
        }
        private async Task<List<object>> GenerateArrayChecksums(SymbolEntry item)
        {
            List<object> list = new List<object>();
            var items = (List<object>)item.value;
            for (int i = 0; i < items.Count; i++)
            {
                switch (item.subType)
                {
                    case QScript.ESymbolType.ESYMBOLTYPE_NAME:
                        if ((((List<object>)item.value)[i]).GetType() != typeof(System.Int64))
                        {
                            var name = (((List<object>)item.value)[i]).ToString();
                            list.Add(await checksumResolver.GenerateChecksum(name));
                        }
                        else
                        {
                            list.Add((System.Int64)((List<object>)item.value)[i]);
                        }
                        break;
                    case QScript.ESymbolType.ESYMBOLTYPE_STRUCTURE:
                        List<SymbolEntry> struct_value = (List<SymbolEntry>)(items)[i];
                        List<SymbolEntry> symbols = await GenerateChecksums(struct_value);
                        list.Add(symbols);
                        break;
                    case QScript.ESymbolType.ESYMBOLTYPE_ARRAY:
                        list.Add(GenerateArrayChecksums((SymbolEntry)((List<object>)item.value)[i]));
                        break;
                    default:
                        list.Add(((List<object>)item.value)[i]);
                        break;
                }
            }
            return list;
        }
    }
}
