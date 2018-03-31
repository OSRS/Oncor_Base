using Newtonsoft.Json.Linq;
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
	internal static class TaxaDomainHandler
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
			}
			context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
		}

		private static void Get(UserSecurityContext user, HttpContext context, CancellationToken cancel)
		{
			try
			{
				TaxaDomainProviderBase provider = TaxonomyManager.Instance.GetTaxaDomainProvider(user);
				if (provider != null)
				{
					IEnumerable<TaxaDomain> domains = provider.Get();
					JArray jdomains = Jsonifier.ToJson(domains);

					if (jdomains != null)
						RestUtils.Push(context.Response, JsonOpStatus.Ok, jdomains.ToString());
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
