using MongoDB.Bson;
using MongoDB.Driver;
using QScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using THPS.API.DbContext;
using THPS.API.Utils;

namespace THPS.API.Repository
{
    public class ScriptKeyRepository : IScriptKeyRepository
    {
        private ITHPSAPIDBContext dbContext;
        public ScriptKeyRepository(ITHPSAPIDBContext dbContext)
        {
            this.dbContext = dbContext;
        }
        public async Task<ScriptKeyRecord> GetRecord(ScriptKeyRecord record)
        {
            ScriptKeyRecord resp = null;
            if (record.name != null)
            {

                resp = await GetRecordByName(record);
                if(resp != null)
                {
                    return resp;
                }
                resp = new ScriptKeyRecord();
                var crc = new Crc32();
                resp.checksum = crc.Get(Encoding.ASCII.GetBytes(record.name.ToLower()));
                resp.name = record.name;
                await SaveRecord(resp);
            } else if(record.checksum.HasValue)
            {
                resp = await GetRecordByChecksum(record);
            }
            if(resp == null)
            {
                resp = new ScriptKeyRecord();
            }
                
            return resp;
        }
        private async Task SaveRecord(ScriptKeyRecord record)
        {
            var db = dbContext.GetDatabase();
            var collection = db.GetCollection<BsonDocument>("scriptkeys");
            var document = new BsonDocument
            {
                { "name", new BsonString(record.name)},
                { "checksum", new BsonInt32((System.Int32)record.checksum.Value)}
            };
            if(record.platform.HasValue)
                document.Add(new BsonElement("platform", new BsonInt32((int)record.platform.Value)));
            if(record.version.HasValue)
                document.Add(new BsonElement("game", new BsonInt32((int)record.version.Value)));
            await collection.InsertOneAsync(document);
        }
        private async Task<ScriptKeyRecord> GetRecordByChecksum(ScriptKeyRecord record)
        {
            if(!record.compressedByteSize.HasValue || record.compressedByteSize.Value == 4)
            {
                return await GetRecordByUncompressedChecksum(record);
            } else if(record.compressedByteSize.HasValue)
            {
                return await GetRecordByCompressedChecksum(record);
            }
            return null;
        }
        private async Task<ScriptKeyRecord> GetRecordByUncompressedChecksum(ScriptKeyRecord record)
        {
            var db = dbContext.GetDatabase();
            var collection = db.GetCollection<BsonDocument>("scriptkeys");

            var filter = Builders<BsonDocument>.Filter.Eq("checksum", new BsonInt32((System.Int32)record.checksum.Value));
            var results = await collection.FindAsync(filter);

            var dbResult = results.FirstOrDefault();
            if (dbResult == null)
            {
                return record;
            }
            return ConvertResultToRecord(dbResult);
        }

        private async Task<ScriptKeyRecord> GetRecordByCompressedChecksum(ScriptKeyRecord record)
        {
            var db = dbContext.GetDatabase();
            var collection = db.GetCollection<BsonDocument>("compress_tables");

            var match = new BsonDocument
            {
                { "game", new BsonInt32((int)record.version)},
                { "platform", new BsonInt32((int)record.platform)},
                { "byteSize", new BsonInt32(record.compressedByteSize.Value)}
            };

            var arrayElem = new BsonArray
            {
                new BsonString("$entries"), new BsonInt32((System.Int32)record.checksum.Value)
            };
            var projection_inner = new BsonDocument
            {
                { "$arrayElemAt", arrayElem }
            };
            var projection = new BsonDocument
            {
                {"result", projection_inner }
            };
            
            PipelineDefinition< BsonDocument, BsonDocument> pipeline = new BsonDocument[]
            {
                new BsonDocument { { "$match", match } },
                new BsonDocument { { "$project", projection } }
            };
            var results = await collection.AggregateAsync(pipeline);

            var dbResult = results.FirstOrDefault();
            if (dbResult == null)
            {
                return null;
            }
            var name = dbResult.Elements.Where(s => s.Name.ToLower() == "result").FirstOrDefault();
            if (name == null || name.Value == null) return null;
            record.name = name.Value.ToString();
            return record;
            
        }
        private async Task<ScriptKeyRecord> GetRecordByName(ScriptKeyRecord record)
        {
            var db = dbContext.GetDatabase();
            var collection = db.GetCollection<BsonDocument>("scriptkeys");

            var filter = Builders<BsonDocument>.Filter.Eq("name", new BsonString(record.name));
            var results = await collection.FindAsync(filter);

            var dbResult = results.FirstOrDefault();
            if (dbResult == null) return null;
            return ConvertResultToRecord(dbResult);
        }
        private ScriptKeyRecord ConvertResultToRecord(BsonDocument document)
        {
            ScriptKeyRecord result = new ScriptKeyRecord();
            var column = document.Elements.Where(s => s.Name.ToLower().Equals("name")).FirstOrDefault();
            if (column.Value != null)
                result.name = column.Value.ToString();

            column = document.Elements.Where(s => s.Name.ToLower().Equals("checksum")).FirstOrDefault();
            if (column.Value != null)
                result.checksum = (System.UInt32)column.Value.ToInt32();

            column = document.Elements.Where(s => s.Name.ToLower().Equals("version")).FirstOrDefault();
            if (column.Value != null)
              result.version = (GameVersion)column.Value.ToInt32();

            column = document.Elements.Where(s => s.Name.ToLower().Equals("platform")).FirstOrDefault();
            if (column.Value != null)
                result.platform = (GamePlatform)column.Value.ToInt32();

            column = document.Elements.Where(s => s.Name.ToLower().Equals("compressedByteSize")).FirstOrDefault();
            if (column.Value != null)
                result.compressedByteSize = column.Value.ToInt32();
            return result;
        }

