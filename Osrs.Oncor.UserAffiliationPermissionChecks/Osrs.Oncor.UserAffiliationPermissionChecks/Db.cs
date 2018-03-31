using Npgsql;
using System.Data;

namespace Osrs.Oncor.UserAffiliationPermissionChecks
{
    internal static class Db
    {
        internal static string ConnectionString;
        internal static string SelectSampleEventLocal = "SELECT \"PrincipalOrgSystemId\", \"PrincipalOrgId\" FROM oncor.\"SamplingEvents\" WHERE \"SystemId\"=:sid AND \"Id\"=:id";
        internal static string SelectSampleEventProjectAndOrgs = "SELECT \"FieldActivities\".\"ProjectSystemId\", \"FieldActivities\".\"ProjectId\", \"FieldActivities\".\"PrincipalOrgSystemId\" AS \"FaOSId\", \"FieldActivities\".\"PrincipalOrgId\" AS \"FaOId\", \"FieldTrips\".\"PrincipalOrgSystemId\" AS \"FtOSId\", \"FieldTrips\".\"PrincipalOrgId\" AS \"FtOId\", \"SamplingEvents\".\"PrincipalOrgSystemId\" AS \"SeOSId\", \"SamplingEvents\".\"PrincipalOrgId\" AS \"SeOId\", \"SamplingEvents\".\"SystemId\", \"SamplingEvents\".\"Id\" FROM oncor.\"FieldActivities\", oncor.\"FieldTrips\", oncor.\"SamplingEvents\" WHERE \"FieldActivities\".\"SystemId\" = \"FieldTrips\".\"FieldActivitySystemId\" AND \"FieldActivities\".\"Id\" = \"FieldTrips\".\"FieldActivityId\" AND \"FieldTrips\".\"SystemId\" = \"SamplingEvents\".\"FieldTripSystemId\" AND \"FieldTrips\".\"Id\" = \"SamplingEvents\".\"FieldTripId\" AND \"SamplingEvents\".\"SystemId\"=:sid AND \"SamplingEvents\".\"Id\"=:id";
        internal const string SelectProj = "SELECT \"OrgSystemId\", \"OrgId\", \"ParentSystemId\", \"ParentId\" FROM oncor.\"Projects\" WHERE \"SystemId\"=:sid AND \"Id\"=:id";
        internal const string SelectAffil = "SELECT \"OrgSystemId\", \"OrgId\" FROM oncor.\"ProjectAffiliates\" WHERE \"SystemId\"=:sid AND \"Id\"=:id";

        internal const string SelectUserAffil = "SELECT \"OrgSystemId\", \"OrgId\" FROM oncor.\"UserAffiliations\" WHERE \"UserId\"=:uid";

        internal static NpgsqlConnection GetCon(string conString)
        {
            try
            {
                NpgsqlConnectionStringBuilder sb = new NpgsqlConnectionStringBuilder(conString);
                if (sb.Timeout == 15) //default
                    sb.Timeout = 60;
                if (sb.CommandTimeout == 30) //default
                    sb.CommandTimeout = 60;
                sb.Pooling = false;
                NpgsqlConnection conn = new NpgsqlConnection(sb.ToString());
                return conn;
            }
            catch
            { }
            return null;
        }

        internal static NpgsqlCommand GetCmd(NpgsqlConnection con)
        {
            if (con == null)
                return null;
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand();
                if (cmd != null)
                {
                    cmd.Connection = con;
                    return cmd;
                }
            }
            catch
            { }
            return null;
        }

        internal static NpgsqlCommand GetCmd(string conString)
        {
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand();
                cmd.Connection = GetCon(conString);
                return cmd;
            }
            catch
            { }
            return null;
        }

        internal static int ExecuteNonQuery(NpgsqlCommand cmd)
        {
            int res = int.MinValue;
            try
            {
                if (cmd.Connection.State != ConnectionState.Open)
                    cmd.Connection.Open();
                res = cmd.ExecuteNonQuery();
            }
            catch
            { }

            try
            {
                if (cmd.Connection.State == ConnectionState.Open)
                    cmd.Connection.Close();
            }
            catch
            { }

            return res;
        }

        internal static NpgsqlDataReader ExecuteReader(NpgsqlCommand cmd)
        {
            try
            {
                if (cmd.Connection.State != ConnectionState.Open)
                    cmd.Connection.Open();
                return cmd.ExecuteReader();
            }
            catch
            { }
            return null;
        }

        internal static void Close(NpgsqlConnection con)
        {
            try
            {
                if (con != null && con.State == ConnectionState.Open)
                    con.Close();
            }
            catch
            { }
        }

        internal static void Close(NpgsqlCommand cmd)
        {
            if (cmd != null && cmd.Connection != null)
                Close(cmd.Connection);
        }

        internal static bool Exists(NpgsqlCommand cmd)
        {
            NpgsqlDataReader rdr = null;
            try
            {
                if (cmd.Connection.State != ConnectionState.Open)
                    cmd.Connection.Open();
                rdr = ExecuteReader(cmd);
                rdr.Read();

                try
                {
                    long ct = (long)(rdr[0]);
                    if (cmd.Connection.State == System.Data.ConnectionState.Open)
                        cmd.Connection.Close();

                    return ct > 0L;
                }
                catch
                { }
            }
            catch
            {
                Close(cmd);
            }
            finally
            {
                cmd.Dispose();
            }
            return false;
        }
    }
}
