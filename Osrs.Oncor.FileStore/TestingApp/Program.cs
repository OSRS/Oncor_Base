using Osrs.Oncor.FileStore;
using Osrs.Runtime.Configuration;
using Osrs.Runtime.Logging;
using System;
using System.IO;

namespace TestingApp
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigurationManager.Instance.Bootstrap();
            ConfigurationManager.Instance.Initialize();
            ConfigurationManager.Instance.Start();
            Console.WriteLine("ConfigurationManager: " + ConfigurationManager.Instance.State.ToString());

            LogManager.Instance.Bootstrap();
            LogManager.Instance.Initialize();
            LogManager.Instance.Start();
            Console.WriteLine("LogManager: " + LogManager.Instance.State.ToString());

            FileStoreManager.Instance.Initialize();
            FileStoreManager.Instance.Start();
            Console.WriteLine("FileStore: "+FileStoreManager.Instance.State.ToString());

            IFileStoreProvider prov = FileStoreManager.Instance.GetProvider();
            Console.WriteLine("Prov: " + (prov == null).ToString());

            if (prov!=null)
            {
                Guid id = Guid.NewGuid();
                FilestoreFile fil = prov.Make(id);
                if (fil!=null)
                {
                    Console.WriteLine("Got file");
                    string s = "Hello from testing";
                    TextWriter wr = new StreamWriter(fil);
                    for(int i=0;i<100;i++)
                    {
                        wr.WriteLine(s);
                    }
                    wr.Flush();
                    wr.Close();
                    fil.Dispose();
                    Console.WriteLine("Done");

                    fil = prov.Get(id);
                    if (fil!=null)
                    {
                        Console.WriteLine("Opened file");
                        wr = new StreamWriter(fil);
                        for (int i = 0; i < 100; i++)
                        {
                            wr.WriteLine(s);
                        }
                        wr.Flush();
                        wr.Close();
                        fil.Dispose();
                        Console.WriteLine("Wrote");
                        prov.Delete(id);
                        Console.WriteLine("Done");
                    }
                }

                prov.DeleteExpired();
            }

            Console.WriteLine("Enter to exit");
            Console.ReadLine();
        }
    }
}
