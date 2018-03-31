using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Numerics;
using Osrs.Net.Http;
using Osrs.Security;
using Osrs.Threading;
using System;
using System.Collections.Generic;
using Osrs.WellKnown.FieldActivities;

namespace Pnnl.Oncor.Rest.FieldActivities
{
    internal static class FieldActivityHandler
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
                                string name = token["name"].ToString();
                                GetByName(name, user, context, cancel);
                                return;
                            }
                            else if (token["orgid"] != null)
                            {
                                CompoundIdentity org_id = JsonUtils.ToId(token["orgid"]);
                                GetByOrg(org_id, user, context, cancel);
                                return;
                            }
                            else if (token["projectid"] != null)
                            {
                                CompoundIdentity proj_id = JsonUtils.ToId(token["projectid"]);
                                GetByProject(proj_id, user, context, cancel);
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
                    string name;
                    JToken token = null;
                    FieldActivity activity = null;
                    CompoundIdentity org_id = null;
                    CompoundIdentity proj_id = null;
                    FieldActivityProviderBase provider = null;
                    string desc = null;
                    
                    try
                    {
                        //token and provider
                        token = JsonUtils.GetDataPayload(context.Request);
                        provider = FieldActivityManager.Instance.GetFieldActivityProvider(user);
                        if (provider != null && token != null)
                        {
                            //required inputs
                            name = token["name"].ToString();
                            org_id = JsonUtils.ToId(token["orgid"]);
                            proj_id = JsonUtils.ToId(token["projectid"]);
                            if (org_id != null && proj_id != null && !string.IsNullOrEmpty(name))
                            {
                                // Optionals
                                desc = token["desc"] != null ? token["desc"].ToString() : null;

                                //date range
                                //ValueRange<DateTime> range = JsonUtils.ToRange(token, "start", "end");

                                //create
                                activity = provider.Create(name, proj_id, org_id, desc);
                                if (activity != null)
                                {
                                    JObject jactivity = Jsonifier.ToJson(activity);
                                    RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jactivity.ToString()));
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
                    string name = null;
                    JToken token = null;
                    FieldActivity activity = null;
                    CompoundIdentity cid = null;
                    CompoundIdentity org_id = null;
                    CompoundIdentity project_id = null;
                    FieldActivityProviderBase provider = null;
                    string desc = null;

                    try
                    {
                        //token and provider
                        token = JsonUtils.GetDataPayload(context.Request);
                        provider = FieldActivityManager.Instance.GetFieldActivityProvider(user);
                        if (provider != null && token != null)
                        {
                            //GUID must be provided
                            cid = JsonUtils.ToId(token["id"]);

                            //fetch stored object
                            bool dirty = false;
                            activity = provider.Get(cid);
                            if (activity != null)
                            {
                                //## REQUIRED ##

                                //name
                                if (token.SelectToken("name") != null)
                                {
                                    name = token["name"].ToString();
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        activity.Name = name;
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
                                    org_id = JsonUtils.ToId(token["orgid"]);
                                    if (org_id != null)
                                    {
                                        activity.PrincipalOrgId = org_id;
                                        dirty = true;
                                    }
                                    else
                                    {
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed)); //owning org is required and not nullable
                                        return;
                                    }
                                }

                                //project
                                if (token.SelectToken("projectid") != null)
                                {
                                    project_id = JsonUtils.ToId(token["projectid"]);
                                    if (project_id != null)
                                    {
                                        activity.ProjectId = project_id;
                                        dirty = true;
                                    }
                                    else
                                    {
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed)); //project is required and not nullable
                                        return;
                                    }
                                }

                                //## OPTIONALS ##

                                //description
                                if (token.SelectToken("desc") != null)
                                {
                                    desc = (token["desc"] != null) ? token["desc"].ToString() : null;
                                    activity.Description = desc;
                                    dirty = true;
                                }

                                //start and end dates
                                //if (token.SelectToken("start") != null)
                                //{
                                //}

                                if (dirty)
                                {
                                    //update
                                    bool result = provider.Update(activity);
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
                else if (method.Equals("delete", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        JToken t = JsonUtils.GetDataPayload(context.Request);
                        HashSet<CompoundIdentity> cids = JsonUtils.ToIds(t);
                        FieldActivityProviderBase provider = FieldActivityManager.Instance.GetFieldActivityProvider(user);
                        if (cids != null && provider != null)
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
            }
            context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
        }

        private static void GetIds(HashSet<CompoundIdentity> ids, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            if (ids.Count == 0)
            {
                RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                return;
            }

            try {
                FieldActivityProviderBase provider = FieldActivityManager.Instance.GetFieldActivityProvider(user);
                if (provider != null)
                {
                    IEnumerable<FieldActivity> activities = provider.Get(ids);
                    JArray jactivities = Jsonifier.ToJson(activities);
                    if (jactivities != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jactivities.ToString()));
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
                FieldActivityProviderBase provider = FieldActivityManager.Instance.GetFieldActivityProvider(user);
                if (provider != null)
                {
                    IEnumerable<FieldActivity> activities = provider.Get(name);
                    JArray jactivities = Jsonifier.ToJson(activities);
                    if (jactivities != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jactivities.ToString()));
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

        private static void GetByProject(CompoundIdentity project_id, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                FieldActivityProviderBase provider = FieldActivityManager.Instance.GetFieldActivityProvider(user);
                if (provider != null)
                {
                    IEnumerable<FieldActivity> activities = provider.GetForProject(project_id);
                    JArray jactivities = Jsonifier.ToJson(activities);
                    if (jactivities != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jactivities.ToString());
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

        private static void GetByOrg(CompoundIdentity org_id, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                FieldActivityProviderBase provider = FieldActivityManager.Instance.GetFieldActivityProvider(user);
                if (provider != null)
                {
                    IEnumerable<FieldActivity> activities = provider.GetForOrg(org_id);
                    JArray jactivities = Jsonifier.ToJson(activities);
                    if (jactivities != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jactivities.ToString());
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
                FieldActivityProviderBase provider = FieldActivityManager.Instance.GetFieldActivityProvider(user);
                if (provider != null)
                {
                    IEnumerable<FieldActivity> activities = provider.Get();
                    JArray jactivities = Jsonifier.ToJson(activities);
                    if (jactivities != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jactivities.ToString()));
                    else
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, "[]"));
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
