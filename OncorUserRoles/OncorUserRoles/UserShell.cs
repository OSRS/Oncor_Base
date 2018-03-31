using System;
using System.Text;

namespace OncorUserRoles
{
    internal sealed class UserShell : BaseShell
    {
        private static readonly char[] seps = { ' ' };
        private const string usage = "enter <list | roles | orgs | delete> list for a list of user requested items, roles to perform role operations or orgs to perform org operations";
        private readonly Db database;
        private readonly Guid userId;
        private readonly string userEmail;

        internal UserShell(Db database, string userEmail, Guid userId) : base(userEmail, usage)
        {
            this.userEmail = userEmail;
            this.database = database;
            this.userId = userId;
        }


        protected override string Eval(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                string tmp = input.ToLowerInvariant();
                if (tmp == "list")
                {
                    return database.ListUserRequestTypes(userId);
                }
                else if (tmp == "role" || tmp== "roles")
                {
                    RolesShell sh = new RolesShell(this.database, userEmail, userId);
                    sh.DoREPL();
                }
                else if (tmp == "org" || tmp == "orgs")
                {
                    OrgsShell sh = new OrgsShell(this.database, userEmail, userId);
                    sh.DoREPL();
                }
                else if (tmp == "delete")
                {
                    //delete any request
                    bool result = database.Remove(userId);
                    if (result)
                        return null;
                    return "unable to delete request, perhaps none existed?";
                }
            }
            Usage();
            return null;
        }
    }
}