        public async Task<List<ScriptKeyRecord>> GetCompressTables(ScriptKeyRecord lookup)
        {
            //.find({entries: {$elemMatch: {$eq: "body_type"}}, platform: 0, game: 4, })
            var db = dbContext.GetDatabase();
            var collection = db.GetCollection<BsonDocument>("compress_tables");

            /*var elemMatchInner = new BsonDocument
            {
                { "$eq", new BsonString(lookup.name)}
            };
            var entries = new BsonDocument
            {
                { "$elemMatch", elemMatchInner}
            };*/
            var match = new BsonDocument
            {
                { "platform", new BsonInt32((int)lookup.platform)},
                { "game", new BsonInt32((int)lookup.version)},
                //{ "byteSize", new BsonInt32(lookup.compressedByteSize.Value)},
                //{"entries", entries }
            };

            PipelineDefinition<BsonDocument, BsonDocument> pipeline = new BsonDocument[]
            {
                new BsonDocument { { "$match", match } }
            };

            var list = new List<ScriptKeyRecord>();
            var results = await collection.AggregateAsync(pipeline);
            await results.ForEachAsync(i =>
            {
                var column = i.Elements.Where(s => s.Name.ToLower().Equals("bytesize")).FirstOrDefault();
                int compressedByteSize = 0;
                if (column.Value != null)
                    compressedByteSize = column.Value.ToInt32();

                column = i.Elements.Where(s => s.Name.ToLower().Equals("platform")).FirstOrDefault();
                int platform = 0;
                if (column.Value != null)
                    platform = column.Value.ToInt32();

                column = i.Elements.Where(s => s.Name.ToLower().Equals("game")).FirstOrDefault();
                int game = 0;
                if (column.Value != null)
                    game = column.Value.ToInt32();

                var entries = i.Elements.Where(s => s.Name.ToLower().Equals("entries")).FirstOrDefault();
                var entries_array = entries.Value.AsBsonArray;
                for(int c=0;c<entries_array.Count;c++)
                {
                    var item = new ScriptKeyRecord();
                    item.name = entries_array[c].AsString;
                    item.checksum = (System.UInt32)c;
                    item.platform = (GamePlatform)platform;
                    item.version = (GameVersion)game;
                    item.compressedByteSize = compressedByteSize;
                    list.Add(item);
                }
            });
            return list;
        }

        public async Task<SaveFileTypeRecord> SaveFileInfo(SaveFileTypeRecord lookup)
        {
            var db = dbContext.GetDatabase();
            var collection = db.GetCollection<BsonDocument>("saveFileTypes");
            var document = new BsonDocument
            {
                { "name", new BsonString(lookup.name)},
                { "platform", new BsonInt32((System.Int32)lookup.platform)},
                { "game", new BsonInt32((System.Int32)lookup.version)},
                { "fixedFileSize", new BsonInt32((System.Int32)lookup.fixedFileSize)},
                { "fileVersion", new BsonInt32((System.Int32)lookup.fileVersion)},
            };
            
            await collection.InsertOneAsync(document);

            return lookup;
        }
        public async Task<SaveFileTypeRecord> GetFileInfo(string name, GameVersion version, GamePlatform platform)
        {
            var db = dbContext.GetDatabase();
            var collection = db.GetCollection<BsonDocument>("saveFileTypes");

            var match = new BsonDocument
            {
                { "platform", new BsonInt32((int)platform)},
                { "game", new BsonInt32((int)version)},
                { "name", new BsonString(name)},
            };

            PipelineDefinition<BsonDocument, BsonDocument> pipeline = new BsonDocument[]
            {
                new BsonDocument { { "$match", match } }
            };

            var list = new List<ScriptKeyRecord>();
            var results = await collection.AggregateAsync(pipeline);
            var result = results.FirstOrDefault();
            if (result == null) return null;
            var record = new SaveFileTypeRecord();

            var column = result.Elements.Where(s => s.Name.ToLower().Equals("fixedfilesize")).FirstOrDefault();
            if (column.Value != null)
                record.fixedFileSize = (System.UInt32)column.Value.ToInt32();

            column = result.Elements.Where(s => s.Name.ToLower().Equals("fileversion")).FirstOrDefault();
            if (column.Value != null)
                record.fileVersion = (System.UInt32)column.Value.ToInt32();

            column = result.Elements.Where(s => s.Name.ToLower().Equals("version")).FirstOrDefault();
            if (column.Value != null)
                record.version = (GameVersion)column.Value.ToInt32();

            column = result.Elements.Where(s => s.Name.ToLower().Equals("platform")).FirstOrDefault();
            if (column.Value != null)
                record.platform = (GamePlatform)column.Value.ToInt32();

            column = result.Elements.Where(s => s.Name.ToLower().Equals("name")).FirstOrDefault();
            if (column.Value != null)
                record.name = column.Value.ToString();

            return record;
        }
    }
}
