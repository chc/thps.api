using Asset;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace QScriptParse
{
    class Program
    {
        static int Main(string[] args)
        {
            var servicesCollection = new ServiceCollection();
            servicesCollection.AddHttpClient(HTTPChecksumResolver.HTTPClientFactoryName, c =>
            {
                c.BaseAddress = new Uri("http://api.thmods.com");

            }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler() { 
                Proxy = new WebProxy("http://localhost:8080"),
                UseProxy = false
            });
            servicesCollection.AddSingleton<QScript.IChecksumResolver, HTTPChecksumResolver>(c => {
                return new HTTPChecksumResolver(c.GetService<IHttpClientFactory>(), QScript.GamePlatform.PlatformType_PC, QScript.GameVersion.GameVersion_THUG2);
            });

            var provider = servicesCollection.BuildServiceProvider();

            using (FileStream fileStream = new FileStream(@"/home/dev/tinySave.CAS", FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader bs = new BinaryReader(fileStream))
                {
                    QScript.Save.CAS.ISerializationProvider deserializer = new QScript.Save.CAS.Games.THPS4Common_SerializationProvider(provider.GetRequiredService<QScript.IChecksumResolver>(), 1, 1);
                    var resultsTask = deserializer.DeserializeCAS(bs);
                    var results = resultsTask.Result;
                    Console.WriteLine(results.summary);
                }
            }
            return -1;       
        }
    }
}
