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
	internal static class TaxonomyHandler
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
				TaxonomyProviderBase provider = TaxonomyManager.Instance.GetTaxonomyProvider(user);
				if (provider != null)
				{
					IEnumerable<Taxonomy> taxa =  provider.Get();
					JArray jtaxonomies = jtaxonomies = Jsonifier.ToJson(taxa);

					if (jtaxonomies != null)
						RestUtils.Push(context.Response, JsonOpStatus.Ok, jtaxonomies.ToString());
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
