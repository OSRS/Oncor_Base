using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Net.Http;
using Osrs.Security;
using Osrs.Threading;
using Osrs.WellKnown.SensorsAndInstruments;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.Instruments
{
    internal static class InstrumentFamilyHandler
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
                    string name = null;
                    string desc = null;
                    InstrumentFamilyProviderBase provider = null;
                    JToken token = null;

                    try
                    {
                        //payload and provider
                        token = JsonUtils.GetDataPayload(context.Request);
                        provider = InstrumentManager.Instance.GetInstrumentFamilyProvider(user);
                        if (provider != null && token != null)
                        {
                            //required inputs
                            name = token["name"].ToString();
                            if (!string.IsNullOrEmpty(name))
                            {
                                //optionals
                                desc = token["desc"] != null ? token["desc"].ToString() : null;
                                CompoundIdentity parent_id = token["parentid"] != null ? JsonUtils.ToId(token["parentid"]) : null;

                                //create
                                InstrumentFamily instrumentFamily = null;
                                instrumentFamily = provider.Create(name, desc, parent_id);
                                if (instrumentFamily != null)
                                {
                                    JObject jinstrumentFamily = Jsonifier.ToJson(instrumentFamily);
                                    if (jinstrumentFamily != null)
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jinstrumentFamily.ToString()));
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
                    string name;
                    string desc = null;
                    JToken token = null;
                    InstrumentFamily instrumentFamily = null;
                    CompoundIdentity cid = null;
                    CompoundIdentity parent_cid = null;
                    InstrumentFamilyProviderBase provider = null;

                    try
                    {
                        token = JsonUtils.GetDataPayload(context.Request);
                        provider = InstrumentManager.Instance.GetInstrumentFamilyProvider(user);
                        if (provider != null && token != null)
                        {
                            //GUID must be provided
                            cid = JsonUtils.ToId(token["id"]);

                            //fetch stored object
                            bool dirty = false;
                            instrumentFamily = provider.Get(cid);
                            if (instrumentFamily != null)
                            {

                                //## REQUIRED ##

                                //name
                                if (token.SelectToken("name") != null)
                                {
                                    name = token["name"].ToString();
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        instrumentFamily.Name = name;
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
                                    desc = (token["desc"] != null) ? token["desc"].ToString() : null;
                                    instrumentFamily.Description = desc;
                                    dirty = true;
                                }

                                //parent family
                                if (token.SelectToken("parentid") != null)
                                {
                                    parent_cid = JsonUtils.ToId(token["parentid"]);
                                    instrumentFamily.ParentId = parent_cid;  //could be null
                                    dirty = true;
                                }

                                if (dirty)
                                {
                                    //update
                                    bool result = provider.Update(instrumentFamily);
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
                        InstrumentFamilyProviderBase provider = InstrumentManager.Instance.GetInstrumentFamilyProvider(user);
                        if (provider != null && cids != null)
                        {
                            bool result = true;
                            foreach (CompoundIdentity cid in cids)
                            {
                                result &= provider.Delete(cid);
                            }

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
            }

            context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
        }

        private static void GetIds(HashSet<CompoundIdentity> ids, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                if (ids.Count == 0)
                {
                    RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                    return;
                }

                InstrumentFamilyProviderBase provider = InstrumentManager.Instance.GetInstrumentFamilyProvider(user);
                if (provider != null)
                {
                    JArray jinstrumentFamilies = Jsonifier.ToJson(provider.Get(ids));
                    if (jinstrumentFamilies != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jinstrumentFamilies.ToString()));
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
                InstrumentFamilyProviderBase provider = InstrumentManager.Instance.GetInstrumentFamilyProvider(user);
                if (provider != null)
                {
                    IEnumerable<InstrumentFamily> instrumentFams = provider.Get();
                    JArray jinstrumentFams = Jsonifier.ToJson(instrumentFams);
                    if (jinstrumentFams != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jinstrumentFams.ToString());
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
