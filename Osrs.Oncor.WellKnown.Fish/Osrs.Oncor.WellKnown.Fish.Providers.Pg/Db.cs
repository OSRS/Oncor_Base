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
using Osrs.Data.Postgres;
using Osrs.Numerics.Spatial.Geometry;
using Osrs.Oncor.UserAffiliationPermissionChecks;
using Osrs.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Text;

namespace Osrs.Oncor.WellKnown.Fish.Providers.Pg
{
    internal static class Db
    {
        internal static string ConnectionString;
        internal static readonly Guid DataStoreIdentity = new Guid("{FC06C3B8-B380-4CD8-A418-18A1272B030D}");

        internal const string Where = " WHERE ";
        internal const string And = " AND ";
        internal const string WhereCId = "\"SystemId\"=:sid AND \"Id\"=:id";
        internal const string WhereId = "\"Id\"=:id";
        internal const string WhereFishId = "\"FishId\"=:fid";
        internal const string WhereEvent = "\"EventSystemId\"=:esid AND \"EventId\"=:eid";
        internal const string WhereEffort = "\"CatchEffortId\"=:ceid";
        internal const string WhereEffortIn = "\"FishId\" IN (SELECT \"Id\" FROM oncor.\"FishIndividual\" WHERE \"CatchEffortId\"=:ceid)";
        internal const string WhereSite = "\"SiteSystemId\"=:sitesid AND \"SiteId\"=:siteid";
        internal const string WhereTaxa = "\"TaxaSystemId\"=:taxasid AND \"TaxaId\"=:taxaid";
        internal const string WhereStart = "\"When\">=:start";
        internal const string WhereEnd = "\"When\"<=:end";


        internal const string CountFishCatchEfforts = "SELECT COUNT(*) FROM oncor.\"FishCatchEfforts\"";
        internal const string CountFishCatchMetrics = "SELECT COUNT(*) FROM oncor.\"FishCatchMetrics\"";
        internal const string CountFishCount = "SELECT COUNT(*) FROM oncor.\"FishCount\"";
        internal const string CountFishDiet = "SELECT COUNT(*) FROM oncor.\"FishDiet\"";
        internal const string CountFishGenetics = "SELECT COUNT(*) FROM oncor.\"FishGenetics\"";
        internal const string CountFishIdTag = "SELECT COUNT(*) FROM oncor.\"FishIdTag\"";
        internal const string CountFishIndividual = "SELECT COUNT(*) FROM oncor.\"FishIndividual\"";
        internal const string CountFishNetHaulEvent = "SELECT COUNT(*) FROM oncor.\"FishNetHaulEvent\"";

        internal const string SelectFishCatchEfforts = "SELECT \"Id\", \"EventSystemId\", \"EventId\", \"SiteSystemId\", \"SiteId\", \"When\", \"PointLocation\", \"Method\", \"Strata\", \"Depth\", \"pH\", \"Temp\", \"DO\", \"Salinity\", \"Velocity\", \"Description\", \"IsPrivate\" FROM oncor.\"FishCatchEfforts\"";
        internal const string SelectFishCatchMetrics = "SELECT \"Id\", \"CatchEffortId\", \"MetricType\", \"Value\", \"Description\" FROM oncor.\"FishCatchMetrics\"";
        internal const string SelectFishCount = "SELECT \"Id\", \"CatchEffortId\", \"TaxaSystemId\", \"TaxaId\", \"Count\", \"Description\" FROM oncor.\"FishCount\"";
        internal const string SelectFishDiet = "SELECT \"Id\", \"FishId\", \"TaxaSystemId\", \"TaxaId\", \"VialId\", \"GutSampleId\", \"Lifestage\", \"Count\", \"WholeAnimalsWeighed\", \"IndividualMass\", \"SampleMass\", \"Description\" FROM oncor.\"FishDiet\"";
        internal const string SelectFishGenetics = "SELECT \"Id\", \"FishId\", \"GeneticSampleId\", \"LabSampleId\", \"StockEstimates\", \"Description\" FROM oncor.\"FishGenetics\"";
        internal const string SelectFishIdTag = "SELECT \"Id\", \"FishId\", \"TagCode\", \"TagType\", \"TagManufacturer\", \"Description\" FROM oncor.\"FishIdTag\"";
        internal const string SelectFishIndividual = "SELECT \"Id\", \"CatchEffortId\", \"TaxaSystemId\", \"TaxaId\", \"LengthStandard\", \"LengthFork\", \"LengthTotal\", \"Weight\", \"AdClipped\", \"CWT\", \"Description\" FROM oncor.\"FishIndividual\"";
        internal const string SelectFishNetHaulEvent = "SELECT \"Id\", \"CatchEffortId\", \"NetSystemId\", \"NetId\", \"AreaSampled\", \"VolumeSampled\", \"Description\" FROM oncor.\"FishNetHaulEvent\"";

