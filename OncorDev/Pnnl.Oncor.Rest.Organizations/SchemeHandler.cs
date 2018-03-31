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
    internal static class SchemeHandler
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

                        if (token["orgid"] != null && token["name"] != null)
                        {
                            GetOrgAndName(JsonUtils.ToId(token["orgid"]), token["name"].ToString(), user, context, cancel);
                            return;
                        }
                        else if (token["name"] != null)
                        {
                            GetName(token["name"].ToString(), user, context, cancel);
                            return;
                        }
                        else if (token["orgid"] != null)
                        {
                            CompoundIdentity cid = JsonUtils.ToId(token["orgid"]);
                            GetOrg(cid, user, context, cancel);
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
                        
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        name = token["name"].ToString();
                        OrganizationProviderBase o = OrganizationManager.Instance.GetOrganizationProvider(user);
                        OrganizationAliasSchemeProviderBase provider = OrganizationManager.Instance.GetOrganizationAliasSchemeProvider(user);
                        if (provider != null && o != null && token != null && !string.IsNullOrEmpty(name))
                        {
                            CompoundIdentity orgid = JsonUtils.ToId(token["orgid"]);
                            Organization org = o.Get(orgid);
                            OrganizationAliasScheme scheme = null;
                            desc = (token["desc"]) != null ? token["desc"].ToString() : null;

                            //create
                            scheme = provider.Create(org, name, desc);

                            if (scheme != null)
                            {
                                JObject jscheme = Jsonifier.ToJson(scheme);
                                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jscheme.ToString()));
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
                        OrganizationAliasSchemeProviderBase provider = OrganizationManager.Instance.GetOrganizationAliasSchemeProvider(user);
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
                    CompoundIdentity orgid = null;
                    OrganizationAliasSchemeProviderBase provider = null;
                    OrganizationAliasScheme scheme = null;
                    string name = null;
                    string desc = null;

                    try
                    {
                        //check for request.body and provider
                        token = JsonUtils.GetDataPayload(context.Request);
                        provider = OrganizationManager.Instance.GetOrganizationAliasSchemeProvider(user);
                        if (provider != null && token != null)
                        {
                            //GUID must be provided
                            cid = JsonUtils.ToId(token["id"]);

                            //fetch stored object
                            bool dirty = false;
                            scheme = provider.Get(cid);
                            if (scheme != null)
                            {
                                //## REQUIRED ##

                                //name
                                if (token.SelectToken("name") != null)
                                {
                                    name = token["name"].ToString();
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        scheme.Name = name;
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
                                    orgid = JsonUtils.ToId(token["orgid"]);
                                    if (orgid != null)
                                    {
                                        scheme.OwningOrganizationIdentity = orgid;
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
                                    scheme.Description = desc;
                                    dirty = true;
                                }

                                if (dirty)
                                {
                                    //update
                                    bool result = provider.Update(scheme);
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

                OrganizationAliasSchemeProviderBase provider = OrganizationManager.Instance.GetOrganizationAliasSchemeProvider(user);
                if (provider != null)
                {
                    JObject jscheme = null;
                    JArray jschemes = new JArray();
                    foreach (CompoundIdentity cid in idList)
                    {
                        jscheme = Jsonifier.ToJson(provider.Get(cid));
                        jschemes.Add(jscheme);
                    }

                    if (jschemes != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jschemes.ToString());
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

        private static void GetOrgAndName(CompoundIdentity orgid, string name, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                OrganizationAliasSchemeProviderBase provider = OrganizationManager.Instance.GetOrganizationAliasSchemeProvider(user);
                Organization org = OrganizationManager.Instance.GetOrganizationProvider(user).Get(orgid);
                if (org != null && provider != null)
                {
                    IEnumerable<OrganizationAliasScheme> schemes = provider.Get(org, name);
                    JArray jschemes = Jsonifier.ToJson(schemes);
                    if (jschemes != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jschemes.ToString());
                    else
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                }

                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
            }
            catch
            {
                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
            }
        }

        private static void GetOrg(CompoundIdentity orgid, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                OrganizationAliasSchemeProviderBase provider = OrganizationManager.Instance.GetOrganizationAliasSchemeProvider(user);
                Organization org = OrganizationManager.Instance.GetOrganizationProvider(user).Get(orgid);
                if (org != null && provider != null)
                {
                    IEnumerable<OrganizationAliasScheme> schemes = provider.Get(org);
                    JArray jschemes = Jsonifier.ToJson(schemes);
                    if (jschemes != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jschemes.ToString());
                    else
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                }

                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
            }
            catch
            {
                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
            }
        }

        private static void GetName(string name, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                OrganizationAliasSchemeProviderBase provider = OrganizationManager.Instance.GetOrganizationAliasSchemeProvider(user);
                if (provider != null)
                {
                    IEnumerable<OrganizationAliasScheme> schemes = provider.Get(name);
                    JArray jschemes = Jsonifier.ToJson(schemes);
                    if (jschemes != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jschemes.ToString());
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
                OrganizationAliasSchemeProviderBase provider = OrganizationManager.Instance.GetOrganizationAliasSchemeProvider(user);
                if (provider != null)
                {
                    IEnumerable<OrganizationAliasScheme> schemes = provider.Get();
                    JArray jschemes = Jsonifier.ToJson(schemes);
                    if (jschemes != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jschemes.ToString());
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
