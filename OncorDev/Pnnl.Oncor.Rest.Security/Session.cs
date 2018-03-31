using Osrs.Net.Http;
using System;
using Osrs.Threading;
using Osrs.Net.Http.Handlers;
using Osrs.Runtime;
using Osrs.Security.Sessions;
using Osrs.Security.Identity;
using Osrs.Security;

namespace Pnnl.Oncor.Rest.Security
{
    /// <summary>
    /// Interceptor handler
    /// auto extends the session id
    /// forwards the session id to the response on all requests
    /// </summary>
    public class SessionIdHeader : HttpHandlerBase, IPassThroughHandler
    {
        SessionProviderBase prov = SessionManager.Instance.GetProvider();

        public IHttpHandler Next
        {
            get;
            set;
        }

        public override void Handle(HttpContext context, CancellationToken cancel)
        {
            if (context != null)
            {
                HttpRequest request = context.Request;
                HttpResponse response = context.Response;

                if (request.Headers.ContainsKey(Session.SessionIdName))
                {
                    Guid ssid;
                    if (Guid.TryParse(request.Headers[Session.SessionIdName], out ssid))
                    {
                        if (prov.Exists(ssid) && prov.Extend(ssid)) //note that this will autoextend and will return false if expired
                        {
                            response.Headers[Session.SessionIdName] = request.Headers[Session.SessionIdName];
                        }
                    }
                }
                if (this.Next!=null)
                    this.Next.Handle(context, cancel);
            }
        }
    }

    /// <summary>
    /// Handles session create/extend/expire explicit requests
    /// </summary>
    public class Session : HttpHandlerBase, IServiceHandler
    {
        private static readonly UserSecurityContext ctx = new UserSecurityContext(new LocalSystemUser(SecurityUtils.AdminIdentity, "Admin", UserState.Active)); //TODO -- change this to a system-level account
        internal static SessionProviderBase Prov = SessionManager.Instance.GetProvider();
        internal static IIdentityProvider IdProv = IdentityManager.Instance.GetProvider(ctx);

        internal const string SessionIdName = "SsId";
        private const string create = "/create";
        private const string extend = "/extend";
        private const string expire = "/expire";

        public string BaseUrl
        {
            get;
        } = "sessions";

        public override void Handle(HttpContext context, CancellationToken cancel)
        {
            MethodContract.NotNull(context, nameof(context)); //this will throw if context==null

            HttpRequest request = context.Request;
            HttpResponse response = context.Response;

            string local = RestUtils.LocalUrl(this, request);
            string meth = RestUtils.StripLocal(this.BaseUrl, local);

            if (create.Equals(meth, StringComparison.OrdinalIgnoreCase))
            {
                Create(response, cancel);
                return;
            }
            else if (extend.Equals(meth, StringComparison.OrdinalIgnoreCase))
            {
                //do nothing, already extended
                RestUtils.Push(response, JsonOpStatus.Ok);
                return;
            }
            else if (expire.Equals(meth, StringComparison.OrdinalIgnoreCase))
            {
                if (request.Headers.ContainsKey(Session.SessionIdName))
                {
                    Guid ssid;
                    if (Guid.TryParse(request.Headers[Session.SessionIdName], out ssid))
                    {
                        if (Prov.Expire(ssid)) //note that this will autoextend and will return false if expired
                        {
                            response.Headers.Remove(Session.SessionIdName);
                            RestUtils.Push(response, JsonOpStatus.Ok);
                            return;
                        }
                    }
                }
            }
            response.StatusCode = HttpStatusCodes.Status400BadRequest;
        }

        private void Create(HttpResponse response, CancellationToken cancel)
        {
            ModuleRuntimeSession sess = Prov.Create();
            response.Headers[Session.SessionIdName] = sess.SessionId.ToString();
            RestUtils.Push(response, RestUtils.JsonOpStatus(JsonOpStatus.Ok));
        }

        public static Guid Get(HttpContext context)
        {
            if (context != null)
            {
                HttpRequest request = context.Request;

                if (request.Headers.ContainsKey(Session.SessionIdName))
                {
                    Guid ssid;
                    if (Guid.TryParse(request.Headers[Session.SessionIdName], out ssid))
                    {
                        ModuleRuntimeSession sess = Prov.Get(ssid);
                        if (sess!=null) //note that this will autoextend and will return false if expired
                            return sess.SessionId;
                    }
                }
            }
            return Guid.Empty;
        }

        public static UserIdentityBase GetUser(HttpContext context)
        {
            Guid sid = Get(context);
            if (!Guid.Empty.Equals(sid))
            {
                ModuleRuntimeSession sess = Prov.Get(sid);
                if (sess != null && !Guid.Empty.Equals(sess.UserId))
                {
                    return IdProv.Get(sess.UserId);
                }
            }
            return null;
        }

        public static bool Bind(Guid uid, Guid sid)
        {
            ModuleRuntimeSession sess = Prov.Get(sid);
            if (sess!=null)
            {
                sess.SetUserBinding(uid, sess.Binding);
                return Prov.Update(sess);
            }
            return false;
        }
    }
}
