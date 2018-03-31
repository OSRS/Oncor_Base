using Npgsql;
using Osrs.Data;
using Osrs.Security;
using System.Collections.Generic;

namespace Osrs.Oncor.UserAffiliationPermissionChecks
{
    public sealed class UserProvider
    {
        private readonly UserSecurityContext ctx;

        private HashSet<CompoundIdentity> orgs;

        public IEnumerable<CompoundIdentity> UserAffiliations()
        {
            return GetUserAffils();
        }

        public bool HasAffiliationForSampleEvent(CompoundIdentity sampleEventId, bool localOnly)
        {
            if (!sampleEventId.IsNullOrEmpty())
            {
                HashSet<CompoundIdentity> orgs = GetUserAffils();
                if (orgs!=null && orgs.Count>0)
                {
                    if (localOnly)
                    {
                        CompoundIdentity org = SampleEventOrgs.Instance.GetLocalOrg(sampleEventId);
                        if (org!=null)
                        {
                            HashSet<CompoundIdentity> tmp = GetUserAffils();
                            if (tmp != null)
                            {
                                foreach (CompoundIdentity cur in tmp)
                                {
                                    if (cur.Equals(org))
                                        return true;
                                }
                            }
                        }
                    }
                    else
                    {
                        SampleEventOrgRollup eventOrgs = SampleEventOrgs.Instance.Get(sampleEventId);
                        if (eventOrgs != null)
                        {
                            return SampleEventOrgs.Matches(eventOrgs.AuthorizedOrganizationIds, orgs);
                        }
                    }
                }
            }
            return false;
        }

        private HashSet<CompoundIdentity> GetUserAffils()
        {
            if (this.orgs == null)
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectUserAffil;
                cmd.Parameters.AddWithValue("uid", this.ctx.User.Uid);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                HashSet<CompoundIdentity> orgs = new HashSet<CompoundIdentity>();
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
                        this.orgs = orgs;
                    }
                    catch
                    { }
                    finally
                    {
                        cmd.Dispose();
                    }
                }
            }
            return this.orgs;
        }

        internal UserProvider(UserSecurityContext ctx)
        {
            this.ctx = ctx;
        }
    }
}
