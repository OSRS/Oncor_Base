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
using Osrs.Security.Authorization;
using Osrs.Security;
using Osrs.Oncor.WellKnown.Vegetation.Module;
using Npgsql;
using Osrs.Data.Postgres;
using System.Text;

namespace Osrs.Oncor.WellKnown.Vegetation.Providers.Pg
{
    public sealed class PgVegSampleProvider : IVegSampleProvider
    {
        private PgVegSurveyProvider helperProvider;
        private Guid lastSampleId = Guid.Empty;
        private bool lastEditSamplePerm;

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

        public bool CanDelete(ShrubSample item)
        {
            if (item != null)
                return this.CanDelete(item.VegSampleId);
            return false;
        }

        public bool CanDelete(TreeSample item)
        {
            if (item != null)
                return this.CanDelete(item.VegSampleId);
            return false;
        }

        public bool CanDelete(HerbSample item)
        {
            if (item != null)
                return this.CanDelete(item.VegSampleId);
            return false;
        }

        public bool CanDelete(VegSample item)
        {
            if (item != null)
            {
                if (this.helperProvider == null)
                    this.helperProvider = new PgVegSurveyProvider(this.Context);

                return this.helperProvider.CanDelete(item.VegSurveyId);
            }
            return false;
        }

        private bool CanDelete(Guid sampleId)
        {
            if (!Guid.Empty.Equals(sampleId) && this.CanDelete())
            {
                if (!sampleId.Equals(lastSampleId))
                {
                    this.lastSampleId = sampleId;
                    VegSample f = this.Get(sampleId);
                    if (f != null)
                        this.lastEditSamplePerm = this.CanDelete(f);
                    else
                        this.lastEditSamplePerm = false;
                }

                return this.lastEditSamplePerm; //same as last check
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

        public bool CanUpdate(ShrubSample item)
        {
            if (item != null)
                return this.CanUpdate(item.VegSampleId);
            return false;
        }

        public bool CanUpdate(TreeSample item)
        {
            if (item != null)
                return this.CanUpdate(item.VegSampleId);
            return false;
        }

        public bool CanUpdate(HerbSample item)
        {
            if (item != null)
                return this.CanUpdate(item.VegSampleId);
            return false;
        }

        public bool CanUpdate(VegSample item)
        {
            if (item!=null)
            {
                if (this.helperProvider == null)
                    this.helperProvider = new PgVegSurveyProvider(this.Context);

                return this.helperProvider.CanUpdate(item.VegSurveyId);
            }
            return false;
        }

        private bool CanUpdate(Guid sampleId)
        {
            if (!Guid.Empty.Equals(sampleId) && this.CanUpdate())
            {
                if (!sampleId.Equals(lastSampleId))
                {
                    this.lastSampleId = sampleId;
                    VegSample f = this.Get(sampleId);
                    if (f != null)
                        this.lastEditSamplePerm = this.CanUpdate(f);
                    else
                        this.lastEditSamplePerm = false;
                }

                return this.lastEditSamplePerm; //same as last check
            }
            return false;
        }

        //CREATES -------------------------------------------------------------------------------------------

        public IEnumerable<VegSample> Create(VegSamplesDTO items)
        {
            if (items != null)
            {
                if (!items.VegSurveyId.IsNullOrEmpty() && this.CanCreate())
                {
                    List<VegSample> results = new List<VegSample>();
                    if (items.Count > 0)
                    {
                        Dictionary<Guid, VegSampleDTO> loadedItems = new Dictionary<Guid, VegSampleDTO>(); //for items with sub-items

                        int count = 0;
                        StringBuilder sb = new StringBuilder(Db.InsertVegSampleB);
                        NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                        foreach (VegSampleDTO cur in items)
                        {
                            //do inserts in batches of 100 items
                            if (count > 99)
                            {
                                cmd.CommandText = sb.ToString();
                                Db.ExecuteNonQuery(cmd);
                                count = 0;
                                sb.Clear();
                                sb.Append(Db.InsertVegSampleB); //this is "INSERT ... VALUES " -- needs the batches as param sets (a,b,c),(a,b,c),...
                            }

                            if (count > 0)
                                sb.Append(",");
                            count++; //increment the indicator
                            Guid id = Guid.NewGuid();

                            //\"Id\", \"VegSurveyId\", \"SiteSystemId\", \"SiteId\", \"When\", \"PointLocation\", \"ElevMin\", \"ElevMax\"
                            sb.Append("('");
                            sb.Append(id.ToString());
                            sb.Append("','");
                            sb.Append(items.VegSurveyId.Identity);
                            if (cur.SiteId.IsNullOrEmpty())
                            {
                                sb.Append("',NULL,NULL,'");
                            }
                            else
                            {
                                sb.Append("','");
                                sb.Append(cur.SiteId.DataStoreIdentity);
                                sb.Append("','");
                                sb.Append(cur.SiteId.Identity);
                                sb.Append("','");
                            }
                            sb.Append(cur.When.ToString("u"));
                            sb.Append("',");
                            if (cur.Location != null)
                            {
                                sb.Append('\'');
                                sb.Append(WktUtils.ToWkt(GeometryFactory2Double.Instance.ConstructPoint(cur.Location.X, cur.Location.Y)));
                                sb.Append('\'');
                            }
                            else
                            {
                                sb.Append("NULL");
                            }
                            sb.Append(',');
                            if (cur.ElevationRange!=null)
                            {
                                if (!Osrs.Numerics.MathUtils.IsInfiniteOrNaN(cur.ElevationRange.Min))
                                    sb.Append(cur.ElevationRange.Min.ToString());
                                else
                                    sb.Append("NULL");
                                sb.Append(",");

                                if (!Osrs.Numerics.MathUtils.IsInfiniteOrNaN(cur.ElevationRange.Max))
                                    sb.Append(cur.ElevationRange.Max.ToString());
                                else
                                    sb.Append("NULL");
                            }
                            else
                            {
                                sb.Append("NULL");
                                sb.Append(",");
                                sb.Append("NULL");
                            }
                            sb.Append(")");

                            if (cur.HasSamples)
                                loadedItems.Add(id, cur);

                            if (cur.ElevationRange!=null)
                                results.Add(new VegSample(id, items.VegSurveyId, cur.SiteId, cur.When, cur.Location, cur.ElevationRange.Min, cur.ElevationRange.Max));
                            else
                                results.Add(new VegSample(id, items.VegSurveyId, cur.SiteId, cur.When, cur.Location, float.NaN, float.NaN));
                        }
                        if (count > 0) //commit any leftovers
                        {
                            cmd.CommandText = sb.ToString();
                            Db.ExecuteNonQuery(cmd);
                        }

                        if (loadedItems.Count>0)
                        {
                            foreach(KeyValuePair<Guid, VegSampleDTO> cur in loadedItems)
                            {
                                //Create(items.VegSurveyId, cur.Key, cur.Value);
                                if (cur.Value.HasSamples)
                                {
                                    if (cur.Value.HasTrees)
                                        CreateTree(cur.Key, cur.Value.Trees);

                                    if (cur.Value.HasHerbs)
                                        CreateHerb(cur.Key, cur.Value.Herbs);

                                    if (cur.Value.HasShrubs)
                                        CreateShrub(cur.Key, cur.Value.Shrubs);
                                }
                            }
                        }
                    }
                    return results;
                }
            }
            return null;
        }

        public VegSample Create(CompoundIdentity vegSurveyId, VegSampleDTO item)
        {
            if (item!=null && !vegSurveyId.IsNullOrEmpty() && vegSurveyId.DataStoreIdentity.Equals(Db.DataStoreIdentity) && this.CanCreate())
            {
                return Create(vegSurveyId, Guid.NewGuid(), item);
            }

            return null;
        }

        private VegSample Create(CompoundIdentity vegSurveyId, Guid sampleId, VegSampleDTO item)
        {
            if (item != null && !vegSurveyId.IsNullOrEmpty()) //permissions etc already checked
            {
                try //:id,:vsid,:sitesid,:siteid,:when,:loc,:eMin,:eMax
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertVegSample;
                    cmd.Parameters.AddWithValue("id", sampleId);
                    cmd.Parameters.AddWithValue("vsid", vegSurveyId.Identity);

                    Db.AddParam(cmd, "sitesid", "siteid", item.SiteId);
                    cmd.Parameters.AddWithValue("when", Db.FixDate(item.When));
                    Db.AddParam(cmd, "loc", item.Location);
                    if (item.ElevationRange != null)
                    {
                        Db.AddParam(cmd, "eMin", item.ElevationRange.Min);
                        Db.AddParam(cmd, "eMax", item.ElevationRange.Max);
                    }
                    else
                    {
                        Db.AddParam(cmd, "eMin", float.NaN);
                        Db.AddParam(cmd, "eMax", float.NaN);
                    }

                    Db.ExecuteNonQuery(cmd); //inserts the sample

                    if (item.HasSamples)
                    {
                        if (item.HasTrees)
                            CreateTree(sampleId, item.Trees);

                        if (item.HasHerbs)
                            CreateHerb(sampleId, item.Herbs);

                        if (item.HasShrubs)
                            CreateShrub(sampleId, item.Shrubs);
                    }

                    if (item.ElevationRange != null)
                        return new VegSample(sampleId, vegSurveyId, item.SiteId, Db.FixDate(item.When), item.Location, item.ElevationRange.Min, item.ElevationRange.Max);
                    else
                        return new VegSample(sampleId, vegSurveyId, item.SiteId, Db.FixDate(item.When), item.Location, float.NaN, float.NaN);
                }
                catch
                { }
            }

            return null;
        }

        private void CreateHerb(Guid sampleId, IEnumerable<VegHerbSampleDTO> items)
        {
            if (items != null)
            {
                int count = 0;
                StringBuilder sb = new StringBuilder(Db.InsertHerbSampleB);
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                foreach (VegHerbSampleDTO cur in items)
                {
                    //do inserts in batches of 100 items
                    if (count > 99)
                    {
                        cmd.CommandText = sb.ToString();
                        Db.ExecuteNonQuery(cmd);
                        count = 0;
                        sb.Clear();
                        sb.Append(Db.InsertHerbSampleB); //this is "INSERT ... VALUES " -- needs the batches as param sets (a,b,c),(a,b,c),...
                    }

                    if (count > 0)
                        sb.Append(",");
                    count++; //increment the indicator
                    Guid id = Guid.NewGuid();

                    //\"Id\", \"VegSampleId\", \"TaxaUnitSystemId\", \"TaxaUnitId\", \"PercentCover\", \"Description\"
                    //:id,:vsid,:taxasid,:taxaid,:cov,:d
                    sb.Append("('");
                    sb.Append(id.ToString());
                    sb.Append("','");
                    sb.Append(sampleId);
                    sb.Append("','");
                    sb.Append(cur.TaxaUnitId.DataStoreIdentity);
                    sb.Append("','");
                    sb.Append(cur.TaxaUnitId.Identity);
                    sb.Append("',");
                    if (!Osrs.Numerics.MathUtils.IsInfiniteOrNaN(cur.PercentCover))
                        sb.Append(cur.PercentCover);
                    else
                        sb.Append("NULL");
                    sb.Append(",");
                    if (!string.IsNullOrEmpty(cur.Description))
                    {
                        sb.Append('\'');
                        sb.Append(cur.Description.Replace("'","''"));
                        sb.Append('\'');
                    }
                    else
                        sb.Append("NULL");
                    sb.Append(")");
                }
                if (count > 0) //commit any leftovers
                {
                    cmd.CommandText = sb.ToString();
                    Db.ExecuteNonQuery(cmd);
                }
            }
        }
        private void CreateShrub(Guid sampleId, IEnumerable<VegShrubSampleDTO> items)
        {
            if (items != null)
            {
                int count = 0;
                StringBuilder sb = new StringBuilder(Db.InsertShrubSampleB);
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                foreach (VegShrubSampleDTO cur in items)
                {
                    //do inserts in batches of 100 items
                    if (count > 99)
                    {
                        cmd.CommandText = sb.ToString();
                        Db.ExecuteNonQuery(cmd);
                        count = 0;
                        sb.Clear();
                        sb.Append(Db.InsertShrubSampleB); //this is "INSERT ... VALUES " -- needs the batches as param sets (a,b,c),(a,b,c),...
                    }

                    if (count > 0)
                        sb.Append(",");
                    count++; //increment the indicator
                    Guid id = Guid.NewGuid();

                    //\"Id\", \"VegSampleId\", \"TaxaUnitSystemId\", \"TaxaUnitId\", \"SizeClass\", \"Count\", \"Description\"
                    //:id,:vsid,:taxasid,:taxaid,:class,:ct,:d
                    sb.Append("('");
                    sb.Append(id.ToString());
                    sb.Append("','");
                    sb.Append(sampleId);
                    sb.Append("','");
                    sb.Append(cur.TaxaUnitId.DataStoreIdentity);
                    sb.Append("','");
                    sb.Append(cur.TaxaUnitId.Identity);
                    sb.Append("',");
                    if (!string.IsNullOrEmpty(cur.SizeClass))
                    {
                        sb.Append('\'');
                        sb.Append(cur.SizeClass.Replace("'", "''"));
                        sb.Append('\'');
                    }
                    sb.Append(",");
                    sb.Append(cur.Count);
                    sb.Append(",");
                    if (!string.IsNullOrEmpty(cur.Description))
                    {
                        sb.Append('\'');
                        sb.Append(cur.Description.Replace("'", "''"));
                        sb.Append('\'');
                    }
                    else
                        sb.Append("NULL");
                    sb.Append(")");
                }
                if (count > 0) //commit any leftovers
                {
                    cmd.CommandText = sb.ToString();
                    Db.ExecuteNonQuery(cmd);
                }
            }
        }
        private void CreateTree(Guid sampleId, IEnumerable<VegTreeSampleDTO> items)
        {
            if (items != null)
            {
                int count = 0;
                StringBuilder sb = new StringBuilder(Db.InsertTreeSampleB);
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                foreach (VegTreeSampleDTO cur in items)
                {
                    //do inserts in batches of 100 items
                    if (count > 99)
                    {
                        cmd.CommandText = sb.ToString();
                        Db.ExecuteNonQuery(cmd);
                        count = 0;
                        sb.Clear();
                        sb.Append(Db.InsertTreeSampleB); //this is "INSERT ... VALUES " -- needs the batches as param sets (a,b,c),(a,b,c),...
                    }

                    if (count > 0)
                        sb.Append(",");
                    count++; //increment the indicator
                    Guid id = Guid.NewGuid();

                    //\"Id\", \"VegSampleId\", \"TaxaUnitSystemId\", \"TaxaUnitId\", \"Dbh\", \"Description\"
                    //:id,:vsid,:taxasid,:taxaid,:dbh,:d
                    sb.Append("('");
                    sb.Append(id.ToString());
                    sb.Append("','");
                    sb.Append(sampleId);
                    sb.Append("','");
                    sb.Append(cur.TaxaUnitId.DataStoreIdentity);
                    sb.Append("','");
                    sb.Append(cur.TaxaUnitId.Identity);
                    sb.Append("',");
                    if (!Osrs.Numerics.MathUtils.IsInfiniteOrNaN(cur.DiameterBreastHigh))
                        sb.Append(cur.DiameterBreastHigh);
                    else
                        sb.Append("NULL");
                    sb.Append(",");
                    if (!string.IsNullOrEmpty(cur.Description))
                    {
                        sb.Append('\'');
                        sb.Append(cur.Description.Replace("'", "''"));
                        sb.Append('\'');
                    }
                    else
                        sb.Append("NULL");
                    sb.Append(")");
                }
                if (count > 0) //commit any leftovers
                {
                    cmd.CommandText = sb.ToString();
                    Db.ExecuteNonQuery(cmd);
                }
            }
        }


        public VegSample Create(CompoundIdentity vegSurveyId, DateTime when, Point2<double> location)
        {
            return Create(vegSurveyId, null, when, location, float.NaN, float.NaN);
        }

        public VegSample Create(CompoundIdentity vegSurveyId, CompoundIdentity siteId, DateTime when)
        {
            return Create(vegSurveyId, siteId, when, null, float.NaN, float.NaN);
        }

        public VegSample Create(CompoundIdentity vegSurveyId, CompoundIdentity siteId, DateTime when, Point2<double> location)
        {
            return Create(vegSurveyId, siteId, when, location, float.NaN, float.NaN);
        }

        public VegSample Create(CompoundIdentity vegSurveyId, DateTime when, Point2<double> location, float minElev, float maxElev)
        {
            return Create(vegSurveyId, null, when, location, minElev, maxElev);
        }

        public VegSample Create(CompoundIdentity vegSurveyId, CompoundIdentity siteId, DateTime when, float minElev, float maxElev)
        {
            return Create(vegSurveyId, siteId, when, null, minElev, maxElev);
        }

        public VegSample Create(CompoundIdentity vegSurveyId, CompoundIdentity siteId, DateTime when, Point2<double> location, float minElev, float maxElev)
        {
            if (!vegSurveyId.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(vegSurveyId.DataStoreIdentity) && VegUtils.GlobalMinDate <= when && when <= DateTime.UtcNow && (!siteId.IsNullOrEmpty() || location != null) && this.CanCreate())
            {
                try //:id,:vsid,:sitesid,:siteid,:when,:loc,:eMin,:eMax
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertVegSample;
                    Guid id = Guid.NewGuid();
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("vsid", vegSurveyId.Identity);

                    Db.AddParam(cmd, "sitesid", "siteid", siteId);
                    cmd.Parameters.AddWithValue("when", Db.FixDate(when));
                    Db.AddParam(cmd, "loc", location);
                    Db.AddParam(cmd, "eMin", minElev);
                    Db.AddParam(cmd, "eMax", maxElev);

                    Db.ExecuteNonQuery(cmd);

                    return new VegSample(id, vegSurveyId, siteId, when, location, minElev, maxElev);
                }
                catch
                { }
            }

            return null;
        }

