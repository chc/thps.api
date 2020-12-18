using Asset;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace QScriptParse
{
    class Program
    {
        static void Main(string[] args)
        {
            var servicesCollection = new ServiceCollection();
            servicesCollection.AddHttpClient(HTTPChecksumResolver.HTTPClientFactoryName, c =>
            {
                c.BaseAddress = new Uri("http://api.thmods.com");
            });
            servicesCollection.AddSingleton<QScript.IChecksumResolver, HTTPChecksumResolver>();

            var provider = servicesCollection.BuildServiceProvider();

            using (FileStream fileStream = new FileStream(@"C:\Levels\TestLevel\TestLevel.scn.xbx", FileMode.Open))
            {
                using (BinaryReader bs = new BinaryReader(fileStream))
                {
                    SceneReader sReader = new SceneReader(provider.GetRequiredService<QScript.IChecksumResolver>(),bs);
                    var scene = sReader.ReadBuffer();
                    scene.Wait();
                    var payload = Newtonsoft.Json.JsonConvert.SerializeObject(scene.Result);
                    File.WriteAllText("out.json", payload);
                    Console.WriteLine(payload);
                }
            }
                
        }
    }
}
