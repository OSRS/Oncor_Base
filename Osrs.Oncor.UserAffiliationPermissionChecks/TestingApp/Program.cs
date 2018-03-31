using Osrs.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestingApp
{
    class Program
    {
        static void Main(string[] args)
        {
            HashSet<CompoundIdentity> ids = new HashSet<CompoundIdentity>();

            CompoundIdentity id1 = new CompoundIdentity(Guid.NewGuid(), Guid.NewGuid());
            CompoundIdentity id2 = new CompoundIdentity(Guid.NewGuid(), Guid.NewGuid());

            CompoundIdentity id3 = new CompoundIdentity(id1.DataStoreIdentity, id1.Identity);
            CompoundIdentity id4 = new CompoundIdentity(id2.DataStoreIdentity, id2.Identity);

            Console.WriteLine("1: " + ids.Add(id1));
            Console.WriteLine("1: " + ids.Add(id2));

            Console.WriteLine("Cont 1: " + ids.Contains(id1) + ", " + ids.Contains(id3));

            Console.WriteLine("1: " + ids.Add(id3));
            Console.WriteLine("1: " + ids.Add(id4));

            Console.ReadLine();
        }
    }
}