        public HerbSample CreateHerb(CompoundIdentity vegSampleId, CompoundIdentity taxaUnitId, float percentCover)
        {
            return CreateHerb(vegSampleId, taxaUnitId, percentCover, null);
        }

        public HerbSample CreateHerb(CompoundIdentity vegSampleId, CompoundIdentity taxaUnitId, float percentCover, string description)
        {
            if (!vegSampleId.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(vegSampleId.DataStoreIdentity) && !taxaUnitId.IsNullOrEmpty() && this.CanCreate())
            {
                try //:id,:vsid,:taxasid,:taxaid,:cov,:d
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertHerbSample;
                    Guid id = Guid.NewGuid();
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("vsid", vegSampleId.Identity);

                    Db.AddParam(cmd, "taxasid", "taxaid", taxaUnitId);
                    Db.AddParam(cmd, "cov", percentCover);
                    Db.AddParam(cmd, "d", description);

                    Db.ExecuteNonQuery(cmd);

                    return new HerbSample(id, vegSampleId.Identity, taxaUnitId, percentCover, description);
                }
                catch
                { }
            }

            return null;
        }

        public ShrubSample CreateShrub(CompoundIdentity vegSampleId, CompoundIdentity taxaUnitId, string sizeClass, uint count)
        {
            return CreateShrub(vegSampleId, taxaUnitId, sizeClass, count, null);
        }

