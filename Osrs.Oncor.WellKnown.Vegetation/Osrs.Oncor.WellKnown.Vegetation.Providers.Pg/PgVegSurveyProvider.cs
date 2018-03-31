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
using Osrs.Numerics.Spatial.Geometry;
using Osrs.Security;
using Osrs.Security.Authorization;
using Osrs.Oncor.WellKnown.Vegetation.Module;
using Osrs.Oncor.UserAffiliationPermissionChecks;
using Npgsql;
using Osrs.Data.Postgres;

namespace Osrs.Oncor.WellKnown.Vegetation.Providers.Pg
{
    public sealed class PgVegSurveyProvider : IVegSurveyProvider
    {
        private CompoundIdentity lastVegSurveyId = null;
        private bool lastEditPermission = false;
        private CompoundIdentity lastUpdateVegSurveyId = null;
        private bool lastUpdateEditPermission = false;

        public bool CanCreate()
        {
            IRoleProvider perms = this.AuthProvider;
            if (perms != null)
            {
                if (this.Context != null && this.Context.User != null)
                {
                    return perms.HasPermission(this.Context.User, VegModuleUtils.CreatePermission);
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
                    return perms.HasPermission(this.Context.User, VegModuleUtils.DeletePermission);
                }
            }
            return false;
        }

        public bool CanDelete(VegPlotType item)
        {
            if (item != null && Db.DataStoreIdentity.Equals(item.Identity.DataStoreIdentity))
            {
                return this.CanDelete();
            }
            return false;
        }

        public bool CanDelete(VegSurvey item)
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

        internal bool CanDelete(CompoundIdentity vegSurveyId)
        {
            if (this.CanDelete())
            {
                if (!vegSurveyId.Equals(lastVegSurveyId))
                {
                    this.lastVegSurveyId = vegSurveyId;
                    VegSurvey depl = this.GetSurvey(vegSurveyId);
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
                    return perms.HasPermission(this.Context.User, VegModuleUtils.GetPermission);
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
                    return perms.HasPermission(this.Context.User, VegModuleUtils.UpdatePermission);
                }
            }
            return false;
        }

        public bool CanUpdate(VegPlotType item)
        {
            if (item != null && Db.DataStoreIdentity.Equals(item.Identity.DataStoreIdentity))
            {
                return this.CanUpdate();
            }
            return false;
        }

