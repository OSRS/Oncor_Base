using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Numerics;
using Osrs.Net.Http;
using Osrs.WellKnown.FieldActivities;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.FieldActivities
{
    internal static class Jsonifier
    {
        private const string Dsid = "dsid";
        private const string Id = "id";

        public static HashSet<CompoundIdentity> ParseIds(string jsonPayload)
        {
            try
            {
                JArray data = JToken.Parse(jsonPayload) as JArray;
                if (data != null)
                {
                    HashSet<CompoundIdentity> ids = new HashSet<CompoundIdentity>();
                    CompoundIdentity item;
                    foreach (JToken cur in data)
                    {
                        item = ToId(cur as JObject);
                        if (item != null)
                            ids.Add(item);
                    }
                    return ids;
                }
            }
            catch
            { }
            return null;
        }

        public static CompoundIdentity ToId(JObject ob)
        {
            if (ob != null)
            {
                if (ob[Dsid] != null && ob[Id] != null)
                {
                    JToken d = ob[Dsid];
                    JToken i = ob[Id];

                    Guid ds;
                    Guid id;

                    if (Guid.TryParse(d.ToString(), out ds) && Guid.TryParse(i.ToString(), out id))
                        return new CompoundIdentity(ds, id);
                }
            }
            return null;
        }

        public static JToken ParseBody(HttpRequest request)
        {
            try
            {
                return JToken.Parse(RestUtils.ReadBody(request));
            }
            catch
            { }
            return null;
        }

        public static JArray ToJson(IEnumerable<FieldActivity> activities)
        {
            if (activities != null)
            {
                JArray o = new JArray();
                foreach (FieldActivity cur in activities)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

        public static JArray ToJson(IEnumerable<SamplingEvent> samples)
        {
            if (samples != null)
            {
                JArray o = new JArray();
                foreach (SamplingEvent cur in samples)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

        public static JArray ToJson(IEnumerable<FieldTrip> trips)
        {
            if (trips != null)
            {
                JArray o = new JArray();
                foreach (FieldTrip cur in trips)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

        public static JObject ToJson(FieldActivity activity)
        {
            if (activity != null)
            {
                JObject o = new JObject();
                o.Add(JsonUtils.Id, JsonUtils.ToJson(activity.Identity));
                o.Add(JsonUtils.Name, activity.Name);
                o.Add(JsonUtils.OwnerId, JsonUtils.ToJson(activity.PrincipalOrgId));
                o.Add(JsonUtils.ProjectId, JsonUtils.ToJson(activity.ProjectId));
                if(activity.Description != null)
                    o.Add(JsonUtils.Description, activity.Description);
                if (activity.DateRange != null)
                {
                    if (activity.DateRange.Min.Equals(DateTime.MinValue))
                        o.Add(JsonUtils.Start, null);
                    else
                        o.Add(JsonUtils.Start, activity.DateRange.Min.ToString());

                    if (activity.DateRange.Max.Equals(DateTime.MinValue) || activity.DateRange.Max.Equals(DateTime.MaxValue))  //never set or closed
                        o.Add(JsonUtils.Finish, null);
                    else
                        o.Add(JsonUtils.Finish, activity.DateRange.Max.ToString());
                }
                return o;
            }
            return null;
        }

        public static JObject ToJson(FieldTrip trip)
        {
            if (trip != null)
            {
                JObject o = new JObject();
                o.Add(JsonUtils.Id, JsonUtils.ToJson(trip.Identity));
                o.Add(JsonUtils.OwnerId, JsonUtils.ToJson(trip.PrincipalOrgId));
                o.Add(JsonUtils.ActivityId, JsonUtils.ToJson(trip.FieldActivityId));
                o.Add(JsonUtils.Name, trip.Name);
                if (trip.Description != null)
                    o.Add(JsonUtils.Description, trip.Description);
                if (trip.DateRange != null)
                {
                    if (trip.DateRange.Min.Equals(DateTime.MinValue))
                        o.Add(JsonUtils.Start, null);
                    else
                        o.Add(JsonUtils.Start, trip.DateRange.Min.ToString());

                    if (trip.DateRange.Max.Equals(DateTime.MinValue) || trip.DateRange.Max.Equals(DateTime.MaxValue))  //never set or closed
                        o.Add(JsonUtils.Finish, null);
                    else
                        o.Add(JsonUtils.Finish, trip.DateRange.Max.ToString());
                }
                return o;
            }
            return null;
        }

        public static JObject ToJson(SamplingEvent sample)
        {
            if (sample != null)
            {
                JObject o = new JObject();
                o.Add(JsonUtils.Id, JsonUtils.ToJson(sample.Identity));
                o.Add(JsonUtils.TripId, JsonUtils.ToJson(sample.FieldTripId));
                o.Add(JsonUtils.OwnerId, JsonUtils.ToJson(sample.PrincipalOrgId));
                o.Add(JsonUtils.Name, sample.Name);
                if (sample.Description != null)
                    o.Add(JsonUtils.Description, sample.Description);
                if (sample.DateRange != null)
                {
                    if (sample.DateRange.Min.Equals(DateTime.MinValue))
                        o.Add(JsonUtils.Start, null);
                    else
                        o.Add(JsonUtils.Start, sample.DateRange.Min.ToString());

                    if (sample.DateRange.Max.Equals(DateTime.MinValue) || sample.DateRange.Max.Equals(DateTime.MaxValue))  //never set or closed
                        o.Add(JsonUtils.Finish, null);
                    else
                        o.Add(JsonUtils.Finish, sample.DateRange.Max.ToString());
                }
                return o;
            }
            return null;
        }

        public static JObject ToJson(CompoundIdentity cid)
        {
            if (cid != null)
            {
                JObject o = new JObject();
                o.Add(JsonUtils.Id, JsonUtils.ToJson(cid));
                return o;
            }
            return null;
        }

        public static bool IsNullOrEmpty(this JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null);
        }
    }
}
