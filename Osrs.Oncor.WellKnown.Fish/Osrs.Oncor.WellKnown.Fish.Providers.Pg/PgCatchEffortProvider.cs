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
using Osrs.Numerics.Spatial.Geometry;
using Osrs.Oncor.WellKnown.Fish.Module;
using Osrs.Oncor.UserAffiliationPermissionChecks;
using Npgsql;
using Osrs.Data.Postgres;

namespace Osrs.Oncor.WellKnown.Fish.Providers.Pg
{
    public sealed class PgCatchEffortProvider : ICatchEffortProvider
    {
        private readonly CatchEffortBuilder builder;
        private CompoundIdentity lastCatchEffortId = null;
        private bool lastEditPermission = false;


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

        public bool CanDelete(CatchEffort item)
        {
            if (item != null && Db.DataStoreIdentity.Equals(item.Identity.DataStoreIdentity))
            {
                if (this.CanDelete())
                {
                    UserProvider prov = UserAffilationSecurityManager.Instance.GetProvider(this.Context);
                    if (prov != null)
                    {
                        return prov.HasAffiliationForSampleEvent(item.SampleEventId, true);
                    }
                }
            }
            return false;
        }

        internal bool CanDelete(CompoundIdentity catchEffortId)
        {
            if (this.CanDelete())
            {
                if (!catchEffortId.Equals(lastCatchEffortId))
                {
                    this.lastCatchEffortId = catchEffortId;
                    CatchEffort depl = this.Get(catchEffortId);
                    if (depl != null)
                    {
                        this.lastEditPermission = this.CanDelete(depl);
                    }
                    else // can't get the effort, so it will always fail
                    {
                        this.lastEditPermission = false;
                    }

                    return this.lastEditPermission;
                }
                else
                    return this.lastEditPermission; //same as last check
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

        public bool CanUpdate()
        {
            IRoleProvider perms = this.AuthProvider;
            if (perms != null)
            {
                if (this.Context != null && this.Context.User != null)
                {
                    return perms.HasPermission(this.Context.User, FishModuleUtils.FishUpdatePermission);
                }
            }
            return false;
        }

        public bool CanUpdate(CatchEffort item)
        {
            if (item != null)
            {
                if (this.CanUpdate())
                {
                    UserProvider prov = UserAffilationSecurityManager.Instance.GetProvider(this.Context);
                    if (prov != null)
                    {
                        return prov.HasAffiliationForSampleEvent(item.SampleEventId, true);
                    }
                }
            }
            return false;
        }

        public CatchEffort Create(CompoundIdentity sampleEventId, CompoundIdentity siteId, DateTime when, bool isPrivate, string method, string strata, Point2<double> location, float depth, float pH, float temp, float DO, float salinity, float vel)
        {
            return this.Create(sampleEventId, siteId, when, null, isPrivate, method, strata, location, depth, pH, temp, DO, salinity, vel);
        }

        public CatchEffort Create(CompoundIdentity sampleEventId, CompoundIdentity siteId, DateTime when, string description, bool isPrivate, string method, string strata, Point2<double> location, float depth, float pH, float temp, float DO, float salinity, float vel)
        {
            if (!sampleEventId.IsNullOrEmpty() && FishUtils.GlobalMinDate<=when && when<=DateTime.UtcNow && (!siteId.IsNullOrEmpty() || location!=null) && this.CanCreate())
            {
                try //:id, :esid, :eid, :ssid, :sid, :when, :loc, :method, :strata, :depth, :ph, :tmp, :dox, :sal, :vel, :d, :priv
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertFishCatchEfforts;
                    Guid id = Guid.NewGuid();
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("esid", sampleEventId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("eid", sampleEventId.Identity);

                    Db.AddParam(cmd, "ssid", "sid", siteId);
                    cmd.Parameters.AddWithValue("when", Db.FixDate(when));
                    Db.AddParam(cmd, "loc", location);
                    Db.AddParam(cmd, "d", description);
                    cmd.Parameters.AddWithValue("priv", isPrivate);
                    Db.AddParam(cmd, "method", method);
                    Db.AddParam(cmd, "strata", strata);
                    Db.AddParam(cmd, "depth", depth);
                    Db.AddParam(cmd, "ph", pH);
                    Db.AddParam(cmd, "tmp", temp);
                    Db.AddParam(cmd, "dox", DO);
                    Db.AddParam(cmd, "sal", salinity);
                    Db.AddParam(cmd, "vel", vel);

                    Db.ExecuteNonQuery(cmd);

                    return new CatchEffort(new CompoundIdentity(Db.DataStoreIdentity, id), sampleEventId, siteId, when, location, method, strata, depth, pH, temp, DO, salinity, vel, description, isPrivate);
                }
                catch
                { }
            }

            return null;
        }

        public bool Delete(CatchEffort item)
        {
            if (item != null && this.CanDelete(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.DeleteFishCatchEfforts;
                    cmd.Parameters.AddWithValue("id", item.Identity.Identity);
                    Db.ExecuteNonQuery(cmd);

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("ceid", item.Identity.Identity); //used for catches, counts, fish, hauls, etc.

                    cmd.CommandText = Db.DeleteFishCatchMetrics + Db.WhereEffort;
                    Db.ExecuteNonQuery(cmd);

                    cmd.CommandText = Db.DeleteFishCount + Db.WhereEffort;
                    Db.ExecuteNonQuery(cmd);

                    cmd.CommandText = Db.DeleteFishNetHaulEvent + Db.WhereEffort;
                    Db.ExecuteNonQuery(cmd);

                    //--items sub to fish do a where fishid in (select)
                    cmd.CommandText = Db.DeleteFishDiet + Db.WhereEffortIn;
                    Db.ExecuteNonQuery(cmd);

                    cmd.CommandText = Db.DeleteFishGenetics + Db.WhereEffortIn;
                    Db.ExecuteNonQuery(cmd);

                    cmd.CommandText = Db.DeleteFishIdTag + Db.WhereEffortIn;
                    Db.ExecuteNonQuery(cmd);
                    //--end all sub-items, now it's ok to remove the fish itself

                    cmd.CommandText = Db.DeleteFishIndividual + Db.WhereEffort;
                    Db.ExecuteNonQuery(cmd);
                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public IEnumerable<CatchEffort> Get()
        {
            if (this.CanGet())
                return new Enumerable<CatchEffort>(new EnumerableCommand<CatchEffort>(this.builder, Db.SelectFishCatchEfforts, Db.ConnectionString));
            return null;
        }

        public IEnumerable<CatchEffort> Get(IEnumerable<CompoundIdentity> ids)
        {
            if (ids != null && this.CanGet())
            {
                string where = Db.GetWhere(ids, "\"Id\"");
                if (where.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectFishCatchEfforts + Db.Where + where.ToString();
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    List<CatchEffort> permissions = new List<CatchEffort>();
                    try
                    {
                        CatchEffort o;
                        while (rdr.Read())
                        {
                            o = this.builder.Build(rdr);
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

                    return permissions;
                }
                else
                    return new CatchEffort[0]; //empty set
            }
            return null;
        }

        public CatchEffort Get(CompoundIdentity id)
        {
            if (!id.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(id.DataStoreIdentity) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishCatchEfforts + Db.Where + Db.WhereId;
                cmd.Parameters.AddWithValue("id", id.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                CatchEffort o = null;
                if (rdr != null)
                {
                    try
                    {
                        rdr.Read();
                        o = this.builder.Build(rdr);
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
                return o;
            }
            return null;
        }

        public IEnumerable<CatchEffort> Get(DateTime start, DateTime end)
        {
            if (this.CanGet())
            {
                DateTime now = DateTime.UtcNow;
                string sql = null;
                if (start > FishUtils.GlobalMinDate)
                    sql = Db.Where + Db.WhereStart;
                if (end < now)
                {
                    if (sql == null)
                        sql = Db.Where + Db.WhereEnd;
                    else //already have start
                        sql = sql + " AND " + Db.WhereEnd;
                }

                if (sql != null)
                {

                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectFishCatchEfforts + sql;

                    if (start > FishUtils.GlobalMinDate)
                        cmd.Parameters.AddWithValue("start", start);
                    if (end < now)
                        cmd.Parameters.AddWithValue("end", end);

                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    CatchEffort o = null;
                    List<CatchEffort> permissions = new List<CatchEffort>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.builder.Build(rdr);
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
            }
            return null;
        }

        public IEnumerable<CatchEffort> Get(IEnumerable<CompoundIdentity> sampleEventIds, IEnumerable<CompoundIdentity> siteIds)
        {
            if (sampleEventIds != null && siteIds != null && this.CanGet())
            {
                string where = Db.GetWhere(sampleEventIds, "\"EventSystemId\"", "\"EventId\""); //\"EventSystemId\", \"EventId\"
                if (where.Length > 0)
                {
                    string sit = Db.GetWhere(siteIds, "\"SiteSystemId\"", "\"SiteId\"");
                    if (sit.Length > 0)
                    {
                        where = where + " AND " + sit;

                        NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                        cmd.CommandText = Db.SelectFishCatchEfforts + Db.Where + where;
                        NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                        List<CatchEffort> permissions = new List<CatchEffort>();
                        try
                        {
                            CatchEffort o;
                            while (rdr.Read())
                            {
                                o = this.builder.Build(rdr);
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

                        return permissions;
                    }
                    else
                        return new CatchEffort[0]; //empty set
                }
                else
                    return new CatchEffort[0]; //empty set
            }
            return null;
        }

        public IEnumerable<CatchEffort> Get(IEnumerable<CompoundIdentity> ids, DateTime start, DateTime end)
        {
            if (ids != null && this.CanGet())
            {
                string where = Db.GetWhere(ids, "\"Id\"");
                if (where.Length > 0)
                {
                    DateTime now = DateTime.UtcNow;
                    string sql = null;
                    if (start > FishUtils.GlobalMinDate)
                        sql = Db.Where + Db.WhereStart;
                    if (end < now)
                    {
                        if (sql == null)
                            sql = Db.Where + Db.WhereEnd;
                        else //already have start
                            sql = sql + " AND " + Db.WhereEnd;
                    }

                    if (sql != null) //we need to have at least a start or end constraint that's limiting
                    {
                        sql = sql + " AND " + where;

                        NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                        cmd.CommandText = Db.SelectFishCatchEfforts + where;
                        if (start > FishUtils.GlobalMinDate)
                            cmd.Parameters.AddWithValue("start", start);
                        if (end < now)
                            cmd.Parameters.AddWithValue("end", end);
                        NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                        List<CatchEffort> permissions = new List<CatchEffort>();
                        try
                        {
                            CatchEffort o;
                            while (rdr.Read())
                            {
                                o = this.builder.Build(rdr);
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

                        return permissions;
                    }
                }
                else
                    return new CatchEffort[0]; //empty set
            }
            return null;
        }

        public IEnumerable<CatchEffort> Get(IEnumerable<CompoundIdentity> sampleEventIds, IEnumerable<CompoundIdentity> siteIds, DateTime start, DateTime end)
        {
            if (sampleEventIds != null && siteIds != null && this.CanGet())
            {
                string where = Db.GetWhere(sampleEventIds, "\"EventSystemId\"", "\"EventId\""); //\"EventSystemId\", \"EventId\"
                if (where.Length > 0)
                {
                    string sit = Db.GetWhere(siteIds, "\"SiteSystemId\"", "\"SiteId\"");
                    if (sit.Length > 0)
                    {
                        where = where + " AND " + sit;
                        DateTime now = DateTime.UtcNow;
                        string sql = null;
                        if (start > FishUtils.GlobalMinDate)
                            sql = Db.Where + Db.WhereStart;
                        if (end < now)
                        {
                            if (sql == null)
                                sql = Db.Where + Db.WhereEnd;
                            else //already have start
                                sql = sql + " AND " + Db.WhereEnd;
                        }

                        if (sql != null) //we need to have at least a start or end constraint that's limiting
                        {
                            sql = sql + " AND " + where;
                            NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                            cmd.CommandText = Db.SelectFishCatchEfforts + Db.Where + where.ToString();
                            if (start > FishUtils.GlobalMinDate)
                                cmd.Parameters.AddWithValue("start", start);
                            if (end < now)
                                cmd.Parameters.AddWithValue("end", end);
                            NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                            List<CatchEffort> permissions = new List<CatchEffort>();
                            try
                            {
                                CatchEffort o;
                                while (rdr.Read())
                                {
                                    o = this.builder.Build(rdr);
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

                            return permissions;
                        }
                    }
                    else
                        return new CatchEffort[0]; //empty set
                }
                else
                    return new CatchEffort[0]; //empty set
            }
            return null;
        }

        public IEnumerable<CatchEffort> GetForSampleEvent(IEnumerable<CompoundIdentity> sampleEventIds)
        {
            if (sampleEventIds != null && this.CanGet())
            {
                string where = Db.GetWhere(sampleEventIds, "\"EventSystemId\"", "\"EventId\""); //\"EventSystemId\", \"EventId\"
                if (where.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectFishCatchEfforts + Db.Where + where.ToString();
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    List<CatchEffort> permissions = new List<CatchEffort>();
                    try
                    {
                        CatchEffort o;
                        while (rdr.Read())
                        {
                            o = this.builder.Build(rdr);
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

                    return permissions;
                }
                else
                    return new CatchEffort[0]; //empty set
            }
            return null;
        }

        public IEnumerable<CatchEffort> GetForSampleEvent(CompoundIdentity sampleEventId)
        {
            if (!sampleEventId.IsNullOrEmpty() && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishCatchEfforts + Db.Where + Db.WhereEvent;
                cmd.Parameters.AddWithValue("esid", sampleEventId.DataStoreIdentity);
                cmd.Parameters.AddWithValue("eid", sampleEventId.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                CatchEffort o = null;
                List<CatchEffort> permissions = new List<CatchEffort>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.builder.Build(rdr);
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

        public IEnumerable<CatchEffort> GetForSampleEvent(IEnumerable<CompoundIdentity> sampleEventIds, DateTime start, DateTime end)
        {
            if (sampleEventIds != null && this.CanGet())
            {
                string where = Db.GetWhere(sampleEventIds, "\"EventSystemId\"", "\"EventId\""); //\"EventSystemId\", \"EventId\"
                if (where.Length > 0)
                {
                    DateTime now = DateTime.UtcNow;
                    string sql = null;
                    if (start > FishUtils.GlobalMinDate)
                        sql = Db.Where + Db.WhereStart;
                    if (end < now)
                    {
                        if (sql == null)
                            sql = Db.Where + Db.WhereEnd;
                        else //already have start
                            sql = sql + " AND " + Db.WhereEnd;
                    }

                    if (sql != null) //we need to have at least a start or end constraint that's limiting
                    {
                        sql = sql + " AND " + where;
                        NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                        cmd.CommandText = Db.SelectFishCatchEfforts + Db.Where + where.ToString();
                        if (start > FishUtils.GlobalMinDate)
                            cmd.Parameters.AddWithValue("start", start);
                        if (end < now)
                            cmd.Parameters.AddWithValue("end", end);
                        NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                        List<CatchEffort> permissions = new List<CatchEffort>();
                        try
                        {
                            CatchEffort o;
                            while (rdr.Read())
                            {
                                o = this.builder.Build(rdr);
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

                        return permissions;
                    }
                }
                else
                    return new CatchEffort[0]; //empty set
            }
            return null;
        }

        public IEnumerable<CatchEffort> GetForSampleEvent(CompoundIdentity sampleEventId, DateTime start, DateTime end)
        {
            if (!sampleEventId.IsNullOrEmpty() && this.CanGet())
            {
                DateTime now = DateTime.UtcNow;
                string sql = null;
                if (start > FishUtils.GlobalMinDate)
                    sql = Db.Where + Db.WhereStart;
                if (end < now)
                {
                    if (sql == null)
                        sql = Db.Where + Db.WhereEnd;
                    else //already have start
                        sql = sql + " AND " + Db.WhereEnd;
                }

                if (sql != null) //we need to have at least a start or end constraint that's limiting
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectFishCatchEfforts + sql + " AND " + Db.WhereEvent;
                    cmd.Parameters.AddWithValue("esid", sampleEventId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("eid", sampleEventId.Identity);
                    if (start > FishUtils.GlobalMinDate)
                        cmd.Parameters.AddWithValue("start", start);
                    if (end < now)
                        cmd.Parameters.AddWithValue("end", end);
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    CatchEffort o = null;
                    List<CatchEffort> permissions = new List<CatchEffort>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.builder.Build(rdr);
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
            }
            return null;
        }

        public IEnumerable<CatchEffort> GetForSite(IEnumerable<CompoundIdentity> siteIds)
        {
            if (siteIds != null && this.CanGet())
            {
                string where = Db.GetWhere(siteIds, "\"SiteSystemId\"", "\"SiteId\""); //\"SiteSystemId\", \"SiteId\"
                if (where.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectFishCatchEfforts + Db.Where + where.ToString();
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    List<CatchEffort> permissions = new List<CatchEffort>();
                    try
                    {
                        CatchEffort o;
                        while (rdr.Read())
                        {
                            o = this.builder.Build(rdr);
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

                    return permissions;
                }
                else
                    return new CatchEffort[0]; //empty set
            }
            return null;
        }

        public IEnumerable<CatchEffort> GetForSite(CompoundIdentity siteId)
        {
            if (!siteId.IsNullOrEmpty() && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishCatchEfforts + Db.Where + Db.WhereSite;
                cmd.Parameters.AddWithValue("sitesid", siteId.DataStoreIdentity);
                cmd.Parameters.AddWithValue("siteid", siteId.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                CatchEffort o = null;
                List<CatchEffort> permissions = new List<CatchEffort>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.builder.Build(rdr);
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

        public IEnumerable<CatchEffort> GetForSite(IEnumerable<CompoundIdentity> siteIds, DateTime start, DateTime end)
        {
            if (siteIds != null && this.CanGet())
            {
                string where = Db.GetWhere(siteIds, "\"SiteSystemId\"", "\"SiteId\"");
                if (where.Length > 0)
                {
                    DateTime now = DateTime.UtcNow;
                    string sql = null;
                    if (start > FishUtils.GlobalMinDate)
                        sql = Db.Where + Db.WhereStart;
                    if (end < now)
                    {
                        if (sql == null)
                            sql = Db.Where + Db.WhereEnd;
                        else //already have start
                            sql = sql + " AND " + Db.WhereEnd;
                    }

                    if (sql != null) //we need to have at least a start or end constraint that's limiting
                    {
                        sql = sql + " AND " + where;
                        NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                        cmd.CommandText = Db.SelectFishCatchEfforts + Db.Where + where.ToString();
                        if (start > FishUtils.GlobalMinDate)
                            cmd.Parameters.AddWithValue("start", start);
                        if (end < now)
                            cmd.Parameters.AddWithValue("end", end);
                        NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                        List<CatchEffort> permissions = new List<CatchEffort>();
                        try
                        {
                            CatchEffort o;
                            while (rdr.Read())
                            {
                                o = this.builder.Build(rdr);
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

                        return permissions;
                    }
                }
                else
                    return new CatchEffort[0]; //empty set
            }
            return null;
        }

        public IEnumerable<CatchEffort> GetForSite(CompoundIdentity siteId, DateTime start, DateTime end)
        {
            if (!siteId.IsNullOrEmpty() && this.CanGet())
            {
                DateTime now = DateTime.UtcNow;
                string sql = null;
                if (start > FishUtils.GlobalMinDate)
                    sql = Db.Where + Db.WhereStart;
                if (end < now)
                {
                    if (sql == null)
                        sql = Db.Where + Db.WhereEnd;
                    else //already have start
                        sql = sql + " AND " + Db.WhereEnd;
                }

                if (sql != null) //we need to have at least a start or end constraint that's limiting
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectFishCatchEfforts + sql + " AND " + Db.WhereSite;
                    cmd.Parameters.AddWithValue("sitesid", siteId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("siteid", siteId.Identity);
                    if (start > FishUtils.GlobalMinDate)
                        cmd.Parameters.AddWithValue("start", start);
                    if (end < now)
                        cmd.Parameters.AddWithValue("end", end);
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    CatchEffort o = null;
                    List<CatchEffort> permissions = new List<CatchEffort>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.builder.Build(rdr);
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
            }
            return null;
        }

        public bool Update(CatchEffort item)
        {
            if (item != null && this.CanUpdate(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.UpdateFishCatchEfforts;

                    cmd.Parameters.AddWithValue("id", item.Identity.Identity);
                    cmd.Parameters.AddWithValue("esid", item.SampleEventId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("eid", item.SampleEventId.Identity);
                    Db.AddParam(cmd, "ssid", "sid", item.SiteId);
                    Db.AddParam(cmd, "when", item.SampleDate);
                    Db.AddParam(cmd, "loc", item.Location);
                    Db.AddParam(cmd, "method", item.CatchMethod);
                    Db.AddParam(cmd, "strata", item.Strata);
                    Db.AddParam(cmd, "depth", item.Depth);
                    Db.AddParam(cmd, "ph", item.pH);
                    Db.AddParam(cmd, "tmp", item.Temp);
                    Db.AddParam(cmd, "dox", item.DO);
                    Db.AddParam(cmd, "sal", item.Salinity);
                    Db.AddParam(cmd, "vel", item.Velocity);
                    Db.AddParam(cmd, "d", item.Description);

                    cmd.Parameters.AddWithValue("priv", item.IsPrivate);

                    Db.ExecuteNonQuery(cmd);

                    return true;
                }
                catch
                { }
            }
            return false;
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

        internal PgCatchEffortProvider(UserSecurityContext context)
        {
            this.Context = context;
            this.builder = new CatchEffortBuilder(context);
        }
    }
}
