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
using System.Text;

namespace Osrs.Oncor.WellKnown.WaterQuality.Providers.Pg
{
    //TODO -- add user level checking per record to the deployment for user permissions
    // started a single level of record caching for deployment to short-circuit check - done for delete, need to roll into gets
    public sealed class PgWQMeasurementProvider : IWQMeasurementProvider
    {
        private readonly WQMeasurementBuilder builder;
        private IWQDeploymentProvider helperProvider = null;
        private CompoundIdentity lastSampleEventId = null;
        private CompoundIdentity lastDeploymentId = null;
        private bool lastEditPermission;
        private bool lastGetPermission;

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

        public bool CanDelete(WaterQualityMeasurement item)
        {
            if (item != null && this.CanDelete())
            {
                if (!item.DeploymentId.Equals(lastDeploymentId))
                {
                    if (this.helperProvider == null)
                        this.helperProvider = PgWaterQualityProviderFactory.Instance.GetDeploymentProvider(this.Context);

                    if (this.helperProvider!=null)
                    {
                        WaterQualityDeployment depl = this.helperProvider.Get(item.DeploymentId);
                        if (depl!=null)
                        {
                            CompoundIdentity se = depl.SampleEventId;
                            if (se != this.lastSampleEventId)
                            {
                                UserProvider prov = UserAffilationSecurityManager.Instance.GetProvider(this.Context);
                                if (prov != null)
                                {
                                    this.lastEditPermission = prov.HasAffiliationForSampleEvent(se, true);
                                    this.lastGetPermission = prov.HasAffiliationForSampleEvent(se, false);
                                    this.lastSampleEventId = se;
                                    this.lastDeploymentId = item.DeploymentId;
                                }
                            }
                            else //same sample event, different deployment
                            {
                                this.lastDeploymentId = item.DeploymentId; //don't need to re-check the sampleevent controls permission
                            }
                        }
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

        public bool Delete(WaterQualityMeasurement item)
        {
            if (item != null && this.CanDelete(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.DeleteMeasurement;
                    cmd.Parameters.AddWithValue("dsid", item.DeploymentId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("did", item.DeploymentId.Identity);
                    Db.ExecuteNonQuery(cmd);

                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public WaterQualityMeasurement Create(CompoundIdentity deploymentId, DateTime sampleDate, double? surfaceElevation, double? temperature, double? ph, double? dissolvedOxygen, double? conductivity, double? salinity, double? velocity)
        {
            if (!deploymentId.IsNullOrEmpty() && WQUtils.GlobalMinDate<=sampleDate && sampleDate<=DateTime.UtcNow && (surfaceElevation.HasValue || temperature.HasValue || ph.HasValue || dissolvedOxygen.HasValue || conductivity.HasValue || salinity.HasValue || velocity.HasValue) && this.CanCreate())
            {
                try  //:dsid, :did, :when, :elev, :tpt, :ph, :doxy, :cond, :sal, :vel
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertMeasurement;
                    cmd.Parameters.AddWithValue("dsid", deploymentId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("did", deploymentId.Identity);
                    cmd.Parameters.AddWithValue("when", Db.FixDate(sampleDate));

                    if (surfaceElevation.HasValue && !double.IsNaN(surfaceElevation.Value))
                        cmd.Parameters.AddWithValue("elev", surfaceElevation.Value);
                    else
                        cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam("elev", NpgsqlTypes.NpgsqlDbType.Double));

                    if (temperature.HasValue && !double.IsNaN(temperature.Value))
                        cmd.Parameters.AddWithValue("tpt", temperature.Value);
                    else
                        cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam("tpt", NpgsqlTypes.NpgsqlDbType.Double));

                    if (ph.HasValue && !double.IsNaN(ph.Value))
                        cmd.Parameters.AddWithValue("ph", ph.Value);
                    else
                        cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam("ph", NpgsqlTypes.NpgsqlDbType.Double));

                    if (dissolvedOxygen.HasValue && !double.IsNaN(dissolvedOxygen.Value))
                        cmd.Parameters.AddWithValue("doxy", dissolvedOxygen.Value);
                    else
                        cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam("doxy", NpgsqlTypes.NpgsqlDbType.Double));

                    if (conductivity.HasValue && !double.IsNaN(conductivity.Value))
                        cmd.Parameters.AddWithValue("cond", conductivity.Value);
                    else
                        cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam("cond", NpgsqlTypes.NpgsqlDbType.Double));

                    if (salinity.HasValue && !double.IsNaN(salinity.Value))
                        cmd.Parameters.AddWithValue("sal", salinity.Value);
                    else
                        cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam("sal", NpgsqlTypes.NpgsqlDbType.Double));

                    if (velocity.HasValue && !double.IsNaN(velocity.Value))
                        cmd.Parameters.AddWithValue("vel", velocity.Value);
                    else
                        cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam("vel", NpgsqlTypes.NpgsqlDbType.Double));

                    Db.ExecuteNonQuery(cmd);

                    return new WaterQualityMeasurement(deploymentId, sampleDate, surfaceElevation, temperature, ph, dissolvedOxygen, conductivity, salinity, velocity);
                }
                catch
                { }
            }
            return null;
        }

        public WaterQualityMeasurement Create(WaterQualityDeployment item, DateTime sampleDate, double? surfaceElevation, double? temperature, double? ph, double? dissolvedOxygen, double? conductivity, double? salinity, double? velocity)
        {
            if (item!=null)
                return this.Create(item.Identity, sampleDate, surfaceElevation, temperature, ph, dissolvedOxygen, conductivity, salinity, velocity);
            return null;
        }

        public IEnumerable<WaterQualityMeasurement> Get()
        {
            if (this.CanGet())
                return new Enumerable<WaterQualityMeasurement>(new EnumerableCommand<WaterQualityMeasurement>(this.builder, Db.SelectMeasurement, Db.ConnectionString));
            return null;
        }

        public IEnumerable<WaterQualityMeasurement> Get(CompoundIdentity deploymentId)
        {
            if (!deploymentId.IsNullOrEmpty() && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectMeasurement + Db.Where + Db.WhereDeploy;
                cmd.Parameters.AddWithValue("dsid", deploymentId.DataStoreIdentity);
                cmd.Parameters.AddWithValue("did", deploymentId.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                WaterQualityMeasurement o = null;
                List<WaterQualityMeasurement> permissions = new List<WaterQualityMeasurement>();
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

        public IEnumerable<WaterQualityMeasurement> Get(DateTime start, DateTime end)
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

                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectMeasurement + sql;

                if (start > WQUtils.GlobalMinDate)
                    cmd.Parameters.AddWithValue("start", start);
                if (end < now)
                    cmd.Parameters.AddWithValue("end", end);

                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                WaterQualityMeasurement o = null;
                List<WaterQualityMeasurement> permissions = new List<WaterQualityMeasurement>();
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

        public IEnumerable<WaterQualityMeasurement> Get(CompoundIdentity deploymentId, DateTime start, DateTime end)
        {
            if (!deploymentId.IsNullOrEmpty() && this.CanGet())
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
                    cmd.CommandText = Db.SelectMeasurement + sql + " AND " + Db.WhereDeploy;
                    cmd.Parameters.AddWithValue("dsid", deploymentId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("did", deploymentId.Identity);
                    if (start > WQUtils.GlobalMinDate)
                        cmd.Parameters.AddWithValue("start", start);
                    if (end < now)
                        cmd.Parameters.AddWithValue("end", end);
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    WaterQualityMeasurement o = null;
                    List<WaterQualityMeasurement> permissions = new List<WaterQualityMeasurement>();
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

        public WaterQualityMeasurement Create(CompoundIdentity deploymentId, WaterQualityMeasurementDTO item)
        {
            if (item != null)
                return Create(deploymentId, item.SampleDate, item.SurfaceElevation, item.Temperature, item.pH, item.DissolvedOxygen, item.Conductivity, item.Salinity, item.Velocity);
            return null;
        }

        public WaterQualityMeasurement Create(WaterQualityDeployment deployment, WaterQualityMeasurementDTO item)
        {
            if (deployment != null && item != null)
                return Create(deployment.Identity, item);
            return null;
        }

        public IEnumerable<WaterQualityMeasurement> Create(WaterQualityMeasurementsDTO items)
        {
            if (items!=null)
            {
                if (!items.DeploymentId.IsNullOrEmpty() && this.CanCreate())
                {
                    List<WaterQualityMeasurement> results = new List<WaterQualityMeasurement>();
                    if (items.Count > 0)
                    {
                        int count = 0;
                        StringBuilder sb = new StringBuilder(Db.InsertMeasurementB);
                        NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                        //we could optimize this further by doing a begin / end transaction first - if needed, hope not - DB transactions suck bad - don't use them if you don't have to.
                        foreach (WaterQualityMeasurementDTO cur in items)
                        {
                            //do inserts in batches of 100 items
                            if (count > 99)
                            {
                                cmd.CommandText = sb.ToString();
                                Db.ExecuteNonQuery(cmd);
                                count = 0;
                                sb.Clear();
                                sb.Append(Db.InsertMeasurementB); //this is "INSERT ... VALUES " -- needs the batches as param sets (a,b,c),(a,b,c),...
                            }

                            if (count > 0)
                                sb.Append(",");
                            count++; //increment the indicator

                            //\"DeploySystemId\", \"DeployId\", \"SampleDate\", \"SurfElev\", \"Temperature\", \"pH\", \"DissOxy\", \"Cond\", \"Sal\", \"Velocity\"
                            sb.Append("('");
                            sb.Append(items.DeploymentId.DataStoreIdentity);
                            sb.Append("','");
                            sb.Append(items.DeploymentId.Identity);
                            sb.Append("','");
                            sb.Append(Db.FixDate(cur.SampleDate).ToString("u"));
                            sb.Append("',");
                            if (cur.SurfaceElevation.HasValue && !double.IsNaN(cur.SurfaceElevation.Value))
                                sb.Append(cur.SurfaceElevation.Value);
                            else
                                sb.Append("NULL");
                            sb.Append(',');

                            if (cur.Temperature.HasValue && !double.IsNaN(cur.Temperature.Value))
                                sb.Append(cur.Temperature.Value);
                            else
                                sb.Append("NULL");
                            sb.Append(',');

                            if (cur.pH.HasValue && !double.IsNaN(cur.pH.Value))
                                sb.Append(cur.pH.Value);
                            else
                                sb.Append("NULL");
                            sb.Append(',');

                            if (cur.DissolvedOxygen.HasValue && !double.IsNaN(cur.DissolvedOxygen.Value))
                                sb.Append(cur.DissolvedOxygen.Value);
                            else
                                sb.Append("NULL");
                            sb.Append(',');

                            if (cur.Conductivity.HasValue && !double.IsNaN(cur.Conductivity.Value))
                                sb.Append(cur.Conductivity.Value);
                            else
                                sb.Append("NULL");
                            sb.Append(',');

                            if (cur.Salinity.HasValue && !double.IsNaN(cur.Salinity.Value))
                                sb.Append(cur.Salinity.Value);
                            else
                                sb.Append("NULL");
                            sb.Append(',');

                            if (cur.Velocity.HasValue && !double.IsNaN(cur.Velocity.Value))
                                sb.Append(cur.Velocity.Value);
                            else
                                sb.Append("NULL");
                            sb.Append(')');
                            results.Add(new WaterQualityMeasurement(items.DeploymentId, Db.FixDate(cur.SampleDate), cur.SurfaceElevation, cur.Temperature, cur.pH, cur.DissolvedOxygen, cur.Conductivity, cur.Salinity, cur.Velocity));
                        }
                        if (count > 0) //commit any leftovers
                        {
                            cmd.CommandText = sb.ToString();
                            Db.ExecuteNonQuery(cmd);
                        }
                    }
                    return results;
                }
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

        internal PgWQMeasurementProvider(UserSecurityContext context)
        {
            this.Context = context;
            this.builder = new WQMeasurementBuilder(context);
        }
    }
}