        internal const string UpdateFishCatchEfforts = "UPDATE oncor.\"FishCatchEfforts\" SET \"EventSystemId\"=:esid, \"EventId\"=:eid, \"SiteSystemId\"=:ssid, \"SiteId\"=:sid, \"When\"=:when, \"PointLocation\"=:loc, \"Method\"=:method, \"Strata\"=:strata, \"Depth\"=:depth, \"pH\"=:ph, \"Temp\"=:tmp, \"DO\"=:dox, \"Salinity\"=:sal, \"Velocity\"=:vel, \"Description\"=:d, \"IsPrivate\"=:priv WHERE \"Id\"=:id";
        internal const string UpdateFishCatchMetrics = "UPDATE oncor.\"FishCatchMetrics\" SET \"CatchEffortId\"=:ceid, \"MetricType\"=:metric, \"Value\"=:val, \"Description\"=:d WHERE \"Id\"=:id";
        internal const string UpdateFishCount = "UPDATE oncor.\"FishCount\" SET \"CatchEffortId\"=:ceid, \"TaxaSystemId\"=:tsid, \"TaxaId\"=:tid, \"Count\"=:ct, \"Description\"=:d WHERE \"Id\"=:id";
        internal const string UpdateFishDiet = "UPDATE oncor.\"FishDiet\" SET \"FishId\":fid, \"TaxaSystemId\"=:tsid, \"TaxaId\"=:tid, \"VialId\"=:vial, \"GutSampleId\"=:gut, \"Lifestage\"=:life, \"Count\"=:ct, \"WholeAnimalsWeighed\"=:whole, \"IndividualMass\"=:ind, \"SampleMass\"=:sam, \"Description\"=:d WHERE \"Id\"=:id";
        internal const string UpdateFishGenetics = "UPDATE oncor.\"FishGenetics\" SET \"FishId\"=:fid, \"GeneticSampleId\"=:gen, \"LabSampleId\"=:lab, \"StockEstimates\"=:stock, \"Description\"=:d WHERE \"Id\"=:id";
        internal const string UpdateFishIdTag = "UPDATE oncor.\"FishIdTag\" SET \"FishId\"=:fid, \"TagCode\"=:tag, \"TagType\"=:typ, \"TagManufacturer\"=:man, \"Description\"=:d WHERE \"Id\"=:id";
        internal const string UpdateFishIndividual = "UPDATE oncor.\"FishIndividual\" SET \"CatchEffortId\"=:ceid, \"TaxaSystemId\"=:tsid, \"TaxaId\"=:tid, \"LengthStandard\"=:std, \"LengthFork\"=:fork, \"LengthTotal\"=:tot, \"Weight\"=:wei, \"AdClipped\"=:ad, \"CWT\"=:cwt, \"Description\"=:d WHERE \"Id\"=:id";
        internal const string UpdateFishNetHaulEvent = "UPDATE oncor.\"FishNetHaulEvent\" SET \"CatchEffortId\"=:ceid, \"NetSystemId\"=:nsid, \"NetId\"=:nid, \"AreaSampled\"=:area, \"VolumeSampled\"=:vol, \"Description\"=:d WHERE \"Id\"=:id";

