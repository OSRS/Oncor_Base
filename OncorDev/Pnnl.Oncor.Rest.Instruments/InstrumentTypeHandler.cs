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
    internal static class InstrumentTypeHandler
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
                    CompoundIdentity family_id = null;
                    CompoundIdentity parent_id = null;
                    HashSet<CompoundIdentity> archids = null;
                    InstrumentTypeProviderBase provider = null;
                    JToken token = null;

                    try
                    {
                        //payload and provider
                        token = JsonUtils.GetDataPayload(context.Request);
                        provider = InstrumentManager.Instance.GetInstrumentTypeProvider(user);
                        if (provider != null && token != null)
                        {
                            //required inputs
                            name = token["name"].ToString();
                            family_id = JsonUtils.ToId(token["familyid"]);
                            if (family_id != null && !string.IsNullOrEmpty(name))
                            {
                                //optionals
                                desc = token["desc"] != null ? token["desc"].ToString() : null;
                                parent_id = token["parentid"] !=null ? JsonUtils.ToId(token["parentid"]) : null;
                                archids = token["archid"] != null ? JsonUtils.ToIds(token["archid"]) : null;

                                //create
                                InstrumentType instrumentType = null;
                                instrumentType = provider.Create(name, family_id, desc, parent_id);
                                if (instrumentType != null)
                                {
                                    bool result = true;
                                    //add archetype mappings if required
                                    InstrumentKnownArchetypeProviderBase apb = InstrumentManager.Instance.GetInstrumentKnownArchetypeProvider(user);
                                    if (token.SelectToken("archid") != null && apb != null)  //user intended to set this value
                                    {
                                        foreach (CompoundIdentity c in archids)
                                        {
                                            result &= apb.AddInstrumentTypeArchetype(c, instrumentType.Identity);
                                        }
                                    }

                                    if (result == true)
                                    {
                                        JObject jinstrumentType = Jsonifier.ToJson(instrumentType);
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jinstrumentType.ToString()));
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
                else if (method.Equals("update", StringComparison.OrdinalIgnoreCase))
                {
                    string name;
                    string desc = null;
                    JToken token = null;
                    InstrumentType instrumentType = null;
                    CompoundIdentity cid = null;
                    CompoundIdentity family_cid = null;
                    CompoundIdentity parent_cid = null;
                    HashSet<CompoundIdentity> arch_cids = null;
                    InstrumentTypeProviderBase provider = null;

                    try
                    {
                        token = JsonUtils.GetDataPayload(context.Request);
                        provider = InstrumentManager.Instance.GetInstrumentTypeProvider(user);
                        if (provider != null && token != null)
                        {
                            //GUID must be provided
                            cid = JsonUtils.ToId(token["id"]);

                            //fetch stored object
                            bool dirty = false;
                            bool result = true;
                            instrumentType = provider.Get(cid);
                            if (instrumentType != null)
                            {
                                //archetype mappings - convenience method, so don't need to set dirty flag if nothing else has changed
                                if (token.SelectToken("archid") != null)
                                {
                                    InstrumentKnownArchetypeProviderBase apb = InstrumentManager.Instance.GetInstrumentKnownArchetypeProvider(user);
                                    arch_cids = token["archid"] != null ? JsonUtils.ToIds(token["archid"]) : null;

                                    //reset mappings
                                    result &= apb.RemoveInstrumentType(instrumentType.Identity);

                                    //add new mappings
                                    if (arch_cids != null)
                                    {
                                        foreach (CompoundIdentity c in arch_cids)
                                            result &= apb.AddInstrumentTypeArchetype(c, instrumentType.Identity);
                                    }
                                }

                                if (result)
                                {
                                    //## REQUIRED ##

                                    //name
                                    if (token.SelectToken("name") != null)
                                    {
                                        name = token["name"].ToString();
                                        if (!string.IsNullOrEmpty(name))
                                        {
                                            instrumentType.Name = name;
                                            dirty = true;
                                        }
                                        else
                                        {
                                            RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed)); //name is required and not nullable
                                            return;
                                        }
                                    }

                                    //family
                                    if (token.SelectToken("familyid") != null)
                                    {
                                        family_cid = JsonUtils.ToId(token["familyid"]);
                                        if (family_cid != null)
                                        {
                                            instrumentType.FamilyId = family_cid;
                                            dirty = true;
                                        }
                                        else
                                        {
                                            RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed)); //family id is required and not nullable
                                            return;
                                        }
                                    }

                                    //## OPTIONALS ##

                                    //description
                                    if (token.SelectToken("desc") != null)
                                    {
                                        desc = (token["desc"] != null) ? token["desc"].ToString() : null;
                                        instrumentType.Description = desc;
                                        dirty = true;
                                    }

                                    //parent type
                                    if (token.SelectToken("parentid") != null)
                                    {
                                        parent_cid = JsonUtils.ToId(token["parentid"]);
                                        instrumentType.ParentId = parent_cid;  //could be null
                                        dirty = true;
                                    }

                                    if (dirty)
                                    {
                                        //update
                                        result &= provider.Update(instrumentType);
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
                        InstrumentTypeProviderBase provider = InstrumentManager.Instance.GetInstrumentTypeProvider(user);
                        InstrumentKnownArchetypeProviderBase archProvider = InstrumentManager.Instance.GetInstrumentKnownArchetypeProvider(user);
                        if (provider != null && cids != null && archProvider != null)
                        {
                            bool result = true;
                            foreach (CompoundIdentity cid in cids)
                            {
                                InstrumentType itype = provider.Get(cid);

                                result &= provider.Delete(cid);
                                result &= archProvider.RemoveInstrumentType(itype.Identity);  //delete related archetype mappings
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

                InstrumentTypeProviderBase provider = InstrumentManager.Instance.GetInstrumentTypeProvider(user);
                if (provider != null)
                {
                    JArray jinstrumentTypes = Jsonifier.ToJson(provider.Get(ids));
                    if (jinstrumentTypes != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jinstrumentTypes.ToString()));
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
                InstrumentTypeProviderBase provider = InstrumentManager.Instance.GetInstrumentTypeProvider(user);
                if (provider != null)
                {
                    IEnumerable<InstrumentType> instrumentTypes = provider.Get();
                    JArray jinstrumentTypes = Jsonifier.ToJson(instrumentTypes);
                    if (jinstrumentTypes != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jinstrumentTypes.ToString());
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
