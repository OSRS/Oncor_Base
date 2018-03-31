using Npgsql;
using Osrs.Data;
using System.Data;
using System.Data.Common;

namespace Osrs.Oncor.EntityBundles
{
    internal static class Db
    {
        internal static string ConnectionString;

        internal const string SelectBundle = "SELECT \"Id\", \"OwnerSystemId\", \"OwnerId\", \"Name\", \"DataType\" FROM oncor.\"EntityBundles\"";
        internal const string SelectElement = "SELECT \"BundleId\", \"EntitySystemId\", \"EntityId\", \"KeyName\", \"DisplayName\" FROM oncor.\"EntityBundleElements\" WHERE \"BundleId\"=:id";

        internal const string SelectById = " WHERE \"Id\"=:id";
        internal const string SelectByPrinId = " WHERE \"OwnerSystemId\"=:osid AND \"OwnerId\"=:oid";

        internal const string InsertBundle = "INSERT INTO oncor.\"EntityBundles\"(\"Id\", \"OwnerSystemId\", \"OwnerId\", \"Name\", \"DataType\") VALUES (:id, :osid, :oid, :name, :dt)";
        internal const string InsertElement = "INSERT INTO oncor.\"EntityBundleElements\"(\"BundleId\", \"EntitySystemId\", \"EntityId\", \"KeyName\", \"DisplayName\") VALUES (:id, :esid, :eid, :name, :val)";

        internal const string UpdateBundle = "UPDATE oncor.\"EntityBundles\" SET \"Name\"=:name, \"OwnerSystemId\"=:osid, \"OwnerId\"=:oid WHERE \"Id\"=:id";

        internal const string DeleteBundle = "DELETE FROM oncor.\"EntityBundles\"";
        internal const string DeleteElement = "DELETE FROM oncor.\"EntityBundleElements\" WHERE \"BundleId\"=:id";

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

    internal sealed class EntityBundleBuilder : IBuilder<EntityBundle>
    {
        internal static readonly EntityBundleBuilder Instance = new EntityBundleBuilder();

        //"Id", "OwnerSystemId", "OwnerId", "Name", "DataType"
        public EntityBundle Build(DbDataReader reader)
        {
            EntityBundle tmp = new EntityBundle(DbReaderUtils.GetGuid(reader, 0), DbReaderUtils.GetString(reader, 3),
                new CompoundIdentity(DbReaderUtils.GetGuid(reader, 1), DbReaderUtils.GetGuid(reader, 2)), (BundleDataType)((int)(reader[4])));
            Fill(tmp);
            return tmp;
        }

        private void Fill(EntityBundle item)
        {
            NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
            cmd.CommandText = Db.SelectElement;
            cmd.Parameters.AddWithValue("id", item.Id);
            NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
            BundleElement o = null;
            if (rdr != null)
            {
                try
                {
                    while (rdr.Read())
                    {
                        o = EntityBundleElementBuilder.Instance.Build(rdr);
                        if (o!=null)
                            item.elements.Add(o.EntityId, o);
                    }

                    if (cmd.Connection.State == System.Data.ConnectionState.Open)
                        cmd.Connection.Close();
                }
                catch
                { }
                finally
                {
                    cmd.Dispose();
                }
            }
        }
    }

    internal sealed class EntityBundleElementBuilder : IBuilder<BundleElement>
    {
        internal static readonly EntityBundleElementBuilder Instance = new EntityBundleElementBuilder();

        //"BundleId", "EntitySystemId", "EntityId", "KeyName", "DisplayName"
        public BundleElement Build(DbDataReader reader)
        {
            return new BundleElement(DbReaderUtils.GetGuid(reader, 0), new CompoundIdentity(DbReaderUtils.GetGuid(reader, 1), DbReaderUtils.GetGuid(reader, 2)), DbReaderUtils.GetString(reader, 3), DbReaderUtils.GetString(reader, 4));
        }
    }
}
