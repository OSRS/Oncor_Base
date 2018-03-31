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
    internal static class AliasHandler
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

                            if (token["id"] != null && token["schemeid"] != null)
                            {
                                GetBySiteAndScheme(JsonUtils.ToId(token["id"]), JsonUtils.ToId(token["schemeid"]), user, context, cancel);
                                return;
                            }
                            else if (token["id"] != null && token["name"] != null)
                            {
                                GetBySiteAndName(JsonUtils.ToId(token["id"]), token["name"].ToString(), user, context, cancel);
                                return;
                            }
                            else if (token["schemeid"] != null && token["name"] != null)
                            {
                                GetBySchemeAndName(JsonUtils.ToId(token["schemeid"]), token["name"].ToString(), user, context, cancel);
                                return;
                            }
                            else if (token["schemeid"] != null)
                            {
                                GetByScheme(JsonUtils.ToId(token["schemeid"]), user, context, cancel);
                                return;
                            }
                            else if (token["id"] != null)
                            {
                                GetBySite(JsonUtils.ToId(token["id"]), user, context, cancel);
                                return;
                            }
                            else if (token["name"] != null)
                            {
                                GetByName(token["name"].ToString(), user, context, cancel);
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
                else if (method.Equals("create", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        //token and providers
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        SiteAliasProviderBase alias_provider = SiteManager.Instance.GetSiteAliasProvider(user);
                        SiteProviderBase site_provider = SiteManager.Instance.GetSiteProvider(user);
                        SiteAliasSchemeProviderBase scheme_provider = SiteManager.Instance.GetSiteAliasSchemeProvider(user);
                        if (alias_provider != null && scheme_provider != null && site_provider != null && token != null)
                        {
                            //required
                            string name = token["name"].ToString();
                            SiteAliasScheme scheme = scheme_provider.Get(JsonUtils.ToId(token["schemeid"]));
                            Site site = site_provider.Get(JsonUtils.ToId(token["id"]));
                            if (site != null && scheme != null && !string.IsNullOrEmpty(name))
                            {
                                //create object
                                SiteAlias alias = alias_provider.Create(scheme, site, name);
                                if (alias != null)
                                {
                                    JObject jalias = Jsonifier.ToJson(alias);
                                    if (jalias != null)
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jalias.ToString()));
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
                else if (method.Equals("update", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        //token and providers
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        SiteAliasProviderBase alias_provider = SiteManager.Instance.GetSiteAliasProvider(user);
                        SiteProviderBase site_provider = SiteManager.Instance.GetSiteProvider(user);
                        SiteAliasSchemeProviderBase scheme_provider = SiteManager.Instance.GetSiteAliasSchemeProvider(user);
                        if (alias_provider != null && scheme_provider != null && site_provider != null && token != null)
                        {
                            //required fields
                            CompoundIdentity site_id = JsonUtils.ToId(token["id"]);
                            Site site = site_provider.Get(site_id);
                            CompoundIdentity scheme_id = JsonUtils.ToId(token["schemeid"]);
                            SiteAliasScheme scheme = scheme_provider.Get(scheme_id);
                            string old_name = token["name"].ToString();
                            string new_name = token["newname"].ToString();
                            if (site != null && scheme == null && !string.IsNullOrEmpty(old_name) && !string.IsNullOrEmpty(new_name))
                            {
                                //retrieve by site, scheme
                                IEnumerable<SiteAlias> aliases = alias_provider.Get(site_id, scheme);
                                if (aliases == null)
                                {
                                    //match alias by name (an org could have multiple aliases in the same scheme, but they must be unique)
                                    SiteAlias alias = null;
                                    foreach (SiteAlias a in aliases)
                                    {
                                        if (a.Name == old_name)
                                        {
                                            alias = a;
                                        }
                                    }
                                    alias.Name = new_name;
                                    bool result = alias_provider.Update(alias);
                                    if (result == true)
                                    {
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok));
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
                    CompoundIdentity scheme_id = null;
                    CompoundIdentity site_id = null;
                    SiteAliasScheme scheme = null;
                    Site site = null;
                    string name = null;

                    try
                    {
                        //token and providers
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        SiteAliasProviderBase alias_provider = SiteManager.Instance.GetSiteAliasProvider(user);
                        SiteProviderBase site_provider = SiteManager.Instance.GetSiteProvider(user);
                        SiteAliasSchemeProviderBase scheme_provider = SiteManager.Instance.GetSiteAliasSchemeProvider(user);
                        if (alias_provider != null && scheme_provider != null && site_provider != null && token != null)
                        {
                            //If a token is provided, it cannot be null
                            //Checking values against intent avoids firing a degenerate delete override

                            //schemeid
                            if (token.SelectToken("schemeid") != null)
                            {
                                if (token["schemeid"] == null)
                                {
                                    RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                                    return;
                                }
                                else
                                {
                                    scheme_id = JsonUtils.ToId(token["schemeid"]);
                                    scheme = scheme_provider.Get(scheme_id);
                                }
                            }

                            //id
                            if (token.SelectToken("id") != null)
                            {
                                if (token["id"] == null)
                                {
                                    RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                                    return;
                                }
                                else
                                {
                                    site_id = JsonUtils.ToId(token["id"]);
                                    site = site_provider.Get(site_id);
                                }
                            }

                            //name
                            if (token.SelectToken("name") != null)
                            {
                                if (token["name"] == null)
                                {
                                    RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                                    return;
                                }
                                else
                                    name = token["name"].ToString();
                            }

                            //determine override
                            bool result = false;
                            if (scheme != null && site != null && name != null)    //delete specific alias
                            {
                                //don't have a provider method that returns specific alias, so get by site, scheme and match alias by name (most of the time a single alias will be returned)
                                IEnumerable<SiteAlias> aliases = alias_provider.Get(site_id, scheme);
                                if (aliases != null)
                                {
                                    foreach (SiteAlias alias in aliases)
                                    {
                                        if (alias.Name == name)
                                        {
                                            result = alias_provider.Delete(alias);
                                            break;                                          //aliases should be unique for a given site in a given scheme
                                        }
                                    }
                                }
                            }
                            else if (scheme != null && site_id != null)
                                result = alias_provider.Delete(site_id, scheme);       //delete * for given site in a given scheme (could be multiple)

                            else if (scheme != null)
                                result = alias_provider.Delete(scheme);                //delete * for a given scheme (across orgs)

                            else if (site_id != null)
                                result = alias_provider.Delete(site_id);               //delete * for a given org (across schemes)

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
            }
            context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
        }

        private static void Get(UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                SiteAliasProviderBase provider = SiteManager.Instance.GetSiteAliasProvider(user);
                if (provider != null)
                {
                    IEnumerable<SiteAlias> aliases = provider.Get();
                    JArray jaliases = Jsonifier.ToJson(aliases);
                    if (jaliases != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jaliases.ToString());
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

        private static void GetByName(string name, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                SiteAliasProviderBase provider = SiteManager.Instance.GetSiteAliasProvider(user);
                if (provider != null)
                {
                    IEnumerable<SiteAlias> aliases = provider.Get(name);
                    JArray jaliases = Jsonifier.ToJson(aliases);
                    if (jaliases != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jaliases.ToString());
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

        private static void GetBySite(CompoundIdentity site_id, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                SiteAliasProviderBase provider = SiteManager.Instance.GetSiteAliasProvider(user);
                Site site = SiteManager.Instance.GetSiteProvider(user).Get(site_id);
                if (provider != null && site != null)
                {
                    IEnumerable<SiteAlias> aliases = provider.Get(site);
                    JArray jaliases = Jsonifier.ToJson(aliases);
                    if (jaliases != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jaliases.ToString());
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

        private static void GetByScheme(CompoundIdentity scheme_id, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                SiteAliasProviderBase provider = SiteManager.Instance.GetSiteAliasProvider(user);
                SiteAliasScheme scheme = SiteManager.Instance.GetSiteAliasSchemeProvider(user).Get(scheme_id);
                if (provider != null && scheme != null)
                {
                    IEnumerable<SiteAlias> aliases = provider.Get(scheme);
                    JArray jaliases = Jsonifier.ToJson(aliases);
                    if (jaliases != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jaliases.ToString());
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

        private static void GetBySchemeAndName(CompoundIdentity scheme_id, string name, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                SiteAliasProviderBase provider = SiteManager.Instance.GetSiteAliasProvider(user);
                SiteAliasScheme scheme = SiteManager.Instance.GetSiteAliasSchemeProvider(user).Get(scheme_id);
                if (provider != null && scheme != null)
                {
                    IEnumerable<SiteAlias> aliases = provider.Get(scheme, name);
                    JArray jaliases = Jsonifier.ToJson(aliases);
                    if (jaliases != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jaliases.ToString());
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

        private static void GetBySiteAndName(CompoundIdentity site_id, string name, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                SiteAliasProviderBase provider = SiteManager.Instance.GetSiteAliasProvider(user);
                Site site = SiteManager.Instance.GetSiteProvider(user).Get(site_id);
                if (site != null && provider != null)
                {
                    IEnumerable<SiteAlias> aliases = provider.Get(site, name);
                    JArray jaliases = Jsonifier.ToJson(aliases);
                    if (jaliases != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jaliases.ToString());
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

        private static void GetBySiteAndScheme(CompoundIdentity site_id, CompoundIdentity scheme_id, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                SiteAliasProviderBase provider = SiteManager.Instance.GetSiteAliasProvider(user);
                SiteAliasScheme scheme = SiteManager.Instance.GetSiteAliasSchemeProvider(user).Get(scheme_id);
                if (provider != null && scheme != null && site_id != null)
                {
                    IEnumerable<SiteAlias> aliases = provider.Get(site_id, scheme);
                    JArray jaliases = Jsonifier.ToJson(aliases);
                    if (jaliases != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jaliases.ToString());
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
