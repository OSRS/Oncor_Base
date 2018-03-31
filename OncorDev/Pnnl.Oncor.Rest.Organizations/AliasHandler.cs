using Newtonsoft.Json.Linq;
using Osrs.Net.Http;
using Osrs.Security;
using Osrs.Threading;
using Osrs.WellKnown.Organizations;
using Osrs.Data;
using System;
using System.Collections.Generic;


namespace Pnnl.Oncor.Rest.Organizations
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
                                GetByOrgAndScheme(JsonUtils.ToId(token["id"]), JsonUtils.ToId(token["schemeid"]), user, context, cancel);
                                return;
                            }
                            else if (token["id"] != null && token["name"] != null)
                            {
                                GetByOrgAndName(JsonUtils.ToId(token["id"]), token["name"].ToString(), user, context, cancel);
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
                                CompoundIdentity cid = JsonUtils.ToId(token["id"]);
                                GetByOrg(cid, user, context, cancel);
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
                        OrganizationProviderBase org_provider = OrganizationManager.Instance.GetOrganizationProvider(user);
                        OrganizationAliasSchemeProviderBase scheme_provider = OrganizationManager.Instance.GetOrganizationAliasSchemeProvider(user);
                        OrganizationAliasProviderBase alias_provider = OrganizationManager.Instance.GetOrganizationAliasProvider(user);
                        if (org_provider != null && scheme_provider != null && alias_provider != null &&  token != null)
                        {
                            //get inputs
                            OrganizationAliasScheme scheme = scheme_provider.Get(JsonUtils.ToId(token["schemeid"]));
                            Organization org = org_provider.Get(JsonUtils.ToId(token["id"]));
                            string name = token["name"].ToString();
                            if (scheme != null && org != null && !string.IsNullOrEmpty(name))
                            {
                                //create object
                                OrganizationAlias alias = alias_provider.Create(scheme, org, name);
                                if (alias != null)
                                {
                                    JObject jalias = Jsonifier.ToJson(alias);
                                    RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jalias.ToString()));
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
                        OrganizationAliasSchemeProviderBase scheme_provider = OrganizationManager.Instance.GetOrganizationAliasSchemeProvider(user);
                        OrganizationAliasProviderBase alias_provider = OrganizationManager.Instance.GetOrganizationAliasProvider(user);
                        if (scheme_provider != null && alias_provider != null && token != null)
                        {
                            //required fields
                            CompoundIdentity orgid = JsonUtils.ToId(token["id"]);
                            OrganizationAliasScheme scheme = scheme_provider.Get(JsonUtils.ToId(token["schemeid"]));
                            string old_name = token["name"].ToString();
                            string new_name = token["newname"].ToString();
                            if (orgid != null && scheme != null && !string.IsNullOrEmpty(old_name) && !string.IsNullOrEmpty(new_name))
                            {
                                //get entity and modify its properties
                                IEnumerable<OrganizationAlias> aliases = alias_provider.Get(orgid, scheme);
                                if (aliases != null)
                                {
                                    //match alias by name (an org could have multiple aliases in the same scheme, but they must be unique)
                                    OrganizationAlias alias = null;
                                    foreach (OrganizationAlias a in aliases)
                                    {
                                        if (a.Name == old_name)
                                            alias = a;
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
                    CompoundIdentity org_id = null;
                    OrganizationAliasScheme scheme = null;
                    string name = null;

                    try
                    {
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        OrganizationProviderBase org_provider = OrganizationManager.Instance.GetOrganizationProvider(user);
                        OrganizationAliasSchemeProviderBase scheme_provider = OrganizationManager.Instance.GetOrganizationAliasSchemeProvider(user);
                        OrganizationAliasProviderBase provider = OrganizationManager.Instance.GetOrganizationAliasProvider(user);
                        if (provider != null && scheme_provider != null && token != null)
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

                            //orgid
                            if (token.SelectToken("id") != null)
                            {
                                if (token["id"] == null)
                                {
                                    RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                                    return;
                                }
                                else
                                {
                                    org_id = JsonUtils.ToId(token["id"]);
                                    Organization org = org_provider.Get(org_id);
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
                            if (scheme_id != null && org_id != null && name != null)    //delete specific alias
                            {
                                //don't have a provider method that returns specific alias, so get by org, scheme and match alias by name (most of the time a single alias will be returned)
                                IEnumerable<OrganizationAlias> aliases = provider.Get(org_id, scheme);
                                if (aliases != null)
                                {
                                    foreach (OrganizationAlias alias in aliases)
                                    {
                                        if (alias.Name == name)
                                        {
                                            result = provider.Delete(alias);
                                            break;                                          //aliases should be unique for a given org in a given scheme
                                        }
                                    }
                                }
                            }
                            else if (scheme_id != null && org_id != null)               //delete * for given org in a given scheme
                                result = provider.Delete(org_id, scheme_id);

                            else if (scheme_id != null)                                 //delete * for a given scheme
                                result = provider.Delete(scheme);

                            else if (org_id != null)                                    //delete * for a given org
                                result = provider.Delete(org_id);

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

        private static void GetByOrgAndScheme(CompoundIdentity org_id, CompoundIdentity scheme_id, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                Organization org = OrganizationManager.Instance.GetOrganizationProvider(user).Get(org_id);
                OrganizationAliasScheme scheme = OrganizationManager.Instance.GetOrganizationAliasSchemeProvider(user).Get(scheme_id);
                OrganizationAliasProviderBase provider = OrganizationManager.Instance.GetOrganizationAliasProvider(user);
                if (scheme != null && org != null && provider != null)
                {
                    IEnumerable<OrganizationAlias> aliases = provider.Get(org_id, scheme);
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
                OrganizationAliasProviderBase provider = OrganizationManager.Instance.GetOrganizationAliasProvider(user);
                OrganizationAliasScheme scheme = OrganizationManager.Instance.GetOrganizationAliasSchemeProvider(user).Get(scheme_id);
                if (scheme != null && provider != null)
                {
                    IEnumerable<OrganizationAlias> aliases = provider.Get(scheme, name);
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

        private static void GetByOrgAndName(CompoundIdentity org_id, string name, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                OrganizationAliasProviderBase provider = OrganizationManager.Instance.GetOrganizationAliasProvider(user);
                Organization org = OrganizationManager.Instance.GetOrganizationProvider(user).Get(org_id);
                if (org != null && provider != null)
                {
                    IEnumerable<OrganizationAlias> aliases = provider.Get(org, name);
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
                OrganizationAliasProviderBase provider = OrganizationManager.Instance.GetOrganizationAliasProvider(user);
                OrganizationAliasScheme scheme = OrganizationManager.Instance.GetOrganizationAliasSchemeProvider(user).Get(scheme_id);
                if (scheme != null && provider != null)
                {
                    IEnumerable<OrganizationAlias> aliases = provider.Get(scheme);
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

        private static void GetByOrg(CompoundIdentity org_id, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                OrganizationAliasProviderBase provider = OrganizationManager.Instance.GetOrganizationAliasProvider(user);
                Organization org = OrganizationManager.Instance.GetOrganizationProvider(user).Get(org_id);
                if(org != null && provider != null)
                {
                    IEnumerable<OrganizationAlias> aliases = provider.Get(org);
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
                OrganizationAliasProviderBase provider = OrganizationManager.Instance.GetOrganizationAliasProvider(user);
                if (provider != null)
                {
                    IEnumerable<OrganizationAlias> aliases = provider.Get(name);
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

        private static void Get(UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                OrganizationAliasProviderBase provider = OrganizationManager.Instance.GetOrganizationAliasProvider(user);
                if (provider != null)
                {
                    IEnumerable<OrganizationAlias> aliases = provider.Get();
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