        internal const string InsertFishCatchEfforts = "INSERT INTO oncor.\"FishCatchEfforts\"(\"Id\", \"EventSystemId\", \"EventId\", \"SiteSystemId\", \"SiteId\", \"When\", \"PointLocation\", \"Method\", \"Strata\", \"Depth\", \"pH\", \"Temp\", \"DO\", \"Salinity\", \"Velocity\", \"Description\", \"IsPrivate\") VALUES (:id, :esid, :eid, :ssid, :sid, :when, :loc, :method, :strata, :depth, :ph, :tmp, :dox, :sal, :vel, :d, :priv)";
        internal const string InsertFishCatchMetrics = "INSERT INTO oncor.\"FishCatchMetrics\"(\"Id\", \"CatchEffortId\", \"MetricType\", \"Value\", \"Description\") VALUES (:id, :ceid, :metric, :val, :d)";
        internal const string InsertFishCount = "INSERT INTO oncor.\"FishCount\"(\"Id\", \"CatchEffortId\", \"TaxaSystemId\", \"TaxaId\", \"Count\", \"Description\") VALUES (:id, :ceid, :tsid, :tid, :ct, :d)";
        internal const string InsertFishDiet = "INSERT INTO oncor.\"FishDiet\"(\"Id\", \"FishId\", \"TaxaSystemId\", \"TaxaId\", \"VialId\", \"GutSampleId\", \"Lifestage\", \"Count\", \"WholeAnimalsWeighed\", \"IndividualMass\", \"SampleMass\", \"Description\") VALUES(:id, :fid, :tsid, :tid, :vial, :gut, :life, :ct, :whole, :ind, :sam, :d)";
        internal const string InsertFishGenetics = "INSERT INTO oncor.\"FishGenetics\"(\"Id\", \"FishId\", \"GeneticSampleId\", \"LabSampleId\", \"StockEstimates\", \"Description\") VALUES (:id, :fid, :gen, :lab, :stock, :d)";
        internal const string InsertFishIdTag = "INSERT INTO oncor.\"FishIdTag\"(\"Id\", \"FishId\", \"TagCode\", \"TagType\", \"TagManufacturer\", \"Description\") VALUES (:id, :fid, :tag, :typ, :man, :d)";
        internal const string InsertFishIndividual = "INSERT INTO oncor.\"FishIndividual\"(\"Id\", \"CatchEffortId\", \"TaxaSystemId\", \"TaxaId\", \"LengthStandard\", \"LengthFork\", \"LengthTotal\", \"Weight\", \"AdClipped\", \"CWT\", \"Description\") VALUES (:id, :ceid, :tsid, :tid, :std, :fork, :tot, :wei, :ad, :cwt, :d)";
        internal const string InsertFishNetHaulEvent = "INSERT INTO oncor.\"FishNetHaulEvent\"(\"Id\", \"CatchEffortId\", \"NetSystemId\", \"NetId\", \"AreaSampled\", \"VolumeSampled\", \"Description\") VALUES (:id, :ceid, :nsid, :nid, :area, :vol, :d)";

        internal const string DeleteFishCatchEfforts = "DELETE FROM oncor.\"FishCatchEfforts\" WHERE \"Id\"=:id";
        internal const string DeleteFishCatchMetrics = "DELETE FROM oncor.\"FishCatchMetrics\" WHERE ";
        internal const string DeleteFishCount = "DELETE FROM oncor.\"FishCount\" WHERE ";
        internal const string DeleteFishDiet = "DELETE FROM oncor.\"FishDiet\" WHERE ";
        internal const string DeleteFishGenetics = "DELETE FROM oncor.\"FishGenetics\" WHERE ";
        internal const string DeleteFishIdTag = "DELETE FROM oncor.\"FishIdTag\" WHERE ";
        internal const string DeleteFishIndividual = "DELETE FROM oncor.\"FishIndividual\" WHERE ";
        internal const string DeleteFishNetHaulEvent = "DELETE FROM oncor.\"FishNetHaulEvent\" WHERE ";


