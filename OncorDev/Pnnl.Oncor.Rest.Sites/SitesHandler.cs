using Osrs.Net.Http;
using Osrs.Net.Http.Handlers;
using Osrs.Security;
using Osrs.Security.Sessions;
using Osrs.Threading;
using System;

namespace Pnnl.Oncor.Rest.Sites
{
    public sealed class SitesHandler : HttpHandlerBase, IServiceHandler
    {
        private const string Sites = "/sites/";
        private const string Alias = "/aliases/";
        private const string AliasScheme = "/schemes/";

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
                return "sites";
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
                        if (meth.StartsWith(Sites, StringComparison.OrdinalIgnoreCase))
                        {
                            SiteHandler.Handle(ctx, meth.Substring(Sites.Length), context, cancel);
                            return;
                        }
                        if (meth.StartsWith(Alias, StringComparison.OrdinalIgnoreCase))
                        {
                            AliasHandler.Handle(ctx, meth.Substring(Alias.Length), context, cancel);
                            return;
                        }
                        if (meth.StartsWith(AliasScheme, StringComparison.OrdinalIgnoreCase))
                        {
                            SchemeHandler.Handle(ctx, meth.Substring(AliasScheme.Length), context, cancel);
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
