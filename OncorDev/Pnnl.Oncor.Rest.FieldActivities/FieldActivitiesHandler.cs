using Osrs.Net.Http;
using Osrs.Net.Http.Handlers;
using Osrs.Security;
using Osrs.Security.Sessions;
using Osrs.Threading;
using System;

namespace Pnnl.Oncor.Rest.FieldActivities
{
    public sealed class FieldActivitiesHandler : HttpHandlerBase, IServiceHandler
    {
        internal const string Activities = "/activities/";
        internal const string Trips = "/trips/";
        internal const string Teams = "/teams/";
        internal const string Roles = "roles/";
        internal const string Samples = "/samples/";

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
                return "fieldactivities";
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
                        if (meth.StartsWith(Activities, StringComparison.OrdinalIgnoreCase))
                        {
                            FieldActivityHandler.Handle(ctx, meth.Substring(Activities.Length), context, cancel);
                            return;
                        }
                        if (meth.StartsWith(Teams, StringComparison.OrdinalIgnoreCase))
                        {
                            FieldTeamHandler.Handle(ctx, meth.Substring(Teams.Length), context, cancel);
                            return;
                        }
                        if (meth.StartsWith(Trips, StringComparison.OrdinalIgnoreCase))
                        {
                            FieldTripHandler.Handle(ctx, meth.Substring(Trips.Length), context, cancel);
                            return;
                        }
                        if (meth.StartsWith(Samples, StringComparison.OrdinalIgnoreCase))
                        {
                            SampleEventHandler.Handle(ctx, meth.Substring(Samples.Length), context, cancel);
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
