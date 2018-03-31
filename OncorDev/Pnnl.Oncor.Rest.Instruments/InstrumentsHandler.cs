using Osrs.Net.Http;
using Osrs.Net.Http.Handlers;
using Osrs.Security;
using Osrs.Security.Sessions;
using Osrs.Threading;
using System;

namespace Pnnl.Oncor.Rest.Instruments
{
    public sealed class InstrumentsHandler : HttpHandlerBase, IServiceHandler
    {
        private const string Instrument = "/instrument/";
        private const string Type = "/type/";
        private const string Family = "/family/";
		private const string Archetype = "/archetype/";

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
                return "instruments";
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
                        if (meth.StartsWith(Instrument, StringComparison.OrdinalIgnoreCase))
                        {
                            InstrumentHandler.Handle(ctx, meth.Substring(Instrument.Length), context, cancel);
                            return;
                        }
                        if (meth.StartsWith(Type, StringComparison.OrdinalIgnoreCase))
                        {
                            InstrumentTypeHandler.Handle(ctx, meth.Substring(Type.Length), context, cancel);
                            return;
                        }
                        if (meth.StartsWith(Family, StringComparison.OrdinalIgnoreCase))
                        {
                            InstrumentFamilyHandler.Handle(ctx, meth.Substring(Family.Length), context, cancel);
                            return;
                        }
						if (meth.StartsWith(Archetype, StringComparison.OrdinalIgnoreCase))
						{
							InstrumentArchetypeHandler.Handle(ctx, meth.Substring(Archetype.Length), context, cancel);
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