        internal static DateTime FixDate(DateTime when)
        {
            if (when.Kind == DateTimeKind.Local)
                when = when.ToUniversalTime();
            if (when.Kind == DateTimeKind.Unspecified)
                when = new DateTime(when.Ticks, DateTimeKind.Utc); //assume UTC

            if (when < FishUtils.GlobalMinDate)
                return new DateTime(FishUtils.GlobalMinDate.Ticks, DateTimeKind.Utc);
            DateTime now = DateTime.UtcNow;
            if (now.Kind == DateTimeKind.Unspecified)
                now = new DateTime(now.Ticks, DateTimeKind.Utc);
            if (when > now)
                return now;
            return when;
        }

        internal static void AddParam(NpgsqlCommand cmd, string dsName, string idName, CompoundIdentity val)
        {
            if (val.IsNullOrEmpty())
            {
                cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(dsName, NpgsqlTypes.NpgsqlDbType.Uuid));
                cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(idName, NpgsqlTypes.NpgsqlDbType.Uuid));
            }
            else
            {
                cmd.Parameters.AddWithValue(dsName, val.DataStoreIdentity);
                cmd.Parameters.AddWithValue(idName, val.Identity);
            }
        }

        internal static void AddParam(NpgsqlCommand cmd, string idName, CompoundIdentity val)
        {
            if (val.IsNullOrEmpty())
                cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(idName, NpgsqlTypes.NpgsqlDbType.Uuid));
            else
                cmd.Parameters.AddWithValue(idName, val.Identity);
        }

        internal static void AddParam(NpgsqlCommand cmd, string name, DateTime val)
        {
            cmd.Parameters.AddWithValue(name, FixDate(val));
        }

        internal static void AddParam(NpgsqlCommand cmd, string name, Guid val)
        {
            if (Guid.Empty.Equals(val))
                cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(name, NpgsqlTypes.NpgsqlDbType.Uuid));
            else
                cmd.Parameters.AddWithValue(name, val);
        }

        internal static void AddParam(NpgsqlCommand cmd, string name, bool? val)
        {
            if (val.HasValue)
                cmd.Parameters.AddWithValue(name, val);
            else
                cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(name, NpgsqlTypes.NpgsqlDbType.Boolean));
        }

        internal static void AddParam(NpgsqlCommand cmd, string name, uint? val)
        {
            if (val.HasValue)
                cmd.Parameters.AddWithValue(name, (int)val); //treat it like a signed int with implicit range limit
            else
                cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(name, NpgsqlTypes.NpgsqlDbType.Integer));
        }

        internal static void AddParam(NpgsqlCommand cmd, string name, float val)
        {
            if (Numerics.MathUtils.IsInfiniteOrNaN(val))
                cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(name, NpgsqlTypes.NpgsqlDbType.Real));
            else
                cmd.Parameters.AddWithValue(name, val);
        }

        internal static void AddParam(NpgsqlCommand cmd, string name, string val)
        {
            if (string.IsNullOrEmpty(val))
                cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(name, NpgsqlTypes.NpgsqlDbType.Varchar));
            else
                cmd.Parameters.AddWithValue(name, val);
        }

        internal static void AddParam(NpgsqlCommand cmd, string name, Point2<double> location)
        {
            if (location != null)
            {
                NpgsqlTypes.PostgisPoint geom = Osrs.Numerics.Spatial.Postgres.NpgSpatialUtils.ToPGis(location);
                if (geom != null)
                    cmd.Parameters.AddWithValue(name, geom);
                else
                    cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(name, NpgsqlTypes.NpgsqlDbType.Geometry));
            }
            else
            {
                cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(name, NpgsqlTypes.NpgsqlDbType.Geometry));
            }
        }

        internal static string GetWhere(string fieldName, string name, StringComparison comparisonOption)
        {
            if (comparisonOption == StringComparison.CurrentCultureIgnoreCase || comparisonOption == StringComparison.OrdinalIgnoreCase)
                return " lower(\"" + fieldName + "\")=lower(:" +name+ ")";
            else
                return " \"" + fieldName + "\"=:" + name;
        }

        internal static string GetWhere(IEnumerable<Guid> ids, string idName)
        {
            StringBuilder where = new StringBuilder();
            where.Append(' ');
            where.Append(idName);
            where.Append(" IN ('");

            HashSet<Guid> seen = new HashSet<Guid>();
            foreach (Guid cur in ids)
            {
                if (cur != null && !Guid.Empty.Equals(cur) && seen.Add(cur))
                {
                    where.Append(cur.ToString());
                    where.Append("','");
                }
            }
            if (seen.Count > 0)
            {
                where.Length = where.Length - 3;
                where.Append("')");

                return where.ToString();
            }
            return string.Empty;
        }

        internal static string GetWhere(IEnumerable<CompoundIdentity> ids, string idName)
        {
            StringBuilder where = new StringBuilder();
            where.Append(' ');
            where.Append(idName);
            where.Append(" IN ('");

            HashSet<Guid> seen = new HashSet<Guid>();
            foreach (CompoundIdentity cur in ids)
            {
                if (cur!=null && !Guid.Empty.Equals(cur.Identity) && seen.Add(cur.Identity))
                {
                    where.Append(cur.Identity.ToString());
                    where.Append("','");
                }
            }
            if (seen.Count > 0)
            {
                where.Length = where.Length - 3;
                where.Append("')");

                return where.ToString();
            }
            return string.Empty;
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

    internal sealed class CatchEffortBuilder : IBuilder<CatchEffort>
    {
        private readonly UserSecurityContext context;
        private readonly UserProvider prov;
        internal CatchEffortBuilder(UserSecurityContext ctx)
        {
            this.context = ctx;
            this.prov = UserAffilationSecurityManager.Instance.GetProvider(this.context);
        }

        //\"Id\", \"EventSystemId\", \"EventId\", \"SiteSystemId\", \"SiteId\", \"When\", 
        //\"PointLocation\", \"Method\", \"Strata\", \"Depth\", \"pH\", \"Temp\", \"DO\", \"Salinity\", \"Velocity\", \"Description\", \"IsPrivate\"
        public CatchEffort Build(DbDataReader reader)
        {
            Point2<double> loc;
            if (DBNull.Value.Equals(reader[6]))
                loc = null;
            else
                loc = Osrs.Numerics.Spatial.Postgres.NpgSpatialUtils.ToGeom((NpgsqlTypes.PostgisGeometry)reader[6]) as Point2<double>;

            CatchEffort tmp = new CatchEffort(new CompoundIdentity(Db.DataStoreIdentity, DbReaderUtils.GetGuid(reader, 0)), new CompoundIdentity(DbReaderUtils.GetGuid(reader, 1), DbReaderUtils.GetGuid(reader, 2)),
                new CompoundIdentity(DbReaderUtils.GetGuid(reader, 3), DbReaderUtils.GetGuid(reader, 4)), Db.FixDate(DbReaderUtils.GetDate(reader, 5)), loc, DbReaderUtils.GetString(reader, 7), DbReaderUtils.GetString(reader, 8),
                DbReaderUtils.GetSingle(reader, 9), DbReaderUtils.GetSingle(reader, 10), DbReaderUtils.GetSingle(reader, 11), DbReaderUtils.GetSingle(reader, 12), DbReaderUtils.GetSingle(reader, 13),
                DbReaderUtils.GetSingle(reader, 14), DbReaderUtils.GetString(reader, 15), DbReaderUtils.GetBoolean(reader, 16));

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

    internal sealed class CatchMetricBuilder : IBuilder<CatchMetric>
    {
        private readonly PgCatchEffortProvider helperBuilder;
        private readonly HashSet<CompoundIdentity> seen = new HashSet<CompoundIdentity>();
        private readonly HashSet<CompoundIdentity> got = new HashSet<CompoundIdentity>();
        private readonly UserSecurityContext context;
        private readonly UserProvider prov;
        internal CatchMetricBuilder(UserSecurityContext ctx)
        {
            this.context = ctx;
            this.prov = UserAffilationSecurityManager.Instance.GetProvider(this.context);
            this.helperBuilder = new PgCatchEffortProvider(ctx);
        }

        //\"Id\", \"CatchEffortId\", \"MetricType\", \"Value\", \"Description\"
        public CatchMetric Build(DbDataReader reader)
        {
            CatchMetric tmp = new CatchMetric(DbReaderUtils.GetGuid(reader, 0), new CompoundIdentity(Db.DataStoreIdentity, DbReaderUtils.GetGuid(reader, 1)),
                DbReaderUtils.GetSingle(reader, 3), DbReaderUtils.GetString(reader, 2), DbReaderUtils.GetString(reader, 4));

            if (!seen.Contains(tmp.CatchEffortId))
            {
                seen.Add(tmp.CatchEffortId);
                CatchEffort e = this.helperBuilder.Get(tmp.CatchEffortId);
                if (e != null)
                    got.Add(tmp.CatchEffortId);
            }

            if (!got.Contains(tmp.CatchEffortId))
                return null;

            return tmp;
        }
    }

    internal sealed class NetHaulEventBuilder : IBuilder<NetHaulEvent>
    {
        private readonly PgCatchEffortProvider helperBuilder;
        private readonly HashSet<CompoundIdentity> seen = new HashSet<CompoundIdentity>();
        private readonly HashSet<CompoundIdentity> got = new HashSet<CompoundIdentity>();
        private readonly UserSecurityContext context;
        private readonly UserProvider prov;
        internal NetHaulEventBuilder(UserSecurityContext ctx)
        {
            this.context = ctx;
            this.prov = UserAffilationSecurityManager.Instance.GetProvider(this.context);
            this.helperBuilder = new PgCatchEffortProvider(ctx);
        }

        //\"Id\", \"CatchEffortId\", \"NetSystemId\", \"NetId\", \"AreaSampled\", \"VolumeSampled\", \"Description\"
        public NetHaulEvent Build(DbDataReader reader)
        {
            NetHaulEvent tmp = new NetHaulEvent(DbReaderUtils.GetGuid(reader, 0), new CompoundIdentity(Db.DataStoreIdentity, DbReaderUtils.GetGuid(reader, 1)), new CompoundIdentity(DbReaderUtils.GetGuid(reader, 2), DbReaderUtils.GetGuid(reader, 3)),
                DbReaderUtils.GetSingle(reader, 4), DbReaderUtils.GetSingle(reader, 5), DbReaderUtils.GetString(reader, 6));

            if (!seen.Contains(tmp.CatchEffortId))
            {
                seen.Add(tmp.CatchEffortId);
                CatchEffort e = this.helperBuilder.Get(tmp.CatchEffortId);
                if (e != null)
                    got.Add(tmp.CatchEffortId);
            }

            if (!got.Contains(tmp.CatchEffortId))
                return null;

            return tmp;
        }
    }

    internal sealed class FishBuilder : IBuilder<Fish>
    {
        private readonly PgCatchEffortProvider helperBuilder;
        private readonly HashSet<CompoundIdentity> seen = new HashSet<CompoundIdentity>();
        private readonly HashSet<CompoundIdentity> got = new HashSet<CompoundIdentity>();
        private readonly UserSecurityContext context;
        private readonly UserProvider prov;
        internal FishBuilder(UserSecurityContext ctx)
        {
            this.context = ctx;
            this.prov = UserAffilationSecurityManager.Instance.GetProvider(this.context);
            this.helperBuilder = new PgCatchEffortProvider(ctx);
        }

        //\"Id\", \"CatchEffortId\", \"TaxaSystemId\", \"TaxaId\", \"LengthStandard\", \"LengthFork\", \"LengthTotal\", \"Weight\", \"AdClipped\", \"CWT\", \"Description\"
        public Fish Build(DbDataReader reader)
        {
            Fish tmp = new Fish(DbReaderUtils.GetGuid(reader, 0), new CompoundIdentity(Db.DataStoreIdentity, DbReaderUtils.GetGuid(reader, 1)), new CompoundIdentity(DbReaderUtils.GetGuid(reader, 2), DbReaderUtils.GetGuid(reader, 3)),
                DbReaderUtils.GetSingle(reader, 4), DbReaderUtils.GetSingle(reader, 5), DbReaderUtils.GetSingle(reader, 6), DbReaderUtils.GetSingle(reader, 7), DbReaderUtils.GetNullableBoolean(reader, 8), 
                DbReaderUtils.GetNullableBoolean(reader, 9), DbReaderUtils.GetString(reader, 10));

            if (!seen.Contains(tmp.CatchEffortId))
            {
                seen.Add(tmp.CatchEffortId);
                CatchEffort e = this.helperBuilder.Get(tmp.CatchEffortId);
                if (e != null)
                    got.Add(tmp.CatchEffortId);
            }

            if (!got.Contains(tmp.CatchEffortId))
                return null;

            return tmp;
        }
    }

    internal sealed class FishCountBuilder : IBuilder<FishCount>
    {
        private readonly PgCatchEffortProvider helperBuilder;
        private readonly HashSet<CompoundIdentity> seen = new HashSet<CompoundIdentity>();
        private readonly HashSet<CompoundIdentity> got = new HashSet<CompoundIdentity>();
        private readonly UserSecurityContext context;
        private readonly UserProvider prov;
        internal FishCountBuilder(UserSecurityContext ctx)
        {
            this.context = ctx;
            this.prov = UserAffilationSecurityManager.Instance.GetProvider(this.context);
            this.helperBuilder = new PgCatchEffortProvider(ctx);
        }

        //\"Id\", \"CatchEffortId\", \"TaxaSystemId\", \"TaxaId\", \"Count\", \"Description\"
        public FishCount Build(DbDataReader reader)
        {
            FishCount tmp = new FishCount(DbReaderUtils.GetGuid(reader, 0), new CompoundIdentity(Db.DataStoreIdentity, DbReaderUtils.GetGuid(reader, 1)), new CompoundIdentity(DbReaderUtils.GetGuid(reader, 2), DbReaderUtils.GetGuid(reader, 3)),
                (uint)DbReaderUtils.GetInt32(reader, 4), DbReaderUtils.GetString(reader, 5));

            if (!seen.Contains(tmp.CatchEffortId))
            {
                seen.Add(tmp.CatchEffortId);
                CatchEffort e = this.helperBuilder.Get(tmp.CatchEffortId);
                if (e != null)
                    got.Add(tmp.CatchEffortId);
            }

            if (!got.Contains(tmp.CatchEffortId))
                return null;

            return tmp;
        }
    }

    internal sealed class FishDietBuilder : IBuilder<FishDiet>
    {
        private readonly PgFishProvider helperBuilder;
        private readonly HashSet<Guid> seen = new HashSet<Guid>();
        private readonly HashSet<Guid> got = new HashSet<Guid>();
        private readonly UserSecurityContext context;
        private readonly UserProvider prov;
        internal FishDietBuilder(UserSecurityContext ctx, PgFishProvider helperBuilder)
        {
            this.context = ctx;
            this.prov = UserAffilationSecurityManager.Instance.GetProvider(this.context);
            this.helperBuilder = helperBuilder;
        }

        //\"Id\", \"FishId\", \"TaxaSystemId\", \"TaxaId\", \"VialId\", \"GutSampleId\", \"Lifestage\", \"Count\", \"WholeAnimalsWeighed\", \"IndividualMass\", \"SampleMass\", \"Description\"
        public FishDiet Build(DbDataReader reader)
        {
            FishDiet tmp = new FishDiet(DbReaderUtils.GetGuid(reader, 0), DbReaderUtils.GetGuid(reader, 1), new CompoundIdentity(DbReaderUtils.GetGuid(reader, 2), DbReaderUtils.GetGuid(reader, 3)),
                DbReaderUtils.GetString(reader, 4), DbReaderUtils.GetString(reader, 5), DbReaderUtils.GetString(reader, 6), (uint?)DbReaderUtils.GetNullableInt32(reader, 7), DbReaderUtils.GetSingle(reader, 9), 
                DbReaderUtils.GetSingle(reader, 10), (uint?)DbReaderUtils.GetNullableInt32(reader, 8), DbReaderUtils.GetString(reader, 11));

            if (!seen.Contains(tmp.FishId))
            {
                seen.Add(tmp.FishId);
                Fish e = this.helperBuilder.GetFish(tmp.FishId);
                if (e != null)
                    got.Add(tmp.FishId);
            }

            if (!got.Contains(tmp.FishId))
                return null;

            return tmp;
        }
    }

    internal sealed class FishGeneticsBuilder : IBuilder<FishGenetics>
    {
        private readonly PgFishProvider helperBuilder;
        private readonly HashSet<Guid> seen = new HashSet<Guid>();
        private readonly HashSet<Guid> got = new HashSet<Guid>();
        private readonly UserSecurityContext context;
        private readonly UserProvider prov;
        internal FishGeneticsBuilder(UserSecurityContext ctx, PgFishProvider helperBuilder)
        {
            this.context = ctx;
            this.prov = UserAffilationSecurityManager.Instance.GetProvider(this.context);
            this.helperBuilder = helperBuilder;
        }

        //\"Id\", \"FishId\", \"GeneticSampleId\", \"LabSampleId\", \"StockEstimates\", \"Description\"   --NOTE Stock Estimates are an encoded value
        public FishGenetics Build(DbDataReader reader)
        {
            FishGenetics tmp = new FishGenetics(DbReaderUtils.GetGuid(reader, 0), DbReaderUtils.GetGuid(reader, 1), DbReaderUtils.GetString(reader, 2), DbReaderUtils.GetString(reader, 3), Decode(DbReaderUtils.GetString(reader, 4)), DbReaderUtils.GetString(reader, 5));

            if (!seen.Contains(tmp.FishId))
            {
                seen.Add(tmp.FishId);
                Fish e = this.helperBuilder.GetFish(tmp.FishId);
                if (e != null)
                    got.Add(tmp.FishId);
            }

            if (!got.Contains(tmp.FishId))
                return null;

            return tmp;
        }

        internal static string Encode(StockEstimates value)
        {
            if (value == null)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            foreach(StockEstimate cur in value)
            {
                sb.Append(cur.Stock);
                sb.Append(":");
                sb.AppendLine(cur.Probability.ToString());
            }
            return sb.ToString();
        }

        internal static StockEstimates Decode(string value)
        {
            if (string.IsNullOrEmpty(value))
                return new StockEstimates();
            StockEstimates res = new StockEstimates();
            StringReader sr = new StringReader(value);
            string cur = sr.ReadLine();
            while (cur != null)
            {
                string[] items = cur.Split(':');
                string name = items[0];
                if (items.Length > 2)
                {
                    for (int i = 1; i < items.Length - 1; i++)
                        name = name + ':' + items[i];
                }
                float pro;
                if (float.TryParse(items[items.Length-1], out pro))
                    res[name]=pro;
                cur = sr.ReadLine();
            }
            return res;
        }
    }

    internal sealed class FishIdTagBuilder : IBuilder<FishIdTag>
    {
        private readonly PgFishProvider helperBuilder;
        private readonly HashSet<Guid> seen = new HashSet<Guid>();
        private readonly HashSet<Guid> got = new HashSet<Guid>();
        private readonly UserSecurityContext context;
        private readonly UserProvider prov;
        internal FishIdTagBuilder(UserSecurityContext ctx, PgFishProvider helperBuilder)
        {
            this.context = ctx;
            this.prov = UserAffilationSecurityManager.Instance.GetProvider(this.context);
            this.helperBuilder = helperBuilder;
        }

        //\"Id\", \"FishId\", \"TagCode\", \"TagType\", \"TagManufacturer\", \"Description\"
        public FishIdTag Build(DbDataReader reader)
        {
            FishIdTag tmp = new FishIdTag(DbReaderUtils.GetGuid(reader, 0), DbReaderUtils.GetGuid(reader, 1), DbReaderUtils.GetString(reader, 2), DbReaderUtils.GetString(reader, 3),
                DbReaderUtils.GetString(reader, 4), DbReaderUtils.GetString(reader, 5));

            if (!seen.Contains(tmp.FishId))
            {
                seen.Add(tmp.FishId);
                Fish e = this.helperBuilder.GetFish(tmp.FishId);
                if (e != null)
                    got.Add(tmp.FishId);
            }

            if (!got.Contains(tmp.FishId))
                return null;

            return tmp;
        }
    }
}
