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
	internal static class TaxaUnitTypeHandler
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
						TaxaDomainUnitTypeProviderBase dutprovider = TaxonomyManager.Instance.GetTaxaDomainUnitTypeProvider(user);
						TaxaDomainProviderBase domprovider = TaxonomyManager.Instance.GetTaxaDomainProvider(user);
						JToken token = JsonUtils.GetDataPayload(context.Request);
						JArray junittypes = null;
						
						if (dutprovider != null && domprovider != null && token != null)
						{
							CompoundIdentity domainId = JsonUtils.ToId(token["domainid"]);
							TaxaDomain domain = domprovider.Get(domainId);

							if (domain != null)
							{
								IEnumerable<TaxaUnitType> units = dutprovider.GetTaxaUnitTypeByDomain(domain);
								junittypes = Jsonifier.ToJson(units);
							}

							if (junittypes != null)
								RestUtils.Push(context.Response, JsonOpStatus.Ok, junittypes.ToString());
							else
								RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
							return;
						}
						RestUtils.Push(context.Response, JsonOpStatus.Failed);
						return;
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

        private static void Get(UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                TaxaUnitTypeProviderBase provider = TaxonomyManager.Instance.GetTaxaUnitTypeProvider(user);
                if (provider != null)
                {
                    IEnumerable<TaxaUnitType> unitTypes = provider.Get();
                    JArray junittypes = Jsonifier.ToJson(unitTypes);
                    if (junittypes != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, junittypes.ToString());
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