        public ShrubSample CreateShrub(CompoundIdentity vegSampleId, CompoundIdentity taxaUnitId, string sizeClass, uint count, string description)
        {
            if (!vegSampleId.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(vegSampleId.DataStoreIdentity) && !taxaUnitId.IsNullOrEmpty() && this.CanCreate())
            {
                try //:id,:vsid,:taxasid,:taxaid,:class,:ct,:d
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertShrubSample;
                    Guid id = Guid.NewGuid();
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("vsid", vegSampleId.Identity);

                    Db.AddParam(cmd, "taxasid", "taxaid", taxaUnitId);
                    Db.AddParam(cmd, "class", sizeClass);
                    Db.AddParam(cmd, "ct", count);
                    Db.AddParam(cmd, "d", description);

                    Db.ExecuteNonQuery(cmd);

                    return new ShrubSample(id, vegSampleId.Identity, taxaUnitId, sizeClass, count, description);
                }
                catch
                { }
            }

            return null;
        }

        public TreeSample CreateTree(CompoundIdentity vegSampleId, CompoundIdentity taxaUnitId, float dbh)
        {
            return CreateTree(vegSampleId, taxaUnitId, dbh, null);
        }

        public TreeSample CreateTree(CompoundIdentity vegSampleId, CompoundIdentity taxaUnitId, float dbh, string description)
        {
            if (!vegSampleId.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(vegSampleId.DataStoreIdentity) && !taxaUnitId.IsNullOrEmpty() && this.CanCreate())
            {
                try //:id,:vsid,:taxasid,:taxaid,:dbh,:d
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertTreeSample;
                    Guid id = Guid.NewGuid();
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("vsid", vegSampleId.Identity);

                    Db.AddParam(cmd, "taxasid", "taxaid", taxaUnitId);
                    Db.AddParam(cmd, "dbh", dbh);
                    Db.AddParam(cmd, "d", description);

                    Db.ExecuteNonQuery(cmd);

                    return new TreeSample(id, vegSampleId.Identity, taxaUnitId, dbh, description);
                }
                catch
                { }
            }

            return null;
        }

        //DELETES -------------------------------------------------------------------------------------------


        public bool DeleteSample(Guid id)
        {
            VegSample s = Get(id);
            if (s != null)
                return Delete(s);
            return false;
        }

        public bool Delete(VegSample item)
        {
            if (item != null && this.CanDelete(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.Parameters.AddWithValue("vsid", item.Identity);

                    cmd.CommandText = Db.DeleteHerbSample + Db.WhereVegSample;
                    Db.ExecuteNonQuery(cmd);
                    cmd.CommandText = Db.DeleteShrubSample + Db.WhereVegSample;
                    Db.ExecuteNonQuery(cmd);
                    cmd.CommandText = Db.DeleteTreeSample + Db.WhereVegSample;
                    Db.ExecuteNonQuery(cmd);

                    cmd.CommandText = Db.DeleteVegSample + Db.WhereId;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("id", item.Identity);
                    Db.ExecuteNonQuery(cmd);
                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public bool DeleteHerb(Guid id)
        {
            HerbSample s = GetHerb(id);
            if (s != null)
                return DeleteHerb(s);
            return false;
        }

        public bool DeleteHerb(HerbSample item)
        {
            if (item != null && this.CanDelete(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.DeleteHerbSample + Db.WhereId;
                    cmd.Parameters.AddWithValue("id", item.Identity);
                    Db.ExecuteNonQuery(cmd);
                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public bool DeleteShrub(Guid id)
        {
            ShrubSample s = GetShrub(id);
            if (s != null)
                return DeleteShrub(s);
            return false;
        }

        public bool DeleteShrub(ShrubSample item)
        {
            if (item != null && this.CanDelete(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.DeleteShrubSample + Db.WhereId;
                    cmd.Parameters.AddWithValue("id", item.Identity);
                    Db.ExecuteNonQuery(cmd);
                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public bool DeleteTree(Guid id)
        {
            TreeSample s = GetTree(id);
            if (s != null)
                return DeleteTree(s);
            return false;
        }

        public bool DeleteTree(TreeSample item)
        {
            if (item != null && this.CanDelete(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.DeleteTreeSample + Db.WhereId;
                    cmd.Parameters.AddWithValue("id", item.Identity);
                    Db.ExecuteNonQuery(cmd);
                    return true;
                }
                catch
                { }
            }
            return false;
        }


        //GETS -------------------------------------------------------------------------------------------


        public VegSample Get(Guid id)
        {
            if (!Guid.Empty.Equals(id) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectVegSample + Db.Where + Db.WhereId;
                cmd.Parameters.AddWithValue("id", id);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                VegSample o = null;
                if (rdr != null)
                {
                    try
                    {
                        rdr.Read();
                        o = this.sampleBuilder.Build(rdr);
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

        public IEnumerable<VegSample> Get()
        {
            if (this.CanGet())
                return new Enumerable<VegSample>(new EnumerableCommand<VegSample>(this.sampleBuilder, Db.SelectVegSample, Db.ConnectionString));
            return null;
        }

        public IEnumerable<VegSample> Get(IEnumerable<Guid> ids)
        {
            if (ids != null && this.CanGet())
            {
                string wh = Db.GetWhere(ids, "\"Id\"");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectVegSample + Db.Where + wh;
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    VegSample o = null;
                    List<VegSample> items = new List<VegSample>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.sampleBuilder.Build(rdr);
                                if (o != null)
                                    items.Add(o);
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
                    return items;
                }
            }
            return null;
        }

        public IEnumerable<VegSample> Get(IEnumerable<Guid> ids, IEnumerable<CompoundIdentity> siteIds)
        {
            if (ids != null && this.CanGet())
            {
                string wh = Db.GetWhere(ids, "\"Id\"");
                if (wh != null && wh.Length > 0)
                {
                    string whB = Db.GetWhere(siteIds, "SiteSystemId", "SiteId");
                    if (whB != null && whB.Length > 0)
                    {
                        NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                        cmd.CommandText = Db.SelectVegSample + Db.Where + wh + " AND " + whB;
                        NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                        VegSample o = null;
                        List<VegSample> items = new List<VegSample>();
                        if (rdr != null)
                        {
                            try
                            {
                                while (rdr.Read())
                                {
                                    o = this.sampleBuilder.Build(rdr);
                                    if (o != null)
                                        items.Add(o);
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
                        return items;
                    }
                }
            }
            return null;
        }

        public IEnumerable<VegSample> GetForSite(IEnumerable<CompoundIdentity> siteIds)
        {
            if (siteIds != null && this.CanGet())
            {
                string where = Db.GetWhere(siteIds, "\"SiteSystemId\"", "\"SiteId\""); //\"SiteSystemId\", \"SiteId\"
                if (where.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectVegSample + Db.Where + where.ToString();
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    List<VegSample> permissions = new List<VegSample>();
                    try
                    {
                        VegSample o;
                        while (rdr.Read())
                        {
                            o = this.sampleBuilder.Build(rdr);
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
                    return new VegSample[0]; //empty set
            }
            return null;
        }

        public IEnumerable<VegSample> GetForSite(CompoundIdentity siteId)
        {
            if (!siteId.IsNullOrEmpty() && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectVegSample + Db.Where + Db.WhereSite;
                cmd.Parameters.AddWithValue("sitesid", siteId.DataStoreIdentity);
                cmd.Parameters.AddWithValue("siteid", siteId.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                VegSample o = null;
                List<VegSample> permissions = new List<VegSample>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.sampleBuilder.Build(rdr);
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

        public IEnumerable<VegSample> GetForSurvey(IEnumerable<CompoundIdentity> vegSurveyIds)
        {
            if (vegSurveyIds != null && this.CanGet())
            {
                string where = Db.GetWhere(vegSurveyIds, "\"VegSurveyId\"");
                if (where.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectVegSample + Db.Where + where.ToString();
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    List<VegSample> permissions = new List<VegSample>();
                    try
                    {
                        VegSample o;
                        while (rdr.Read())
                        {
                            o = this.sampleBuilder.Build(rdr);
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
                    return new VegSample[0]; //empty set
            }
            return null;
        }

        public IEnumerable<VegSample> GetForSurvey(CompoundIdentity vegSurveyId)
        {
            if (!vegSurveyId.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(vegSurveyId.DataStoreIdentity) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectVegSample + Db.Where + Db.WhereVegSurvey;
                cmd.Parameters.AddWithValue("vsid", vegSurveyId.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                VegSample o = null;
                List<VegSample> permissions = new List<VegSample>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.sampleBuilder.Build(rdr);
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

        //HERB ****************

        public HerbSample GetHerb(Guid id)
        {
            if (!Guid.Empty.Equals(id) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectHerbSample + Db.Where + Db.WhereId;
                cmd.Parameters.AddWithValue("id", id);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                HerbSample o = null;
                if (rdr != null)
                {
                    try
                    {
                        rdr.Read();
                        o = this.herbBuilder.Build(rdr);
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

        public IEnumerable<HerbSample> GetHerb()
        {
            if (this.CanGet())
                return new Enumerable<HerbSample>(new EnumerableCommand<HerbSample>(this.herbBuilder, Db.SelectHerbSample, Db.ConnectionString));
            return null;
        }

        public IEnumerable<HerbSample> GetHerb(IEnumerable<Guid> ids)
        {
            if (ids != null && this.CanGet())
            {
                string wh = Db.GetWhere(ids, "\"Id\"");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectHerbSample + Db.Where + wh;
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    HerbSample o = null;
                    List<HerbSample> items = new List<HerbSample>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.herbBuilder.Build(rdr);
                                if (o != null)
                                    items.Add(o);
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
                    return items;
                }
            }
            return null;
        }

        public IEnumerable<HerbSample> GetHerbForVegSample(IEnumerable<Guid> vegSampleIds)
        {
            if (vegSampleIds != null && this.CanGet())
            {
                string wh = Db.GetWhere(vegSampleIds, "\"VegSampleId\"");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectHerbSample + Db.Where + wh;
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    HerbSample o = null;
                    List<HerbSample> items = new List<HerbSample>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.herbBuilder.Build(rdr);
                                if (o != null)
                                    items.Add(o);
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
                    return items;
                }
            }
            return null;
        }

        public IEnumerable<HerbSample> GetHerbForTaxa(IEnumerable<CompoundIdentity> taxaUnitIds)
        {
            if (taxaUnitIds != null && this.CanGet())
            {
                string where = Db.GetWhere(taxaUnitIds, "\"TaxaUnitSystemId\"", "\"TaxaUnitId\"");
                if (where.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectHerbSample + Db.Where + where.ToString();
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    List<HerbSample> permissions = new List<HerbSample>();
                    try
                    {
                        HerbSample o;
                        while (rdr.Read())
                        {
                            o = this.herbBuilder.Build(rdr);
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
                    return new HerbSample[0]; //empty set
            }
            return null;
        }

        public IEnumerable<HerbSample> GetHerbForTaxa(CompoundIdentity taxaUnitId)
        {
            if (!taxaUnitId.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(taxaUnitId.DataStoreIdentity) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectHerbSample + Db.Where + Db.WhereTaxa;
                cmd.Parameters.AddWithValue("taxasid", taxaUnitId.DataStoreIdentity);
                cmd.Parameters.AddWithValue("taxaid", taxaUnitId.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                HerbSample o = null;
                List<HerbSample> permissions = new List<HerbSample>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.herbBuilder.Build(rdr);
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

        public IEnumerable<HerbSample> GetHerbForTaxa(IEnumerable<Guid> ids, IEnumerable<CompoundIdentity> taxaUnitIds)
        {
            if (ids != null && taxaUnitIds != null && this.CanGet())
            {
                string where = Db.GetWhere(ids, "\"Id\"");
                if (where.Length > 0)
                {
                    string sit = Db.GetWhere(taxaUnitIds, "\"TaxaUnitSystemId\"", "\"TaxaUnitId\"");
                    if (sit.Length > 0)
                    {
                        where = where + " AND " + sit;

                        NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                        cmd.CommandText = Db.SelectHerbSample + Db.Where + where;
                        NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                        List<HerbSample> permissions = new List<HerbSample>();
                        try
                        {
                            HerbSample o;
                            while (rdr.Read())
                            {
                                o = this.herbBuilder.Build(rdr);
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
                        return new HerbSample[0]; //empty set
                }
                else
                    return new HerbSample[0]; //empty set
            }
            return null;
        }

        public IEnumerable<HerbSample> GetHerbForTaxa(IEnumerable<Guid> ids, CompoundIdentity taxaUnitId)
        {
            if (ids != null && !taxaUnitId.IsNullOrEmpty() && this.CanGet())
            {
                string wh = Db.GetWhere(ids, "\"Id\"");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectHerbSample + Db.Where + Db.WhereTaxa + " AND " + wh;
                    Db.AddParam(cmd, "taxasid", "taxaid", taxaUnitId);
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    HerbSample o = null;
                    List<HerbSample> items = new List<HerbSample>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.herbBuilder.Build(rdr);
                                if (o != null)
                                    items.Add(o);
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
                    return items;
                }
            }
            return null;
        }

        //SHRUB ****************

        public ShrubSample GetShrub(Guid id)
        {
            if (!Guid.Empty.Equals(id) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectShrubSample + Db.Where + Db.WhereId;
                cmd.Parameters.AddWithValue("id", id);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                ShrubSample o = null;
                if (rdr != null)
                {
                    try
                    {
                        rdr.Read();
                        o = this.shrubBuilder.Build(rdr);
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

        public IEnumerable<ShrubSample> GetShrub()
        {
            if (this.CanGet())
                return new Enumerable<ShrubSample>(new EnumerableCommand<ShrubSample>(this.shrubBuilder, Db.SelectShrubSample, Db.ConnectionString));
            return null;
        }

        public IEnumerable<ShrubSample> GetShrub(IEnumerable<Guid> ids)
        {
            if (ids != null && this.CanGet())
            {
                string wh = Db.GetWhere(ids, "\"Id\"");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectShrubSample + Db.Where + wh;
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    ShrubSample o = null;
                    List<ShrubSample> items = new List<ShrubSample>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.shrubBuilder.Build(rdr);
                                if (o != null)
                                    items.Add(o);
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
                    return items;
                }
            }
            return null;
        }

        public IEnumerable<ShrubSample> GetShrubForVegSample(IEnumerable<Guid> vegSampleIds)
        {
            if (vegSampleIds != null && this.CanGet())
            {
                string wh = Db.GetWhere(vegSampleIds, "\"VegSampleId\"");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectShrubSample + Db.Where + wh;
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    ShrubSample o = null;
                    List<ShrubSample> items = new List<ShrubSample>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.shrubBuilder.Build(rdr);
                                if (o != null)
                                    items.Add(o);
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
                    return items;
                }
            }
            return null;
        }

        public IEnumerable<ShrubSample> GetShrubForTaxa(IEnumerable<CompoundIdentity> taxaUnitIds)
        {
            if (taxaUnitIds != null && this.CanGet())
            {
                string where = Db.GetWhere(taxaUnitIds, "\"TaxaUnitSystemId\"", "\"TaxaUnitId\"");
                if (where.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectShrubSample + Db.Where + where.ToString();
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    List<ShrubSample> permissions = new List<ShrubSample>();
                    try
                    {
                        ShrubSample o;
                        while (rdr.Read())
                        {
                            o = this.shrubBuilder.Build(rdr);
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
                    return new ShrubSample[0]; //empty set
            }
            return null;
        }

        public IEnumerable<ShrubSample> GetShrubForTaxa(CompoundIdentity taxaUnitId)
        {
            if (!taxaUnitId.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(taxaUnitId.DataStoreIdentity) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectShrubSample + Db.Where + Db.WhereTaxa;
                cmd.Parameters.AddWithValue("taxasid", taxaUnitId.DataStoreIdentity);
                cmd.Parameters.AddWithValue("taxaid", taxaUnitId.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                ShrubSample o = null;
                List<ShrubSample> permissions = new List<ShrubSample>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.shrubBuilder.Build(rdr);
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

        public IEnumerable<ShrubSample> GetShrubForTaxa(IEnumerable<Guid> ids, IEnumerable<CompoundIdentity> taxaUnitIds)
        {
            if (ids != null && taxaUnitIds != null && this.CanGet())
            {
                string where = Db.GetWhere(ids, "\"Id\"");
                if (where.Length > 0)
                {
                    string sit = Db.GetWhere(taxaUnitIds, "\"TaxaUnitSystemId\"", "\"TaxaUnitId\"");
                    if (sit.Length > 0)
                    {
                        where = where + " AND " + sit;

                        NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                        cmd.CommandText = Db.SelectShrubSample + Db.Where + where;
                        NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                        List<ShrubSample> permissions = new List<ShrubSample>();
                        try
                        {
                            ShrubSample o;
                            while (rdr.Read())
                            {
                                o = this.shrubBuilder.Build(rdr);
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
                        return new ShrubSample[0]; //empty set
                }
                else
                    return new ShrubSample[0]; //empty set
            }
            return null;
        }

        public IEnumerable<ShrubSample> GetShrubForTaxa(IEnumerable<Guid> ids, CompoundIdentity taxaUnitId)
        {
            if (ids != null && !taxaUnitId.IsNullOrEmpty() && this.CanGet())
            {
                string wh = Db.GetWhere(ids, "\"Id\"");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectShrubSample + Db.Where + Db.WhereTaxa + " AND " + wh;
                    Db.AddParam(cmd, "taxasid", "taxaid", taxaUnitId);
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    ShrubSample o = null;
                    List<ShrubSample> items = new List<ShrubSample>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.shrubBuilder.Build(rdr);
                                if (o != null)
                                    items.Add(o);
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
                    return items;
                }
            }
            return null;
        }

        //TREE ****************

        public TreeSample GetTree(Guid id)
        {
            if (!Guid.Empty.Equals(id) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectTreeSample + Db.Where + Db.WhereId;
                cmd.Parameters.AddWithValue("id", id);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                TreeSample o = null;
                if (rdr != null)
                {
                    try
                    {
                        rdr.Read();
                        o = this.treeBuilder.Build(rdr);
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

        public IEnumerable<TreeSample> GetTree()
        {
            if (this.CanGet())
                return new Enumerable<TreeSample>(new EnumerableCommand<TreeSample>(this.treeBuilder, Db.SelectTreeSample, Db.ConnectionString));
            return null;
        }

        public IEnumerable<TreeSample> GetTree(IEnumerable<Guid> ids)
        {
            if (ids != null && this.CanGet())
            {
                string wh = Db.GetWhere(ids, "\"Id\"");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectTreeSample + Db.Where + wh;
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    TreeSample o = null;
                    List<TreeSample> items = new List<TreeSample>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.treeBuilder.Build(rdr);
                                if (o != null)
                                    items.Add(o);
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
                    return items;
                }
            }
            return null;
        }

        public IEnumerable<TreeSample> GetTreeForVegSample(IEnumerable<Guid> vegSampleIds)
        {
            if (vegSampleIds != null && this.CanGet())
            {
                string wh = Db.GetWhere(vegSampleIds, "\"VegSampleId\"");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectTreeSample + Db.Where + wh;
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    TreeSample o = null;
                    List<TreeSample> items = new List<TreeSample>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.treeBuilder.Build(rdr);
                                if (o != null)
                                    items.Add(o);
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
                    return items;
                }
            }
            return null;
        }

        public IEnumerable<TreeSample> GetTreeForTaxa(CompoundIdentity taxaUnitId)
        {
            if (!taxaUnitId.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(taxaUnitId.DataStoreIdentity) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectTreeSample + Db.Where + Db.WhereTaxa;
                cmd.Parameters.AddWithValue("taxasid", taxaUnitId.DataStoreIdentity);
                cmd.Parameters.AddWithValue("taxaid", taxaUnitId.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                TreeSample o = null;
                List<TreeSample> permissions = new List<TreeSample>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.treeBuilder.Build(rdr);
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

        public IEnumerable<TreeSample> GetTreeForTaxa(IEnumerable<CompoundIdentity> taxaUnitIds)
        {
            if (taxaUnitIds != null && this.CanGet())
            {
                string where = Db.GetWhere(taxaUnitIds, "\"TaxaUnitSystemId\"", "\"TaxaUnitId\"");
                if (where.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectTreeSample + Db.Where + where.ToString();
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    List<TreeSample> permissions = new List<TreeSample>();
                    try
                    {
                        TreeSample o;
                        while (rdr.Read())
                        {
                            o = this.treeBuilder.Build(rdr);
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
                    return new TreeSample[0]; //empty set
            }
            return null;
        }

        public IEnumerable<TreeSample> GetTreeForTaxa(IEnumerable<Guid> ids, IEnumerable<CompoundIdentity> taxaUnitIds)
        {
            if (ids != null && taxaUnitIds != null && this.CanGet())
            {
                string where = Db.GetWhere(ids, "\"Id\"");
                if (where.Length > 0)
                {
                    string sit = Db.GetWhere(taxaUnitIds, "\"TaxaUnitSystemId\"", "\"TaxaUnitId\"");
                    if (sit.Length > 0)
                    {
                        where = where + " AND " + sit;

                        NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                        cmd.CommandText = Db.SelectTreeSample + Db.Where + where;
                        NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                        List<TreeSample> permissions = new List<TreeSample>();
                        try
                        {
                            TreeSample o;
                            while (rdr.Read())
                            {
                                o = this.treeBuilder.Build(rdr);
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
                        return new TreeSample[0]; //empty set
                }
                else
                    return new TreeSample[0]; //empty set
            }
            return null;
        }

        public IEnumerable<TreeSample> GetTreeForTaxa(IEnumerable<Guid> ids, CompoundIdentity taxaUnitId)
        {
            if (ids != null && !taxaUnitId.IsNullOrEmpty() && this.CanGet())
            {
                string wh = Db.GetWhere(ids, "\"Id\"");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectTreeSample + Db.Where + Db.WhereTaxa + " AND " + wh;
                    Db.AddParam(cmd, "taxasid", "taxaid", taxaUnitId);
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    TreeSample o = null;
                    List<TreeSample> items = new List<TreeSample>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.treeBuilder.Build(rdr);
                                if (o != null)
                                    items.Add(o);
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
                    return items;
                }
            }
            return null;
        }


        //UPDATES -------------------------------------------------------------------------------------------


        //\"PointLocation\"=:loc, \"ElevMin\"=:eMin, \"ElevMax\"=:eMax WHERE \"Id\"=:id
        public bool Update(VegSample item)
        {
            if (item != null && this.CanUpdate(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.UpdateVegSample;

                    cmd.Parameters.AddWithValue("id", item.Identity);
                    Db.AddParam(cmd, "loc", item.Location);
                    Db.AddParam(cmd, "eMin", "eMax", item.ElevationRange);

                    Db.ExecuteNonQuery(cmd);

                    return true;
                }
                catch
                { }
            }
            return false;
        }

        //\"PercentCover\"=:cov, \"Description\"=:d WHERE \"Id\"=:id
        public bool UpdateHerb(HerbSample item)
        {
            if (item != null && this.CanUpdate(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.UpdateVegSample;

                    cmd.Parameters.AddWithValue("id", item.Identity);
                    Db.AddParam(cmd, "cov", item.PercentCover);
                    Db.AddParam(cmd, "d", item.Description);

                    Db.ExecuteNonQuery(cmd);

                    return true;
                }
                catch
                { }
            }
            return false;
        }

        //\"SizeClass\"=:class, \"Count\"=:ct, \"Description\"=:d WHERE \"Id\"=:id
        public bool UpdateShrub(ShrubSample item)
        {
            if (item != null && this.CanUpdate(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.UpdateVegSample;

                    cmd.Parameters.AddWithValue("id", item.Identity);
                    Db.AddParam(cmd, "class", item.SizeClass);
                    Db.AddParam(cmd, "ct", item.Count);
                    Db.AddParam(cmd, "d", item.Description);

                    Db.ExecuteNonQuery(cmd);

                    return true;
                }
                catch
                { }
            }
            return false;
        }

        //\"Dbh\"=:dbh, \"Description\"=:d WHERE \"Id\"=:id
        public bool UpdateTree(TreeSample item)
        {
            if (item != null && this.CanUpdate(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.UpdateVegSample;

                    cmd.Parameters.AddWithValue("id", item.Identity);
                    Db.AddParam(cmd, "dbh", item.DiameterBreastHigh);
                    Db.AddParam(cmd, "d", item.Description);

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


        private readonly VegSampleBuilder sampleBuilder;
        private readonly HerbSampleBuilder herbBuilder;
        private readonly TreeSampleBuilder treeBuilder;
        private readonly ShrubSampleBuilder shrubBuilder;

        internal PgVegSampleProvider(UserSecurityContext context)
        {
            this.Context = context;
            this.sampleBuilder = new VegSampleBuilder(context);
            this.herbBuilder = new HerbSampleBuilder(context, this);
            this.shrubBuilder = new ShrubSampleBuilder(context, this);
            this.treeBuilder = new TreeSampleBuilder(context, this);
        }
    }
}
