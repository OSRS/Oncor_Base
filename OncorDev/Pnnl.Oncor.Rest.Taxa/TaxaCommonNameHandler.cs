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
	internal static class TaxaCommonNameHandler
	{
		public static void Handle(UserSecurityContext user, string method, HttpContext context, CancellationToken cancel)
		{
			if (context.Request.Method == "POST")
			{
				if (method.Equals("find", StringComparison.OrdinalIgnoreCase))
				{
					try
					{
						TaxaCommonNameProviderBase cnprovider = TaxonomyManager.Instance.GetTaxaCommonNameProvider(user);
						TaxaUnitProviderBase uprovider = TaxonomyManager.Instance.GetTaxaUnitProvider(user);
						JToken token = JsonUtils.GetDataPayload(context.Request);
						JArray jcommonNames = null;

						if (cnprovider != null && uprovider != null && token != null)
						{
							CompoundIdentity unitId = JsonUtils.ToId(token["unitid"]);
							TaxaUnit unit = uprovider.Get(unitId);
							if (unit != null)
							{
								IEnumerable<TaxaCommonName> names = cnprovider.GetCommonNamesByTaxa(unit);
								jcommonNames = Jsonifier.ToJson(names);
							}

							if (jcommonNames != null)
								RestUtils.Push(context.Response, JsonOpStatus.Ok, jcommonNames.ToString());
							else
								RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
							return;
						}
					}
					catch
					{
						RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
						return;
					}
					RestUtils.Push(context.Response, JsonOpStatus.Failed);
					return;
				}
			}
			context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
		}
	}
}
