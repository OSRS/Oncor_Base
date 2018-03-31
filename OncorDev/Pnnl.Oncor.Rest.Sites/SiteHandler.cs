using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Net.Http;
using Osrs.Security;
using Osrs.Threading;
using Osrs.WellKnown.Sites;
using Osrs.Numerics.Spatial.Geometry;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.Sites
{
    internal static class SiteHandler
    {
        public static void Handle(UserSecurityContext user, string method, HttpContext context, CancellationToken cancel)
        {
            if (context.Request.Method == "POST")
            {
                if (method.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    Get(user, context, cancel);
                    return;
                }
                else if (method.Equals("find", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        if (token != null)
                        {
                            if (token["name"] != null)
                            {
                                GetName(token["name"].ToString(), user, context, cancel);
                                return;
                            }
                            else if (token["orgid"] != null)
                            {
                                GetByOwner(JsonUtils.ToId(token["orgid"]), user, context, cancel);
                                return;
                            }       
                        }

                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                        return;
                    }
                }
                else if (method.Equals("in", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        HashSet<CompoundIdentity> ids = JsonUtils.ToIds(JsonUtils.GetDataPayload(context.Request));
                        if (ids != null)
                        {
                            GetIds(ids, user, context, cancel);
                            return;
                        }
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                        return;
                    }
                }
                else if (method.Equals("parent", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        CompoundIdentity id = JsonUtils.ToId(token["id"]);
                        CompoundIdentity parentid = JsonUtils.ToId(token["parentid"]);
                        HashSet<CompoundIdentity> cids = JsonUtils.ToIds(token["childid"]);
                        bool del = token["remove"] != null ? (bool) token["remove"] : false;

                        if (parentid != null && cids != null)
                        {
                            if (del)
                            {
                                Remove(parentid, cids, user, context, cancel);
                                return;
                            }
                            else
                            {
                                Add(parentid, cids, user, context, cancel);
                                return;
                            }
                        }
                        else if (id != null)
                        {
                            GetParent(id, user, context, cancel);
                            return;
                        }

                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                        return;
                    }
                }
                else if (method.Equals("children", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        CompoundIdentity cid = JsonUtils.ToId(JsonUtils.GetDataPayload(context.Request));
                        if (cid != null)
                        {
                            GetChildren(cid, user, context, cancel);
                            return;
                        }

                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                        return;
                    }
                }
                else if (method.Equals("create", StringComparison.OrdinalIgnoreCase))
                {

                    CompoundIdentity owning_orgid = null;
                    string name = null;
                    string desc = null;
                    SiteProviderBase provider = null;
                    JToken token = null;

                    try
                    {
                        //payload and provider
                        token = JsonUtils.GetDataPayload(context.Request);
                        provider = SiteManager.Instance.GetSiteProvider(user);
                        if (provider != null && token != null)
                        {
                            //required inputs
                            name = token["name"].ToString();
                            owning_orgid = JsonUtils.ToId(token["orgid"]);
                            if (owning_orgid != null && !string.IsNullOrEmpty(name))
                            {
                                desc = token["desc"] != null ? token["desc"].ToString() : null;

                                HashSet<CompoundIdentity> pids = JsonUtils.ToIds(token["parentid"]); //could be >= 1, or null
                                
                                //create
                                Site site = null;
                                site = provider.Create(owning_orgid, name, desc);
                                if (site != null)
                                {
                                    //add parents if necessary
                                    bool result = true;
                                    if (pids != null)
                                    {
                                        foreach (CompoundIdentity p in pids)
                                            result &= provider.AddParent(site.Identity, p);  //parents are not returned with newly created site.  This was just a convenience for REST /create
                                    }
                                    if (result == true)
                                    {
                                        JObject jsite = Jsonifier.ToJson(site);
                                        if (jsite != null)
                                            RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jsite.ToString()));
                                        else
                                            RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                                        return;
                                    }
                                }
                            }
                        }

                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                        return;
                    }
                }
                else if (method.Equals("delete", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        JToken t = JsonUtils.GetDataPayload(context.Request);
                        HashSet<CompoundIdentity> cids = JsonUtils.ToIds(t);
                        SiteProviderBase provider = SiteManager.Instance.GetSiteProvider(user);
                        if (provider != null && cids != null)
                        {
                            bool result = true;
                            foreach (CompoundIdentity cid in cids)
                            {
                                result &= provider.Delete(cid);
                            }

                            if (result == true)
                                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok));
                            else
                                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                            return;
                        }

                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                        return;
                    }
                }
                else if (method.Equals("update", StringComparison.OrdinalIgnoreCase))
                {
                    CompoundIdentity cid = null;
                    CompoundIdentity org_cid = null;
                    HashSet<CompoundIdentity> pids = null;
                    string name = null;
                    string desc = null;
                    bool dirty = false;

                    try
                    {
                        //provider and token
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        SiteProviderBase provider = SiteManager.Instance.GetSiteProvider(user);
                        if (provider != null && token != null)
                        {
                            //GUID must be provided
                            cid = JsonUtils.ToId(token["id"]);

                            //fetch stored object
                            Site site = provider.Get(cid);
                            if (site != null)
                            {
                                //## REQUIRED ##

                                //name
                                if (token.SelectToken("name") != null)
                                {
                                    name = token["name"].ToString();
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        site.Name = name;
                                        dirty = true;
                                    }
                                    else
                                    {
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed)); //name is required and not nullable
                                        return;
                                    }
                                }

                                //owning org
                                if (token.SelectToken("orgid") != null)
                                {
                                    org_cid = JsonUtils.ToId(token["orgid"]);
                                    if (org_cid != null)
                                    {
                                        site.OwningOrganizationIdentity = org_cid;
                                        dirty = true;
                                    }
                                    else
                                    {
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed)); //owning org is required and not nullable
                                        return;
                                    }
                                }

                                //## OPTIONALS ##

                                //description
                                if (token.SelectToken("desc") != null)
                                {
                                    desc = (token["desc"] != null) ? token["desc"].ToString() : null;
                                    site.Description = desc;
                                    dirty = true;
                                }

                                //geom
                                if (token.SelectToken("geom") != null)
                                {
                                    IGeometry2<double> geom = Jsonifier.IsNullOrEmpty(token["geom"]) ? null : GeoJsonUtils.ParseGeometry(token["geom"].ToString());
                                    site.Location = geom;
                                    dirty = true;
                                }

                                //altgeom
                                if (token.SelectToken("altgeom") != null)
                                {
                                    Point2<double> altgeom = Jsonifier.IsNullOrEmpty(token["altgeom"]) ? null : GeoJsonUtils.ParseGeometry(token["altgeom"].ToString()) as Point2<double>;
                                    site.LocationMark = altgeom;
                                    dirty = true;
                                }

                                //update
                                bool result = true;
                                if (dirty)
                                {
                                    //update
                                    result &= provider.Update(site);
                                    if (result == false)
                                    {
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                                        return;
                                    }
                                }

                                //site hierarchy
                                if (token.SelectToken("parentid") != null)
                                {
                                    pids = JsonUtils.ToIds(token["parentid"]);  //new parents
                                    IEnumerable<Site> existing_parents = provider.GetParents(cid);  //existing parents

                                    if (pids == null) //clear all parent assignments
                                    {
                                        foreach (Site p in existing_parents)
                                            result &= provider.RemoveParent(cid, p.Identity);
                                    }
                                    else
                                    {
                                        //remove unlisted, keep listed
                                        foreach (Site p in existing_parents)
                                        {
                                            if (pids.Contains(p.Identity) == false)
                                                result &= provider.RemoveParent(cid, p.Identity);
                                        }

                                        //add new
                                        foreach (CompoundIdentity new_pid in pids)
                                        {
                                            bool contains = false;
                                            foreach (Site p in existing_parents)
                                            {
                                                if (p.Identity == new_pid)
                                                    contains = true;
                                                break;
                                            }
                                            if (contains == false)
                                                result &= provider.AddParent(cid, new_pid);
                                        }
                                    }
                                }

                                if (result == true)
                                {
                                    //return ok - no values were modified
                                    RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok));
                                    return;
                                }
                            }
                        }

                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                        return;
                    }
                }
            }

            context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
        }

        private static void Remove(CompoundIdentity parent_cid, HashSet<CompoundIdentity> cids, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                SiteProviderBase provider = SiteManager.Instance.GetSiteProvider(user);
                if (provider != null)
                {
                    bool result = provider.RemoveParent(cids, parent_cid);
                    if (result == true)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok);
                    else
                        RestUtils.Push(context.Response, JsonOpStatus.Failed);
                    return;
                }

                RestUtils.Push(context.Response, JsonOpStatus.Failed);
            }
            catch
            {
                RestUtils.Push(context.Response, JsonOpStatus.Failed);
            }
        }

        private static void Add(CompoundIdentity parent_cid, HashSet<CompoundIdentity> cids, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                SiteProviderBase provider = SiteManager.Instance.GetSiteProvider(user);
                if (provider != null)
                {
                    bool result = provider.AddParent(cids, parent_cid);
                    if (result == true)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok);
                    else
                        RestUtils.Push(context.Response, JsonOpStatus.Failed);
                    return;
                }

                RestUtils.Push(context.Response, JsonOpStatus.Failed);
            }
            catch
            {
                RestUtils.Push(context.Response, JsonOpStatus.Failed);
            }
        }

        private static void GetIds(HashSet<CompoundIdentity> idList, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                if (idList.Count == 0)
                {
                    RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                    return;
                }

                SiteProviderBase provider = SiteManager.Instance.GetSiteProvider(user);
                if (provider != null)
                {
                    JObject site = null;
                    JArray sites = new JArray();
                    foreach (CompoundIdentity cid in idList)
                    {
                        site = Jsonifier.ToJson(provider.Get(cid));

                        if (site != null)
                            sites.Add(site);
                    }

                    if (sites != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, sites.ToString());
                    else
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                    return;
                }

                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
            }
            catch
            {
                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
            }
        }

        private static void GetChildren(CompoundIdentity cid, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                SiteProviderBase provider = SiteManager.Instance.GetSiteProvider(user);
                if (provider != null)
                {
                    IEnumerable<Site> sites = provider.GetChildren(cid);
                    JArray jchildren = Jsonifier.ToJson(sites);
                    if (jchildren != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jchildren.ToString());
                    else
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                    return;
                }

                RestUtils.Push(context.Response, JsonOpStatus.Failed);
            }
            catch
            {
                RestUtils.Push(context.Response, JsonOpStatus.Failed);
                return;
            }
        }

        private static void GetParent(CompoundIdentity cid, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                SiteProviderBase provider = SiteManager.Instance.GetSiteProvider(user);
                if (provider != null)
                {
                    IEnumerable<Site> sites = provider.GetParents(cid);
                    JArray jsites = Jsonifier.ToJson(sites);
                    if (jsites != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jsites.ToString());
                    else
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                    return;
                }

                RestUtils.Push(context.Response, JsonOpStatus.Failed);
            }
            catch
            {
                RestUtils.Push(context.Response, JsonOpStatus.Failed);
                return;
            }
        }

        private static void GetByOwner(CompoundIdentity owner_cid, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                SiteProviderBase provider = SiteManager.Instance.GetSiteProvider(user);
                if (provider != null)
                {
                    IEnumerable<Site> sites = provider.GetByOwner(owner_cid);
                    JArray jsites = Jsonifier.ToJson(sites);
                    if (jsites != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jsites.ToString());
                    else
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                    return;
                }

                RestUtils.Push(context.Response, JsonOpStatus.Failed);
            }
            catch
            {
                RestUtils.Push(context.Response, JsonOpStatus.Failed);
                return;
            }
        }

        private static void GetName(string name, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                SiteProviderBase provider = SiteManager.Instance.GetSiteProvider(user);
                if (provider != null)
                {
                    IEnumerable<Site> sites = provider.Get(name);
                    JArray jsites = Jsonifier.ToJson(sites);
                    if (jsites != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jsites.ToString());
                    else
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                    return;
                }

                RestUtils.Push(context.Response, JsonOpStatus.Failed);
            }
            catch
            {
                RestUtils.Push(context.Response, JsonOpStatus.Failed);
                return;
            }
        }

        private static void Get(UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                SiteProviderBase provider = SiteManager.Instance.GetSiteProvider(user);
                if (provider != null)
                {
                    IEnumerable<Site> sites = provider.Get();
                    JArray jsites = Jsonifier.ToJson(sites);
                    if (jsites != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jsites.ToString());
                    else
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                    return;
                }

                RestUtils.Push(context.Response, JsonOpStatus.Failed);
            }
            catch
            {
                RestUtils.Push(context.Response, JsonOpStatus.Failed);
                return;
            }
        }
    }
}
