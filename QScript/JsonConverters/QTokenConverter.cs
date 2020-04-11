using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace QScript.JsonConverters
{
    public class QTokenConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(List<TokenEntry>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        private bool isLiteral(EScriptToken token)
        {
            switch(token)
            {
                case EScriptToken.ESCRIPTTOKEN_LOCALSTRING:
                case EScriptToken.ESCRIPTTOKEN_STRING:
                case EScriptToken.ESCRIPTTOKEN_INTEGER:
                case EScriptToken.ESCRIPTTOKEN_FLOAT:
                    return true;
            }
            return false;
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            var qList = (List<TokenEntry>)value;

            bool inAssignment = false;
            bool hasDoubleObject = false;
            bool skipFunction = false;
            bool inArray = false;
            EScriptToken lastToken = EScriptToken.ESCRIPTTOKEN_ENDOFFILE;
            for (var i=0;i<qList.Count;i++)
            {
                var item = qList[i];
                if(skipFunction && item.type != EScriptToken.ESCRIPTTOKEN_KEYWORD_ENDSCRIPT) continue;
                switch(item.type)
                {
                    case EScriptToken.ESCRIPTTOKEN_NAME:
                        if(inAssignment || inArray)
                        {
                            writer.WriteValue(item.value.ToString());
                            inAssignment = false;
                        } else
                        {
                            writer.WritePropertyName(item.value.ToString());
                            if(qList[i + 1].type != EScriptToken.ESCRIPTTOKEN_EQUALS && qList[i + 2].type != EScriptToken.ESCRIPTTOKEN_EQUALS)
                            {
                                writer.WriteValue(true);
                            }
                            
                        }
                        
                        break;
                    case EScriptToken.ESCRIPTTOKEN_INTEGER:
                        if (isLiteral(lastToken)) continue;
                        inAssignment = false;
                        writer.WriteValue((int)item.value);
                        break;
                    case EScriptToken.ESCRIPTTOKEN_FLOAT:
                        if (isLiteral(lastToken)) continue;
                        inAssignment = false;
                        writer.WriteValue((float)item.value);
                        break;
                    case EScriptToken.ESCRIPTTOKEN_STRING:
                    case EScriptToken.ESCRIPTTOKEN_LOCALSTRING:
                        if (isLiteral(lastToken)) continue;
                        inAssignment = false;
                        writer.WriteValue((string)item.value);
                        break;
                    case EScriptToken.ESCRIPTTOKEN_ARRAY:
                    case EScriptToken.ESCRIPTTOKEN_STARTARRAY:
                        inAssignment = false;
                        inArray = true;
                        writer.WriteStartArray();
                        break;
                    case EScriptToken.ESCRIPTTOKEN_ENDARRAY:
                        inArray = false;
                        writer.WriteEndArray();
                        break;
                    case EScriptToken.ESCRIPTTOKEN_STARTSTRUCT:
                        if (lastToken == item.type)
                        {
                            hasDoubleObject = true;
                            continue;
                        }
                        try
                        {
                            writer.WriteStartObject();
                            inAssignment = false;
                            inArray = false;
                        } catch
                        {
                            writer.WritePropertyName("unnamed");

                            writer.WriteStartObject();
                        }
                        break;
                    case EScriptToken.ESCRIPTTOKEN_ENDSTRUCT:
                        if (lastToken == item.type && hasDoubleObject)
                        {
                            hasDoubleObject = false;
                            continue;
                        }
                        writer.WriteEndObject();
                        break;
                    case EScriptToken.ESCRIPTTOKEN_EQUALS:
                        inAssignment = true;
                        break;
                    case EScriptToken.ESCRIPTTOKEN_ENDOFLINE:
                        break;
                    case EScriptToken.ESCRIPTTOKEN_KEYWORD_SCRIPT:
                        skipFunction = true;
                        break;
                    case EScriptToken.ESCRIPTTOKEN_KEYWORD_ENDSCRIPT:
                        skipFunction = false;
                        break;
                    case EScriptToken.ESCRIPTTOKEN_CHECKSUM_NAME:
                    case EScriptToken.ESCRIPTTOKEN_ENDOFFILE:
                        break;
                    default:
                        throw new NotImplementedException();
                        break;
                }
                if(item.type != EScriptToken.ESCRIPTTOKEN_ENDOFLINE)
                    lastToken = item.type;
            }
            writer.WriteEndObject();
        }
    }
}
