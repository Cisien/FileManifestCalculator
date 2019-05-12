using System;
using System.IO;
using System.Threading.Tasks;

namespace FileManifestCalculator
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var manifestCalc = new ManifestCalculator();
            var manifest = await manifestCalc.BuildManifest(Environment.CurrentDirectory);

            File.WriteAllText("manifest.csv", manifest);
            Console.WriteLine(manifest);
            Console.WriteLine("Done");
            Console.ReadKey();
        }
    }
}
