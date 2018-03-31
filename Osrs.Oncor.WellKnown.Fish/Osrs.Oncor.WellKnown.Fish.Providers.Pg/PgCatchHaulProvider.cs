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

using System;
using System.Collections.Generic;
using Osrs.Data;
using Osrs.Security;
using Osrs.Security.Authorization;
using Osrs.Oncor.WellKnown.Fish.Module;
using Npgsql;
using Osrs.Data.Postgres;

namespace Osrs.Oncor.WellKnown.Fish.Providers.Pg
{
    public sealed class PgCatchHaulProvider : ICatchHaulProvider
    {
        private readonly FishCountBuilder fishBuilder;
        private readonly NetHaulEventBuilder netBuilder;
        private readonly CatchMetricBuilder catchBuilder;
        private PgCatchEffortProvider helperProvider = null;
        private CompoundIdentity lastCatchEffortId = null;
        private bool lastEditPermission=false;

        public bool CanCreate()
        {
            IRoleProvider perms = this.AuthProvider;
            if (perms != null)
            {
                if (this.Context != null && this.Context.User != null)
                {
                    return perms.HasPermission(this.Context.User, FishModuleUtils.FishCreatePermission);
                }
            }
            return false;
        }

        public bool CanDelete()
        {
            IRoleProvider perms = this.AuthProvider;
            if (perms != null)
            {
                if (this.Context != null && this.Context.User != null)
                {
                    return perms.HasPermission(this.Context.User, FishModuleUtils.FishDeletePermission);
                }
            }
            return false;
        }

        public bool CanDelete(FishCount item)
        {
            if (item != null)
            {
                if (this.helperProvider == null)
                    this.helperProvider = new PgCatchEffortProvider(this.Context);
                return this.helperProvider.CanDelete(item.CatchEffortId);
            }
            return false;
        }

        public bool CanDelete(NetHaulEvent item)
        {
            if (item != null)
            {
                if (this.helperProvider == null)
                    this.helperProvider = new PgCatchEffortProvider(this.Context);
                return this.helperProvider.CanDelete(item.CatchEffortId);
            }
            return false;
        }

        public bool CanDelete(CatchMetric item)
        {
            if (item != null)
            {
                if (this.helperProvider == null)
                    this.helperProvider = new PgCatchEffortProvider(this.Context);
                return this.helperProvider.CanDelete(item.CatchEffortId);
            }
            return false;
        }

        public bool CanGet()
        {
            IRoleProvider perms = this.AuthProvider;
            if (perms != null)
            {
                if (this.Context != null && this.Context.User != null)
                {
                    return perms.HasPermission(this.Context.User, FishModuleUtils.FishGetPermission);
                }
            }
            return false;
        }

        public FishCount CreateFishCount(CompoundIdentity catchEffortId, CompoundIdentity taxaId, uint count)
        {
            return CreateFishCount(catchEffortId, taxaId, count, null);
        }

        public FishCount CreateFishCount(CompoundIdentity catchEffortId, CompoundIdentity taxaId, uint count, string description)
        {
            if (!catchEffortId.IsNullOrEmpty() && !taxaId.IsNullOrEmpty() && catchEffortId.DataStoreIdentity.Equals(Db.DataStoreIdentity) && this.CanCreate())
            {
                try //:id, :ceid, :tsid, :tid, :ct, :d
                {
                    Guid id = Guid.NewGuid();
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertFishCount;
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("ceid", catchEffortId.Identity);
                    Db.AddParam(cmd, "tsid", "tid", taxaId);
                    cmd.Parameters.AddWithValue("ct", (int)count);
                    Db.AddParam(cmd, "d", description);
                    Db.ExecuteNonQuery(cmd);

                    return new FishCount(id, catchEffortId, taxaId, count, description);
                }
                catch
                { }
            }
            return null;
        }

        public NetHaulEvent CreateHaul(CompoundIdentity catchEffortId, CompoundIdentity netId, float areaSampled, float volumeSampled)
        {
            return CreateHaul(catchEffortId, netId, areaSampled, volumeSampled, null);
        }

