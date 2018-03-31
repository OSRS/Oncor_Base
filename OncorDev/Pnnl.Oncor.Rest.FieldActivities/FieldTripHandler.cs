using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Numerics;
using Osrs.Net.Http;
using Osrs.Security;
using Osrs.Threading;
using System;
using Osrs.WellKnown.FieldActivities;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.FieldActivities
{
    internal static class FieldTripHandler
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
                                GetName(name, user, context, cancel);
                                return;
                            }
                            else if (token["orgid"] != null)
                            {
                                CompoundIdentity org_id = JsonUtils.ToId(token["orgid"]);
                                GetByOrg(org_id, user, context, cancel);
                                return;
                            }
                            else if (token["activityid"] != null)
                            {
                                CompoundIdentity activity_id = JsonUtils.ToId(token["activityid"]);
                                GetByActivity(activity_id, user, context, cancel);
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
                    JToken token = null;
                    FieldTripProviderBase provider = null;
                    FieldTrip trip = null;
                    FieldActivity activity = null;
                    CompoundIdentity org_id = null;
                    CompoundIdentity act_id = null;
                    string name;
                    string desc = null;

                    try
                    {
                        //token and provider
                        token = JsonUtils.GetDataPayload(context.Request);
                        provider = FieldActivityManager.Instance.GetFieldTripProvider(user);
                        if (provider != null && token != null)
                        {
                            //required inputs
                            name = token["name"].ToString();
                            org_id = JsonUtils.ToId(token["orgid"]);
                            act_id = JsonUtils.ToId(token["activityid"]);
                            activity = FieldActivityManager.Instance.GetFieldActivityProvider(user).Get(act_id);
                            if (org_id != null && activity != null && !string.IsNullOrEmpty(name))
                            {
                                desc = token["desc"] != null ? token["desc"].ToString() : null;

                                //start and end dates
                                //ValueRange<DateTime> range = JsonUtils.ToRange(token, "start", "end");

                                //create
                                trip = provider.Create(name, activity, org_id, desc);
                                if (trip != null)
                                {
                                    JObject jtrip = Jsonifier.ToJson(trip);
                                    RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jtrip.ToString()));
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
                    JToken token = null;
                    FieldTripProviderBase provider = null;
                    FieldTrip trip = null;
                    CompoundIdentity cid = null;
                    CompoundIdentity org_id = null;
                    CompoundIdentity act_id = null;
                    string name;
                    string desc = null;

                    try
                    {
                        //token and provider
                        token = JsonUtils.GetDataPayload(context.Request);
                        provider = FieldActivityManager.Instance.GetFieldTripProvider(user);
                        if (provider != null && token != null)
                        {
                            //GUID must be provided
                            cid = JsonUtils.ToId(token["id"]);

                            //fetch stored object
                            bool dirty = false;
                            trip = provider.Get(cid);
                            if (trip != null)
                            {
                                //## REQUIRED ##

                                //name
                                if (token.SelectToken("name") != null)
                                {
                                    name = token["name"].ToString();
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        trip.Name = name;
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
                                        trip.PrincipalOrgId = org_id;
                                        dirty = true;
                                    }
                                    else
                                    {
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed)); //owning org is required and not nullable
                                        return;
                                    }
                                }

                                //field activity
                                if (token.SelectToken("activityid") != null)
                                {
                                    act_id = JsonUtils.ToId(token["activityid"]);
                                    if (act_id != null)
                                    {
                                        trip.FieldActivityId = act_id;
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
                                    trip.Description = desc;
                                    dirty = true;
                                }

                                //start and end dates
                                //if (token.SelectToken("start") != null)
                                //{
                                //}

                                if (dirty)
                                {
                                    //update
                                    bool result = provider.Update(trip);
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
                        FieldTripProviderBase provider = FieldActivityManager.Instance.GetFieldTripProvider(user);
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

            try
            {
                FieldTripProviderBase provider = FieldActivityManager.Instance.GetFieldTripProvider(user);
                if (provider != null)
                {
                    IEnumerable<FieldTrip> trips = provider.Get(ids);
                    JArray jtrips = Jsonifier.ToJson(trips);
                    if (jtrips != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jtrips.ToString()));
                    else
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, "[]"));
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

        private static void GetByActivity(CompoundIdentity activity_id, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                FieldTripProviderBase provider = FieldActivityManager.Instance.GetFieldTripProvider(user);
                if (provider != null)
                {
                    IEnumerable<FieldTrip> trips = provider.GetForActivity(activity_id);
                    JArray jtrips = Jsonifier.ToJson(trips);
                    if (jtrips != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jtrips.ToString()));
                    else
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, "[]"));
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

        private static void GetByOrg(CompoundIdentity org_id, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                FieldTripProviderBase provider = FieldActivityManager.Instance.GetFieldTripProvider(user);
                if (provider != null)
                {
                    IEnumerable<FieldTrip> trips = provider.GetForOrg(org_id);
                    JArray jtrips = Jsonifier.ToJson(trips);
                    if (jtrips != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jtrips.ToString()));
                    else
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, "[]"));
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
                FieldTripProviderBase provider = FieldActivityManager.Instance.GetFieldTripProvider(user);
                if (provider != null)
                {
                    IEnumerable<FieldTrip> trips = provider.Get(name);
                    JArray jtrips = Jsonifier.ToJson(trips);
                    if (jtrips != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jtrips.ToString()));
                    else
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, "[]"));
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
                FieldTripProviderBase provider = FieldActivityManager.Instance.GetFieldTripProvider(user);
                if (provider != null)
                {
                    IEnumerable<FieldTrip> trips = provider.Get();
                    JArray jtrips = Jsonifier.ToJson(trips);
                    if (jtrips != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jtrips.ToString()));
                    else
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, "[]"));
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
    }
}
