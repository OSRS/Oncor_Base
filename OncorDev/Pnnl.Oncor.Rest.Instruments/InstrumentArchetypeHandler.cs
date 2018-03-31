using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Net.Http;
using Osrs.Security;
using Osrs.Threading;
using Osrs.WellKnown.SensorsAndInstruments;
using Osrs.WellKnown.SensorsAndInstruments.Archetypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pnnl.Oncor.Rest.Instruments
{
	internal static class InstrumentArchetypeHandler
	{
		public static void Handle(UserSecurityContext user, string method, HttpContext context, CancellationToken cancel)
		{
			if (context.Request.Method == "POST")
			{
				if (method.Equals("all", StringComparison.OrdinalIgnoreCase))
				{
					//get known archetypes
				}
				else if (method.Equals("create", StringComparison.OrdinalIgnoreCase))
				{
                    //create known archetype
				}
				else if (method.Equals("update", StringComparison.OrdinalIgnoreCase))
				{
                    //update known archetype
				}
				else if (method.Equals("delete", StringComparison.OrdinalIgnoreCase))
				{
                    //delete known archetype
				}
                else if (method.Equals("types", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        
                        if (token == null)
                        {
                            GetForAllTypes(user, context, cancel);
                            return;
                        }
                        else if(token["typeid"] != null)
                        {
                            CompoundIdentity cid = JsonUtils.ToId(token["typeid"]);
                            GetByType(cid, user, context, cancel);
                            return;
                        }
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                        return;
                    }
                }
                else if (method.Equals("instances", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        CompoundIdentity id = JsonUtils.ToId(token["id"]);
                        bool remove = token["remove"] != null ? (bool) token["remove"] : false;
                        
                        if (remove)
                        {
                            RemoveInstance(id, user, context, cancel);
                            return;
                        }
                        else
                        {
                            CompoundIdentity archid = JsonUtils.ToId(token["archid"]);
                            JToken archToken = token["archdata"];
                            if (id != null && archid != null && archToken != null)
                            {
                                AddInstance(id, archid, archToken, user, context, cancel);
                                return;
                            }

                            RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                        }

                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                        return;
                    }
                }
                
                context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
			}
		}

        private static void RemoveInstance(CompoundIdentity cid, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                InstrumentKnownArchetypeProviderBase archProvider = InstrumentManager.Instance.GetInstrumentKnownArchetypeProvider(user);
                if (cid != null && archProvider != null)
                {
                    bool result = archProvider.Delete(cid);

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

        private static void AddInstance(CompoundIdentity cid, CompoundIdentity archid, JToken archToken, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                InstrumentKnownArchetypeProviderBase archProvider = InstrumentManager.Instance.GetInstrumentKnownArchetypeProvider(user);
                if (archProvider != null)
                {
                    bool result = true;
                    string archetypeString = archProvider.GetArchetypeType(archid);
                    switch (archetypeString)
                    {
                        case "SimpleTrapDredge":
                            SimpleTrapDredge std = archProvider.AddSimpleTrapDredge(cid);
                            if (archToken["openarea"] != null)
                                std.OpenArea = double.Parse(archToken["openarea"].ToString());
                            result &= archProvider.Update(std);
                            break;
                        case "StandardMeshNet":
                            StandardMeshNet smn = archProvider.AddStandardMeshNet(cid);
                            if (archToken["length"] != null)
                                smn.Length = double.Parse(archToken["length"].ToString());
                            if (archToken["depth"] != null)
                                smn.Depth = double.Parse(archToken["depth"].ToString());
                            if (archToken["meshsize"] != null)
                                smn.MeshSize = double.Parse(archToken["meshsize"].ToString());
                            result &= archProvider.Update(smn);
                            break;
                        case "StandardPlanktonNet":
                            StandardPlanktonNet spn = archProvider.AddStandardPlanktonNet(cid);
                            if (archToken["openarea"] != null)
                                spn.OpenArea = double.Parse(archToken["openarea"].ToString());
                            if (archToken["meshsize"] != null)
                                spn.MeshSize = double.Parse(archToken["meshsize"].ToString());
                            if (archToken["codsize"] != null)
                                spn.CodSize = double.Parse(archToken["codsize"].ToString());
                            result &= archProvider.Update(spn);
                            break;
                        case "WingedBagNet":
                            WingedBagNet wbn = archProvider.AddWingedBagNet(cid);
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

                    if (result == true)
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok));
                        return;
                    }
                }

                RestUtils.Push(context.Response, JsonOpStatus.Failed);
            }
            catch
            {
                RestUtils.Push(context.Response, JsonOpStatus.Failed);
                return;
            }
        }

        private static void GetForAllTypes(UserSecurityContext user, HttpContext context, CancellationToken cancel)
		{
			try
			{
				InstrumentKnownArchetypeProviderBase provider = InstrumentManager.Instance.GetInstrumentKnownArchetypeProvider(user);
				if (provider != null)
				{
					IEnumerable<Tuple<CompoundIdentity, CompoundIdentity>> archetypes = provider.GetInstrumentTypeKnownArchetypes();
					JArray jarchetypes = Jsonifier.ToJson(archetypes);

					if (jarchetypes != null)
						RestUtils.Push(context.Response, JsonOpStatus.Ok, jarchetypes.ToString());
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

        private static void GetByType(CompoundIdentity typeid, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                InstrumentKnownArchetypeProviderBase provider = InstrumentManager.Instance.GetInstrumentKnownArchetypeProvider(user);
                if (provider != null)
                {
                    IEnumerable<CompoundIdentity> archetypes_ids = provider.ArchetypesForInstrumentType(typeid);
                    if (archetypes_ids != null  && archetypes_ids.Any())
                    {
                        JArray jarchetypes_ids = JsonUtils.ToJson(archetypes_ids);
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jarchetypes_ids.ToString());
                    }
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
