using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;

namespace OncorUserRoles
{
    internal sealed class Db
    {
        private const string selectUser = "SELECT \"Id\" FROM \"osrs\".\"SecurityIdentity\" WHERE lower(\"Name\")=:n";
        private const string insertGrant = "INSERT INTO \"osrs\".\"SecurityRoleMemberUsers\"(\"RoleId\", \"UserId\") VALUES (:r,:u)";
        private const string removeGrant = "DELETE FROM \"osrs\".\"SecurityRoleMemberUsers\" WHERE \"RoleId\"=:r AND \"UserId\"=:u";
        private const string insertGrantOrg = "INSERT INTO \"oncor\".\"UserAffiliations\"(\"OrgSystemId\", \"OrgId\", \"UserId\") VALUES (:rs,:r,:u)";
        private const string removeGrantOrg = "DELETE FROM \"oncor\".\"UserAffiliations\" WHERE \"OrgSystemId\"=:rs AND \"OrgId\"=:r AND \"UserId\"=:u";
        private const string selectOrgs = "SELECT \"Name\", \"SystemId\", \"Id\" FROM \"oncor\".\"Organizations\"";

        private readonly string connString;

        private const string listUsers = "SELECT \"Name\" FROM \"osrs\".\"SecurityIdentity\" WHERE \"Id\"<>'9D272AFE-3BEB-49D6-847F-A1000C958C20'";
        internal IEnumerable<string> ListUsers(string partial)
        {
            List<String> items = new List<string>();
            string q = listUsers;
            if (!string.IsNullOrEmpty(partial))
                q = q + " AND lower(\"Name\") LIKE '" + partial.ToLowerInvariant() + "%'";

            NpgsqlCommand cmd = Setup(q);
            NpgsqlDataReader rdr = ExecuteReader(cmd);
            if (rdr != null)
            {
                while (rdr.Read())
                {
                    items.Add((string)rdr[0]);
                }
            }
            rdr = null;
            cmd.Dispose();
            return items;
        }

        private const string rolesSe = "SELECT \"UserId\", \"RoleName\", \"Reason\" FROM \"oncor\".\"RequestRole\" WHERE \"UserId\"=:d";
        private const string orgsSe = "SELECT \"UserId\", \"OrgSystemId\", \"OrgId\", \"WriteAccess\", \"Reason\"  FROM \"oncor\".\"RequestAffiliation\" WHERE \"UserId\"=:d";

        internal IEnumerable<string> ListRequests()
        {
            List<String> items = new List<string>();
            string q = listUsers + " AND \"Id\" IN (SELECT \"UserId\" FROM \"oncor\".\"RequestAffiliation\" UNION ALL SELECT \"UserId\" FROM \"oncor\".\"RequestRole\")";

            NpgsqlCommand cmd = Setup(q);
            NpgsqlDataReader rdr = ExecuteReader(cmd);
            if (rdr != null)
            {
                while (rdr.Read())
                {
                    items.Add((string)rdr[0]);
                }
            }
            rdr = null;
            cmd.Dispose();
            return items;
        }

        private const string rolesDe = "DELETE FROM \"oncor\".\"RequestRole\" WHERE \"UserId\"=:d";
        private const string orgsDe = "DELETE FROM \"oncor\".\"RequestAffiliation\" WHERE \"UserId\"=:d";
        internal bool Remove(Guid user)
        {
            if (!Guid.Empty.Equals(user))
            {
                int res = 0;
                try
                {
                    NpgsqlCommand cmd = Setup(rolesDe);
                    cmd.Parameters.AddWithValue("d", user);
                    res += cmd.ExecuteNonQuery();
                    Close(cmd);

                    cmd = Setup(orgsDe);
                    cmd.Parameters.AddWithValue("d", user);
                    res += cmd.ExecuteNonQuery();
                    Close(cmd);
                    return res > 0;
                }
                catch
                { }
            }
            return false;
        }

