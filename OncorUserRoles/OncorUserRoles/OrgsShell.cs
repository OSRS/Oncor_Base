using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OncorUserRoles
{
    internal sealed class OrgsShell : BaseShell
    {
        private static readonly char[] seps = { ' ' };
        private const string usage = "enter <list <all>| grant | revoke> an org name, list for a list of orgs, help for this message or quit to return\n\tExample: grant PNNL";
        private readonly Db database;
        private readonly Guid userId;

        internal OrgsShell(Db database, string userEmail, Guid userId) : base(userEmail+":orgs", usage)
        {
            this.database = database;
            this.userId = userId;
        }


        protected override string Eval(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                string tmp = input.ToLowerInvariant();
                if (tmp.StartsWith("list"))
                {
                    StringBuilder res = new StringBuilder("Orgs\n");
                    res.AppendLine("------------------------------------");
                    if (tmp == "list")
                    {
                        foreach (string cur in database.RequestedOrgs(this.userId))
                            res.AppendLine(cur);
                    }
                    else if (tmp == "list all")
                    {
                        foreach (string cur in database.Orgs)
                            res.AppendLine(cur);
                    }
                    else
                        return null;
                    return res.ToString();
                }
                else
                {
                    string[] args = tmp.Split(seps); //already lowered
                    if (args != null && args.Length >= 2)
                    {
                        bool grant = true;
                        if (args[0] == "grant")
                        {
                            //do nothing, we're set
                        }
                        else if (args[0] == "revoke")
                            grant = false;
                        else
                            return usage;
                        Console.WriteLine("getting: "+ tmp.Substring(args[0].Length + 1));
                        Tuple<Guid, Guid> roleId = database.Org(tmp.Substring(args[0].Length+1));
                        if (roleId!=null)
                        {
                            bool result = false;
                            if (grant)
                            {
                                result = database.Grant(userId, roleId);
                            }
                            else
                                result = database.Revoke(userId, roleId);

                            if (!result)
                                return "unknown failure, possibly already set? \n" + usage;
                            else
                                return null;
                        }
                        else
                            return "org not found \n" + usage;
                    }
                    else
                        return usage;
                }
            }
            return usage;
        }
    }
}
