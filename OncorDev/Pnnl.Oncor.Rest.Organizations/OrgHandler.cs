using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Net.Http;
using Osrs.Security;
using Osrs.Threading;
using Osrs.WellKnown.Organizations;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.Organizations
{
    internal static class OrgHandler
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
                    }
                }
                else if (method.Equals("create", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        string name = null;
                        string desc = null;

                        OrganizationProviderBase provider = OrganizationManager.Instance.GetOrganizationProvider(user);
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        name = token["name"].ToString();
                        if (provider != null && token != null && !string.IsNullOrEmpty(name))
                        {
                            desc = (token["desc"]) != null ? token["desc"].ToString() : null;

                            Organization org = null;
                            org = provider.Create(name, desc);

                            if (org != null)
                            {
                                JObject jorg = Jsonifier.ToJson(org);
                                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jorg.ToString()));
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
                else if (method.Equals("delete", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        JToken t = JsonUtils.GetDataPayload(context.Request);
                        HashSet<CompoundIdentity> cids = JsonUtils.ToIds(t);
                        OrganizationProviderBase provider = OrganizationManager.Instance.GetOrganizationProvider(user);
                        if (provider != null && cids != null)
                        {
                            bool result = true;
                            foreach (CompoundIdentity cid in cids)
                            {
                                result &= provider.Delete(cid);
                            }

                            if (result == true)
                            {
                                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok));
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
                else if (method.Equals("update", StringComparison.OrdinalIgnoreCase))
                {
                    
                    JToken token = null;
                    CompoundIdentity cid = null;
                    string name = null;
                    string desc = null;

                    try
                    {
                        //check for request.body and provider
                        token = JsonUtils.GetDataPayload(context.Request);
                        OrganizationProviderBase provider = OrganizationManager.Instance.GetOrganizationProvider(user);
                        if (provider != null && token != null)
                        {
                            //GUID must be provided
                            cid = JsonUtils.ToId(token["id"]);

                            //fetch stored object
                            bool dirty = false;
                            Organization org = provider.Get(cid);
                            if (org != null)
                            {
                                //## REQUIRED ##

                                //name
                                if (token.SelectToken("name") != null)
                                {
                                    name = token["name"].ToString();
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        org.Name = name;
                                        dirty = true;
                                    }
                                    else
                                    {
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed)); //name is required and not nullable
                                        return;
                                    }
                                }

                                //## OPTIONALS ##

                                //description
                                if (token.SelectToken("desc") != null)
                                {
                                    desc = token["desc"].ToString();
                                    org.Description = desc;
                                    dirty = true;
                                }

                                if (dirty)
                                {
                                    //update
                                    bool result = provider.Update(org);
                                    if (result == true)
                                    {
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok));
                                        return;
                                    }
                                }
                                else
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

        private static void GetIds(HashSet<CompoundIdentity> idList, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                if (idList.Count == 0)
                {
                    RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                    return;
                }

                OrganizationProviderBase provider = OrganizationManager.Instance.GetOrganizationProvider(user);
                if (provider != null)
                {
                    IEnumerable<Organization> orgs = provider.Get(idList);
                    JArray jorgs = Jsonifier.ToJson(orgs);
                    if (jorgs != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jorgs.ToString());
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

        private static void GetName(string name, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                OrganizationProviderBase provider = OrganizationManager.Instance.GetOrganizationProvider(user);
                if (provider != null)
                {
                    IEnumerable<Organization> orgs = provider.Get(name);
                    JArray jorgs = Jsonifier.ToJson(orgs);
                    if (jorgs != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jorgs.ToString());
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

        private static void Get(UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                OrganizationProviderBase provider = OrganizationManager.Instance.GetOrganizationProvider(user);
                if (provider != null)
                {
                    IEnumerable<Organization> orgs = provider.Get();
                    JArray jorgs = Jsonifier.ToJson(orgs);
                    if (jorgs != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jorgs.ToString());
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
    }
}
