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
using Osrs.Numerics;
using Osrs.Numerics.Spatial.Geometry;
using Osrs.Oncor.UserAffiliationPermissionChecks;
using Osrs.Security;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Osrs.Oncor.WellKnown.Vegetation.Providers.Pg
{
    internal static class Db
    {
        internal static string ConnectionString;
        internal static readonly Guid DataStoreIdentity = new Guid("{A38A6254-8AB1-4D12-A576-DD058813F856}");

        internal const string Where = " WHERE ";
        internal const string And = " AND ";
        internal const string WhereId = "\"Id\"=:id";
        internal const string WhereVegSurvey = "\"VegSurveyId\"=:vsid";
        internal const string WhereVegSample = "\"VegSampleId\"=:vsid";
        internal const string WhereVegSurveyIn = "\"VegSampleId\" IN (SELECT \"Id\" FROM oncor.\"VegSample\" WHERE \"VegSurveyId\"=:vsid)";
        internal const string WhereEvent = "\"EventSystemId\"=:esid AND \"EventId\"=:eid";
        internal const string WhereSite = "\"SiteSystemId\"=:sitesid AND \"SiteId\"=:siteid";
        internal const string WhereTaxa = "\"TaxaSystemId\"=:taxasid AND \"TaxaId\"=:taxaid";
        internal const string WhereStart = "\"When\">=:start";
        internal const string WhereEnd = "\"When\"<=:end";

        internal const string SelectSurvey = "SELECT \"Id\", \"EventSystemId\", \"EventId\", \"SiteSystemId\", \"SiteId\", \"PlotTypeId\", \"PointLocation\", \"Area\", \"ElevMin\", \"ElevMax\", \"Description\", \"IsPrivate\" FROM oncor.\"VegSurvey\"";
        internal const string SelectPlotType = "SELECT \"Id\", \"Name\", \"Description\" FROM oncor.\"VegPlotTypes\"";
        internal const string SelectVegSample = "SELECT \"Id\", \"VegSurveyId\", \"SiteSystemId\", \"SiteId\", \"When\", \"PointLocation\", \"ElevMin\", \"ElevMax\" FROM oncor.\"VegSample\"";
        internal const string SelectHerbSample = "SELECT \"Id\", \"VegSampleId\", \"TaxaUnitSystemId\", \"TaxaUnitId\", \"PercentCover\", \"Description\" FROM oncor.\"VegHerbSample\"";
        internal const string SelectShrubSample = "SELECT \"Id\", \"VegSampleId\", \"TaxaUnitSystemId\", \"TaxaUnitId\", \"SizeClass\", \"Count\", \"Description\" FROM oncor.\"VegShrubSample\"";
        internal const string SelectTreeSample = "SELECT \"Id\", \"VegSampleId\", \"TaxaUnitSystemId\", \"TaxaUnitId\", \"Dbh\", \"Description\" FROM oncor.\"VegTreeSample\"";

        internal const string UpdateSurvey = "UPDATE oncor.\"VegSurvey\" SET \"SiteSystemId\"=:sitesid, \"SiteId\"=:siteid, \"PlotTypeId\"=:ptid, \"PointLocation\"=:loc, \"Area\"=:area, \"ElevMin\"=:eMin, \"ElevMax\"=:eMax, \"Description\"=:d, \"IsPrivate\"=:private WHERE \"Id\"=:id";
        internal const string UpdatePlotType = "UPDATE oncor.\"VegPlotTypes\" SET \"Name\"=:n, \"Description\"=:d WHERE \"Id\"=:id";
        internal const string UpdateVegSample = "UPDATE oncor.\"VegSample\" SET \"PointLocation\"=:loc, \"ElevMin\"=:eMin, \"ElevMax\"=:eMax WHERE \"Id\"=:id";
        internal const string UpdateHerbSample = "UPDATE oncor.\"VegHerbSample\" SET \"PercentCover\"=:cov, \"Description\"=:d WHERE \"Id\"=:id";
        internal const string UpdateShrubSample = "UPDATE oncor.\"VegShrubSample\" SET \"SizeClass\"=:class, \"Count\"=:ct, \"Description\"=:d WHERE \"Id\"=:id";
        internal const string UpdateTreeSample = "UPDATE oncor.\"VegTreeSample\" SET \"Dbh\"=:dbh, \"Description\"=:d WHERE \"Id\"=:id";

        internal const string InsertSurvey = "INSERT INTO oncor.\"VegSurvey\"(\"Id\", \"EventSystemId\", \"EventId\", \"SiteSystemId\", \"SiteId\", \"PlotTypeId\", \"PointLocation\", \"Area\", \"ElevMin\", \"ElevMax\", \"Description\", \"IsPrivate\") VALUES (:id,:esid,:eid,:sitesid,:siteid,:ptid,:loc,:area,:eMin,:eMax,:d,:private)";
        internal const string InsertSurveyB = "INSERT INTO oncor.\"VegSurvey\"(\"Id\", \"EventSystemId\", \"EventId\", \"SiteSystemId\", \"SiteId\", \"PlotTypeId\", \"PointLocation\", \"Area\", \"ElevMin\", \"ElevMax\", \"Description\", \"IsPrivate\") VALUES ";
        internal const string InsertPlotType = "INSERT INTO oncor.\"VegPlotTypes\"(\"Id\", \"Name\", \"Description\") VALUES (:id,:n,:d)";
        internal const string InsertVegSample = "INSERT INTO oncor.\"VegSample\"(\"Id\", \"VegSurveyId\", \"SiteSystemId\", \"SiteId\", \"When\", \"PointLocation\", \"ElevMin\", \"ElevMax\") VALUES (:id,:vsid,:sitesid,:siteid,:when,:loc,:eMin,:eMax)";
        internal const string InsertVegSampleB = "INSERT INTO oncor.\"VegSample\"(\"Id\", \"VegSurveyId\", \"SiteSystemId\", \"SiteId\", \"When\", \"PointLocation\", \"ElevMin\", \"ElevMax\") VALUES ";
        internal const string InsertHerbSample = "INSERT INTO oncor.\"VegHerbSample\"(\"Id\", \"VegSampleId\", \"TaxaUnitSystemId\", \"TaxaUnitId\", \"PercentCover\", \"Description\") VALUES (:id,:vsid,:taxasid,:taxaid,:cov,:d)";
        internal const string InsertHerbSampleB = "INSERT INTO oncor.\"VegHerbSample\"(\"Id\", \"VegSampleId\", \"TaxaUnitSystemId\", \"TaxaUnitId\", \"PercentCover\", \"Description\") VALUES ";
        internal const string InsertShrubSample = "INSERT INTO oncor.\"VegShrubSample\"(\"Id\", \"VegSampleId\", \"TaxaUnitSystemId\", \"TaxaUnitId\", \"SizeClass\", \"Count\", \"Description\") VALUES (:id,:vsid,:taxasid,:taxaid,:class,:ct,:d)";
        internal const string InsertShrubSampleB = "INSERT INTO oncor.\"VegShrubSample\"(\"Id\", \"VegSampleId\", \"TaxaUnitSystemId\", \"TaxaUnitId\", \"SizeClass\", \"Count\", \"Description\") VALUES ";
        internal const string InsertTreeSample = "INSERT INTO oncor.\"VegTreeSample\"(\"Id\", \"VegSampleId\", \"TaxaUnitSystemId\", \"TaxaUnitId\", \"Dbh\", \"Description\") VALUES (:id,:vsid,:taxasid,:taxaid,:dbh,:d)";
        internal const string InsertTreeSampleB = "INSERT INTO oncor.\"VegTreeSample\"(\"Id\", \"VegSampleId\", \"TaxaUnitSystemId\", \"TaxaUnitId\", \"Dbh\", \"Description\") VALUES ";

        internal const string DeleteSurvey = "DELETE FROM oncor.\"VegSurvey\" WHERE \"Id\"=:id";
        internal const string DeletePlotType = "DELETE FROM oncor.\"VegPlotTypes\" WHERE \"Id\"=:id";
        internal const string DeleteVegSample = "DELETE FROM oncor.\"VegSample\" WHERE ";
        internal const string DeleteHerbSample = "DELETE FROM oncor.\"VegHerbSample\" WHERE ";
        internal const string DeleteShrubSample = "DELETE FROM oncor.\"VegShrubSample\" WHERE ";
        internal const string DeleteTreeSample = "DELETE FROM oncor.\"VegTreeSample\" WHERE ";

        internal static DateTime FixDate(DateTime when)
        {
            return VegUtils.FixDate(when);
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

        internal static void AddParam(NpgsqlCommand cmd, string minName, string maxName, ValueRange<float> val)
        {
            if (val == null)
            {
                cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(minName, NpgsqlTypes.NpgsqlDbType.Real));
                cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(maxName, NpgsqlTypes.NpgsqlDbType.Real));
            }
            else
            {
                if (float.IsNegativeInfinity(val.Min))
                    cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(minName, NpgsqlTypes.NpgsqlDbType.Real));
                else
                    cmd.Parameters.AddWithValue(minName, val.Min);

                if (float.IsPositiveInfinity(val.Max))
                    cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(maxName, NpgsqlTypes.NpgsqlDbType.Real));
                else
                    cmd.Parameters.AddWithValue(maxName, val.Max);
            }
        }

        internal static void AddParam(NpgsqlCommand cmd, string name, float val)
        {
            if (Numerics.MathUtils.IsInfiniteOrNaN(val))
                cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(name, NpgsqlTypes.NpgsqlDbType.Real));
            else
                cmd.Parameters.AddWithValue(name, val);
        }

        internal static void AddParam(NpgsqlCommand cmd, string name, uint val)
        {
            cmd.Parameters.AddWithValue(name, (int)val);
        }

        internal static void AddParam(NpgsqlCommand cmd, string name, string val)
        {
            if (string.IsNullOrEmpty(val))
                cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(name, NpgsqlTypes.NpgsqlDbType.Varchar));
            else
                cmd.Parameters.AddWithValue(name, val);
        }

        internal static uint GetUint32(DbDataReader rdr, int index)
        {
            return (uint)((int)rdr[index]);
        }

        internal static Point2<double> GetLoc(DbDataReader rdr, int index)
        {
            if (DBNull.Value.Equals(rdr[index]))
                return null;

            return Osrs.Numerics.Spatial.Postgres.NpgSpatialUtils.ToGeom((NpgsqlTypes.PostgisGeometry)rdr[index]) as Point2<double>;
        }

        internal static string GetWhere(string name, StringComparison comparisonOption)
        {
            if (comparisonOption == StringComparison.CurrentCultureIgnoreCase || comparisonOption == StringComparison.OrdinalIgnoreCase)
                return " lower(\"Name\")=lower(:name)";
            else
                return " \"Name\"=:name";
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

        internal static string GetWhere(IEnumerable<CompoundIdentity> ids, string idName)
        {
            StringBuilder where = new StringBuilder();
            where.Append(' ');
            where.Append(idName);
            where.Append(" IN ('");

            HashSet<Guid> seen = new HashSet<Guid>();
            foreach (CompoundIdentity cur in ids)
            {
                if (cur != null && !Guid.Empty.Equals(cur.Identity) && seen.Add(cur.Identity))
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

    internal sealed class VegSurveyBuilder : IBuilder<VegSurvey>
    {
        private readonly UserSecurityContext context;
        private readonly UserProvider prov;

        internal VegSurveyBuilder(UserSecurityContext ctx)
        {
            this.context = ctx;
            this.prov = UserAffilationSecurityManager.Instance.GetProvider(this.context);
        }

        //\"Id\", \"EventSystemId\", \"EventId\", \"SiteSystemId\", \"SiteId\", \"PlotTypeId\", \"PointLocation\", \"Area\", \"ElevMin\", \"ElevMax\", \"Description\", \"IsPrivate\"
        public VegSurvey Build(DbDataReader reader)
        {
            VegSurvey tmp = new VegSurvey(new CompoundIdentity(Db.DataStoreIdentity, DbReaderUtils.GetGuid(reader, 0)), new CompoundIdentity(DbReaderUtils.GetGuid(reader, 1), DbReaderUtils.GetGuid(reader, 2)),
                new CompoundIdentity(DbReaderUtils.GetGuid(reader, 3), DbReaderUtils.GetGuid(reader, 4)), new CompoundIdentity(Db.DataStoreIdentity, DbReaderUtils.GetGuid(reader, 5)),
                Db.GetLoc(reader, 6), DbReaderUtils.GetSingle(reader, 7), DbReaderUtils.GetSingle(reader, 8), DbReaderUtils.GetSingle(reader, 9), DbReaderUtils.GetString(reader, 10), (bool)reader[11]);

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

    internal sealed class VegPlotTypeBuilder : IBuilder<VegPlotType>
    {
        private readonly UserSecurityContext context;
        private readonly UserProvider prov;

        internal VegPlotTypeBuilder(UserSecurityContext ctx)
        {
            this.context = ctx;
            this.prov = UserAffilationSecurityManager.Instance.GetProvider(this.context);
        }

        //\"Id\", \"Name\", \"Description\"
        public VegPlotType Build(DbDataReader reader)
        {
            return new VegPlotType(new CompoundIdentity(Db.DataStoreIdentity, DbReaderUtils.GetGuid(reader, 0)), DbReaderUtils.GetString(reader, 1), DbReaderUtils.GetString(reader, 2));
        }
    }

    internal sealed class VegSampleBuilder : IBuilder<VegSample>
    {
        private readonly PgVegSurveyProvider helperBuilder;
        private readonly HashSet<CompoundIdentity> seen = new HashSet<CompoundIdentity>();
        private readonly HashSet<CompoundIdentity> got = new HashSet<CompoundIdentity>();
        private readonly UserSecurityContext context;
        private readonly UserProvider prov;
        internal VegSampleBuilder(UserSecurityContext ctx)
        {
            this.context = ctx;
            this.prov = UserAffilationSecurityManager.Instance.GetProvider(this.context);
            this.helperBuilder = new PgVegSurveyProvider(ctx);
        }

        //\"Id\", \"VegSurveyId\", \"SiteSystemId\", \"SiteId\", \"When\", \"PointLocation\", \"ElevMin\", \"ElevMax\"
        public VegSample Build(DbDataReader reader)
        {
            VegSample tmp = new VegSample((Guid)reader[0], new CompoundIdentity(Db.DataStoreIdentity, DbReaderUtils.GetGuid(reader, 1)), new CompoundIdentity(DbReaderUtils.GetGuid(reader, 2), DbReaderUtils.GetGuid(reader, 3)),
                Db.FixDate(DbReaderUtils.GetDate(reader, 4)), Db.GetLoc(reader, 5), DbReaderUtils.GetSingle(reader, 6), DbReaderUtils.GetSingle(reader, 7));

            if (!seen.Contains(tmp.VegSurveyId))
            {
                seen.Add(tmp.VegSurveyId);
                VegSurvey e = this.helperBuilder.GetSurvey(tmp.VegSurveyId);
                if (e != null)
                    got.Add(tmp.VegSurveyId);
            }

            if (!got.Contains(tmp.VegSurveyId))
                return null;

            return tmp;
        }
    }

    internal sealed class HerbSampleBuilder : IBuilder<HerbSample>
    {
        private readonly PgVegSampleProvider helperBuilder;
        private readonly HashSet<Guid> seen = new HashSet<Guid>();
        private readonly HashSet<Guid> got = new HashSet<Guid>();
        private readonly UserSecurityContext context;
        private readonly UserProvider prov;
        internal HerbSampleBuilder(UserSecurityContext ctx, PgVegSampleProvider helperBuilder)
        {
            this.context = ctx;
            this.prov = UserAffilationSecurityManager.Instance.GetProvider(this.context);
            this.helperBuilder = helperBuilder;
        }

        //\"Id\", \"VegSampleId\", \"TaxaUnitSystemId\", \"TaxaUnitId\", \"PercentCover\", \"Description\"
        public HerbSample Build(DbDataReader reader)
        {
            HerbSample tmp = new HerbSample((Guid)reader[0], DbReaderUtils.GetGuid(reader, 1), new CompoundIdentity(DbReaderUtils.GetGuid(reader, 2), DbReaderUtils.GetGuid(reader, 3)),
                DbReaderUtils.GetSingle(reader, 4), DbReaderUtils.GetString(reader, 5));

            if (!seen.Contains(tmp.VegSampleId))
            {
                seen.Add(tmp.VegSampleId);
                VegSample e = this.helperBuilder.Get(tmp.VegSampleId);
                if (e != null)
                    got.Add(tmp.VegSampleId);
            }

            if (!got.Contains(tmp.VegSampleId))
                return null;

            return tmp;
        }
    }

    internal sealed class ShrubSampleBuilder : IBuilder<ShrubSample>
    {
        private readonly PgVegSampleProvider helperBuilder;
        private readonly HashSet<Guid> seen = new HashSet<Guid>();
        private readonly HashSet<Guid> got = new HashSet<Guid>();
        private readonly UserSecurityContext context;
        private readonly UserProvider prov;
        internal ShrubSampleBuilder(UserSecurityContext ctx, PgVegSampleProvider helperBuilder)
        {
            this.context = ctx;
            this.prov = UserAffilationSecurityManager.Instance.GetProvider(this.context);
            this.helperBuilder = helperBuilder;
        }

        //\"Id\", \"VegSampleId\", \"TaxaUnitSystemId\", \"TaxaUnitId\", \"SizeClass\", \"Count\", \"Description\"
        public ShrubSample Build(DbDataReader reader)
        {
            ShrubSample tmp = new ShrubSample((Guid)reader[0], DbReaderUtils.GetGuid(reader, 1), new CompoundIdentity(DbReaderUtils.GetGuid(reader, 2), DbReaderUtils.GetGuid(reader, 3)),
                DbReaderUtils.GetString(reader, 4), Db.GetUint32(reader, 5), DbReaderUtils.GetString(reader, 6));

            if (!seen.Contains(tmp.VegSampleId))
            {
                seen.Add(tmp.VegSampleId);
                VegSample e = this.helperBuilder.Get(tmp.VegSampleId);
                if (e != null)
                    got.Add(tmp.VegSampleId);
            }

            if (!got.Contains(tmp.VegSampleId))
                return null;

            return tmp;
        }
    }

    internal sealed class TreeSampleBuilder : IBuilder<TreeSample>
    {
        private readonly PgVegSampleProvider helperBuilder;
        private readonly HashSet<Guid> seen = new HashSet<Guid>();
        private readonly HashSet<Guid> got = new HashSet<Guid>();
        private readonly UserSecurityContext context;
        private readonly UserProvider prov;
        internal TreeSampleBuilder(UserSecurityContext ctx, PgVegSampleProvider helperBuilder)
        {
            this.context = ctx;
            this.prov = UserAffilationSecurityManager.Instance.GetProvider(this.context);
            this.helperBuilder = helperBuilder;
        }

        //\"Id\", \"VegSampleId\", \"TaxaUnitSystemId\", \"TaxaUnitId\", \"Dbh\", \"Description\"
        public TreeSample Build(DbDataReader reader)
        {
            TreeSample tmp = new TreeSample((Guid)reader[0], DbReaderUtils.GetGuid(reader, 1), new CompoundIdentity(DbReaderUtils.GetGuid(reader, 2), DbReaderUtils.GetGuid(reader, 3)),
                DbReaderUtils.GetSingle(reader, 4), DbReaderUtils.GetString(reader, 5));

            if (!seen.Contains(tmp.VegSampleId))
            {
                seen.Add(tmp.VegSampleId);
                VegSample e = this.helperBuilder.Get(tmp.VegSampleId);
                if (e != null)
                    got.Add(tmp.VegSampleId);
            }

            if (!got.Contains(tmp.VegSampleId))
                return null;

            return tmp;
        }
    }
}