        public NetHaulEvent CreateHaul(CompoundIdentity catchEffortId, CompoundIdentity netId, float areaSampled, float volumeSampled, string description)
        {
            if (!catchEffortId.IsNullOrEmpty() && !netId.IsNullOrEmpty() && catchEffortId.DataStoreIdentity.Equals(Db.DataStoreIdentity) && this.CanCreate())
            {
                try //:id, :ceid, :nsid, :nid, :area, :vol, :d
                {
                    Guid id = Guid.NewGuid();
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertFishNetHaulEvent;
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("ceid", catchEffortId.Identity);
                    Db.AddParam(cmd, "nsid", "nid", netId);
                    cmd.Parameters.AddWithValue("area", areaSampled);
                    cmd.Parameters.AddWithValue("vol", volumeSampled);
                    Db.AddParam(cmd, "d", description);
                    Db.ExecuteNonQuery(cmd);

                    return new NetHaulEvent(id, catchEffortId, netId, areaSampled, volumeSampled, description);
                }
                catch
                { }
            }
            return null;
        }

        public CatchMetric CreateMetric(CompoundIdentity catchEffortId, float value, string metricType)
        {
            return CreateMetric(catchEffortId, value, metricType, null);
        }

        public CatchMetric CreateMetric(CompoundIdentity catchEffortId, float value, string metricType, string description)
        {
            if (!catchEffortId.IsNullOrEmpty() && !string.IsNullOrEmpty(metricType) && catchEffortId.DataStoreIdentity.Equals(Db.DataStoreIdentity) && this.CanCreate())
            {
                try //:id, :ceid, :metric, :val, :d
                {
                    Guid id = Guid.NewGuid();
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertFishCatchMetrics;
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("ceid", catchEffortId.Identity);
                    Db.AddParam(cmd, "metric", metricType);
                    Db.AddParam(cmd, "val", value);
                    Db.AddParam(cmd, "d", description);
                    Db.ExecuteNonQuery(cmd);

                    return new CatchMetric(id, catchEffortId, value, metricType, description);
                }
                catch
                { }
            }
            return null;
        }

