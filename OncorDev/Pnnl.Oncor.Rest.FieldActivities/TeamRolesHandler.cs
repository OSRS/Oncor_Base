using Osrs.Net.Http;
using Osrs.Security;
using Osrs.Threading;
using System;

namespace Pnnl.Oncor.Rest.FieldActivities
{
    internal static class TeamRolesHandler
    {
        public static void Handle(UserSecurityContext user, string method, HttpContext context, CancellationToken cancel)
        {
            if (context.Request.Method == "POST") //all we support is get/post
            {
                if (method.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    //test
                    RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok));
                    return;
                }
            }

            context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
        }
    }
}
