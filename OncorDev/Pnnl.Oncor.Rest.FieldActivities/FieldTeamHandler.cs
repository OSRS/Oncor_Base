using Osrs.Net.Http;
using Osrs.Security;
using Osrs.Threading;
using System;

namespace Pnnl.Oncor.Rest.FieldActivities
{
    internal static class FieldTeamHandler
    {
        public static void Handle(UserSecurityContext user, string method, HttpContext context, CancellationToken cancel)
        {
            //check for teams/roles and dispatch accordingly
            if (method.StartsWith(FieldActivitiesHandler.Roles))
            {
                TeamRolesHandler.Handle(user, method.Substring(FieldActivitiesHandler.Roles.Length), context, cancel);
                return;
            }

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