        public bool Delete(FishCount item)
        {
            if (item != null && this.CanDelete(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.DeleteFishCount + Db.WhereId;
                    cmd.Parameters.AddWithValue("id", item.Identity);
                    Db.ExecuteNonQuery(cmd);

                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public bool Delete(NetHaulEvent item)
        {
            if (item != null && this.CanDelete(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.DeleteFishNetHaulEvent + Db.WhereId;
                    cmd.Parameters.AddWithValue("id", item.Identity);
                    Db.ExecuteNonQuery(cmd);

                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public bool Delete(CatchMetric item)
        {
            if (item != null && this.CanDelete(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.DeleteFishCatchMetrics + Db.WhereId;
                    cmd.Parameters.AddWithValue("id", item.Identity);
                    Db.ExecuteNonQuery(cmd);

                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public IEnumerable<FishCount> GetFishCounts()
        {
            if (this.CanGet())
                return new Enumerable<FishCount>(new EnumerableCommand<FishCount>(this.fishBuilder, Db.SelectFishCount, Db.ConnectionString));
            return null;
        }

        public IEnumerable<FishCount> GetFishCounts(CompoundIdentity catchEffortId)
        {
            if (!catchEffortId.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(catchEffortId.DataStoreIdentity) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishCount + Db.Where + Db.WhereEffort;
                cmd.Parameters.AddWithValue("ceid", catchEffortId.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                FishCount o = null;
                List<FishCount> permissions = new List<FishCount>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.fishBuilder.Build(rdr);
                            if (o != null)
                                permissions.Add(o);
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
                return permissions;
            }
            return null;
        }

        public IEnumerable<FishCount> GetFishCounts(CompoundIdentity catchEffortId, IEnumerable<CompoundIdentity> taxaId)
        {
            if (!catchEffortId.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(catchEffortId.DataStoreIdentity) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishCount + Db.Where + Db.WhereEffort + " AND " + Db.GetWhere(taxaId, "\"TaxaSystemId\"", "\"TaxaId\"");
                cmd.Parameters.AddWithValue("ceid", catchEffortId.Identity);
                
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                FishCount o = null;
                List<FishCount> permissions = new List<FishCount>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.fishBuilder.Build(rdr);
                            if (o != null)
                                permissions.Add(o);
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
                return permissions;
            }
            return null;
        }

        public IEnumerable<FishCount> GetFishCounts(CompoundIdentity catchEffortId, CompoundIdentity taxaId)
        {
            if (!catchEffortId.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(catchEffortId.DataStoreIdentity) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishCount + Db.Where + Db.WhereEffort + " AND " + Db.WhereTaxa;
                cmd.Parameters.AddWithValue("ceid", catchEffortId.Identity);
                Db.AddParam(cmd, "taxasid", "taxaid", taxaId);

                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                FishCount o = null;
                List<FishCount> permissions = new List<FishCount>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.fishBuilder.Build(rdr);
                            if (o != null)
                                permissions.Add(o);
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
                return permissions;
            }
            return null;
        }

        public IEnumerable<FishCount> GetFishCountsByTaxa(CompoundIdentity taxaId)
        {
            if (this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishCount + Db.Where + Db.WhereTaxa;
                Db.AddParam(cmd, "taxasid", "taxaid", taxaId);

                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                FishCount o = null;
                List<FishCount> permissions = new List<FishCount>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.fishBuilder.Build(rdr);
                            if (o != null)
                                permissions.Add(o);
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
                return permissions;
            }
            return null;
        }

        public IEnumerable<NetHaulEvent> GetHauls()
        {
            if (this.CanGet())
                return new Enumerable<NetHaulEvent>(new EnumerableCommand<NetHaulEvent>(this.netBuilder, Db.SelectFishNetHaulEvent, Db.ConnectionString));
            return null;
        }

        public IEnumerable<NetHaulEvent> GetHauls(CompoundIdentity catchEffortId)
        {
            if (!catchEffortId.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(catchEffortId.DataStoreIdentity) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishNetHaulEvent + Db.Where + Db.WhereEffort;
                cmd.Parameters.AddWithValue("ceid", catchEffortId.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                NetHaulEvent o = null;
                List<NetHaulEvent> permissions = new List<NetHaulEvent>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.netBuilder.Build(rdr);
                            if (o != null)
                                permissions.Add(o);
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
                return permissions;
            }
            return null;
        }

        public IEnumerable<CatchMetric> GetMetrics()
        {
            if (this.CanGet())
                return new Enumerable<CatchMetric>(new EnumerableCommand<CatchMetric>(this.catchBuilder, Db.SelectFishCatchMetrics, Db.ConnectionString));
            return null;
        }

        public IEnumerable<CatchMetric> GetMetrics(string metricType)
        {
            if (this.CanGet())
            {
                if (string.IsNullOrEmpty(metricType))
                    return new List<CatchMetric>(); //can't have any with null or empty metric type, so always an empty set

                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishCatchMetrics + Db.Where + Db.GetWhere("MetricType", "metric", StringComparison.OrdinalIgnoreCase);
                cmd.Parameters.AddWithValue("metric", metricType);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                CatchMetric o = null;
                List<CatchMetric> permissions = new List<CatchMetric>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.catchBuilder.Build(rdr);
                            if (o != null)
                                permissions.Add(o);
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
                return permissions;
            }
            return null;
        }

        public IEnumerable<CatchMetric> GetMetrics(CompoundIdentity catchEffortId)
        {
            if (!catchEffortId.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(catchEffortId.DataStoreIdentity) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishCatchMetrics + Db.Where + Db.WhereEffort;
                cmd.Parameters.AddWithValue("ceid", catchEffortId.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                CatchMetric o = null;
                List<CatchMetric> permissions = new List<CatchMetric>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.catchBuilder.Build(rdr);
                            if (o != null)
                                permissions.Add(o);
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
                return permissions;
            }
            return null;
        }

        public IEnumerable<CatchMetric> GetMetrics(CompoundIdentity catchEffortId, string metricType)
        {
            if (!catchEffortId.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(catchEffortId.DataStoreIdentity) && this.CanGet())
            {
                if (string.IsNullOrEmpty(metricType))
                    return new List<CatchMetric>(); //can't have any with null or empty metric type, so always an empty set

                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishCatchMetrics + Db.Where + Db.WhereEffort + " AND " + Db.GetWhere("MetricType", "metric", StringComparison.OrdinalIgnoreCase);
                cmd.Parameters.AddWithValue("ceid", catchEffortId.Identity);
                cmd.Parameters.AddWithValue("metric", metricType);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                CatchMetric o = null;
                List<CatchMetric> permissions = new List<CatchMetric>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.catchBuilder.Build(rdr);
                            if (o != null)
                                permissions.Add(o);
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
                return permissions;
            }
            return null;
        }

        private UserSecurityContext Context
        {
            get;
            set;
        }

        private IRoleProvider prov;
        private IRoleProvider AuthProvider
        {
            get
            {
                if (prov == null)
                    prov = AuthorizationManager.Instance.GetRoleProvider(this.Context);
                return prov;
            }
        }

        internal PgCatchHaulProvider(UserSecurityContext context)
        {
            this.Context = context;
            this.fishBuilder = new FishCountBuilder(context);
            this.netBuilder = new NetHaulEventBuilder(context);
            this.catchBuilder = new CatchMetricBuilder(context);
        }
    }
}
