using Osrs.Net.Http;
using Osrs.Net.Http.Handlers;
using Osrs.Security;
using Osrs.Security.Sessions;
using Osrs.Threading;
using System;

namespace Pnnl.Oncor.Rest.Taxa
{
    public sealed class TaxaHandler : HttpHandlerBase, IServiceHandler
    {
		private const string Taxonomy = "/taxonomy/";
		private const string Domain = "/domain/";
		private const string UnitType = "/unittype/";
		private const string Unit = "/unit/";
		private const string CommonName = "/commonname/";

        private SessionProviderBase sessionProvider;
        private SessionProviderBase SessionProvider
        {
            get
            {
                if (sessionProvider == null)
                    sessionProvider = SessionManager.Instance.GetProvider();
                return sessionProvider;
            }
        }

        public string BaseUrl
        {
            get
            {
                return "taxonomies";
            }
        }

        public override void Handle(HttpContext context, CancellationToken cancel)
        {
            if (context != null)
            {
                UserIdentityBase user = Security.Session.GetUser(context);
                if (user != null)
                {
                    UserSecurityContext ctx = new UserSecurityContext(user);
                    string localUrl = RestUtils.LocalUrl(this, context.Request);
                    string meth = RestUtils.StripLocal(this.BaseUrl, localUrl);

                    if (!string.IsNullOrEmpty(meth))
                    {
						if (meth.StartsWith(Taxonomy, StringComparison.OrdinalIgnoreCase))
						{
							TaxonomyHandler.Handle(ctx, meth.Substring(Taxonomy.Length), context, cancel);
							return;
						}
						if (meth.StartsWith(Domain, StringComparison.OrdinalIgnoreCase))
						{
							TaxaDomainHandler.Handle(ctx, meth.Substring(Domain.Length), context, cancel);
							return;
						}
						if (meth.StartsWith(UnitType, StringComparison.OrdinalIgnoreCase))
						{
							TaxaUnitTypeHandler.Handle(ctx, meth.Substring(UnitType.Length), context, cancel);
							return;
						}
						if (meth.StartsWith(Unit, StringComparison.OrdinalIgnoreCase))
						{
							TaxaUnitHandler.Handle(ctx, meth.Substring(Unit.Length), context, cancel);
							return;
						}
						if (meth.StartsWith(CommonName, StringComparison.OrdinalIgnoreCase))
						{
							TaxaCommonNameHandler.Handle(ctx, meth.Substring(CommonName.Length), context, cancel);
							return;
						}
					}
                }
                else
                {
                    context.Response.StatusCode = HttpStatusCodes.Status401Unauthorized;
                    return;
                }
            }
            context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
        }
    }
}
