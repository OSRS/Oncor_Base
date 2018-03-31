using Newtonsoft.Json.Linq;
using Osrs.Net.Http;
using Osrs.Net.Http.Handlers;
using Osrs.Security;
using Osrs.Security.Authorization;
using Osrs.Threading;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.Security
{
    public class UserAuthorizations : HttpHandlerBase, IServiceHandler
    {
        private readonly UserSecurityContext ctx = new UserSecurityContext(new LocalSystemUser(SecurityUtils.AdminIdentity, "Admin", UserState.Active)); //TODO -- change this to a system-level account

        public string BaseUrl
        {
            get
            {
                return "authorizations";
            }
        }

        public override void Handle(HttpContext context, CancellationToken cancel)
        {
            if (context != null && AuthorizationManager.Instance.State == Osrs.Runtime.RunState.Running && context.Request.Method == "POST") //all we support is post
            {
                UserIdentityBase user = Security.Session.GetUser(context);
                if (user != null)
                {
                    UserSecurityContext ctx = new UserSecurityContext(user);
                    IRoleProvider prov = AuthorizationManager.Instance.GetRoleProvider(ctx);
                    if (prov!=null)
                    {
                        IEnumerable<Permission> perms = prov.GetPermissions(user);
                        if (perms!=null)
                        {
                            JArray orgs = Jsonifier.ToJson(perms);
                            if (orgs != null)
                                RestUtils.Push(context.Response, JsonOpStatus.Ok, orgs);
                            else
                                RestUtils.Push(context.Response, JsonOpStatus.Failed);
                        }
                        else
                            RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                    }
                    else
                        RestUtils.Push(context.Response, JsonOpStatus.Failed, "No provider");
                    return;
                }
                else
                    context.Response.StatusCode = HttpStatusCodes.Status401Unauthorized;
            }
            context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
        }
    }
}
