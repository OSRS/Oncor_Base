using System;
using System.Text;

namespace OncorUserRoles
{
    internal sealed class RolesShell : BaseShell
    {
        private static readonly char[] seps = { ' ' };
        private const string usage = "enter <list <all>| grant | revoke> a role name, roles for a list of roles, help for this message or quit to return\n\tExample: grant Reader";
        private readonly Db database;
        private readonly Guid userId;

        internal RolesShell(Db database, string userEmail, Guid userId) : base(userEmail+":roles", usage)
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
                    StringBuilder res = new StringBuilder("Roles\n");
                    res.AppendLine("------------------------------------");
                    if (tmp == "list")
                    {
                        foreach (string cur in database.RequestedRoles(this.userId))
                            res.AppendLine(cur);
                    }
                    else if (tmp == "list all")
                    {
                        foreach (string cur in database.Roles)
                            res.AppendLine(cur);
                    }
                    else
                        return usage;
                    return res.ToString();
                }
                else
                {
                    string[] args = tmp.Split(seps); //already lowered
                    if (args != null && args.Length == 2)
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

                        Guid roleId = database.Role(args[1]);
                        if (!Guid.Empty.Equals(roleId))
                        {
                            bool result = false;
                            if (grant)
                            {
                                if (args[1] == "writerwq" || args[1] == "writerfish" || args[1] == "writerveg")
                                    database.Grant(userId, database.Role("writersampleevent")); //it's fine that we won't revoke it, it's effectively inaccessible, just needed to create any det data
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
                            return "role not found \n" + usage;
                    }
                    else
                        return usage;
                }
            }
            return usage;
        }
    }
}
