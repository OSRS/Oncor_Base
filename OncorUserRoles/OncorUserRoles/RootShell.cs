
using System;
using System.Collections.Generic;
using System.Text;

namespace OncorUserRoles
{
    internal sealed class RootShell : BaseShell
    {
        private readonly Db database;

        internal RootShell(Db database) : base("Oncor", "list | requests | useremail \n\tlist: lists all user emails, can be a \"starts with\" like list mi or list will \n\trequests: lists all active requests \n\tuseremail: selects a specific user to operate on")
        {
            this.database = database;
        }

        private string To(IEnumerable<string> items)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string cur in items)
            {
                sb.AppendLine("\t" + cur);
            }
            if (sb.Length < 1)
                return "NONE FOUND";
            return sb.ToString();
        }

        protected override string Eval(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                string tmp = input.ToLowerInvariant();
                if (tmp == "requests")
                {
                    return To(database.ListRequests());
                }
                else if (tmp.StartsWith("list"))
                {
                    if (tmp.Length > 5)
                    {
                        tmp = tmp.Substring(5);
                    }
                    else
                        tmp = string.Empty;
                    return To(database.ListUsers(tmp));
                }
                else
                {
                    Guid id = database.FindUser(input);
                    if (!Guid.Empty.Equals(id))
                    {
                        UserShell sh = new UserShell(this.database, input, id);
                        sh.DoREPL();
                    }
                    else
                        Usage("You must provide an email address for a valid Oncor user, or quit to exit.");
                }
                return null;
            }

            Usage("You must provide an email address for a valid Oncor user, or quit to exit.");
            return null;
        }
    }
}