        public bool CanUpdate(VegSurvey item)
        {
            if (item != null)
            {
                if (this.CanUpdate() && Db.DataStoreIdentity.Equals(item.Identity.DataStoreIdentity))
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

        internal bool CanUpdate(CompoundIdentity vegSurveyId)
        {
            if (this.CanUpdate())
            {
                if (!vegSurveyId.Equals(lastUpdateVegSurveyId))
                {
                    this.lastUpdateVegSurveyId = vegSurveyId;
                    VegSurvey depl = this.GetSurvey(vegSurveyId);
                    if (depl != null)
                    {
                        this.lastUpdateEditPermission = this.CanUpdate(depl);
                    }
                    else // can't get the effort, so it will always fail
                    {
                        this.lastUpdateEditPermission = false;
                    }

                    return this.lastUpdateEditPermission;
                }
                else
                    return this.lastUpdateEditPermission; //same as last check
            }
            return false;
        }

        public VegPlotType Create(string name)
        {
            return Create(name, null);
        }

        public VegPlotType Create(string name, string description)
        {
            if (!string.IsNullOrEmpty(name) && this.CanCreate())
            {
                try //:id, :n, :d
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertPlotType;
                    Guid id = Guid.NewGuid();
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("n", name);
                    Db.AddParam(cmd, "d", description);

                    Db.ExecuteNonQuery(cmd);

                    return new VegPlotType(new CompoundIdentity(Db.DataStoreIdentity, id), name, description);
                }
                catch
                { }
            }

            return null;
        }

        public VegSurvey Create(CompoundIdentity sampleEventId, CompoundIdentity siteId, CompoundIdentity plotTypeId, Point2<double> location, float area, float minElev, float maxElev, bool isPrivate)
        {
            return Create(sampleEventId, siteId, plotTypeId, location, area, minElev, maxElev, null, isPrivate);
        }

        public VegSurvey Create(CompoundIdentity sampleEventId, CompoundIdentity siteId, CompoundIdentity plotTypeId, Point2<double> location, float area, float minElev, float maxElev, string description, bool isPrivate)
        {
            if (!sampleEventId.IsNullOrEmpty() && (!siteId.IsNullOrEmpty() || location != null) && this.CanCreate())
            {
                try //:id,:esid,:eid,:sitesid,:siteid,:ptid,:loc,:area,:eMin,:eMax,:d,:private
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertSurvey;
                    Guid id = Guid.NewGuid();
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("esid", sampleEventId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("eid", sampleEventId.Identity);

                    Db.AddParam(cmd, "sitesid", "siteid", siteId);
                    Db.AddParam(cmd, "ptid", plotTypeId);
                    Db.AddParam(cmd, "loc", location);

                    Db.AddParam(cmd, "area", area);
                    Db.AddParam(cmd, "eMin", minElev);
                    Db.AddParam(cmd, "eMax", maxElev);
                    Db.AddParam(cmd, "d", description);
                    cmd.Parameters.AddWithValue("private", isPrivate);


                    Db.ExecuteNonQuery(cmd);

                    return new VegSurvey(new CompoundIdentity(Db.DataStoreIdentity, id), sampleEventId, siteId, plotTypeId, location, area, minElev, maxElev, description, isPrivate);
                }
                catch
                { }
            }

            return null;
        }

        public bool Delete(VegPlotType item)
        {
            if (item != null && this.CanDelete(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.DeletePlotType;
                    cmd.Parameters.AddWithValue("id", item.Identity.Identity);
                    Db.ExecuteNonQuery(cmd);
                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public bool Delete(VegSurvey item)
        {
            if (item != null && this.CanDelete(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.DeleteSurvey;
                    cmd.Parameters.AddWithValue("id", item.Identity.Identity);
                    Db.ExecuteNonQuery(cmd);

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("vsid", item.Identity.Identity); //used for veg samples, etc.

                    //--items sub to fish do a where fishid in (select)
                    cmd.CommandText = Db.DeleteHerbSample + Db.WhereVegSurveyIn;
                    Db.ExecuteNonQuery(cmd);

                    cmd.CommandText = Db.DeleteTreeSample + Db.WhereVegSurveyIn;
                    Db.ExecuteNonQuery(cmd);

                    cmd.CommandText = Db.DeleteShrubSample + Db.WhereVegSurveyIn;
                    Db.ExecuteNonQuery(cmd);
                    //--end all sub-items, now it's ok to remove the fish itself

                    cmd.CommandText = Db.DeleteVegSample + Db.WhereVegSurvey;
                    Db.ExecuteNonQuery(cmd);
                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public bool DeletePlotType(CompoundIdentity id)
        {
            VegPlotType tmp = this.GetPlotType(id);
            if (tmp != null)
                return Delete(tmp);
            return false;
        }

        public bool DeleteSurvey(CompoundIdentity id)
        {
            VegSurvey tmp = this.GetSurvey(id);
            if (tmp != null)
                return Delete(tmp);
            return false;
        }

        public IEnumerable<VegSurvey> GetSurvey()
        {
            if (this.CanGet())
                return new Enumerable<VegSurvey>(new EnumerableCommand<VegSurvey>(this.surveyBuilder, Db.SelectSurvey, Db.ConnectionString));
            return null;
        }

        public VegSurvey GetSurvey(CompoundIdentity id)
        {
            if (!id.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(id.DataStoreIdentity) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectSurvey + Db.Where + Db.WhereId;
                cmd.Parameters.AddWithValue("id", id.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                VegSurvey o = null;
                if (rdr != null)
                {
                    try
                    {
                        rdr.Read();
                        o = this.surveyBuilder.Build(rdr);
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

        public IEnumerable<VegSurvey> Get(IEnumerable<CompoundIdentity> ids)
        {
            if (ids != null && this.CanGet())
            {
                string where = Db.GetWhere(ids, "\"Id\"");
                if (where.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectSurvey + Db.Where + where.ToString();
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    List<VegSurvey> permissions = new List<VegSurvey>();
                    try
                    {
                        VegSurvey o;
                        while (rdr.Read())
                        {
                            o = this.surveyBuilder.Build(rdr);
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
                    return new VegSurvey[0]; //empty set
            }
            return null;
        }

        public IEnumerable<VegSurvey> Get(IEnumerable<CompoundIdentity> sampleEventIds, IEnumerable<CompoundIdentity> siteIds)
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
                        cmd.CommandText = Db.SelectSurvey + Db.Where + where;
                        NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                        List<VegSurvey> permissions = new List<VegSurvey>();
                        try
                        {
                            VegSurvey o;
                            while (rdr.Read())
                            {
                                o = this.surveyBuilder.Build(rdr);
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
                        return new VegSurvey[0]; //empty set
                }
                else
                    return new VegSurvey[0]; //empty set
            }
            return null;
        }

        public IEnumerable<VegSurvey> GetForSampleEvent(IEnumerable<CompoundIdentity> sampleEventIds)
        {
            if (sampleEventIds != null && this.CanGet())
            {
                string where = Db.GetWhere(sampleEventIds, "\"EventSystemId\"", "\"EventId\""); //\"EventSystemId\", \"EventId\"
                if (where.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectSurvey + Db.Where + where.ToString();
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    List<VegSurvey> permissions = new List<VegSurvey>();
                    try
                    {
                        VegSurvey o;
                        while (rdr.Read())
                        {
                            o = this.surveyBuilder.Build(rdr);
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
                    return new VegSurvey[0]; //empty set
            }
            return null;
        }

        public IEnumerable<VegSurvey> GetForSampleEvent(CompoundIdentity sampleEventId)
        {
            if (!sampleEventId.IsNullOrEmpty() && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectSurvey + Db.Where + Db.WhereEvent;
                cmd.Parameters.AddWithValue("esid", sampleEventId.DataStoreIdentity);
                cmd.Parameters.AddWithValue("eid", sampleEventId.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                VegSurvey o = null;
                List<VegSurvey> permissions = new List<VegSurvey>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.surveyBuilder.Build(rdr);
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

        public IEnumerable<VegSurvey> GetForSite(IEnumerable<CompoundIdentity> siteIds)
        {
            if (siteIds != null && this.CanGet())
            {
                string where = Db.GetWhere(siteIds, "\"SiteSystemId\"", "\"SiteId\""); //\"SiteSystemId\", \"SiteId\"
                if (where.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectSurvey + Db.Where + where.ToString();
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    List<VegSurvey> permissions = new List<VegSurvey>();
                    try
                    {
                        VegSurvey o;
                        while (rdr.Read())
                        {
                            o = this.surveyBuilder.Build(rdr);
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
                    return new VegSurvey[0]; //empty set
            }
            return null;
        }

        public IEnumerable<VegSurvey> GetForSite(CompoundIdentity siteId)
        {
            if (!siteId.IsNullOrEmpty() && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectSurvey + Db.Where + Db.WhereSite;
                cmd.Parameters.AddWithValue("sitesid", siteId.DataStoreIdentity);
                cmd.Parameters.AddWithValue("siteid", siteId.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                VegSurvey o = null;
                List<VegSurvey> permissions = new List<VegSurvey>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.surveyBuilder.Build(rdr);
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

        public IEnumerable<VegPlotType> GetPlotType()
        {
            if (this.CanGet())
                return new Enumerable<VegPlotType>(new EnumerableCommand<VegPlotType>(this.plotBuilder, Db.SelectPlotType, Db.ConnectionString));
            return null;
        }

        public IEnumerable<VegPlotType> GetPlotType(IEnumerable<CompoundIdentity> ids)
        {
            if (ids != null && this.CanGet())
            {
                string where = Db.GetWhere(ids, "\"Id\"");
                if (where.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectPlotType + Db.Where + where.ToString();
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    List<VegPlotType> permissions = new List<VegPlotType>();
                    try
                    {
                        VegPlotType o;
                        while (rdr.Read())
                        {
                            o = this.plotBuilder.Build(rdr);
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
                    return new VegPlotType[0]; //empty set
            }
            return null;
        }

        public VegPlotType GetPlotType(CompoundIdentity id)
        {
            if (!id.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(id.DataStoreIdentity) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectPlotType + Db.Where + Db.WhereId;
                cmd.Parameters.AddWithValue("id", id.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                VegPlotType o = null;
                if (rdr != null)
                {
                    try
                    {
                        rdr.Read();
                        o = this.plotBuilder.Build(rdr);
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

        public bool Update(VegPlotType item)
        {
            if (item != null && this.CanUpdate(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.UpdatePlotType;

                    cmd.Parameters.AddWithValue("id", item.Identity.Identity);
                    Db.AddParam(cmd, "n", item.Name);
                    Db.AddParam(cmd, "d", item.Description);

                    Db.ExecuteNonQuery(cmd);

                    return true;
                }
                catch
                { }
            }
            return false;
        }

        //:sitesid, :siteid, :ptid, :loc, :area, :eMin, :eMax, :d, :private WHERE \"Id\"=:id";
        public bool Update(VegSurvey item)
        {
            if (item != null && this.CanUpdate(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.UpdateSurvey;

                    cmd.Parameters.AddWithValue("id", item.Identity.Identity);
                    Db.AddParam(cmd, "sitesid", "siteid", item.SiteId);
                    Db.AddParam(cmd, "ptid", item.PlotTypeId);
                    Db.AddParam(cmd, "loc", item.Location);

                    Db.AddParam(cmd, "area", item.Area);
                    Db.AddParam(cmd, "eMin", "eMax", item.ElevationRange);
                    Db.AddParam(cmd, "d", item.Description);

                    cmd.Parameters.AddWithValue("private", item.IsPrivate);

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

        private readonly VegPlotTypeBuilder plotBuilder;
        private readonly VegSurveyBuilder surveyBuilder;

        internal PgVegSurveyProvider(UserSecurityContext context)
        {
            this.Context = context;
            this.plotBuilder = new VegPlotTypeBuilder(context);
            this.surveyBuilder = new VegSurveyBuilder(context);
        }
    }
}
