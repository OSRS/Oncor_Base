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
using Osrs.Oncor.WellKnown.WaterQuality.Module;
using Npgsql;
using Osrs.Data.Postgres;
using Osrs.Oncor.UserAffiliationPermissionChecks;

namespace Osrs.Oncor.WellKnown.WaterQuality.Providers.Pg
{
    //TODO -- add user level checking on all GET methods -- currently edit methods are doing the check
    // likely easiest is to modify builders to be non-singleton with usercontext as an internal property to allow checking of affiliation at that level per record.
    public sealed class PgWQDeploymentProvider : IWQDeploymentProvider
    {
        private readonly WQDeploymentBuilder builder;

        public bool CanGet()
        {
            IRoleProvider perms = this.AuthProvider;
            if (perms != null)
            {
                if (this.Context != null && this.Context.User != null)
                {
                    return perms.HasPermission(this.Context.User, WQModuleUtils.WQGetPermission);
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
                    return perms.HasPermission(this.Context.User, WQModuleUtils.WQUpdatePermission);
                }
            }
            return false;
        }

        public bool CanUpdate(WaterQualityDeployment item)
        {
            if (item!=null)
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

        public bool CanDelete()
        {
            IRoleProvider perms = this.AuthProvider;
            if (perms != null)
            {
                if (this.Context != null && this.Context.User != null)
                {
                    return perms.HasPermission(this.Context.User, WQModuleUtils.WQDeletePermission);
                }
            }
            return false;
        }

        public bool CanDelete(WaterQualityDeployment item)
        {
            if (item != null)
            {
                if(this.CanDelete())
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

        public bool CanCreate()
        {
            IRoleProvider perms = this.AuthProvider;
            if (perms != null)
            {
                if (this.Context != null && this.Context.User != null)
                {
                    return perms.HasPermission(this.Context.User, WQModuleUtils.WQCreatePermission);
                }
            }
            return false;
        }

        public bool Update(WaterQualityDeployment item)
        {
            if (item != null && this.CanUpdate(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.UpdateDeployment;
                    cmd.Parameters.AddWithValue("sid", item.Identity.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("id", item.Identity.Identity);
                    cmd.Parameters.AddWithValue("n", item.Name);
                    cmd.Parameters.AddWithValue("esid", item.SampleEventId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("eid", item.SampleEventId.Identity);
                    cmd.Parameters.AddWithValue("sitesid", item.SiteId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("siteid", item.SiteId.Identity);
                    cmd.Parameters.AddWithValue("isid", item.SensorId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("iid", item.SensorId.Identity);

                    if (item.Range.StartDate.HasValue)
                        cmd.Parameters.AddWithValue("start", item.Range.StartDate.Value);
                    else
                        cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam("start", NpgsqlTypes.NpgsqlDbType.TimestampTZ));
                    if (item.Range.EndDate.HasValue)
                        cmd.Parameters.AddWithValue("end", item.Range.EndDate.Value);
                    else
                        cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam("end", NpgsqlTypes.NpgsqlDbType.TimestampTZ));

                    if (string.IsNullOrEmpty(item.Description))
                        cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam("d", NpgsqlTypes.NpgsqlDbType.Varchar));
                    else
                        cmd.Parameters.AddWithValue("d", item.Description);

                    cmd.Parameters.AddWithValue("priv", item.IsPrivate);

                    Db.ExecuteNonQuery(cmd);

                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public bool Delete(WaterQualityDeployment item)
        {
            if (item!=null && this.CanDelete(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.DeleteDeployment;
                    cmd.Parameters.AddWithValue("sid", item.Identity.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("id", item.Identity.Identity);
                    Db.ExecuteNonQuery(cmd);

                    cmd.CommandText = Db.DeleteMeasurement; //remove all measurements in this deployment
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("dsid", item.Identity.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("did", item.Identity.Identity);
                    Db.ExecuteNonQuery(cmd);

                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public WaterQualityDeployment Create(string name, CompoundIdentity sampleEventId, CompoundIdentity siteId, CompoundIdentity sensorId, DateRange range, bool isPrivate)
        {
            return Create(name, sampleEventId, siteId, sensorId, range, null, isPrivate);
        }

        public WaterQualityDeployment Create(string name, CompoundIdentity sampleEventId, CompoundIdentity siteId, CompoundIdentity sensorId, DateRange range, string description, bool isPrivate)
        {
            if (!string.IsNullOrEmpty(name) && !sampleEventId.IsNullOrEmpty() && !siteId.IsNullOrEmpty() && !sensorId.IsNullOrEmpty() && range!=null && this.CanCreate())
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertDeployment;
                    Guid id = Guid.NewGuid();
                    cmd.Parameters.AddWithValue("sid", Db.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("n", name);
                    cmd.Parameters.AddWithValue("esid", sampleEventId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("eid", sampleEventId.Identity);
                    cmd.Parameters.AddWithValue("sitesid", siteId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("siteid", siteId.Identity);
                    cmd.Parameters.AddWithValue("isid", sensorId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("iid", sensorId.Identity);

                    if (range.StartDate.HasValue)
                        cmd.Parameters.AddWithValue("start", range.StartDate.Value);
                    else
                        cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam("start", NpgsqlTypes.NpgsqlDbType.TimestampTZ));
                    if (range.EndDate.HasValue)
                        cmd.Parameters.AddWithValue("end", range.EndDate.Value);
                    else
                        cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam("end", NpgsqlTypes.NpgsqlDbType.TimestampTZ));

                    if (string.IsNullOrEmpty(description))
                        cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam("d", NpgsqlTypes.NpgsqlDbType.Varchar));
                    else
                        cmd.Parameters.AddWithValue("d", description);

                    cmd.Parameters.AddWithValue("priv", isPrivate);

                    Db.ExecuteNonQuery(cmd);

                    return new WaterQualityDeployment(new CompoundIdentity(Db.DataStoreIdentity, id), name, sampleEventId, siteId, sensorId, range, description, isPrivate);
                }
                catch
                { }
            }
            return null;
        }

        public IEnumerable<WaterQualityDeployment> Get()
        {
            if (this.CanGet())
                return new Enumerable<WaterQualityDeployment>(new EnumerableCommand<WaterQualityDeployment>(this.builder, Db.SelectDeployment, Db.ConnectionString));
            return null;
        }

        public IEnumerable<WaterQualityDeployment> Get(string name)
        {
            return Get(name, StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<WaterQualityDeployment> Get(IEnumerable<CompoundIdentity> ids)
        {
            if (ids != null && this.CanGet())
            {
                string where = Db.GetWhere(ids, "\"SystemId\"", "\"Id\"");
                if (where.Length>0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectDeployment + Db.Where + where.ToString();
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    List<WaterQualityDeployment> permissions = new List<WaterQualityDeployment>();
                    try
                    {
                        WaterQualityDeployment o;
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
                    return new WaterQualityDeployment[0]; //empty set
            }
            return null;
        }

        public WaterQualityDeployment Get(CompoundIdentity id)
        {
            if (!id.IsNullOrEmpty() && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectDeployment + Db.Where + Db.WhereId;
                cmd.Parameters.AddWithValue("sid", id.DataStoreIdentity);
                cmd.Parameters.AddWithValue("id", id.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                WaterQualityDeployment o = null;
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

        public IEnumerable<WaterQualityDeployment> Get(DateTime start, DateTime end)
        {
            if (this.CanGet())
            {
                DateTime now = DateTime.UtcNow;
                string sql = null;
                if (start > WQUtils.GlobalMinDate)
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
                    cmd.CommandText = Db.SelectDeployment + sql;

                    if (start > WQUtils.GlobalMinDate)
                        cmd.Parameters.AddWithValue("start", start);
                    if (end < now)
                        cmd.Parameters.AddWithValue("end", end);

                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    WaterQualityDeployment o = null;
                    List<WaterQualityDeployment> permissions = new List<WaterQualityDeployment>();
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

        public IEnumerable<WaterQualityDeployment> Get(string name, StringComparison comparisonOption)
        {
            if (!string.IsNullOrEmpty(name) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectDeployment + Db.Where + Db.GetWhere(name, comparisonOption);
                cmd.Parameters.AddWithValue("name", name);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                List<WaterQualityDeployment> permissions = new List<WaterQualityDeployment>();
                try
                {
                    WaterQualityDeployment o;
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
            return null;
        }

        public IEnumerable<WaterQualityDeployment> Get(IEnumerable<CompoundIdentity> ids, DateTime start, DateTime end)
        {
            if (ids != null && this.CanGet())
            {
                string where = Db.GetWhere(ids, "\"SystemId\"", "\"Id\"");
                if (where.Length > 0)
                {
                    DateTime now = DateTime.UtcNow;
                    string sql = null;
                    if (start > WQUtils.GlobalMinDate)
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
                        cmd.CommandText = Db.SelectDeployment + where;
                        if (start > WQUtils.GlobalMinDate)
                            cmd.Parameters.AddWithValue("start", start);
                        if (end < now)
                            cmd.Parameters.AddWithValue("end", end);
                        NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                        List<WaterQualityDeployment> permissions = new List<WaterQualityDeployment>();
                        try
                        {
                            WaterQualityDeployment o;
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
                    return new WaterQualityDeployment[0]; //empty set
            }
            return null;
        }

        public IEnumerable<WaterQualityDeployment> Get(string name, DateTime start, DateTime end)
        {
            return Get(name, StringComparison.OrdinalIgnoreCase, start, end);
        }

        public IEnumerable<WaterQualityDeployment> Get(string name, StringComparison comparisonOption, DateTime start, DateTime end)
        {
            if (!string.IsNullOrEmpty(name) && this.CanGet())
            {
                DateTime now = DateTime.UtcNow;
                string sql = null;
                if (start > WQUtils.GlobalMinDate)
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
                    sql = sql + " AND ";

                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectDeployment + sql + Db.GetWhere(name, comparisonOption);
                    cmd.Parameters.AddWithValue("name", name);
                    if (start > WQUtils.GlobalMinDate)
                        cmd.Parameters.AddWithValue("start", start);
                    if (end < now)
                        cmd.Parameters.AddWithValue("end", end);
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    List<WaterQualityDeployment> permissions = new List<WaterQualityDeployment>();
                    try
                    {
                        WaterQualityDeployment o;
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
            return null;
        }

        public IEnumerable<WaterQualityDeployment> GetForSampleEvent(IEnumerable<CompoundIdentity> sampleEventIds)
        {
            if (sampleEventIds != null && this.CanGet())
            {
                string where = Db.GetWhere(sampleEventIds, "\"EventSystemId\"", "\"EventId\""); //\"EventSystemId\", \"EventId\"
                if (where.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectDeployment + Db.Where + where.ToString();
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    List<WaterQualityDeployment> permissions = new List<WaterQualityDeployment>();
                    try
                    {
                        WaterQualityDeployment o;
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
                    return new WaterQualityDeployment[0]; //empty set
            }
            return null;
        }

        public IEnumerable<WaterQualityDeployment> GetForSampleEvent(CompoundIdentity sampleEventId)
        {
            if (!sampleEventId.IsNullOrEmpty() && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectDeployment + Db.Where + Db.WhereEvent;
                cmd.Parameters.AddWithValue("esid", sampleEventId.DataStoreIdentity);
                cmd.Parameters.AddWithValue("eid", sampleEventId.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                WaterQualityDeployment o = null;
                List<WaterQualityDeployment> permissions = new List<WaterQualityDeployment>();
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

        public IEnumerable<WaterQualityDeployment> GetForSampleEvent(IEnumerable<CompoundIdentity> sampleEventIds, DateTime start, DateTime end)
        {
            if (sampleEventIds != null && this.CanGet())
            {
                string where = Db.GetWhere(sampleEventIds, "\"EventSystemId\"", "\"EventId\""); //\"EventSystemId\", \"EventId\"
                if (where.Length > 0)
                {
                    DateTime now = DateTime.UtcNow;
                    string sql = null;
                    if (start > WQUtils.GlobalMinDate)
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
                        cmd.CommandText = Db.SelectDeployment + Db.Where + where.ToString();
                        if (start > WQUtils.GlobalMinDate)
                            cmd.Parameters.AddWithValue("start", start);
                        if (end < now)
                            cmd.Parameters.AddWithValue("end", end);
                        NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                        List<WaterQualityDeployment> permissions = new List<WaterQualityDeployment>();
                        try
                        {
                            WaterQualityDeployment o;
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
                    return new WaterQualityDeployment[0]; //empty set
            }
            return null;
        }

        public IEnumerable<WaterQualityDeployment> GetForSampleEvent(CompoundIdentity sampleEventId, DateTime start, DateTime end)
        {
            if (!sampleEventId.IsNullOrEmpty() && this.CanGet())
            {
                DateTime now = DateTime.UtcNow;
                string sql = null;
                if (start > WQUtils.GlobalMinDate)
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
                    cmd.CommandText = Db.SelectDeployment + sql + " AND " + Db.WhereEvent;
                    cmd.Parameters.AddWithValue("esid", sampleEventId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("eid", sampleEventId.Identity);
                    if (start > WQUtils.GlobalMinDate)
                        cmd.Parameters.AddWithValue("start", start);
                    if (end < now)
                        cmd.Parameters.AddWithValue("end", end);
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    WaterQualityDeployment o = null;
                    List<WaterQualityDeployment> permissions = new List<WaterQualityDeployment>();
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

        public IEnumerable<WaterQualityDeployment> GetForSite(IEnumerable<CompoundIdentity> siteIds)
        {
            if (siteIds != null && this.CanGet())
            {
                string where = Db.GetWhere(siteIds, "\"SiteSystemId\"", "\"SiteId\""); //\"SiteSystemId\", \"SiteId\"
                if (where.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectDeployment + Db.Where + where.ToString();
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    List<WaterQualityDeployment> permissions = new List<WaterQualityDeployment>();
                    try
                    {
                        WaterQualityDeployment o;
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
                    return new WaterQualityDeployment[0]; //empty set
            }
            return null;
        }

        public IEnumerable<WaterQualityDeployment> GetForSite(CompoundIdentity siteId)
        {
            if (!siteId.IsNullOrEmpty() && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectDeployment + Db.Where + Db.WhereSite;
                cmd.Parameters.AddWithValue("sitesid", siteId.DataStoreIdentity);
                cmd.Parameters.AddWithValue("siteid", siteId.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                WaterQualityDeployment o = null;
                List<WaterQualityDeployment> permissions = new List<WaterQualityDeployment>();
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

        public IEnumerable<WaterQualityDeployment> GetForSite(IEnumerable<CompoundIdentity> siteIds, DateTime start, DateTime end)
        {
            if (siteIds != null && this.CanGet())
            {
                string where = Db.GetWhere(siteIds, "\"SiteSystemId\"", "\"SiteId\""); 
                if (where.Length > 0)
                {
                    DateTime now = DateTime.UtcNow;
                    string sql = null;
                    if (start > WQUtils.GlobalMinDate)
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
                        cmd.CommandText = Db.SelectDeployment + Db.Where + where.ToString();
                        if (start > WQUtils.GlobalMinDate)
                            cmd.Parameters.AddWithValue("start", start);
                        if (end < now)
                            cmd.Parameters.AddWithValue("end", end);
                        NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                        List<WaterQualityDeployment> permissions = new List<WaterQualityDeployment>();
                        try
                        {
                            WaterQualityDeployment o;
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
                    return new WaterQualityDeployment[0]; //empty set
            }
            return null;
        }

        public IEnumerable<WaterQualityDeployment> GetForSite(CompoundIdentity siteId, DateTime start, DateTime end)
        {
            if (!siteId.IsNullOrEmpty() && this.CanGet())
            {
                DateTime now = DateTime.UtcNow;
                string sql = null;
                if (start > WQUtils.GlobalMinDate)
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
                    cmd.CommandText = Db.SelectDeployment + sql + " AND " + Db.WhereSite;
                    cmd.Parameters.AddWithValue("sitesid", siteId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("siteid", siteId.Identity);
                    if (start > WQUtils.GlobalMinDate)
                        cmd.Parameters.AddWithValue("start", start);
                    if (end < now)
                        cmd.Parameters.AddWithValue("end", end);
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    WaterQualityDeployment o = null;
                    List<WaterQualityDeployment> permissions = new List<WaterQualityDeployment>();
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

        public IEnumerable<WaterQualityDeployment> Get(IEnumerable<CompoundIdentity> sampleEventIds, IEnumerable<CompoundIdentity> siteIds, DateTime start, DateTime end)
        {
            if (sampleEventIds != null && siteIds!=null && this.CanGet())
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
                        if (start > WQUtils.GlobalMinDate)
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
                            cmd.CommandText = Db.SelectDeployment + Db.Where + where.ToString();
                            if (start > WQUtils.GlobalMinDate)
                                cmd.Parameters.AddWithValue("start", start);
                            if (end < now)
                                cmd.Parameters.AddWithValue("end", end);
                            NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                            List<WaterQualityDeployment> permissions = new List<WaterQualityDeployment>();
                            try
                            {
                                WaterQualityDeployment o;
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
                        return new WaterQualityDeployment[0]; //empty set
                }
                else
                    return new WaterQualityDeployment[0]; //empty set
            }
            return null;
        }

        public IEnumerable<WaterQualityDeployment> Get(IEnumerable<CompoundIdentity> sampleEventIds, IEnumerable<CompoundIdentity> siteIds)
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
                        cmd.CommandText = Db.SelectDeployment + Db.Where + where;
                        NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                        List<WaterQualityDeployment> permissions = new List<WaterQualityDeployment>();
                        try
                        {
                            WaterQualityDeployment o;
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
                        return new WaterQualityDeployment[0]; //empty set
                }
                else
                    return new WaterQualityDeployment[0]; //empty set
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

        internal PgWQDeploymentProvider(UserSecurityContext context)
        {
            this.Context = context;
            this.builder = new WQDeploymentBuilder(context);
        }
    }
}
