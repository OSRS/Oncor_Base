using Npgsql;
using Osrs.Data;
using Osrs.Data.Postgres;
using Osrs.Runtime;
using Osrs.Runtime.Configuration;
using Osrs.Runtime.Logging;
using Pnnl.Oncor.DetProcessing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Pnnl.Oncor.DetProcessor
{
    //This class stores the simple list of dets associated with a sample event.
    //In the initial version, it's likely there will only be 1 det per sample event, but this is to allow that to grow.
    //The registry does not (nor should it ever) support multiple dets of the same type in a sample event -
    internal sealed class DetRegistry
    {
        private LogProviderBase logger;
        private string connString;

        //TODO -- update database and queries for isPrivate and bundles as string
        private static readonly string Select = "SELECT \"SampleEventSystemId\", \"SampleEventId\", \"FileTypes\", \"FileIds\", \"BundleIds\", \"Privacies\" FROM oncor.\"SampleEventDETFiles\"";
        private static readonly string Insert = "INSERT INTO oncor.\"SampleEventDETFiles\"(\"SampleEventSystemId\", \"SampleEventId\", \"FileTypes\", \"FileIds\", \"BundleIds\", \"Privacies\") VALUES (:sid, :id, :fts, :fis, :bis, :pri)";
        private static readonly string UpdateSql = "UPDATE oncor.\"SampleEventDETFiles\" SET \"FileTypes\"=:fts, \"FileIds\"=:fis, \"BundleIds\"=:bis, \"Privacies\"=:pri WHERE \"SampleEventSystemId\"=:sid AND \"SampleEventId\"=:id";
        private static readonly string DeleteSql = "DELETE FROM oncor.\"SampleEventDETFiles\" WHERE \"SampleEventSystemId\"=:sid AND \"SampleEventId\"=:id";

        internal bool Exists(CompoundIdentity sampleEventId)
        {
            return Get(sampleEventId) != null;
        }

        internal bool Exists(Guid fileId)
        {
            return Get(fileId) != null;
        }

        internal SampleEventMap Create(CompoundIdentity sampleEventId)
        {
            if (this.connString!=null && sampleEventId!=null && !sampleEventId.IsEmpty && this.Get(sampleEventId)==null)
            {
                try
                {
                    SampleEventMap map = SampleEventMap.Create(sampleEventId);
                    NpgsqlCommand cmd = GetCmd(this.connString);
                    cmd.CommandText = Insert;
                    cmd.Parameters.AddWithValue("sid", map.SampleEventId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("id", map.SampleEventId.Identity);
                    List<Guid> fileIds = new List<Guid>();
                    List<int> fileTypes = new List<int>();
                    List<bool> privacies = new List<bool>();

                    cmd.Parameters.AddWithValue("fts", fileTypes.ToArray());
                    cmd.Parameters.AddWithValue("fis", fileIds.ToArray());
                    cmd.Parameters.AddWithValue("bis", EncodeBundleIds(map));
                    cmd.Parameters.AddWithValue("pri", privacies.ToArray());

                    ExecuteNonQuery(cmd);

                    return map;
                }
                catch
                { }
            }
            return null;
        }

        internal bool Update(SampleEventMap item)
        {
            if (this.connString != null && item != null && item.Count>0 && this.Get(item.SampleEventId) != null) //can't push an empty item
            {
                try
                {
                    NpgsqlCommand cmd = GetCmd(this.connString);
                    cmd.CommandText = UpdateSql;
                    cmd.Parameters.AddWithValue("sid", item.SampleEventId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("id", item.SampleEventId.Identity);
                    List<Guid> fileIds = new List<Guid>();
                    List<int> fileTypes = new List<int>();
                    List<bool> privacies = new List<bool>();
                    foreach(SampleEventMapItem cur in item)
                    {
                        fileIds.Add(cur.DetId);
                        fileTypes.Add((int)cur.DetType);
                        privacies.Add(cur.IsPrivate);
                    }

                    cmd.Parameters.AddWithValue("fts", fileTypes.ToArray());
                    cmd.Parameters.AddWithValue("fis", fileIds.ToArray());
                    cmd.Parameters.AddWithValue("bis", EncodeBundleIds(item));
                    cmd.Parameters.AddWithValue("pri", privacies.ToArray());

                    ExecuteNonQuery(cmd);

                    return true;
                }
                catch
                { }
            }
            return false;
        }

        //format:    int:guid,guid,...|int:guid,guid,...
        internal void DecodeBundleIds(string item, SampleEventMap map)
        {
            if (!string.IsNullOrEmpty(item))
            {
                try
                {
                    string[] blocks = item.Split('|');
                    foreach (string cur in blocks)
                    {
                        int idx = cur.IndexOf(':');
                        KnownDetType tp = (KnownDetType)int.Parse(cur.Substring(0, idx));
                        string[] ids = cur.Substring(idx + 1).Split(',');
                        List<Guid> parts = map.GetBundles(tp);
                        foreach (string id in ids)
                        {
                            parts.Add(Guid.Parse(id));
                        }
                    }
                }
                catch { }
            }
        }

        internal string EncodeBundleIds(SampleEventMap map)
        {
            if (map != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach(SampleEventMapItem cur in map)
                {
                    sb.Append((int)(cur.DetType));
                    sb.Append(':');
                    foreach(Guid cc in cur.BundleIds)
                    {
                        sb.Append(cc.ToString());
                        sb.Append(',');
                    }
                    sb.Length = sb.Length - 1;
                    sb.Append('|');
                }
                if (sb.Length > 0)
                    sb.Length = sb.Length - 1; //trailing |
                return sb.ToString();
            }
            return null;
        }

        internal SampleEventMap Get(Guid fileId)
        {
            if (this.connString != null && !Guid.Empty.Equals(fileId))
            {
                NpgsqlCommand cmd = GetCmd(connString);
                cmd.CommandText = Select + " WHERE :id = ANY(\"FileIds\")";
                cmd.Parameters.AddWithValue("id", fileId);
                NpgsqlDataReader rdr = ExecuteReader(cmd);
                try
                {
                    if (rdr.Read())
                    {
                        CompoundIdentity id = new CompoundIdentity((Guid)rdr[0], (Guid)rdr[1]);
                        int[] fileTypes = (int[])rdr[2];
                        Guid[] fileIds = (Guid[])rdr[3];
                        bool[] privacies = (bool[])rdr[5];
                        SampleEventMap tmp = SampleEventMap.Create(id);
                        for (int i = 0; i < fileTypes.Length; i++)
                        {
                            tmp.Add(fileIds[i], (KnownDetType)fileTypes[i], privacies[i]);
                        }
                        if (!DBNull.Value.Equals(rdr[4]))
                            DecodeBundleIds((string)rdr[4], tmp);

                        return tmp;
                    }
                }
                catch
                { }
                finally
                {
                    cmd.Dispose();
                }
            }
            return null;
        }

        internal SampleEventMap Get(CompoundIdentity sampleEventId)
        {
            if (this.connString != null && sampleEventId != null && !sampleEventId.IsEmpty)
            {
                NpgsqlCommand cmd = GetCmd(connString);
                cmd.CommandText = Select + " WHERE \"SampleEventSystemId\"=:sid AND \"SampleEventId\"=:id";
                cmd.Parameters.AddWithValue("sid", sampleEventId.DataStoreIdentity);
                cmd.Parameters.AddWithValue("id", sampleEventId.Identity);
                NpgsqlDataReader rdr = ExecuteReader(cmd);
                try
                {
                    if (rdr.Read())
                    {
                        int[] fileTypes = (int[])rdr[2];
                        Guid[] fileIds = (Guid[])rdr[3];
                        bool[] privacies = (bool[])rdr[5];
                        SampleEventMap tmp = SampleEventMap.Create(sampleEventId);
                        for(int i = 0; i < fileTypes.Length; i++)
                        {
                            tmp.Add(fileIds[i], (KnownDetType)fileTypes[i], privacies[i]);
                        }
                        if (!DBNull.Value.Equals(rdr[4]))
                            DecodeBundleIds((string)rdr[4], tmp);

                        return tmp;
                    }
                }
                catch
                { }
                finally
                {
                    cmd.Dispose();
                }
            }
            return null;
        }

        internal bool Delete(CompoundIdentity sampleEventId)
        {
            if (this.connString != null && sampleEventId != null && !sampleEventId.IsEmpty)
            {
                try
                {
                    NpgsqlCommand cmd = GetCmd(this.connString);
                    cmd.CommandText = DeleteSql;
                    cmd.Parameters.AddWithValue("sid", sampleEventId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("id", sampleEventId.Identity);

                    ExecuteNonQuery(cmd);

                    return true;
                }
                catch
                { }
            }
            return false;
        }

        private static int ExecuteNonQuery(NpgsqlCommand cmd)
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

        private static NpgsqlCommand GetCmd(string conString)
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

        private static NpgsqlConnection GetCon(string conString)
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

        private static NpgsqlDataReader ExecuteReader(NpgsqlCommand cmd)
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

        internal bool Init()
        {
            string meth = "Initialize";
            this.logger = LogManager.Instance.GetProvider(typeof(DetRegistry));
            Log(meth, LogLevel.Info, "Called");

            ConfigurationProviderBase config = ConfigurationManager.Instance.GetProvider();
            if (config != null)
            {
                ConfigurationParameter param = config.Get(typeof(DetRegistry), "connectionString");
                if (param != null)
                {
                    string tName = param.Value as string;
                    if (!string.IsNullOrEmpty(tName))
                    {
                        if (NpgSqlCommandUtils.TestConnection(tName))
                        {
                            this.connString = tName;
                            Log(meth, LogLevel.Info, "Succeeded");
                            return true;
                        }
                    }
                    else
                        Log(meth, LogLevel.Error, "Failed to get connectionString param value");
                }
                else
                    Log(meth, LogLevel.Error, "Failed to get connectionString param");
            }
            else
                Log(meth, LogLevel.Error, "Failed to get ConfigurationProvider");
            return false;
        }

        private void Log(string method, LogLevel level, string message)
        {
            if (this.logger != null)
                this.logger.Log(method, LogLevel.Info, message);
        }

        private DetRegistry()
        {
            SingletonHelper<DetRegistry> help = new SingletonHelper<DetRegistry>();
            help.Construct(this);
        }

        private static DetRegistry instance = new DetRegistry();
        public static DetRegistry Instance
        {
            get { return instance; }
        }
    }
}
