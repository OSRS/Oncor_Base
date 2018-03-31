//Copyright 2017 Open Science, Engineering, Research and Development Information Systems Open, LLC. (OSRS Open)
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//       http://www.apache.org/licenses/LICENSE-2.0
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Npgsql;
using Osrs.Data;
using Osrs.Oncor.UserAffiliationPermissionChecks;
using Osrs.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Osrs.Oncor.WellKnown.WaterQuality.Providers.Pg
{
    internal static class Db
    {
        internal static string ConnectionString;
        internal static readonly Guid DataStoreIdentity = new Guid("{9D4B5A7E-8A34-4E2C-9119-6D4DCECF0B6C}");

        internal const string Where = " WHERE ";
        internal const string And = " AND ";
        internal const string WhereId = "\"SystemId\"=:sid AND \"Id\"=:id";
        internal const string WhereEvent = "\"EventSystemId\"=:esid AND \"EventId\"=:eid";
        internal const string WhereSite = "\"SiteSystemId\"=:sitesid AND \"SiteId\"=:siteid";
        internal const string WhereDeploy = "\"DeploySystemId\"=:dsid AND \"DeployId\"=:did";
        internal const string WhereStart = "\"StartDate\">=:start";
        internal const string WhereEnd = "\"EndDate\"<=:end";


        internal const string CountDeployment = "SELECT COUNT(*) FROM oncor.\"WQDeployments\"";
        internal const string CountMeasurement = "SELECT COUNT(*) FROM oncor.\"WQMeasurements\"";

        internal const string SelectDeployment = "SELECT \"SystemId\", \"Id\", \"Name\", \"EventSystemId\", \"EventId\", \"SiteSystemId\", \"SiteId\", \"InstrumentSystemId\", \"InstrumentId\", \"StartDate\", \"EndDate\", \"Description\", \"IsPrivate\" FROM oncor.\"WQDeployments\"";
        internal const string SelectMeasurement = "SELECT \"DeploySystemId\", \"DeployId\", \"SampleDate\", \"SurfElev\", \"Temperature\", \"pH\", \"DissOxy\", \"Cond\", \"Sal\", \"Velocity\" FROM oncor.\"WQMeasurements\"";

        internal const string UpdateDeployment = "UPDATE oncor.\"WQDeployments\" SET \"Name\"=:n, \"EventSystemId\"=:esid, \"EventId\"=:eid, \"SiteSystemId\"=:sitesid, \"SiteId\"=:siteid, \"InstrumentSystemId\"=:isid, \"InstrumentId\"=:iid, \"StartDate\"=:start, \"EndDate\"=:end, \"Description\"=:d , \"IsPrivate\"=:priv WHERE \"SystemId\"=:sid AND \"Id\"=:id";

        internal const string InsertDeployment = "INSERT INTO oncor.\"WQDeployments\"(\"SystemId\", \"Id\", \"Name\", \"EventSystemId\", \"EventId\", \"SiteSystemId\", \"SiteId\", \"InstrumentSystemId\", \"InstrumentId\", \"StartDate\", \"EndDate\", \"Description\", \"IsPrivate\") VALUES (:sid, :id, :n, :esid, :eid, :sitesid, :siteid, :isid, :iid, :start, :end, :d, :priv)";
        internal const string InsertMeasurement = "INSERT INTO oncor.\"WQMeasurements\"(\"DeploySystemId\", \"DeployId\", \"SampleDate\", \"SurfElev\", \"Temperature\", \"pH\", \"DissOxy\", \"Cond\", \"Sal\", \"Velocity\") VALUES (:dsid, :did, :when, :elev, :tpt, :ph, :doxy, :cond, :sal, :vel)";
        internal const string InsertMeasurementB = "INSERT INTO oncor.\"WQMeasurements\"(\"DeploySystemId\", \"DeployId\", \"SampleDate\", \"SurfElev\", \"Temperature\", \"pH\", \"DissOxy\", \"Cond\", \"Sal\", \"Velocity\") VALUES ";

        internal const string DeleteDeployment = "DELETE FROM oncor.\"WQDeployments\" WHERE \"SystemId\"=:sid AND \"Id\"=:id";
        internal const string DeleteMeasurement = "DELETE FROM oncor.\"WQMeasurements\" WHERE \"DeploySystemId\"=:dsid AND \"DeployId\"=:did";

        internal static DateTime FixDate(DateTime item)
        {
            return WQUtils.FixDate(item);
        }

        internal static DateTime? FixDate(DateTime? item)
        {
            if (item.HasValue)
                return FixDate(item.Value);
            return null;
        }

        internal static string GetWhere(string name, StringComparison comparisonOption)
        {
            if (comparisonOption == StringComparison.CurrentCultureIgnoreCase || comparisonOption == StringComparison.OrdinalIgnoreCase)
                return " lower(\"Name\")=lower(:name)";
            else
                return " \"Name\"=:name";
        }

        internal static string GetWhere(IEnumerable<CompoundIdentity> ids, string dsName, string idName)
        {
            Dictionary<Guid, HashSet<Guid>> systemIds = new Dictionary<Guid, HashSet<Guid>>();
            HashSet<Guid> oids;
            foreach (CompoundIdentity cur in ids)
            {
                if (!cur.IsNullOrEmpty())
                {
                    if (systemIds.ContainsKey(cur.DataStoreIdentity))
                        oids = systemIds[cur.DataStoreIdentity];
                    else
                    {
                        oids = new HashSet<Guid>();
                        systemIds[cur.DataStoreIdentity] = oids;
                    }
                    oids.Add(cur.Identity);
                }
            }

            if (systemIds.Count > 0)
            {
                StringBuilder where = new StringBuilder();
                where.Append(' ');

                foreach (KeyValuePair<Guid, HashSet<Guid>> cur in systemIds)
                {
                    oids = cur.Value;
                    if (oids.Count > 0)
                    {
                        if (systemIds.Count > 1)
                            where.Append('(');
                        where.Append(dsName);
                        where.Append("='");
                        where.Append(cur.Key.ToString());
                        where.Append("' AND ");
                        where.Append(idName);
                        where.Append(" IN (");
                        foreach (Guid cid in oids)
                        {
                            where.Append('\'');
                            where.Append(cid.ToString());
                            where.Append("',");
                        }
                        where[where.Length - 1] = ')';
                        if (systemIds.Count > 1)
                            where.Append(") OR (");
                    }
                }
                if (where[where.Length - 1] == '(') //need to lop off the " OR ("
                    where.Length = where.Length - 5;
                return where.ToString();
            }
            return string.Empty;
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

    //Field Order:  \"SystemId\", \"Id\", \"Name\", \"EventSystemId\", \"EventId\", \"SiteSystemId\", \"SiteId\", \"InstrumentSystemId\", \"InstrumentId\", \"StartDate\", \"EndDate\", \"Description\"
    internal sealed class WQDeploymentBuilder : IBuilder<WaterQualityDeployment>
    {
        private readonly UserSecurityContext context;
        private readonly UserProvider prov;
        internal WQDeploymentBuilder(UserSecurityContext ctx)
        {
            this.context = ctx;
            this.prov = UserAffilationSecurityManager.Instance.GetProvider(this.context);
        }

        public WaterQualityDeployment Build(DbDataReader reader)
        {
            WaterQualityDeployment tmp = new WaterQualityDeployment(new CompoundIdentity(DbReaderUtils.GetGuid(reader, 0), DbReaderUtils.GetGuid(reader, 1)), DbReaderUtils.GetString(reader, 2), 
                new CompoundIdentity(DbReaderUtils.GetGuid(reader, 3), DbReaderUtils.GetGuid(reader, 4)), new CompoundIdentity(DbReaderUtils.GetGuid(reader, 5), DbReaderUtils.GetGuid(reader, 6)),
                new CompoundIdentity(DbReaderUtils.GetGuid(reader, 7), DbReaderUtils.GetGuid(reader, 8)), DateRange.Create(Db.FixDate(DbReaderUtils.GetNullableDateTime(reader, 9)), Db.FixDate(DbReaderUtils.GetNullableDateTime(reader, 10))),
                DbReaderUtils.GetString(reader, 11), DbReaderUtils.GetBoolean(reader, 12));
            if (tmp.IsPrivate)
            {
                if (prov == null)
                    return null; //can't verify
                if (!prov.HasAffiliationForSampleEvent(tmp.SampleEventId, false))
                    return null;
            }

            return tmp;
        }
    }

    //Field Order:  \"DeploySystemId\", \"DeployId\", \"SampleDate\", \"SurfElev\", \"Temperature\", \"pH\", \"DissOxy\", \"Cond\", \"Sal\", \"Velocity\"
    internal sealed class WQMeasurementBuilder : IBuilder<WaterQualityMeasurement>
    {
        private readonly PgWQDeploymentProvider helperBuilder;
        private readonly HashSet<CompoundIdentity> seen = new HashSet<CompoundIdentity>();
        private readonly HashSet<CompoundIdentity> got = new HashSet<CompoundIdentity>();

        internal WQMeasurementBuilder(UserSecurityContext ctx)
        {
            this.helperBuilder = new PgWQDeploymentProvider(ctx);
        }

        public WaterQualityMeasurement Build(DbDataReader reader)
        {
            WaterQualityMeasurement tmp = new WaterQualityMeasurement(new CompoundIdentity(DbReaderUtils.GetGuid(reader, 0), DbReaderUtils.GetGuid(reader, 1)), Db.FixDate(DbReaderUtils.GetDate(reader, 2)), 
                DbReaderUtils.GetNullableDouble(reader, 3), DbReaderUtils.GetNullableDouble(reader, 4), DbReaderUtils.GetNullableDouble(reader, 5), DbReaderUtils.GetNullableDouble(reader, 6), 
            DbReaderUtils.GetNullableDouble(reader, 7), DbReaderUtils.GetNullableDouble(reader, 8), DbReaderUtils.GetNullableDouble(reader, 9));

            if (!seen.Contains(tmp.DeploymentId))
            {
                seen.Add(tmp.DeploymentId);
                WaterQualityDeployment depl = this.helperBuilder.Get(tmp.DeploymentId);
                if (depl != null)
                    got.Add(tmp.DeploymentId);
            }

            if (!got.Contains(tmp.DeploymentId))
                return null;

            return tmp;
        }
    }
}
