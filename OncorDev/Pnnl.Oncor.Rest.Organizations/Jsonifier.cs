using Newtonsoft.Json.Linq;
using Osrs.WellKnown.Organizations;
using System.Collections.Generic;
using Osrs.WellKnown.OrganizationHierarchies;
using Osrs.Data;

namespace Pnnl.Oncor.Rest.Organizations
{
    internal static class Jsonifier
    {

        public static JArray ToJson(IEnumerable<CompoundIdentity> cids)
        {
            if (cids != null)
            {
                JArray o = new JArray();
                foreach (CompoundIdentity cur in cids)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

        public static JArray ToJson(IEnumerable<OrganizationHierarchy> org)
        {
            if (org != null)
            {
                JArray o = new JArray();
                foreach (OrganizationHierarchy cur in org)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

        public static JArray ToJson(IEnumerable<OrganizationAliasScheme> org)
        {
            if (org != null)
            {
                JArray o = new JArray();
                foreach (OrganizationAliasScheme cur in org)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

        public static JArray ToJson(IEnumerable<OrganizationAlias> org)
        {
            if (org != null)
            {
                JArray o = new JArray();
                foreach (OrganizationAlias cur in org)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

        public static JArray ToJson(IEnumerable<Organization> org)
        {
            if (org != null)
            {
                JArray o = new JArray();
                foreach (Organization cur in org)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

        public static JArray ToJson(IEnumerable<KeyValuePair<CompoundIdentity, CompoundIdentity>> links)
        {
            if(links != null)
            {
                JArray o = new JArray();
                foreach (KeyValuePair<CompoundIdentity,CompoundIdentity> cur in links)
                {
                    JObject j = new JObject();
                    j.Add("parentid", JsonUtils.ToJson(cur.Key));
                    j.Add("childid", JsonUtils.ToJson(cur.Value));
                    o.Add(j);
                }
                return o;
            }
            return null;
        }

        public static JObject ToJson(CompoundIdentity cid)
        {
            if (cid != null)
            {
                JObject o = new JObject();
                o.Add(JsonUtils.Id, JsonUtils.ToJson(cid));
                return o;
            }
            return null;
        }

        public static JObject ToJson(OrganizationHierarchy org)
        {
            if (org != null)
            {
                JObject o = new JObject();
                o.Add(JsonUtils.Id, JsonUtils.ToJson(org.Identity));
                o.Add(JsonUtils.OwnerId, JsonUtils.ToJson(org.OwningOrgId));
                o.Add(JsonUtils.Name, org.Name);
                o.Add(JsonUtils.Description, org.Description);
                return o;
            }
            return null;
        }

        public static JObject ToJson(OrganizationAliasScheme org)
        {
            if (org != null)
            {
                JObject o = new JObject();
                o.Add(JsonUtils.Id, JsonUtils.ToJson(org.Identity));
                o.Add(JsonUtils.OwnerId, JsonUtils.ToJson(org.OwningOrganizationIdentity));
                o.Add(JsonUtils.Name, org.Name);
                o.Add(JsonUtils.Description, org.Description);
                return o;
            }
            return null;
        }

        public static JObject ToJson(OrganizationAlias org)
        {
            if (org != null)
            {
                JObject o = new JObject();
                o.Add(JsonUtils.Id, JsonUtils.ToJson(org.Identity));
                o.Add(JsonUtils.SchemeId, JsonUtils.ToJson(org.AliasSchemeIdentity));
                o.Add(JsonUtils.Name, org.Name);
                return o;
            }
            return null;
        }

        public static JObject ToJson(Organization org)
        {
            if (org != null)
            {
                JObject o = new JObject();
                o.Add(JsonUtils.Id, JsonUtils.ToJson(org.Identity));
                o.Add(JsonUtils.Name, org.Name);
                o.Add(JsonUtils.Description, org.Description);
                return o;
            }
            return null;
        }
    }
}