        private const string rolesCt = "SELECT COUNT(*) FROM \"oncor\".\"RequestRole\" WHERE \"UserId\"=:d";
        private const string orgsCt = "SELECT COUNT(*) FROM \"oncor\".\"RequestAffiliation\" WHERE \"UserId\"=:d";
        internal string ListUserRequestTypes(Guid uid)
        {

            int a = Count(rolesCt, uid);
            int b = Count(orgsCt, uid);

            if (a == 0 && b == 0)
                return "None";
            else if (a > 0 && b > 0)
                return "Roles, Orgs";
            else if (a > 0)
                return "Roles";
            return "Orgs";
        }

        private int Count(string sql, Guid uid)
        {
            long id = 0;
            NpgsqlCommand cmd = Setup(sql);
            cmd.Parameters.AddWithValue("d", uid);
            NpgsqlDataReader rdr = ExecuteReader(cmd);
            if (rdr != null && rdr.Read())
            {
                id = (long)rdr[0];
            }
            rdr = null;
            cmd.Dispose();
            return (int)id;
        }

        internal Guid FindUser(string email)
        {
            Guid id = Guid.Empty;
            if (!string.IsNullOrEmpty(email))
            {
                NpgsqlCommand cmd = Setup(selectUser);
                cmd.Parameters.AddWithValue("n", email.ToLowerInvariant());
                NpgsqlDataReader rdr = ExecuteReader(cmd);
                if (rdr!=null && rdr.Read())
                {
                    id = (Guid)rdr[0];
                }
                rdr = null;
                cmd.Dispose();
            }
            return id;
        }

        internal Tuple<Guid, Guid> Org(string name)
        {
            Tuple<Guid, Guid> item=null;
            NpgsqlCommand cmd = Setup(selectOrgs + " WHERE lower(\"Name\")=lower(:d)");
            cmd.Parameters.AddWithValue("d", name);
            NpgsqlDataReader rdr = ExecuteReader(cmd);
            if (rdr != null)
            {
                if (rdr.Read())
                    item = new Tuple<Guid, Guid>((Guid)rdr[1], (Guid)rdr[2]);
            }
            rdr = null;
            cmd.Dispose();
            return item;
        }

        internal bool Grant(Guid user, Tuple<Guid, Guid> role)
        {
            if (!Guid.Empty.Equals(user) && !Guid.Empty.Equals(role))
            {
                try
                {
                    NpgsqlCommand cmd = Setup(insertGrantOrg);
                    cmd.Parameters.AddWithValue("u", user);
                    cmd.Parameters.AddWithValue("rs", role.Item1);
                    cmd.Parameters.AddWithValue("r", role.Item2);
                    int res = cmd.ExecuteNonQuery();
                    Close(cmd);
                    return res > 0;
                }
                catch
                { }
            }
            return false;
        }

        internal bool Revoke(Guid user, Tuple<Guid, Guid> role)
        {
            if (!Guid.Empty.Equals(user) && !Guid.Empty.Equals(role))
            {
                try
                {
                    NpgsqlCommand cmd = Setup(removeGrantOrg);
                    cmd.Parameters.AddWithValue("u", user);
                    cmd.Parameters.AddWithValue("rs", role.Item1);
                    cmd.Parameters.AddWithValue("r", role.Item2);
                    int res = cmd.ExecuteNonQuery();
                    Close(cmd);
                    return res > 0;
                }
                catch
                { }
            }
            return false;
        }

        internal IEnumerable<string> Orgs
        {
            get
            {
                List<String> items = new List<string>();

                NpgsqlCommand cmd = Setup(selectOrgs);
                NpgsqlDataReader rdr = ExecuteReader(cmd);
                if (rdr != null)
                {
                    while (rdr.Read())
                    {
                        items.Add((string)rdr[0]);
                    }
                }
                rdr = null;
                cmd.Dispose();
                return items;
            }
        }

