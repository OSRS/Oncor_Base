using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Net.Http;
using Osrs.Security;
using Osrs.Threading;
using Osrs.WellKnown.Taxonomy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pnnl.Oncor.Rest.Taxa
{
	internal static class TaxaUnitHandler
	{
		public static void Handle(UserSecurityContext user, string method, HttpContext context, CancellationToken cancel)
		{
			if (context.Request.Method == "POST")
			{
				if (method.Equals("find", StringComparison.OrdinalIgnoreCase))
				{
                    try
                    {
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        if (token != null)
                        {
                            if (token["unittypeid"] != null)
                            {
                                GetByTaxaUnitType(JsonUtils.ToId(token["unittypeid"]), user, context, cancel);
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
                else if (method.Equals("nonLiving", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        GetNonLivingTaxa(user, context, cancel);
                        return;
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                        return;
                    }
                }
                else if (method.Equals("children", StringComparison.OrdinalIgnoreCase))
				{
					try
					{
						TaxaUnitProviderBase provider = TaxonomyManager.Instance.GetTaxaUnitProvider(user);
						CompoundIdentity id = JsonUtils.ToId(JsonUtils.GetDataPayload(context.Request));
						JArray junits = null;

						if (provider != null && id != null)
						{
							IEnumerable<TaxaUnit> units = provider.GetChildren(id);
							junits = Jsonifier.ToJson(units);

							if (junits != null)
								RestUtils.Push(context.Response, JsonOpStatus.Ok, junits.ToString());
							else
								RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
							return;
						}
						RestUtils.Push(context.Response, JsonOpStatus.Failed);
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
						TaxaUnitProviderBase provider = TaxonomyManager.Instance.GetTaxaUnitProvider(user);
						IEnumerable<CompoundIdentity> ids = JsonUtils.ToIds(JsonUtils.GetDataPayload(context.Request));
						JArray junits = null;

						if (provider != null && ids != null)
						{
							IEnumerable<TaxaUnit> units = provider.Get(ids);
							junits = Jsonifier.ToJson(units);

							if (junits != null)
								RestUtils.Push(context.Response, JsonOpStatus.Ok, junits.ToString());
							else
								RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
							return;
						}
						RestUtils.Push(context.Response, JsonOpStatus.Failed);
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

        private static void GetByTaxaUnitType(CompoundIdentity taxaunit_cid, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                TaxaUnitProviderBase provider = TaxonomyManager.Instance.GetTaxaUnitProvider(user);
                if (provider != null)
                {
                    IEnumerable<TaxaUnit> units = provider.GetByTaxaUnitTypeId(taxaunit_cid);
                    JArray junits = Jsonifier.ToJson(units);
                    if (junits != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, junits.ToString());
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
        private static void GetNonLivingTaxa(UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            //This is a workaround until lists of observables can be abstracted out of taxonomy
            try
            {
                Guid ds = Guid.Empty;
                Guid id = Guid.Empty;
                Guid.TryParse("e578ca70-6cec-4961-bb43-14fd45f455bd", out ds);
                Guid.TryParse("237f8c0a-dc5f-4104-a0b8-dbb5a7c73aa2", out id);
                CompoundIdentity nonLivingTaxonomy = new CompoundIdentity(ds, id);

                TaxaUnitProviderBase provider = TaxonomyManager.Instance.GetTaxaUnitProvider(user);
                if (provider != null)
                {
                    IEnumerable<TaxaUnit> units = provider.GetByTaxonomy(nonLivingTaxonomy);
                    JArray junits = Jsonifier.ToJson(units);
                    if (junits != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, junits.ToString());
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
