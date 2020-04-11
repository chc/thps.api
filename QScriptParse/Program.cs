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

            using (FileStream fileStream = new FileStream(@"D:\code\thug2_scripts\game\cas_skater_m.qb", FileMode.Open))
            {
                using (BinaryReader bs = new BinaryReader(fileStream))
                {
                    QScript.TokenBufferReader qReader = new QScript.TokenBufferReader(provider.GetRequiredService<QScript.IChecksumResolver>(),bs);
                    var entries = qReader.ReadBuffer();
                    entries.Wait();
                    Console.WriteLine("xxx");
                }
            }
                
        }
    }
}
