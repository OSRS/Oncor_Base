using System;

namespace OncorUserRoles
{
    internal abstract class BaseShell
    {
        private string prompt;

        protected BaseShell(string startPrompt, string usage)
        {
            this.prompt = startPrompt + "> ";
            this.usage = usage;
        }


       public void DoREPL()
       {
            string action;
            while (keepOn)
            {
                Console.Write(prompt);
                action = Console.ReadLine();
                if (string.IsNullOrEmpty(action))
                    Usage();
                else
                {
                    string tmp = action.ToLowerInvariant();
                    if (action == "help" || action == "?" || action == "-help" || action == "--help" || action == "/?" || action == "-h")
                        Usage();
                    else if (tmp == "q" || tmp == "quit")
                        this.keepOn = false;
                    else
                    {
                        action = Eval(action);
                        if (keepOn)
                            Print(action);
                    }
                }
            }
        }

        private bool keepOn = true;
        protected void Bail(string message)
        {
            this.keepOn = false;
            if (!string.IsNullOrEmpty(message))
                Console.WriteLine(message);
        }

        protected abstract string Eval(string input);

        protected virtual void Print(string message)
        {
            if (!string.IsNullOrEmpty(message))
                Console.WriteLine(message);
        }

        private readonly string usage;
        protected void Usage()
        {
            Console.WriteLine(hLine);
            Console.WriteLine(usage);
            Console.WriteLine(hLine);
        }

        protected void Usage(string message)
        {
            Console.WriteLine(hLine);
            Console.WriteLine(message);
            Usage();
        }

        private const string hLine = "-----------------------------";
    }
}
