using Newtonsoft.Json.Linq;
using System;
using Osrs.Net.Http.Handlers;
using Osrs.Net.Http;
using Osrs.Threading;
using Osrs.Security.Sessions;
using Osrs.Security;
using Osrs.WellKnown.Projects;
using Osrs.Data;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.Projects
{
    public sealed class ProjectsHandler : HttpHandlerBase, IServiceHandler
    {
        private const string Projects = "/projects/";
        
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
                return "projects";
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
                    string meth = RestUtils.StripLocal(this.BaseUrl + "/", localUrl);

                    if (!string.IsNullOrEmpty(meth))
                    {
                        Handle(ctx, meth, context, cancel);
                    }
                }
                else
                    context.Response.StatusCode = HttpStatusCodes.Status401Unauthorized;
            }
            context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
        }

        public static void Handle(UserSecurityContext user, string method, HttpContext context, CancellationToken cancel)
        {
            if (context.Request.Method == "POST") //all we support is get/post
            {
                if (method.Equals("all", StringComparison.OrdinalIgnoreCase))
                {
                    Get(user, context, cancel);
                    return;
                }
                else if (method.Equals("in", StringComparison.OrdinalIgnoreCase))
                {
                    try{
                        HashSet<CompoundIdentity> ids = JsonUtils.ToIds(JsonUtils.GetDataPayload(context.Request));
                        if(ids != null)
                        {
                            GetIds(ids, user, context, cancel);
                            return;
                        }
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                    }
                }
                else if (method.Equals("find", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        if (token != null)
                        {
                            if (token["name"] != null)
                            {
                                string name = token["name"].ToString();
                                GetName(name, user, context, cancel);
                                return;
                            }
                            else if (token["orgid"] != null)
                            {
                                CompoundIdentity owner_id = JsonUtils.ToId(token["orgid"]);
                                GetByOwner(owner_id, user, context, cancel);
                                return;
                            }
                        }

                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                        return;
                    }
                }
                else if (method.Equals("children", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        ProjectProviderBase provider = ProjectManager.Instance.GetProvider(user);
                        JToken token = JsonUtils.GetDataPayload(context.Request);
                        if (token != null && provider != null)
                        {
                            if (token["parentid"] != null)
                            {
                                CompoundIdentity parent_id = JsonUtils.ToId(token["parentid"]);
                                Project parent = provider.Get(parent_id);
                                if (parent != null)
                                {
                                    IEnumerable<Project> projects = provider.GetFor(parent);
                                    JArray jprojects = Jsonifier.ToJson(projects);
                                    if (jprojects != null)
                                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jprojects);
                                    else
                                        RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                                    return;
                                }
                            }
                        }

                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                        return;
                    }
                }
                else if (method.Equals("create", StringComparison.OrdinalIgnoreCase))
                {
                    string name;
                    string desc = null;
                    JToken token = null;
                    Project project = null;
                    Project parent_project = null;
                    CompoundIdentity org_cid = null;
                    CompoundIdentity proj_cid = null;
                    HashSet<CompoundIdentity> affiliate_cids = null;
                    ProjectProviderBase provider = null;

                    try
                    {
                        //token and provider
                        token = JsonUtils.GetDataPayload(context.Request);
                        provider = ProjectManager.Instance.GetProvider(user);
                        if(provider != null && token != null)
                        {
                            //required inputs
                            org_cid = JsonUtils.ToId(token["orgid"]);
                            name = token["name"].ToString();
                            if (org_cid != null && !string.IsNullOrEmpty(name))
                            {
                                desc = token["desc"] != null ? token["desc"].ToString() : null;

                                if (token["affiliateorgid"] != null)
                                    affiliate_cids = JsonUtils.ToIds(token["affiliateorgid"]);  //could be multiple

                                if (token["parentid"] != null)
                                {
                                    proj_cid = JsonUtils.ToId(token["parentid"]);
                                    parent_project = (proj_cid != null) ? provider.Get(proj_cid) : null;
                                }

                                //create; optionally update affiliations
                                project = provider.Create(name, org_cid, parent_project, desc);
                                if (project != null)
                                {
                                    bool result = true;
                                    //update with affiliates if necessary
                                    if (affiliate_cids != null)
                                    {
                                        foreach (CompoundIdentity cid in affiliate_cids)
                                        {
                                            project.Affiliates.Add(cid);
                                        }
                                        result &= provider.Update(project);
                                    }

                                    if (result == true)
                                    {
                                        JObject jproject = Jsonifier.ToJson(project);
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jproject.ToString()));
                                        return;
                                    }
                                }
                            }
                        }

                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                        return;
                    }
                }
                else if (method.Equals("delete", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        JToken t = JsonUtils.GetDataPayload(context.Request);
                        HashSet<CompoundIdentity> cids = JsonUtils.ToIds(t);
                        ProjectProviderBase provider = ProjectManager.Instance.GetProvider(user);
                        if (provider != null && cids != null)
                        {
                            bool result = true;
                            foreach (CompoundIdentity cid in cids)
                            {
                                result &= provider.Delete(cid);
                            }

                            if (result == true)
                                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok));
                            else
                                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                            return;
                        }

                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                        return;
                    }
                }
                else if (method.Equals("update", StringComparison.OrdinalIgnoreCase))
                {
                    string name;
                    string desc = null;
                    JToken token = null;
                    Project project = null;
                    CompoundIdentity cid = null;
                    CompoundIdentity org_cid = null;
                    CompoundIdentity parent_cid = null;
                    HashSet<CompoundIdentity> affiliate_cids = null;
                    ProjectProviderBase provider = null;

                    try
                    {
                        token = JsonUtils.GetDataPayload(context.Request);
                        provider = ProjectManager.Instance.GetProvider(user);
                        if (provider != null && token != null)
                        {
                            //GUID must be provided
                            cid = JsonUtils.ToId(token["id"]);

                            //fetch stored object
                            bool dirty = false;
                            project = provider.Get(cid);
                            if (project != null)
                            {

                                //## REQUIRED ##

                                //name
                                if (token.SelectToken("name") != null)
                                {
                                    name = token["name"].ToString();
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        project.Name = name;
                                        dirty = true;
                                    }
                                    else
                                    {
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed)); //name is required and not nullable
                                        return;
                                    }
                                }

                                //owning org
                                if (token.SelectToken("orgid") != null)
                                {
                                    org_cid = JsonUtils.ToId(token["orgid"]);
                                    if (org_cid != null)
                                    {
                                        project.PrincipalOrganization = org_cid;
                                        dirty = true;
                                    }
                                    else
                                    {
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed)); //owning org is required and not nullable
                                        return;
                                    }
                                }

                                //## OPTIONALS ##

                                //description
                                if (token.SelectToken("desc") != null)
                                {
                                    desc = (token["desc"] != null) ? token["desc"].ToString() : null;
                                    project.Description = desc;
                                    dirty = true;
                                }

                                //parent project
                                if (token.SelectToken("parentid") != null)
                                {
                                    parent_cid = JsonUtils.ToId(token["parentid"]);
                                    project.ParentId = parent_cid;  //could be null
                                    dirty = true;
                                }

                                //affiliate orgs
                                if (token.SelectToken("affiliateorgid") != null)
                                {
                                    affiliate_cids = JsonUtils.ToIds(token["affiliateorgid"]);
                                    //reset affiliates
                                    List<CompoundIdentity> clist = new List<CompoundIdentity>();
                                    foreach (CompoundIdentity c in project.Affiliates)
                                        clist.Add(c);
                                    foreach (CompoundIdentity c in clist)
                                        project.Affiliates.Remove(c);
                                    if (affiliate_cids != null)
                                    {
                                        foreach (CompoundIdentity c in affiliate_cids)
                                            project.Affiliates.Add(c);
                                    }
                                    dirty = true;
                                }

                                if (dirty)
                                {
                                    //update
                                    bool result = provider.Update(project);
                                    if (result == true)
                                    {
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok));
                                        return;
                                    }
                                }
                                else
                                {
                                    //return ok - no values were modified
                                    RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok));
                                    return;
                                }
                            }
                        }

                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                    }
                    catch
                    {
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                        return;
                    }
                }
            }
            context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
        }

        private static void GetIds(HashSet<CompoundIdentity> ids, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                if (ids.Count == 0)
                {
                    RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                    return;
                }

                ProjectProviderBase provider = ProjectManager.Instance.GetProvider(user);
                if (provider != null)
                {
                    JArray projects = Jsonifier.ToJson(provider.Get(ids));
                    if (projects != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, projects.ToString()));
                    else
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                    return;
                }

                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
            }
            catch
            {
                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
            }
        }

        private static void GetName(string name, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                ProjectProviderBase provider = ProjectManager.Instance.GetProvider(user);
                if (provider != null)
                {
                    IEnumerable<Project> projects = provider.Get(name);
                    JArray jprojects = Jsonifier.ToJson(projects);
                    if (jprojects != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jprojects.ToString()));
                    else
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, "[]"));
                    return;
                }

                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
            }
            catch
            {
                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
            }
        }

        private static void GetByOwner(CompoundIdentity owner_cid, UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                ProjectProviderBase provider = ProjectManager.Instance.GetProvider(user);
                if (provider != null)
                {
                    IEnumerable<Project> projects = provider.GetFor(owner_cid);
                    JArray jprojects = Jsonifier.ToJson(projects);
                    if (projects != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jprojects.ToString());
                    else
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                    return;
                }

                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
            }
            catch
            {
                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                return;
            }
        }

        private static void Get(UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                ProjectProviderBase provider = ProjectManager.Instance.GetProvider(user);
                if (provider != null)
                {
                    IEnumerable<Project> projects = provider.Get();
                    JArray arr_projects = Jsonifier.ToJson(projects);
                    if (arr_projects != null)
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, arr_projects.ToString()));
                    else
                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, "[]"));
                    return;
                }

                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
            }
            catch
            {
                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
            }
        }
    }
}

