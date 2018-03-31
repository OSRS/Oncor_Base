using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Net.Http;
using Osrs.WellKnown.Projects;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.Projects
{
    internal static class Jsonifier
    {
        private const string Dsid = "dsid";
        private const string Id = "id";

        public static HashSet<CompoundIdentity> ParseIds(string jsonPayload)
        {
            try
            {
                JArray data = JToken.Parse(jsonPayload) as JArray;
                if (data!=null)
                {
                    HashSet<CompoundIdentity> ids = new HashSet<CompoundIdentity>();
                    CompoundIdentity item;
                    foreach(JToken cur in data)
                    {
                        item = ToId(cur as JObject);
                        if (item != null)
                            ids.Add(item);
                    }
                    return ids;
                }
            }
            catch
            { }
            return null;
        }

        public static CompoundIdentity ToId(JObject ob)
        {
            if (ob!=null)
            {
                if (ob[Dsid]!=null && ob[Id]!=null)
                {
                    JToken d = ob[Dsid];
                    JToken i = ob[Id];

                    Guid ds;
                    Guid id;

                    if (Guid.TryParse(d.ToString(), out ds) && Guid.TryParse(i.ToString(), out id))
                        return new CompoundIdentity(ds, id);
                }
            }
            return null;
        }

        public static JToken ParseBody(HttpRequest request)
        {
            try
            {
                return JToken.Parse(RestUtils.ReadBody(request));
            }
            catch
            { }
            return null;
        }

        public static JArray ToJson(IEnumerable<Project> projects)
        {
            if (projects != null)
            {
                JArray o = new JArray();
                foreach (Project cur in projects)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

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

        public static JObject ToJson(Project project)
        {
            if (project != null)
            {
                JObject o = new JObject();
                o.Add(JsonUtils.Id, JsonUtils.ToJson(project.Identity));
                o.Add(JsonUtils.Name, project.Name);
                o.Add(JsonUtils.OwnerId, JsonUtils.ToJson(project.PrincipalOrganization));
                o.Add(JsonUtils.ParentId, JsonUtils.ToJson(project.ParentId));
                o.Add(JsonUtils.Description, project.Description);
                o.Add(JsonUtils.Affiliates, JsonUtils.ToJson(project.Affiliates));
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
    }
}
