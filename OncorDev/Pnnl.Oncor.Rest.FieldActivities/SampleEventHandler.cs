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
    internal static class SampleEventHandler
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
                            else if (token["tripid"] != null)
                            {
                                CompoundIdentity trip_id = JsonUtils.ToId(token["tripid"]);
                                GetByTrip(trip_id, user, context, cancel);
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
                SampleEventProviderBase provider = FieldActivityManager.Instance.GetSampleEventProvider(user);
                if (provider != null)
                {
                    IEnumerable<SamplingEvent> events = provider.Get(ids);
                    JArray jevents = Jsonifier.ToJson(events);

                    if (jevents != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jevents.ToString()));
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

        private static void GetByTrip(CompoundIdentity trip_id, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                SampleEventProviderBase provider = FieldActivityManager.Instance.GetSampleEventProvider(user);
                if (provider != null)
                {
                    IEnumerable<SamplingEvent> events = provider.GetForTrip(trip_id);
                    JArray jevents = Jsonifier.ToJson(events);
                    if (jevents != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jevents.ToString()));
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
                SampleEventProviderBase provider = FieldActivityManager.Instance.GetSampleEventProvider(user);
                if (provider != null)
                {
                    IEnumerable<SamplingEvent> events = provider.GetForOrg(org_id);
                    JArray jevents = Jsonifier.ToJson(events);
                    if (jevents != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jevents.ToString()));
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

        private static void GetByName(string name, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                SampleEventProviderBase provider = FieldActivityManager.Instance.GetSampleEventProvider(user);
                if (provider != null)
                {
                    IEnumerable<SamplingEvent> events = provider.Get(name);
                    JArray jevents = Jsonifier.ToJson(events);
                    if (jevents != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jevents.ToString()));
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
                SampleEventProviderBase provider = FieldActivityManager.Instance.GetSampleEventProvider(user);
                if (provider != null)
                {
                    IEnumerable<SamplingEvent> events = provider.Get();
                    JArray jevents = Jsonifier.ToJson(events);
                    if (jevents != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jevents.ToString()));
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
