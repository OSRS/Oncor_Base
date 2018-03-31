using System;
using System.IO;
using System.Reflection;

namespace OncorUserRoles
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "OncorUserRoles.db");
            if (args != null && args.Length > 0)
            {
                path = args[0];
            }

            if (!File.Exists(path))
            {
                PrintHLine();
                Console.WriteLine("Config file not found: " + path);
                PrintUsage();
                return;
            }

            try
            {
                path = File.ReadAllText(path);
                if (string.IsNullOrEmpty(path))
                {
                    PrintHLine();
                    Console.WriteLine("Couldn't read config file");
                    PrintUsage();
                    return;
                }
            }
            catch
            {
                PrintHLine();
                Console.WriteLine("Couldn't read config file");
                PrintUsage();
                return;
            }

            Db database = new Db(path);
            if (!database.TestConn())
            {
                PrintHLine();
                Console.WriteLine("Couldn't connect to database");
                PrintUsage();
                return;
            }

            RootShell r = new RootShell(database);
            r.DoREPL(); //this loops forever

            Console.WriteLine("Bye!");
        }

        private static void PrintUsage()
        {
            PrintHLine();
            Console.WriteLine("USAGE: OncorUserRoles <optionalConfigFile>");
            Console.WriteLine("\tIf no configFile provided, uses .\\OncorUserRoles.db");
            PrintHLine();
        }

        private static void PrintHLine()
        {
            Console.WriteLine("------------------------------------");
        }
    }
}
