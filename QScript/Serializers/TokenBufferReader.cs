using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace QScript
{
    public class TokenEntry
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(StringEnumConverter))]
        public EScriptToken type;
        public object value;
    }
    public class TokenBufferReader
    {
        BinaryReader _bs;
        IChecksumResolver _resolver;
        bool printTokens;
        bool insertChecksums;
        public TokenBufferReader(IChecksumResolver resolver, BinaryReader bs)
        {
            _resolver = resolver;
            _bs = bs;
            printTokens = false;
            insertChecksums = false;
        }

        public void Dispose()
        {
            _bs.Dispose();
        }

        public async Task<List<TokenEntry>> ReadBuffer()
        {
            List<TokenEntry> objects = new List<TokenEntry>();
            while (true)
            {
                EScriptToken symbol = GetNextToken();
                if (symbol == EScriptToken.ESCRIPTTOKEN_ENDOFFILE) break;
                object value = await GetSymbolValue(symbol);

                objects.Add(new TokenEntry { type = symbol, value = value });
                if(printTokens)
                    Console.WriteLine("Got token" + symbol.ToString() + " " + (value == null ? "null" : value.ToString()));
            }
            return objects;
        }
        private EScriptToken GetNextToken()
        {
            System.Byte b = (System.Byte)_bs.ReadByte();
            return (EScriptToken)(b);
        }
        private async Task<object> GetSymbolValue(EScriptToken type)
        {
            switch (type)
            {
                case EScriptToken.ESCRIPTTOKEN_FLOAT:
                    return _bs.ReadSingle();
                case EScriptToken.ESCRIPTTOKEN_INTEGER:
                    return _bs.ReadInt32();
                case EScriptToken.ESCRIPTTOKEN_NAME:
                    var checksum = _bs.ReadUInt32();
                    try
                    {
                        return await _resolver.ResolveChecksum(checksum);
                    } catch
                    {
                        return checksum;
                    }
                case EScriptToken.ESCRIPTTOKEN_LOCALSTRING:
                case EScriptToken.ESCRIPTTOKEN_STRING:
                
                    var length = _bs.ReadInt32();
                    string s = "";
                    while(length-- > 0)
                    {
                        var c =  _bs.ReadChar();
                        if(c != 0)
                            s += c;
                    }
                    return s;
                case EScriptToken.ESCRIPTTOKEN_KEYWORD_SCRIPT:
                case EScriptToken.ESCRIPTTOKEN_KEYWORD_ENDSCRIPT:

                case EScriptToken.ESCRIPTTOKEN_ENDOFLINE:
                case EScriptToken.ESCRIPTTOKEN_EQUALS:
                case EScriptToken.ESCRIPTTOKEN_STARTSTRUCT:
                case EScriptToken.ESCRIPTTOKEN_ENDSTRUCT:
                case EScriptToken.ESCRIPTTOKEN_STARTARRAY:
                case EScriptToken.ESCRIPTTOKEN_ENDARRAY:
                    return null;
                case EScriptToken.ESCRIPTTOKEN_CHECKSUM_NAME:
                    var checksum_number = _bs.ReadUInt32();
                    string checksum_name = "";
                    while (true)
                    {
                        var c = _bs.ReadChar();
                        if (c == 0)
                            break;
                        checksum_name += c;
                    }
                    if(insertChecksums)
                    {
                        var generated_checksum = await _resolver.GenerateChecksum(checksum_name);
                        if (generated_checksum != checksum_number)
                        {
                            throw new ArgumentException("Checksum mismatch", checksum_name);
                        }
                    }

                    return checksum_name;
                default:
                    throw new ArgumentException("Got unhandled symbol", type.ToString());
            }
        }
    }
}
