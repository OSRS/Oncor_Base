using Osrs.Net.Http.Handlers;
using System;
using Osrs.Net.Http;
using Osrs.Threading;
using Pnnl.Oncor.Rest.Security;
using Osrs.Security.Sessions;
using Osrs.Security;

namespace Pnnl.Oncor.Rest.Organizations
{
    public sealed class OrganizationHandler : HttpHandlerBase, IServiceHandler
    {
        private const string Orgs = "/orgs/";
        private const string Alias = "/aliases/";
        private const string AliasScheme = "/schemes/";
        private const string Hierarchy = "/hierarchy/";

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
                return "organizations";
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
                        if (meth.StartsWith(Orgs, StringComparison.OrdinalIgnoreCase))
                        {
                            OrgHandler.Handle(ctx, meth.Substring(Orgs.Length), context, cancel);
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
                        if (meth.StartsWith(Hierarchy, StringComparison.OrdinalIgnoreCase))
                        {
                            HierarchyHandler.Handle(ctx, meth.Substring(Hierarchy.Length), context, cancel);
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
