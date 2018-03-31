using Npgsql;
using Osrs.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osrs.Oncor.UserAffiliationPermissionChecks
{
    internal sealed class SampleEventOrgs
    {
        private const int maxSize = 40; //we'll keep up to 40 sample events active
        private readonly Random r = new Random();
        private Dictionary<CompoundIdentity, SampleEventOrgRollup> cached = new Dictionary<CompoundIdentity, SampleEventOrgRollup>();

        internal CompoundIdentity GetLocalOrg(CompoundIdentity sampleEventId)
        {
            NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
            cmd.CommandText = Db.SelectSampleEventLocal;
            cmd.Parameters.AddWithValue("sid", sampleEventId.DataStoreIdentity);
            cmd.Parameters.AddWithValue("id", sampleEventId.Identity);
            NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
            CompoundIdentity tmp = null;
            if (rdr != null)
            {
                try
                {
                    if (rdr.Read())
                    {
                        tmp=new CompoundIdentity(DbReaderUtils.GetGuid(rdr, 0), DbReaderUtils.GetGuid(rdr, 1));
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
            return tmp;
        }

        internal SampleEventOrgRollup Get(CompoundIdentity sampleEventId)
        {
            if (sampleEventId!=null)
            {
                lock (this.cached) //TODO -- make this faster by dropping the lock
                {
                    if (this.cached.ContainsKey(sampleEventId))
                        return this.cached[sampleEventId];
                    SampleEventOrgRollup r = SampleEventOrgRollup.Create(sampleEventId);
                    if (r != null)
                        this.cached[sampleEventId] = r;
                    if (this.cached.Count>maxSize)
                    {
                        //randomly remove
                        int target = this.r.Next(maxSize);
                        int ix = 0;
                        CompoundIdentity rem=null;
                        foreach(CompoundIdentity cur in this.cached.Keys) //TODO -- make this better, its just ok for now
                        {
                            ix++;
                            if (ix == target)
                                rem = cur;
                        }
                        this.cached.Remove(rem);
                    }
                    return r;
                }
            }
            return null;
        }

        internal static bool Matches(IEnumerable<CompoundIdentity> orgsA, HashSet<CompoundIdentity> orgsB)
        {
            if (orgsA == null || orgsB == null)
                return false;

            HashSet<CompoundIdentity> tmp = new HashSet<CompoundIdentity>();
            foreach (CompoundIdentity cur in orgsA)
            {
                tmp.Add(cur);
            }

            foreach(CompoundIdentity cur in orgsB)
            {
                if (!tmp.Add(cur))
                    return true;
            }
            return false;
        }

        private static readonly SampleEventOrgs instance = new SampleEventOrgs();
        internal static SampleEventOrgs Instance
        {
            get { return instance; }
        }

        private SampleEventOrgs()
        { }
    }
}
