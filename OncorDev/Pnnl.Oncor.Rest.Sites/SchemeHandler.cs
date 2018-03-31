using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Net.Http;
using Osrs.Security;
using Osrs.Threading;
using Osrs.WellKnown.Sites;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.Sites
{
    internal static class SchemeHandler
    {
        //  --->   /sites/schemes/all
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
                            string name = null;
                            CompoundIdentity org_id = null;

                            if (token["name"] != null)
                                name = token["name"].ToString();

                            if (token["orgid"] != null)
                                org_id = JsonUtils.ToId(token["orgid"]);

                            GetOrgAndName(org_id, name, user, context, cancel);
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
                        return;
                    }
                }
                else if (method.Equals("create", StringComparison.OrdinalIgnoreCase))
                {
                    CompoundIdentity orgid = null;
                    string name = null;
                    string desc = null;
                    JToken token = null;
                    SiteAliasSchemeProviderBase provider = null;

                    try
                    {
                        //payload and provider
                        token = JsonUtils.GetDataPayload(context.Request);
                        provider = SiteManager.Instance.GetSiteAliasSchemeProvider(user);
                        if (provider != null && token != null)
                        {
                            //required inputs
                            name = token["name"].ToString();
                            orgid = JsonUtils.ToId(token["orgid"]);
                            if (!string.IsNullOrEmpty(name) && orgid != null)
                            {
                                //optionals
                                desc = token["desc"] != null ? token["desc"].ToString() : null;

                                //create
                                SiteAliasScheme scheme = scheme = provider.Create(orgid, name, desc);
                                if (scheme != null)
                                {
                                    JObject jscheme = Jsonifier.ToJson(scheme);
                                    if (jscheme != null)
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jscheme.ToString()));
                                    else
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
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
                else if (method.Equals("delete", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        JToken t = JsonUtils.GetDataPayload(context.Request);
                        CompoundIdentity cid = JsonUtils.ToId(t);
                        SiteAliasSchemeProviderBase provider = SiteManager.Instance.GetSiteAliasSchemeProvider(user);
                        if (provider != null && cid != null)
                        {
                            bool result = provider.Delete(cid);
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
                    string name = null;
                    string desc = null;

                    try
                    {
                        //provider and token
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        SiteAliasSchemeProviderBase provider = SiteManager.Instance.GetSiteAliasSchemeProvider(user);
                        if (provider != null && token != null)
                        {
                            //GUID must be provided
                            cid = JsonUtils.ToId(token["id"]);

                            //fetch stored object
                            bool dirty = false;
                            SiteAliasScheme scheme = provider.Get(cid);
                            if (scheme == null)
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
                                    org_cid = JsonUtils.ToId(token["orgid"]);
                                    if (org_cid != null)
                                    {
                                        scheme.OwningOrganizationIdentity = org_cid;
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

        private static void GetOrgAndName(CompoundIdentity orgid,String name,  UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                SiteAliasSchemeProviderBase provider = SiteManager.Instance.GetSiteAliasSchemeProvider(user);
                if (provider != null)
                {
                    JArray schemes = null;
                    if (orgid != null && !string.IsNullOrEmpty(name))
                        schemes = Jsonifier.ToJson(provider.GetByOwner(orgid, name));
                    else if (orgid != null)
                        schemes = Jsonifier.ToJson(provider.GetByOwner(orgid));
                    else if (!string.IsNullOrEmpty(name))
                        schemes = Jsonifier.ToJson(provider.Get(name));

                    if (schemes != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, schemes.ToString());
                    else
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
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

                SiteAliasSchemeProviderBase provider = SiteManager.Instance.GetSiteAliasSchemeProvider(user);
                if (provider != null)
                {
                    JObject scheme = null;
                    JArray schemes = new JArray();
                    foreach (CompoundIdentity cid in idList)
                    {
                        scheme = Jsonifier.ToJson(provider.Get(cid));

                        if (scheme != null)
                            schemes.Add(scheme);
                    }

                    if (schemes != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, schemes.ToString());
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
                SiteAliasSchemeProviderBase provider = SiteManager.Instance.GetSiteAliasSchemeProvider(user);
                if (provider != null)
                {
                    IEnumerable<SiteAliasScheme> schemes = provider.Get();
                    JArray jschemes = Jsonifier.ToJson(schemes);
                    if (jschemes != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jschemes.ToString());
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
