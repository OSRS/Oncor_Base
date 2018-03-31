using Pnnl.Oncor.Host;
using System;

namespace OncorDev
{
    class Program
    {
        static void Main(string[] args)
        {
            OncorServer srv = OncorServer.Instance;
            Console.WriteLine("Starting");
            if (srv.Start())
                Console.WriteLine("Started " + srv.State.ToString());
            else
            {
                Console.WriteLine("Failed Starting (review log) "+srv.State);
            }

            Console.WriteLine("Done, enter to exit");
            Console.ReadLine();
            if (srv.State == Osrs.Runtime.RunState.Running)
            {
                Console.WriteLine("Stopping...");
                srv.Stop();
            }
            Console.WriteLine("Final state: " + srv.State.ToString());
            Console.WriteLine("Bye");
        }
    }
}
