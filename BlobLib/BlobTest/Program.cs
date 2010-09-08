using System;
using System.IO;
using System.Text;
using BlobLib;

namespace BlobTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DumpCDRFromClientRegistry();
        }

        static void DumpCDRFromBin()
        {
            byte[] CDRdata = File.ReadAllBytes("C:\\steamre\\CDR.blob");
            Blob root = BlobParser.ParseBlob(CDRdata);

            /*
            StringBuilder output = new StringBuilder();
            root.Dump(output, 0);

            File.WriteAllText("C:\\steamre\\DumpCDR.txt", output.ToString());*/
        }

        static void DumpCDRFromClientRegistry()
        {
            StringBuilder output = new StringBuilder();

            byte[] clientdata = File.ReadAllBytes("D:\\Steam\\ClientRegistry.blob");

            DateTime x = DateTime.Now;
            Blob root = BlobParser.ParseBlob(clientdata);
            Console.WriteLine(DateTime.Now - x);

            Console.ReadKey();
          
            Blob TopKey = root.GetBlobDescriptor("TopKey");
            Blob Info = TopKey.GetBlobDescriptor(2);

            Blob KVBlob = Info.GetBlobDescriptor("SteamInstancePublicKey");
            byte[] instanceKey = KVBlob.GetDescriptor(2);

            File.WriteAllBytes("E:\\steamre\\instancekey.bin", instanceKey);

            KVBlob = Info.GetBlobDescriptor("ContentDescriptionRecord");
            Blob CDRRoot = KVBlob.GetBlobDescriptor(2);

            //CDRRoot.Dump(output, 0);
            File.WriteAllText("E:\\steamre\\CDRDump.txt", output.ToString());

        }
    }
}
