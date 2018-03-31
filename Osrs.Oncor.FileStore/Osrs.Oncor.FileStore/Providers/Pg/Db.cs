using Npgsql;
using Osrs.Data;
using Osrs.Data.Postgres;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Osrs.Oncor.FileStore.Providers.Pg
{
    internal static class Db
    {
        internal static string ConnectionString;
        
        internal static string SelectSql = "SELECT \"Id\", \"Created\", \"Updated\", \"Accessed\", \"Expires\", \"IsTmp\", \"FileName\" FROM oncor.\"FileStoreFiles\"";
        internal static string UpdateSql = "UPDATE oncor.\"FileStoreFiles\" SET \"Updated\"=:u, \"Accessed\"=:a, \"Expires\"=:ex, \"IsTmp\"=:tmp, \"FileName\"=:nam WHERE \"Id\"=:id";
        internal static string DeleteBareSql = "DELETE FROM oncor.\"FileStoreFiles\"";
        internal static string DeleteSql = "DELETE FROM oncor.\"FileStoreFiles\" WHERE \"Id\"=:id";
        internal static string InsertSql = "INSERT INTO oncor.\"FileStoreFiles\"(\"Id\", \"Created\", \"Updated\", \"Accessed\", \"Expires\", \"IsTmp\", \"FileName\") VALUES (:id, :c, :u, :a, :ex, :tmp, :nam)";

        internal static FilestoreFile Open(Guid id)
        {
            if (!Guid.Empty.Equals(id))
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectSql + " WHERE \"Id\"=:id";
                cmd.Parameters.AddWithValue("id", id);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                FilestoreFile tmp = null;
                try
                {
                    if (rdr.Read())
                    {
                        DateTime cr = DbReaderUtils.GetDate(rdr, 1);
                        DateTime u = DbReaderUtils.GetDate(rdr, 2);
                        DateTime a = DbReaderUtils.GetDate(rdr, 3);
                        DateTime e = DbReaderUtils.GetDate(rdr, 4);
                        bool t = DbReaderUtils.GetBoolean(rdr, 5);
                        string fn = DbReaderUtils.GetString(rdr, 6);
                        if (DateTime.MinValue == e)
                            e = DateTime.MaxValue;

                        tmp = new FilestoreFile(id, cr, u, a, e, t, fn);
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

                if (DateTime.Now >= tmp.ExpiresAt) //just handle last minute deletes
                {
                    Delete(id);
                    tmp = null;
                }
                return tmp;
            }
            return null;
        }

        internal static List<FilestoreFile> GetExpired(DateTime abs)
        {
            try
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectSql + " WHERE \"Expires\" < :d";
                cmd.Parameters.AddWithValue("d", abs);

                NpgsqlDataReader rdr = ExecuteReader(cmd);
                List<FilestoreFile> items = new List<FilestoreFile>();
                while (rdr.Read())
                {
                    Guid id = DbReaderUtils.GetGuid(rdr, 0);
                    DateTime cr = DbReaderUtils.GetDate(rdr, 1);
                    DateTime u = DbReaderUtils.GetDate(rdr, 2);
                    DateTime a = DbReaderUtils.GetDate(rdr, 3);
                    DateTime e = DbReaderUtils.GetDate(rdr, 4);
                    bool t = DbReaderUtils.GetBoolean(rdr, 5);
                    string fn = DbReaderUtils.GetString(rdr, 6);
                    if (DateTime.MinValue == e)
                        e = DateTime.MaxValue;

                    items.Add(new FilestoreFile(id, cr, u, a, e, t, fn));
                }

                return items;
            }
            catch
            { }

            return null;
        }

        internal static void DeleteExpired(List<FilestoreFile> items)
        {
            if (items!=null && items.Count>0)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (FilestoreFile cur in items)
                    {
                        if (sb.Length > 0)
                            sb.Append(',');
                        sb.Append('\'');
                        sb.Append(cur.FileId.ToString("D"));
                        sb.Append('\'');
                    }

                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.DeleteBareSql + " WHERE \"Id\" IN (" + sb.ToString() + ")";

                    Db.ExecuteNonQuery(cmd);
                }
                catch
                { }
            }
        }

        internal static bool Delete(Guid id)
        {
            if (!Guid.Empty.Equals(id))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.DeleteSql;
                    cmd.Parameters.AddWithValue("id", id);

                    return 0 != Db.ExecuteNonQuery(cmd);
                }
                catch
                { }
            }
            return false;
        }

        internal static bool Insert(Guid id, DateTime created, DateTime lastMod, DateTime lastAccess, DateTime exp, bool isTmp, string fileName)
        {
            if (!Guid.Empty.Equals(id))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertSql;
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("c", created);
                    cmd.Parameters.AddWithValue("u", lastMod);
                    cmd.Parameters.AddWithValue("a", lastAccess);
                    if (exp < DateTime.MaxValue)
                        cmd.Parameters.AddWithValue("ex", exp);
                    else
                        cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam("ex", NpgsqlTypes.NpgsqlDbType.TimestampTZ));
                    cmd.Parameters.AddWithValue("tmp", isTmp);
                    if (fileName!=null && fileName.Length>0)
                        cmd.Parameters.AddWithValue("nam", fileName);
                    else
                        cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam("nam", NpgsqlTypes.NpgsqlDbType.Varchar));

                    return 0 !=Db.ExecuteNonQuery(cmd);
                }
                catch
                { }
            }
            return false;
        }

        internal static bool Update(Guid id, DateTime lastMod, DateTime lastAccess, DateTime exp, bool isTmp, string fileName)
        {
            if (!Guid.Empty.Equals(id))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.UpdateSql;
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("u", lastMod);
                    cmd.Parameters.AddWithValue("a", lastAccess);
                    if (exp < DateTime.MaxValue)
                        cmd.Parameters.AddWithValue("ex", exp);
                    else
                        cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam("ex", NpgsqlTypes.NpgsqlDbType.TimestampTZ));
                    cmd.Parameters.AddWithValue("tmp", isTmp);
                    if (fileName != null && fileName.Length > 0)
                        cmd.Parameters.AddWithValue("nam", fileName);
                    else
                        cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam("nam", NpgsqlTypes.NpgsqlDbType.Varchar));

                    return 0 != Db.ExecuteNonQuery(cmd);
                }
                catch
                { }
            }
            return false;
        }

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
