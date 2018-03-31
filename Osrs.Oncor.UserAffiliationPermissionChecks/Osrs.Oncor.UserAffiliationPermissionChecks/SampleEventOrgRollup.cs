using Npgsql;
using Osrs.Data;
using System;
using System.Collections.Generic;

namespace Osrs.Oncor.UserAffiliationPermissionChecks
{
    public sealed class SampleEventOrgRollup
    {
        private readonly CompoundIdentity eventId;
        public CompoundIdentity SampleEventId
        {
            get { return this.eventId; }
        }

        private HashSet<CompoundIdentity> orgIds;
        public IEnumerable<CompoundIdentity> AuthorizedOrganizationIds
        {
            get
            {
                if (this.orgIds == null)
                    Load();
                return this.orgIds;
            }
        }

        private void Load()
        {
            Tuple<CompoundIdentity, HashSet<CompoundIdentity>> sample = DoEvent(this.eventId);
            if (sample!=null && this.orgIds==null) //allow another chance to escape a redundant load
            {
                DoProjects(sample.Item1, sample.Item2);
                this.orgIds = sample.Item2;
            }
        }

        //FieldActivities.ProjectSystemId, FieldActivities.ProjectId, FieldActivities.OrgSystemId, FieldActivities.OrgId, 
        //FieldTrips.OrgSystemId, FieldTrips.OrgId, SamplingEvents.OrgSystemId, SamplingEvents.OrgId
        private static Tuple<CompoundIdentity, HashSet<CompoundIdentity>> DoEvent(CompoundIdentity eventId)
        {
            NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
            cmd.CommandText = Db.SelectSampleEventProjectAndOrgs;
            cmd.Parameters.AddWithValue("sid", eventId.DataStoreIdentity);
            cmd.Parameters.AddWithValue("id", eventId.Identity);
            NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
            Tuple<CompoundIdentity, HashSet<CompoundIdentity>> items=null;
            if (rdr != null)
            {
                try
                {
                    if (rdr.Read())
                    {
                        items = new Tuple<CompoundIdentity, HashSet<CompoundIdentity>>(
                            new CompoundIdentity(DbReaderUtils.GetGuid(rdr, 0), DbReaderUtils.GetGuid(rdr, 1)), new HashSet<CompoundIdentity>()
                        );

                        items.Item2.Add(new CompoundIdentity(DbReaderUtils.GetGuid(rdr, 2), DbReaderUtils.GetGuid(rdr, 3)));
                        items.Item2.Add(new CompoundIdentity(DbReaderUtils.GetGuid(rdr, 4), DbReaderUtils.GetGuid(rdr, 5)));
                        items.Item2.Add(new CompoundIdentity(DbReaderUtils.GetGuid(rdr, 6), DbReaderUtils.GetGuid(rdr, 7)));
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

        private static void DoProjects(CompoundIdentity projectId, HashSet<CompoundIdentity> orgs)
        {
            DoProjects(projectId, orgs, new HashSet<CompoundIdentity>());
        }

        //\"OrgSystemId\", \"OrgId\", \"ParentSystemId\", \"ParentId\"  -base
        //\"OrgSystemId\", \"OrgId\"  -affiliates
        private static void DoProjects(CompoundIdentity projectId, HashSet<CompoundIdentity> orgs, HashSet<CompoundIdentity> visited)
        {
            NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
            cmd.CommandText = Db.SelectProj;
            cmd.Parameters.AddWithValue("sid", projectId.DataStoreIdentity);
            cmd.Parameters.AddWithValue("id", projectId.Identity);
            NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
            CompoundIdentity parentId = null;
            if (rdr != null)
            {
                try
                {
                    if (rdr.Read())
                    {
                        orgs.Add(new CompoundIdentity(DbReaderUtils.GetGuid(rdr, 0), DbReaderUtils.GetGuid(rdr, 1)));
                        if (!DBNull.Value.Equals(rdr[2]))
                            parentId = new CompoundIdentity(DbReaderUtils.GetGuid(rdr, 2), DbReaderUtils.GetGuid(rdr, 3));
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

                cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectAffil;
                cmd.Parameters.AddWithValue("sid", projectId.DataStoreIdentity);
                cmd.Parameters.AddWithValue("id", projectId.Identity);
                rdr = Db.ExecuteReader(cmd);
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            orgs.Add(new CompoundIdentity(DbReaderUtils.GetGuid(rdr, 0), DbReaderUtils.GetGuid(rdr, 1)));
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

            }
            if (parentId != null && visited.Add(projectId)) //this will halt if we try to process a project a second time - this is a cheap cycle short-circuit
            {
                DoProjects(parentId, orgs, visited);
            }
        }

        internal static SampleEventOrgRollup Create(CompoundIdentity eventId)
        {
            if (SampleEventOrgs.Instance.GetLocalOrg(eventId) !=null) //this ensures it exists
                return new SampleEventOrgRollup(eventId);
            return null;
        }

        private SampleEventOrgRollup(CompoundIdentity eventId)
        {
            this.eventId = eventId;
        }
    }
}