        private Dictionary<Tuple<Guid, Guid>, string> OrgDict
        {
            get
            {
                Dictionary<Tuple<Guid, Guid>, string> items = new Dictionary<Tuple<Guid, Guid>, string>();

                NpgsqlCommand cmd = Setup(selectOrgs);
                NpgsqlDataReader rdr = ExecuteReader(cmd);
                if (rdr != null)
                {
                    while (rdr.Read())
                    {
                        items.Add(new Tuple<Guid, Guid>((Guid)rdr[1], (Guid)rdr[2]), (string)rdr[0]);
                    }
                }
                rdr = null;
                cmd.Dispose();
                return items;
            }
        }

        internal IEnumerable<string> RequestedOrgs(Guid user)
        {
            Dictionary<Tuple<Guid, Guid>, string> orgs = OrgDict;
            List<String> items = new List<string>();
            NpgsqlCommand cmd = Setup(orgsSe);
            cmd.Parameters.AddWithValue("d", user);
            NpgsqlDataReader rdr = ExecuteReader(cmd);
            if (rdr != null)
            {
                while (rdr.Read())
                {
                    string orgName = orgs[new Tuple<Guid, Guid>((Guid)rdr[1], (Guid)rdr[2])];
                    if ((bool)rdr[3])
                        orgName = orgName + " WRITE: ";
                    else
                        orgName = orgName + " READ: ";

                    if (DBNull.Value.Equals(rdr[4]))
                        items.Add(orgName);
                    else
                        items.Add(orgName + (string)rdr[4]);
                }
            }
            rdr = null;
            cmd.Dispose();
            return items;
        }

        internal IEnumerable<string> RequestedRoles(Guid user)
        {
            List<String> items = new List<string>();
            NpgsqlCommand cmd = Setup(rolesSe);
            cmd.Parameters.AddWithValue("d", user);
            NpgsqlDataReader rdr = ExecuteReader(cmd);
            if (rdr != null)
            {
                while (rdr.Read())
                {
                    if (DBNull.Value.Equals(rdr[2]))
                        items.Add((string)rdr[1]);
                    else
                        items.Add((string)rdr[1] + ": " + (string)rdr[2]);

                }
            }
            rdr = null;
            cmd.Dispose();
            return items;
        }

        internal bool Grant(Guid user, Guid role)
        {
            if (!Guid.Empty.Equals(user) && !Guid.Empty.Equals(role))
            {
                try
                {
                    NpgsqlCommand cmd = Setup(insertGrant);
                    cmd.Parameters.AddWithValue("u", user);
                    cmd.Parameters.AddWithValue("r", role);
                    int res = cmd.ExecuteNonQuery();
                    Close(cmd);
                    return res > 0;
                }
                catch
                { }
            }
            return false;
        }

        internal bool Revoke(Guid user, Guid role)
        {
            if (!Guid.Empty.Equals(user) && !Guid.Empty.Equals(role))
            {
                try
                {
                    NpgsqlCommand cmd = Setup(removeGrant);
                    cmd.Parameters.AddWithValue("u", user);
                    cmd.Parameters.AddWithValue("r", role);
                    int res = cmd.ExecuteNonQuery();
                    Close(cmd);
                    return res > 0;
                }
                catch
                { }
            }
            return false;
        }

        private readonly Dictionary<string, Guid> items = new Dictionary<string, Guid>();
        internal Guid Role(string name)
        {
            if (items.ContainsKey(name))
                return items[name];
            return Guid.Empty;
        }

        private readonly HashSet<string> names = new HashSet<string>();
        internal IEnumerable<string> Roles
        {
            get { return names; }
        }

        private NpgsqlCommand Setup(string text)
        {
            NpgsqlConnection con = new NpgsqlConnection(connString);
            NpgsqlCommand cmd = new NpgsqlCommand(text, con);
            try
            {
                con.Open();
            }
            catch
            {
                cmd.Dispose();
                cmd = null;
                con.Dispose();
                con = null;
                return null;
            }
            return cmd;
        }

