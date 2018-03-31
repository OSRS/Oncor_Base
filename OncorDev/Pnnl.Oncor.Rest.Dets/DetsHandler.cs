using Newtonsoft.Json.Linq;
using Osrs.Net.Http;
using Osrs.Net.Http.Handlers;
using Osrs.Security;
using Osrs.Security.Sessions;
using Osrs.Threading;
using System;

namespace Pnnl.Oncor.Rest.Dets
{
    //This is a simple abstraction to allow create/update/get/delete of DET specific (currently only Excel) files for data interchange.
    //If the model changes to how it "should be" (no dets), this entire part would go away.
    //As such, this is abstracted to be a single model of interaction to exchange files, with a binding to process those files on the server once transferred
    public sealed class DetsHandler : HttpHandlerBase, IServiceHandler
    {
        internal const string Create = "/create";
        internal const string Get = "/fetch";
        internal const string Update = "/upload";
        internal const string Delete = "/delete";

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
                return "dets";
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
                        JToken dat = JsonUtils.GetDataPayload(context.Request);

                        if (meth.StartsWith(Create, StringComparison.OrdinalIgnoreCase))
                        {
                            DetGeneralHandler.Create(ctx, dat, context, cancel);
                            return;
                        }
                        if (meth.StartsWith(Update, StringComparison.OrdinalIgnoreCase))
                        {
                            DetGeneralHandler.Update(ctx, dat, context, cancel);
                            return;
                        }
                        if (meth.StartsWith(Get, StringComparison.OrdinalIgnoreCase))
                        {
                            DetGeneralHandler.Get(ctx, dat, context, cancel);
                            return;
                        }
                        if (meth.StartsWith(Delete, StringComparison.OrdinalIgnoreCase))
                        {
                            DetGeneralHandler.Delete(ctx, dat, context, cancel);
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
