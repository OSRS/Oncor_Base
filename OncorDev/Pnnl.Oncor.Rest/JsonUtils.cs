using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Numerics;
using Osrs.Net.Http;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest
{
    /// <summary>
    ///Standard client/server communication for Oncor REST calls...
    ///  all methods will use POST with a JSON formatted payload
    ///standard method data payloads:
    ///  compound ids -- formatted as an object as {dsid:GUID, id:GUID}
    ///  for most get/delete methods, treated as an array [<cmpid>, <cmpid>, ...]
    ///  for specific get 1 item (e.g. getbyid), may send bare compound id
    ///  for most create/update -- send an object in data field

    ///client to server:
    ///  {data:&ltmethod specific stuff}

    ///server to client:
    ///  {status:&ltJsonOpStatus>, data:&ltmethod specific stuff>}

    ///formatted examples:
    ///  XXX/get/in  ->   {data:[{dsid:GUID,id:GUID},{dsid:GUID,id:GUID},...]}
    ///  XXX/get/id  ->   {data:{dsid:GUID,id:GUID}}   OR   {data:[{dsid:GUID,id:GUID},{dsid:GUID,id:GUID},...]}
    ///  XXX/delete  ->   {data:{dsid:GUID,id:GUID}}   OR   {data:[{dsid:GUID,id:GUID},{dsid:GUID,id:GUID},...]}
    ///  XXX/update  ->   {data:{id:{dsid:GUID,id:GUID},k:v,k:v,...}   (where k/v are the properties of the entity to be updated and id is the compoundId identifying the entity)
    ///  XXX/create  ->   {data:{k:v,k:v,...}   (where k/v are the properties of the entity to be updated with no id sent - the constructed entity will be returned by the server)

    ///To extract a json payload from the client on the server do:
    ///      given a request body like this:     {data:[{dsid, id},{dsid,id}],status:"foo"}
    ///  full object:  JToken t = JsonUtils.ParseBody(request);
    ///      returns:  {data:[{dsid, id},{dsid,id}],status:"foo"}   (as a JToken)
    ///
    ///  just the data field:  JToken t = JsonUtils.GetDataPayload(request);
    ///      returns:  [{dsid, id},{dsid,id}]   (as a JToken)
    /// </summary>
    public static class JsonUtils
    {
        public const string Dsid = "dsid";
        public const string Id = "id";
        public const string Name = "name";
        public const string Description = "desc";
        public const string SchemeId = "schemeid";
        public const string OwnerId = "orgid";
        public const string ParentId = "parentid";
        public const string ProjectId = "projectid";
        public const string ActivityId = "activityid";
        public const string TripId = "fieldtripid";
        public const string Affiliates = "affiliateorgid";
        public const string Location = "geom";
        public const string LocationMark = "altgeom";
        public const string Start = "start";
        public const string Finish = "end";
        public const string Type = "type";

        public static string ToString(JToken dataPayload)
        {
            if (dataPayload != null && dataPayload is JValue)
            {
                JValue tmp = dataPayload as JValue;
                if (dataPayload !=null)
                {
                    return dataPayload.ToString();
                }
            }
            return null;
        }

        public static DateTime? ToDate(JToken dataPayload)
        {
            if (dataPayload != null && dataPayload is JValue)
            {
                JValue tmp = dataPayload as JValue;
                if (tmp != null)
                {
                    DateTime res;
                    if (tmp.Type == JTokenType.Date)
                    {
                        try
                        {
                            res = (DateTime)tmp;
                            if (res.Kind != DateTimeKind.Utc)
                                res = res.ToUniversalTime();
                            return res;
                        }
                        catch { }
                    }
                    if (DateTime.TryParse(tmp.ToString(), out res))
                        return res;
                }
            }
            return null;
        }

        public static ValueRange<DateTime> ToRange(JToken outer, string startName, string endName)
        {
            if (outer!=null)
            {
                DateTime? start = ToDate(outer[startName]);
                DateTime? end = ToDate(outer[endName]);
                if (start == null)
                {
                    if (end == null)
                        return null; //both were null
                    start = DateTime.MinValue; //end was not null
                }
                else if (end == null) //note we already did a end check if start was null
                    end = DateTime.MaxValue; //start was not null

                if (start<=end)
                    return new ValueRange<DateTime>(start.Value, end.Value);
            }
            return null;
        }


        public static HashSet<CompoundIdentity> ToIds(JToken dataPayload)
        {
            if (dataPayload != null)
            {
                if (dataPayload is JArray)
                    return ToIds(dataPayload as JArray);
                else
                    return ToIds(dataPayload as JObject);
            }
            return null;
        }

        public static HashSet<CompoundIdentity> ToIds(JObject dataPayload)
        {
            if (dataPayload != null)
            {
                try
                {
                    CompoundIdentity id = ToId(dataPayload);
                    if (id != null)
                    {
                        HashSet<CompoundIdentity> ids = new HashSet<CompoundIdentity>();
                        ids.Add(id);
                        return ids;
                    }
                }
                catch
                { }
            }
            return null;
        }

        public static HashSet<CompoundIdentity> ToIds(JArray dataPayload)
        {
            if (dataPayload != null)
            {
                try
                {
                    HashSet<CompoundIdentity> ids = new HashSet<CompoundIdentity>();
                    CompoundIdentity item;
                    foreach (JToken cur in dataPayload)
                    {
                        item = ToId(cur as JObject);
                        if (item != null)
                            ids.Add(item);
                    }
                    return ids;
                }
                catch
                { }
            }
            return null;
        }

        public static CompoundIdentity ToId(JToken ob)
        {
            if (ob != null)
                return ToId(ob as JObject);
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

        public static HashSet<Guid> ToGuids(JToken dataPayload)
        {
            if (dataPayload != null)
            {
                if (dataPayload is JArray)
                    return ToGuids(dataPayload as JArray);
                else
                    return ToGuids(dataPayload as JValue);
            }
            return null;
        }

        public static HashSet<Guid> ToGuids(JValue dataPayload)
        {
            if (dataPayload != null)
            {
                try
                {
                    Guid id = ToGuid(dataPayload);
                    if (!Guid.Empty.Equals(id))
                    {
                        HashSet<Guid> ids = new HashSet<Guid>();
                        ids.Add(id);
                        return ids;
                    }
                }
                catch
                { }
            }
            return null;
        }

        public static HashSet<Guid> ToGuids(JArray dataPayload)
        {
            if (dataPayload != null)
            {
                try
                {
                    HashSet<Guid> ids = new HashSet<Guid>();
                    Guid item;
                    foreach (JToken cur in dataPayload)
                    {
                        item = ToGuid(cur as JValue);
                        if (!Guid.Empty.Equals(item))
                            ids.Add(item);
                    }
                    return ids;
                }
                catch
                { }
            }
            return null;
        }

        public static Guid ToGuid(JToken ob)
        {
            if (ob != null)
                return ToGuid(ob as JValue);
            return Guid.Empty;
        }

        public static Guid ToGuid(JValue ob)
        {
            if (ob != null)
            {
                Guid tmp = Guid.Empty;
                if (Guid.TryParse(ob.ToString(), out tmp))
                    return tmp;
            }
            return Guid.Empty;
        }

        public static JToken GetDataPayload(HttpRequest request)
        {
            return GetDataPayload(ParseBody(request));
        }

        public static JToken GetDataPayload(JToken requestBodyObject)
        {
            if (requestBodyObject != null)
            {
                return requestBodyObject["data"];
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

        public static JArray ToJson(IEnumerable<CompoundIdentity> ids)
        {
            if (ids != null)
            {
                JArray o = new JArray();
                foreach (CompoundIdentity cur in ids)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

        public static JObject ToJson(CompoundIdentity id)
        {
            if (id != null)
            {
                JObject o = new JObject();
                o.Add(Dsid, id.DataStoreIdentity);
                o.Add(Id, id.Identity);
                return o;
            }
            return null;
        }

        public static JObject ToJson(Dictionary<string, Guid> ids)
        {
            if (ids != null)
            {
                JObject o = new JObject();
                foreach (KeyValuePair<string, Guid> cur in ids)
                {
                    o.Add(cur.Key, ToJson(cur.Value));
                }
                return o;
            }
            return null;
        }

        public static JArray ToJson(IEnumerable<Guid> ids)
        {
            if (ids != null)
            {
                JArray o = new JArray();
                foreach (Guid cur in ids)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

        public static JValue ToJson(Guid id)
        {
            return new JValue(id);
        }

        public static JObject ToJson(ValueRange<DateTime> range)
        {
            if (range != null)
            {
                JObject o = new JObject();
                if (range.Min.Equals(DateTime.MinValue))
                    o.Add(Start, null);
                else
                    o.Add(Start, range.Min.ToString());

                if (range.Max.Equals(DateTime.MinValue) || range.Max.Equals(DateTime.MaxValue))  //never set or closed
                    o.Add(Finish, null);
                else
                    o.Add(Finish, range.Max.ToString());

                return o;
            }
            return null;
        }
    }
}
