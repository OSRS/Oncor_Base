using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Net.Http;
using Osrs.Security;
using Osrs.Threading;
using Osrs.WellKnown.SensorsAndInstruments;
using Osrs.WellKnown.SensorsAndInstruments.Archetypes;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.Instruments
{
    internal static class InstrumentHandler
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

                    CompoundIdentity owner = null;
                    CompoundIdentity type = null;
                    string name = null;
                    string desc = null;
					string serial = null;
					CompoundIdentity manf = null;
                    InstrumentProviderBase provider = null;
					InstrumentKnownArchetypeProviderBase archProvider = null;
                    JToken token = null;

                    try
                    {
                        //payload and provider
                        token = JsonUtils.GetDataPayload(context.Request);
                        provider = InstrumentManager.Instance.GetInstrumentProvider(user);
						archProvider = InstrumentManager.Instance.GetInstrumentKnownArchetypeProvider(user);
                        if (provider != null && token != null)
                        {
                            //required inputs
                            name = token["name"].ToString();
                            owner = JsonUtils.ToId(token["orgid"]);
                            type = JsonUtils.ToId(token["typeid"]);
                            if (owner != null && type != null && !string.IsNullOrEmpty(name))
                            {
                                //optionals
                                desc = token["desc"] != null ? token["desc"].ToString() : null;
								serial = token["serial"] != null ? token["serial"].ToString() : null;
								manf = JsonUtils.ToId(token["manf"]);

                                //create
                                Instrument instrument = null;
                                instrument = provider.Create(owner, name, type, desc, serial, manf);
                                if (instrument != null)
                                {
									JObject jinstrument = null;

									//call update to persist architype info
                                    //archid and archdata are expected to be valid and consistent; non-matching properties are ignored
									if (token.SelectToken("archid") != null) //JSON property was present
									{
										CompoundIdentity archid = JsonUtils.ToId(token["archid"]);
										string archetypeString = archProvider.GetArchetypeType(archid);
										JToken archToken = token["archdata"];
										switch (archetypeString)
										{
											case "SimpleTrapDredge":
												SimpleTrapDredge std = archProvider.AddSimpleTrapDredge(instrument.Identity);
												if (archToken["openarea"] != null)
													std.OpenArea = double.Parse(archToken["openarea"].ToString());
                                                archProvider.Update(std);
												jinstrument = Jsonifier.ToJson(instrument, std);
												break;
											case "StandardMeshNet":
												StandardMeshNet smn = archProvider.AddStandardMeshNet(instrument.Identity);
												if (archToken["length"] != null)
													smn.Length = double.Parse(archToken["length"].ToString());
												if (archToken["depth"] != null)
													smn.Depth = double.Parse(archToken["depth"].ToString());
												if (archToken["meshsize"] != null)
													smn.MeshSize = double.Parse(archToken["meshsize"].ToString());
												archProvider.Update(smn);
												jinstrument = Jsonifier.ToJson(instrument, smn);
												break;
											case "StandardPlanktonNet":
												StandardPlanktonNet spn = archProvider.AddStandardPlanktonNet(instrument.Identity);
												if (archToken["openarea"] != null)
													spn.OpenArea = double.Parse(archToken["openarea"].ToString());
												if (archToken["meshsize"] != null)
													spn.MeshSize = double.Parse(archToken["meshsize"].ToString());
												if (archToken["codsize"] != null)
													spn.MeshSize = double.Parse(archToken["codsize"].ToString());
												archProvider.Update(spn);
												jinstrument = Jsonifier.ToJson(instrument, spn);
												break;
											case "WingedBagNet":
												WingedBagNet wbn = archProvider.AddWingedBagNet(instrument.Identity);
												if (archToken["length"] != null)
													wbn.Length = double.Parse(archToken["length"].ToString());
												if (archToken["depth"] != null)
													wbn.Depth = double.Parse(archToken["depth"].ToString());
												if (archToken["meshsizewings"] != null)
													wbn.Length = double.Parse(archToken["meshsizewings"].ToString());
												if (archToken["meshsizebag"] != null)
													wbn.Depth = double.Parse(archToken["meshsizebag"].ToString());
												archProvider.Update(wbn);
												jinstrument = Jsonifier.ToJson(instrument, wbn);
												break;
										}
									}
                                    else
                                    {
                                        jinstrument = Jsonifier.ToJson(instrument);
                                    }

                                    if (jinstrument != null)
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jinstrument.ToString()));
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
                        //provider and token
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        InstrumentProviderBase provider = InstrumentManager.Instance.GetInstrumentProvider(user);
						InstrumentKnownArchetypeProviderBase archProvider = InstrumentManager.Instance.GetInstrumentKnownArchetypeProvider(user);
                        if (provider != null && token != null)
                        {
                            //GUID must be provided
                            CompoundIdentity cid = JsonUtils.ToId(token["id"]);  //instrument ID

                            //fetch stored object
                            bool dirty = false;
                            bool result = true;
                            Instrument instrument = provider.Get(cid);
                            if (instrument != null)
                            {
                                //update archetype instance if necessary
                                //assumes inbound archtype is self-consistent and consistent with instrument type (validation should occur in provider and by requester)
                                if (token.SelectToken("archid") != null)
                                {
                                    //clear previous
                                    archProvider.Delete(instrument.Identity);

                                    //add new, if defined
                                    if (JsonUtils.ToId(token["archid"]) != null)
                                    {
                                        CompoundIdentity arch_id = JsonUtils.ToId(token["archid"]);
                                        JToken archToken = token["archdata"];
                                        string archetypeString = archProvider.GetArchetypeType(arch_id);
                                        switch (archetypeString)
                                        {
                                            case "SimpleTrapDredge":
                                                SimpleTrapDredge std = archProvider.AddSimpleTrapDredge(instrument.Identity);
                                                if (archToken["openarea"] != null)
                                                    std.OpenArea = double.Parse(archToken["openarea"].ToString());
                                                result &= archProvider.Update(std);
                                                break;
                                            case "StandardMeshNet":
                                                StandardMeshNet smn = archProvider.AddStandardMeshNet(instrument.Identity);
                                                if (archToken["length"] != null)
                                                    smn.Length = double.Parse(archToken["length"].ToString());
                                                if (archToken["depth"] != null)
                                                    smn.Depth = double.Parse(archToken["depth"].ToString());
                                                if (archToken["meshsize"] != null)
                                                    smn.MeshSize = double.Parse(archToken["meshsize"].ToString());
                                                result &= archProvider.Update(smn);
                                                break;
                                            case "StandardPlanktonNet":
                                                StandardPlanktonNet spn = archProvider.AddStandardPlanktonNet(instrument.Identity);
                                                if (archToken["openarea"] != null)
                                                    spn.OpenArea = double.Parse(archToken["openarea"].ToString());
                                                if (archToken["meshsize"] != null)
                                                    spn.MeshSize = double.Parse(archToken["meshsize"].ToString());
                                                if (archToken["codsize"] != null)
                                                    spn.CodSize = double.Parse(archToken["codsize"].ToString());
                                                result &= archProvider.Update(spn);
                                                break;
                                            case "WingedBagNet":
                                                WingedBagNet wbn = archProvider.AddWingedBagNet(instrument.Identity);
                                                if (archToken["length"] != null)
                                                    wbn.Length = double.Parse(archToken["length"].ToString());
                                                if (archToken["depth"] != null)
                                                    wbn.Depth = double.Parse(archToken["depth"].ToString());
                                                if (archToken["meshsizewings"] != null)
                                                    wbn.MeshSizeWings = double.Parse(archToken["meshsizewings"].ToString());
                                                if (archToken["meshsizebag"] != null)
                                                    wbn.MeshSizeBag = double.Parse(archToken["meshsizebag"].ToString());
                                                result &= archProvider.Update(wbn);
                                                break;
                                        }
                                    }
                                }

                                if (result)
                                {
                                    //continue to check instrument
                                    //## REQUIRED ##
                                    //name
                                    if (token.SelectToken("name") != null)
                                    {
                                        string name = token["name"].ToString();
                                        if (!string.IsNullOrEmpty(name))
                                        {
                                            instrument.Name = name;
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
                                        CompoundIdentity org_cid = JsonUtils.ToId(token["orgid"]);
                                        if (org_cid != null)
                                        {
                                            instrument.OwningOrganizationIdentity = org_cid;
                                            dirty = true;
                                        }
                                        else
                                        {
                                            RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed)); //owning org is required and not nullable
                                            return;
                                        }
                                    }

                                    //type
                                    if (token.SelectToken("typeid") != null)

                                    {
                                        CompoundIdentity type = JsonUtils.ToId(token["typeid"]);
                                        if (type != null)
                                        {
                                            instrument.InstrumentTypeIdentity = type;
                                            dirty = true;
                                        }
                                        else
                                        {
                                            RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed)); //intrument type is required and not nullable
                                            return;
                                        }
                                    }

                                    //## OPTIONALS ##

                                    //description
                                    if (token.SelectToken("desc") != null)
                                    {
                                        string desc = (token["desc"] != null) ? token["desc"].ToString() : null;
                                        instrument.Description = desc;
                                        dirty = true;
                                    }

                                    //serial number
                                    if (token.SelectToken("serial") != null)
                                    {
                                        string serial = token["serial"] != null ? token["serial"].ToString() : null;
                                        instrument.SerialNumber = serial;
                                        dirty = true;
                                    }

                                    //manufacturer
                                    if (token.SelectToken("manf") != null)

                                    {
                                        CompoundIdentity manf = JsonUtils.ToId(token["manf"]);
                                        instrument.ManufacturerId = manf;
                                        dirty = true;
                                    }

                                    //update instrument if necessary
                                    if (dirty)
                                    {
                                        //update
                                        result &= provider.Update(instrument);
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

                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));  //default
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
                        InstrumentProviderBase provider = InstrumentManager.Instance.GetInstrumentProvider(user);
						InstrumentKnownArchetypeProviderBase archProvider = InstrumentManager.Instance.GetInstrumentKnownArchetypeProvider(user);
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

                InstrumentProviderBase provider = InstrumentManager.Instance.GetInstrumentProvider(user);
                InstrumentKnownArchetypeProviderBase archProvider = InstrumentManager.Instance.GetInstrumentKnownArchetypeProvider(user);
                if (provider != null && archProvider != null)
                {
                    IEnumerable<Instrument> instruments = provider.Get(ids);
                    JArray jinstruments = new JArray();
                    foreach (Instrument inst in instruments)
                    {
                        IArchetype arch = archProvider.Get(inst.Identity);
                        if (arch != null)
                        {
                            string archetypeString = archProvider.GetArchetypeType(arch.Identity);
                            switch (archetypeString)
                            {
                                case "SimpleTrapDredge":
                                    jinstruments.Add(Jsonifier.ToJson(inst, arch as SimpleTrapDredge));
                                    break;
                                case "StandardMeshNet":
                                    jinstruments.Add(Jsonifier.ToJson(inst, arch as StandardMeshNet));
                                    break;
                                case "StandardPlanktonNet":
                                    jinstruments.Add(Jsonifier.ToJson(inst, arch as StandardPlanktonNet));
                                    break;
                                case "WingedBagNet":
                                    jinstruments.Add(Jsonifier.ToJson(inst, arch as WingedBagNet));
                                    break;
                            }
                        }
                        else  //instrument has no archetype
                        {
                            jinstruments.Add(Jsonifier.ToJson(inst));
                        }
                    }

                    if (jinstruments != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jinstruments.ToString());
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
                InstrumentProviderBase provider = InstrumentManager.Instance.GetInstrumentProvider(user);
				InstrumentKnownArchetypeProviderBase archProvider = InstrumentManager.Instance.GetInstrumentKnownArchetypeProvider(user);
                if (provider != null)
                {
                    IEnumerable<Instrument> instruments = provider.Get();
                    JArray jinstruments = new JArray();
					foreach (Instrument inst in instruments)
					{
						IArchetype arch = archProvider.Get(inst.Identity);
						if (arch != null)
						{
							string archetypeString = archProvider.GetArchetypeType(arch.Identity);
							switch (archetypeString)
							{
								case "SimpleTrapDredge":
									jinstruments.Add(Jsonifier.ToJson(inst, arch as SimpleTrapDredge));
									break;
								case "StandardMeshNet":
									jinstruments.Add(Jsonifier.ToJson(inst, arch as StandardMeshNet));
									break;
								case "StandardPlanktonNet":
									jinstruments.Add(Jsonifier.ToJson(inst, arch as StandardPlanktonNet));
									break;
								case "WingedBagNet":
									jinstruments.Add(Jsonifier.ToJson(inst, arch as WingedBagNet));
									break;
							}
						}
                        else  //instrument has no archetype
                        {
                            jinstruments.Add(Jsonifier.ToJson(inst));
                        }
					}

                    if (jinstruments != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jinstruments.ToString());
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
