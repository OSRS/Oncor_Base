using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Net.Http;
using Osrs.Net.Http.Handlers;
using Osrs.Oncor.EntityBundles;
using Osrs.Security;
using Osrs.Security.Sessions;
using Osrs.Threading;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.EntityBundles
{
    //create =  {name:<name>, orgid:<org id>, type:<datatype - site | instrument | taxa>, items:[<item>,...]}
    //  item = {id: <entityid>, key: <string>, display: <string>}
    public sealed class EntityBundlesHandler : HttpHandlerBase, IServiceHandler
    {
        private const string Create = "create";
        private const string Get = "get";
        private const string In = "in";
        private const string Update = "update";
        private const string Delete = "delete";  //NOTE - not currently supported for safety

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
                return "entitybundles";
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
                        try
                        {
                            if (meth.StartsWith(Get, StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    EntityBundleProvider prov = EntityBundleManager.Instance.GetProvider(ctx);
                                    if (prov != null)
                                    {
                                        JArray jbundles = Jsonifier.ToJson(prov.Get());
                                        if (jbundles != null)
                                        {
                                            RestUtils.Push(context.Response, JsonOpStatus.Ok, jbundles.ToString());
                                            return;
                                        }
                                        else
                                        {
                                            RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                                            return;
                                        }
                                    }

                                    RestUtils.Push(context.Response, JsonOpStatus.Failed);
                                }
                                catch
                                {
                                    RestUtils.Push(context.Response, JsonOpStatus.Failed);
                                    return;
                                }
                                
                                
                            }
                            else if (meth.StartsWith(In, StringComparison.OrdinalIgnoreCase))
                            {
                                //just need a bundle id to get  {id:<id>}
                                JToken token = JsonUtils.GetDataPayload(context.Request);
                                if (token != null)
                                {
                                    JObject o = token as JObject;
                                    if (o != null)
                                    {
                                        if (o[JsonUtils.Id] != null)
                                        {
                                            //Guid id = JsonUtils.ToGuid(token[JsonUtils.Id]);
                                            Guid id = JsonUtils.ToGuid(o[JsonUtils.Id] as JToken);
                                            if (!Guid.Empty.Equals(id))
                                            {
                                                EntityBundleProvider prov = EntityBundleManager.Instance.GetProvider(ctx);
                                                EntityBundle bundle = prov.Get(id);
                                                if (bundle != null)
                                                {
                                                    RestUtils.Push(context.Response, JsonOpStatus.Ok, Jsonifier.ToJson(bundle));
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (meth.StartsWith(Create, StringComparison.OrdinalIgnoreCase))
                            {
                                JToken token = JsonUtils.GetDataPayload(context.Request);
                                if (token != null)
                                {
                                    if (token[JsonUtils.Name] != null && token[JsonUtils.OwnerId] != null && token[Jsonifier.Type] != null && token[Jsonifier.Items] != null)
                                    {
                                        string name = token[JsonUtils.Name].ToString();
                                        if (!string.IsNullOrEmpty(name))
                                        {
                                            string type = token[Jsonifier.Type].ToString();
                                            if (!string.IsNullOrEmpty(type))
                                            {
                                                BundleDataType datType;
                                                if (type.StartsWith("site", StringComparison.OrdinalIgnoreCase))
                                                    datType = BundleDataType.Site;
                                                else if (type.StartsWith("taxa", StringComparison.OrdinalIgnoreCase))
                                                    datType = BundleDataType.TaxaUnit;
                                                else if (type.StartsWith("inst", StringComparison.OrdinalIgnoreCase))
                                                    datType = BundleDataType.Instrument;
                                                else
                                                {
                                                    context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
                                                    return;
                                                }
                                                CompoundIdentity cid = JsonUtils.ToId(token[JsonUtils.OwnerId]);
                                                if (cid != null && !cid.IsEmpty)
                                                {
                                                    JArray itemsArr = Items.GetItems(token[Jsonifier.Items]);
                                                    if (itemsArr != null && itemsArr.Count > 0)
                                                    {
                                                        List<Tuple<CompoundIdentity, string, string>> items = new List<Tuple<CompoundIdentity, string, string>>();
                                                        foreach (JToken cur in itemsArr)
                                                        {
                                                            Tuple<CompoundIdentity, string, string> tpl = Items.GetItem(cur);
                                                            if (tpl != null)
                                                                items.Add(tpl);
                                                        }

                                                        if (items.Count == itemsArr.Count)
                                                        {
                                                            if (Items.VandV(items, datType, ctx))
                                                            {
                                                                EntityBundleProvider prov = EntityBundleManager.Instance.GetProvider(ctx);
                                                                EntityBundle bundle = prov.Create(name, cid, datType);
                                                                if (bundle != null)
                                                                {
                                                                    foreach (Tuple<CompoundIdentity, string, string> tpl in items)
                                                                    {
                                                                        if (bundle.Add(tpl.Item1, tpl.Item2, tpl.Item3) == null)
                                                                        {
                                                                            RestUtils.Push(context.Response, JsonOpStatus.Failed, "\"failed to add item to bundle\"");
                                                                            return;
                                                                        }
                                                                    }

                                                                    if (prov.Update(bundle))
                                                                    {
                                                                        RestUtils.Push(context.Response, JsonOpStatus.Ok, Jsonifier.ToJson(bundle));
                                                                        return;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            else if (meth.StartsWith(Update, StringComparison.OrdinalIgnoreCase)) //note, this will just append items to the existing bundle and update bundle display name
                            {
                                //needs to have same content as create mostly -- doesn't require orgid (we ignore that since it can't change), and name is optional
                                JToken token = JsonUtils.GetDataPayload(context.Request);
                                if (token != null)
                                {
                                    if (token[JsonUtils.Id]!=null && token[Jsonifier.Items] != null)
                                    {
                                        string name = null;
                                        if (token[JsonUtils.Name] != null)
                                            name = token[JsonUtils.Name].ToString();

                                        Guid id = JsonUtils.ToGuid(token[JsonUtils.Id]);
                                        if (!Guid.Empty.Equals(id))
                                        {
                                            JArray itemsArr = Items.GetItems(token[Jsonifier.Items]);
                                            if (itemsArr != null && itemsArr.Count > 0)
                                            {
                                                List<Tuple<CompoundIdentity, string, string>> items = new List<Tuple<CompoundIdentity, string, string>>();
                                                foreach (JToken cur in itemsArr)
                                                {
                                                    Tuple<CompoundIdentity, string, string> tpl = Items.GetItem(cur);
                                                    if (tpl != null)
                                                        items.Add(tpl);
                                                }

                                                if (items.Count == itemsArr.Count)
                                                {
                                                    token = null;
                                                    itemsArr = null;

                                                    EntityBundleProvider prov = EntityBundleManager.Instance.GetProvider(ctx);
                                                    EntityBundle bundle = prov.Get(id);
                                                    if (bundle != null)
                                                    {
                                                        if (!string.IsNullOrEmpty(name) && !bundle.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                                                            bundle.Name = name;

                                                        //now we can actually try to add the items as needed
                                                        foreach(Tuple<CompoundIdentity, string, string> tpl in items)
                                                        {
                                                            if (!bundle.Contains(tpl.Item2))
                                                            {
                                                                if (bundle.Add(tpl.Item1, tpl.Item2, tpl.Item3) == null)
                                                                {
                                                                    RestUtils.Push(context.Response, JsonOpStatus.Failed, "\"Failed to update bundle\"");
                                                                    return;
                                                                }
                                                            }                                                            
                                                        }

                                                        if (prov.Update(bundle))
                                                        {
                                                            RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok));
                                                            return;
                                                        }
                                                        else
                                                        {
                                                            RestUtils.Push(context.Response, JsonOpStatus.Failed, "\"Failed to update bundle\"");
                                                            return;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        catch
                        {
                            context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
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