        private NpgsqlDataReader ExecuteReader(NpgsqlCommand cmd)
        {
            if (cmd!=null)
            {
                NpgsqlDataReader rdr = null;
                try
                {
                    rdr = cmd.ExecuteReader();
                }
                catch (Exception e)
                {
                    this.Cancel(cmd);
                    return null;
                }
                return rdr;
            }
            return null;
        }

        private void Cancel(NpgsqlCommand cmd)
        {
            try
            {
                cmd.Cancel();
                Close(cmd);
            }
            catch { }
        }

        private void Close(NpgsqlCommand cmd)
        {
            try
            {
                cmd.Connection.Close();
            }
            catch
            { }

            if (cmd.Connection.State != ConnectionState.Closed)
            {
                cmd.Connection.Dispose();
            }
        }

        internal bool TestConn()
        {
            if (connString == null || string.Empty.Equals(connString))
            {
                return false;
            }

            NpgsqlConnection con = new NpgsqlConnection(connString);
            NpgsqlCommand cmd = new NpgsqlCommand();
            cmd.Connection = con;

            try
            {
                con.Open();
            }
            catch
            {
                cmd.Dispose();
                cmd = null;
                con.Dispose();
                con = null;
                return false;
            }
            cmd.Dispose();
            cmd = null;
            try
            {
                con.Close();
            }
            catch (NpgsqlException e)
            {
            }

            con.Dispose();
            con = null;
            return true;
        }

        internal Db(string connString)
        {
            this.connString = connString;
            names.Add("Reader");
            items["Reader".ToLowerInvariant()] = new Guid("a55c4810-de06-466e-8f30-4f02b138186f");

            names.Add("WriterOrg");
            items["WriterOrg".ToLowerInvariant()] = new Guid("9c555c3e-945a-454d-9143-90fe9ee47323");

            names.Add("WriterProject");
            items["WriterProject".ToLowerInvariant()] = new Guid("782a8610-1865-4829-8960-1ddf8fbccdf2");

            names.Add("WriterFieldActivity");
            items["WriterFieldActivity".ToLowerInvariant()] = new Guid("0ee21680-b310-4693-9eaa-ac2941778622");

            names.Add("WriterFieldTrip");
            items["WriterFieldTrip".ToLowerInvariant()] = new Guid("f8f98631-4766-4c70-8311-918164759809");

            names.Add("WriterSampleEvent");
            items["WriterSampleEvent".ToLowerInvariant()] = new Guid("9bb48b5d-11ee-45c8-9f99-37b054ea458c");

            names.Add("WriterFieldTeam");
            items["WriterFieldTeam".ToLowerInvariant()] = new Guid("3dfceac0-fe8c-4b93-85a0-9bc40ccfcb40");

            names.Add("WriterSite");
            items["WriterSite".ToLowerInvariant()] = new Guid("ea6df8f0-498a-4523-b810-ec1b283cbfa5");

            names.Add("WriterInstrument");
            items["WriterInstrument".ToLowerInvariant()] = new Guid("c12ce36f-2b24-49e8-aaaa-7c71c086e763");

            names.Add("WriterList");
            items["WriterList".ToLowerInvariant()] = new Guid("b826c11d-3c52-4f08-88b5-85c31fee2cd5");

            names.Add("WriterWQ");
            items["WriterWQ".ToLowerInvariant()] = new Guid("0912552a-58c9-490e-9f1b-1b30cc8d1235");

            names.Add("WriterFish");
            items["WriterFish".ToLowerInvariant()] = new Guid("5a8b7b45-df2f-4f71-a319-cbd11509fbbf");

            names.Add("WriterVeg");
            items["WriterVeg".ToLowerInvariant()] = new Guid("8af27102-de6a-4fd5-853c-71eb8311472b");
        }
    }
}
