using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Net.Http;
using Osrs.Net.Http.Handlers;
using Osrs.Security;
using Osrs.Security.Identity;
using Osrs.Threading;
using Osrs.WellKnown.Organizations;
using Osrs.WellKnown.UserAffiliation;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.UserAffiliation
{
    public sealed class UserAffiliationHandler : HttpHandlerBase, IServiceHandler
    {
        private const string userId = "userid";

        private const string get = "/find";
        private const string add = "/add";
        private const string delete = "/delete";

        public string BaseUrl
        {
            get
            {
                return "affiliations";
            }
        }

        public override void Handle(HttpContext context, CancellationToken cancel)
        {
            if (context != null && context.Request.Method == "POST") //all we support is post
            {
                UserIdentityBase user = Security.Session.GetUser(context);
                if (user != null)
                {
                    UserSecurityContext ctx = new UserSecurityContext(user);
                    string localUrl = RestUtils.LocalUrl(this, context.Request);
                    string meth = RestUtils.StripLocal(this.BaseUrl, localUrl);

                    if (!string.IsNullOrEmpty(meth))
                    {
                        JObject body = JsonUtils.ParseBody(context.Request) as JObject;

                        if (body != null)
                        {
                            if (meth.Equals(get, StringComparison.OrdinalIgnoreCase))
                            {
                                Get(context, ctx, body);
                                return;
                            }
                            if (meth.Equals(add, StringComparison.OrdinalIgnoreCase))
                            {
                                Add(context, ctx, body);
                                return;
                            }
                            if (meth.Equals(delete, StringComparison.OrdinalIgnoreCase))
                            {
                                Delete(context, ctx, body);
                                return;
                            }
                        }
                        else
                        {
                            GetForUser(context, ctx);
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

        private static void GetForUser(HttpContext context, UserSecurityContext ctx)
        {
            IUserAffiliationProvider prov = UserAffiliationManager.Instance.GetProvider(ctx);
            if (prov != null)
            {
                IEnumerable<CompoundIdentity> ids = null;
                ids = prov.GetIds(ctx.User);
                if (ids != null)
                {
                    JArray orgs = JsonUtils.ToJson(ids);
                    if (orgs != null)
                    {
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, orgs);
                        return;
                    }
                }
                else
                {
                    RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                    return;
                }
            }
            RestUtils.Push(context.Response, JsonOpStatus.Failed);
        }

        private static void Get(HttpContext context, UserSecurityContext ctx, JObject body)
        {
            IUserAffiliationProvider prov = UserAffiliationManager.Instance.GetProvider(ctx);
            if (prov != null)
            {
                CompoundIdentity orgId = JsonUtils.ToId(body[JsonUtils.OwnerId]);
                if (orgId == null)
                {
                    Guid userId = JsonUtils.ToGuid(body[userId]);
                    IEnumerable<CompoundIdentity> ids = null;
                    if (Guid.Empty.Equals(userId) || userId.Equals(ctx.Identity))
                    {
                        //this is for self
                        ids = prov.GetIds(ctx.User);
                    }
                    else
                    {
                        //this is for other
                        IIdentityProvider idp = IdentityManager.Instance.GetProvider(ctx);
                        if (idp != null)
                        {
                            IUserIdentity uid = idp.Get(userId); //make sure this is a valid user
                            if (uid != null)
                                ids = prov.GetIds(uid);
                        }
                    }

                    if (ids != null)
                    {
                        JArray orgs = JsonUtils.ToJson(ids);
                        if (orgs != null)
                        {
                            RestUtils.Push(context.Response, JsonOpStatus.Ok, orgs);
                            return;
                        }
                    }
                }
                else //we're searching by org rather than by user
                {
                    IOrganizationProvider oProv = OrganizationManager.Instance.GetOrganizationProvider(ctx);
                    if (oProv != null)
                    {
                        Organization org = oProv.Get(orgId);
                        if (org != null)
                        {
                            JArray uids = JsonUtils.ToJson(prov.GetIds(org));
                            if (uids!=null)
                            {
                                RestUtils.Push(context.Response, JsonOpStatus.Ok, uids);
                                return;
                            }
                        }
                    }
                }
                RestUtils.Push(context.Response, JsonOpStatus.Failed);
            }
        }

        private static IEnumerable<Organization> Fill(HashSet<CompoundIdentity> orgIds, UserSecurityContext ctx)
        {
            if (orgIds!=null)
            {
                IOrganizationProvider oProv = OrganizationManager.Instance.GetOrganizationProvider(ctx);
                if (oProv != null)
                {
                    if (orgIds.Count > 0)
                    {
                        return oProv.Get(orgIds);
                    }
                }
            }
            return null;
        }

        private static IEnumerable<IUserIdentity> Fill(HashSet<Guid> orgIds, UserSecurityContext ctx)
        {
            if (orgIds != null)
            {
                IIdentityProvider idp = IdentityManager.Instance.GetProvider(ctx);
                if (idp != null)
                {
                    if (orgIds.Count > 0)
                    {
                        IUserIdentity id;
                        HashSet<IUserIdentity> uids = new HashSet<IUserIdentity>();
                        foreach(Guid cur in orgIds)
                        {
                            id = idp.Get(cur);
                            if (!Guid.Empty.Equals(id))
                                uids.Add(id);
                        }
                        return uids;
                    }
                }
            }
            return null;
        }

        private static void Add(HttpContext context, UserSecurityContext ctx, JObject body)
        {
            IUserAffiliationProvider prov = UserAffiliationManager.Instance.GetProvider(ctx);
            if (prov!=null)
            {
                HashSet<CompoundIdentity> orgId = JsonUtils.ToIds(body[JsonUtils.OwnerId]);
                HashSet<Guid> userids = JsonUtils.ToGuids(body[userId]);

                if (orgId != null && userids!=null && orgId.Count>0 && userids.Count>0)
                {
                    if (orgId.Count==1 || userids.Count==1) //we can't have many of both
                    {
                        IEnumerable<Organization> orgs = Fill(orgId, ctx);
                        if (orgs!=null)
                        {
                            IEnumerable<IUserIdentity> users = Fill(userids, ctx);
                            if (users!=null)
                            {
                                if (orgId.Count > 1) //many orgs, 1 user
                                {
                                    foreach(IUserIdentity curUser in users)
                                    {
                                        if (prov.Add(curUser, orgs))
                                            RestUtils.Push(context.Response, JsonOpStatus.Ok);
                                        else
                                            RestUtils.Push(context.Response, JsonOpStatus.Failed);
                                        return;
                                    }
                                }
                                else if (userids.Count > 1) //many users, 1 org
                                {
                                    foreach (Organization curOrg in orgs)
                                    {
                                        if (prov.Add(users, curOrg))
                                            RestUtils.Push(context.Response, JsonOpStatus.Ok);
                                        else
                                            RestUtils.Push(context.Response, JsonOpStatus.Failed);
                                        return;
                                    }
                                }
                                else //1 of each
                                {
                                    foreach (IUserIdentity curUser in users)
                                    {
                                        foreach (Organization curOrg in orgs)
                                        {
                                            if (prov.Add(curUser, curOrg))
                                                RestUtils.Push(context.Response, JsonOpStatus.Ok);
                                            else
                                                RestUtils.Push(context.Response, JsonOpStatus.Failed);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            RestUtils.Push(context.Response, JsonOpStatus.Failed);
        }

        private static void Delete(HttpContext context, UserSecurityContext ctx, JObject body)
        {
            IUserAffiliationProvider prov = UserAffiliationManager.Instance.GetProvider(ctx);
            if (prov != null)
            {
                HashSet<CompoundIdentity> orgId = JsonUtils.ToIds(body[JsonUtils.OwnerId]);
                HashSet<Guid> userids = JsonUtils.ToGuids(body[userId]);

                if (orgId != null && userids != null && orgId.Count > 0 && userids.Count > 0)
                {
                    if (orgId.Count == 1 || userids.Count == 1) //we can't have many of both
                    {
                        IEnumerable<Organization> orgs = Fill(orgId, ctx);
                        if (orgs != null)
                        {
                            IEnumerable<IUserIdentity> users = Fill(userids, ctx);
                            if (users != null)
                            {
                                if (orgId.Count > 1) //many orgs, 1 user
                                {
                                    foreach (IUserIdentity curUser in users)
                                    {
                                        if (prov.Remove(curUser, orgs))
                                            RestUtils.Push(context.Response, JsonOpStatus.Ok);
                                        else
                                            RestUtils.Push(context.Response, JsonOpStatus.Failed);
                                        return;
                                    }
                                }
                                else if (userids.Count > 1) //many users, 1 org
                                {
                                    foreach (Organization curOrg in orgs)
                                    {
                                        if (prov.Remove(users, curOrg))
                                            RestUtils.Push(context.Response, JsonOpStatus.Ok);
                                        else
                                            RestUtils.Push(context.Response, JsonOpStatus.Failed);
                                        return;
                                    }
                                }
                                else //1 of each
                                {
                                    foreach (IUserIdentity curUser in users)
                                    {
                                        foreach (Organization curOrg in orgs)
                                        {
                                            if (prov.Remove(curUser, curOrg))
                                                RestUtils.Push(context.Response, JsonOpStatus.Ok);
                                            else
                                                RestUtils.Push(context.Response, JsonOpStatus.Failed);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            RestUtils.Push(context.Response, JsonOpStatus.Failed);
        }
    }
}
