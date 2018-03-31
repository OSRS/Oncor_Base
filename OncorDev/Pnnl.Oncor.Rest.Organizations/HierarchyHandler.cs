using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Net.Http;
using Osrs.Security;
using Osrs.Threading;
using Osrs.WellKnown.OrganizationHierarchies;
using Osrs.WellKnown.Organizations;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.Organizations
{
    internal static class HierarchyHandler
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
                else if (method.Equals("children", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        //parse request
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        CompoundIdentity cid = JsonUtils.ToId(token["parentid"]);
                        string outputType = (token["outputType"] != null) ? token["outputType"].ToString() : "";
                        string recurse = token["recurse"] != null ? token["recurse"].ToString() : null;

                        //get default hierarchy
                        OrganizationHierarchyProviderBase provider = OrganizationHierarchyManager.Instance.GetProvider(user);
                        OrganizationHierarchy hierarchy = provider.GetReporting();

                        //get children ids
                        IEnumerable<CompoundIdentity> children = null;
                        if (recurse != null)
                            children = hierarchy.GetChildrenIds(cid, Convert.ToBoolean(recurse));
                        else
                            children = hierarchy.GetChildrenIds(cid);

                        if (children == null)
                        {
                            RestUtils.Push(context.Response, JsonOpStatus.Ok,"[]");  //empty
                            return;
                        }

                        //return children ids or objects
                        JArray jchildren = null;
                        if (outputType.Equals("values"))
                        {
                            OrganizationProviderBase orgProvider = OrganizationManager.Instance.GetOrganizationProvider(user);
                            List<Organization> orgs = new List<Organization>();
                            foreach (CompoundIdentity child in children)
                            {
                                orgs.Add(orgProvider.Get(child));
                            }
                            jchildren = Jsonifier.ToJson(orgs);
                        }
                        else
                        {
                            jchildren = Jsonifier.ToJson(children);
                        }

                        if (jchildren != null)
                            RestUtils.Push(context.Response, JsonOpStatus.Ok, jchildren.ToString());
                        else
                            RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));  //empty returned above
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                    }
                    return;
                }
                else if (method.Equals("parent", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        CompoundIdentity cid = JsonUtils.ToId(JsonUtils.GetDataPayload(context.Request));
                        if (cid != null)
                        {
                            GetParent(cid, user, context, cancel);
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
                else if (method.Equals("add", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        //parse request
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        CompoundIdentity parent = JsonUtils.ToId(token["parentid"]);
                        HashSet<CompoundIdentity> child_cids = JsonUtils.ToIds(token["childid"]);  //1 or more

                        //get default hierarchy
                        OrganizationHierarchyProviderBase provider = OrganizationHierarchyManager.Instance.GetProvider(user);
                        OrganizationHierarchy hierarchy = provider.GetReporting();

                        //insert
                        bool result = hierarchy.Add(parent, child_cids);
                        if (result == true)
                            RestUtils.Push(context.Response, JsonOpStatus.Ok);
                        else
                            RestUtils.Push(context.Response, JsonOpStatus.Failed);
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, JsonOpStatus.Failed);
                    }
                    return;
                }
                else if (method.Equals("move", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        //parse request
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        CompoundIdentity old_cid = JsonUtils.ToId(token["parentid"]);
                        CompoundIdentity new_cid = JsonUtils.ToId(token["newparentid"]);
                        HashSet<CompoundIdentity> cids = JsonUtils.ToIds(token["childid"]);

                        //get default hierarchy
                        OrganizationHierarchyProviderBase provider = OrganizationHierarchyManager.Instance.GetProvider(user);
                        OrganizationHierarchy hierarchy = provider.GetReporting();

                        //move
                        bool result = hierarchy.Move(old_cid, new_cid, cids);
                        if (result == true)
                            RestUtils.Push(context.Response, JsonOpStatus.Ok);
                        else
                            RestUtils.Push(context.Response, JsonOpStatus.Failed);
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, JsonOpStatus.Failed);
                    }

                    return;
                }
                else if (method.Equals("remove", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        //parse request
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        CompoundIdentity parent_cid = JsonUtils.ToId(token["parentid"]);
                        HashSet<CompoundIdentity> child_cids = JsonUtils.ToIds(token["childid"]);

                        //get default hierarchy
                        OrganizationHierarchyProviderBase provider = OrganizationHierarchyManager.Instance.GetProvider(user);
                        OrganizationHierarchy hierarchy = provider.GetReporting();

                        //remove membership
                        bool result = hierarchy.Remove(parent_cid, child_cids);
                        if (result == true)
                            RestUtils.Push(context.Response, JsonOpStatus.Ok);
                        else
                            RestUtils.Push(context.Response, JsonOpStatus.Failed);
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, JsonOpStatus.Failed);
                    }
                    return;
                }
            }
            context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
        }

        private static void GetParent(CompoundIdentity cid, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                OrganizationHierarchyProviderBase provider = OrganizationHierarchyManager.Instance.GetProvider(user);
                OrganizationHierarchy hierarchy = provider.GetReporting();
                if (provider != null)
                {
                    IEnumerable<CompoundIdentity> parentids = hierarchy.GetParentIds(cid);
                    JArray jparentids = Jsonifier.ToJson(parentids);
                    if (jparentids != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jparentids.ToString());
                    else
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
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

        private static void Get(UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                OrganizationHierarchyProviderBase provider = OrganizationHierarchyManager.Instance.GetProvider(user);
                OrganizationHierarchy hierarchy = provider.GetReporting();
                if (provider != null)
                {
                    IEnumerable<KeyValuePair<CompoundIdentity,CompoundIdentity>> links = hierarchy.GetAllPairs();
                    JArray jlinks = Jsonifier.ToJson(links);
                    if (jlinks != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jlinks.ToString());
                    else
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                }
                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
            }
            catch
            {
                RestUtils.Push(context.Response, JsonOpStatus.Failed);
                return;
            }
        }
    }
}

        