using Osrs.Net.Http;
using Osrs.Net.Http.Handlers;
using Osrs.Security;
using Osrs.Security.Sessions;
using Osrs.Threading;
using System;
using Osrs.Oncor.Wellknown.Persons;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Osrs.Data;

namespace Pnnl.Oncor.Rest.Persons
{
    public sealed class PersonsHandler : HttpHandlerBase, IServiceHandler
    {
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
                return "persons";
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
                    meth = meth.Substring(1);

                    if (!string.IsNullOrEmpty(meth))
                    {
                        if (context.Request.Method == "POST")
                        {
                            if (meth.Equals("all", StringComparison.OrdinalIgnoreCase))
                            {
                                Get(ctx, context, cancel);
                                return;
                            }
                            else if (meth.Equals("create", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    JToken token = JsonUtils.GetDataPayload(context.Request);
                                    PersonProvider provider = PersonManager.Instance.GetProvider(ctx);
                                    Person person = null;
                                    JObject jperson = null;

                                    string firstName = token["firstname"].ToString();
                                    string lastName = token["lastname"].ToString();
                                    JArray contacts = token["contacts"] != null ? token["contacts"] as JArray : null;

                                    if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                                    {
                                        person = provider.Create(firstName, lastName);
                                        if (person != null)
                                        {
                                            var result = true;
                                            if (contacts != null)
                                            {
                                                //add all contacts to person object
                                                foreach (JToken contact in contacts)
                                                {
                                                    string name = contact["name"].ToString();
                                                    string email = contact["email"].ToString();
                                                    person.Contacts.Add(name, new EmailAddress(email));
                                                }

                                                //persist contact info
                                                result &= provider.Update(person);
                                            }

                                            if (result)
                                            {
                                                jperson = Jsonifier.ToJson(person);
                                                if (person != null)
                                                    RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, jperson.ToString()));
                                                else
                                                    RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
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
                            else if (meth.Equals("update", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    JToken token = JsonUtils.GetDataPayload(context.Request);
                                    PersonProvider provider = PersonManager.Instance.GetProvider(ctx);
                                    Person person = null;
                                    bool result = true;

                                    CompoundIdentity id = JsonUtils.ToId(token["id"]);
                                    string firstName = token["firstname"].ToString();
                                    string lastName = token["lastname"].ToString();
                                    if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && id != null)
                                    {
                                        person = new Person(id, firstName, lastName);

                                        if (token.SelectToken("contacts") != null && person != null)
                                        {
                                            JToken contact = token["contacts"];
                                            string name = contact["name"].ToString();
                                            string email = contact["schemeid"].ToString();
                                            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
                                                person.Contacts.Add(name, new EmailAddress(email));
                                        }

                                        result &= provider.Update(person);
                                    }

                                    if (person != null && result)
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok));
                                    else
                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                                    return;
                                }
                                catch
                                {
                                    RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                                    return;
                                }
                            }
                            else if (meth.Equals("delete", StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    bool result = true;
                                    JToken token = JsonUtils.GetDataPayload(context.Request);
                                    PersonProvider provider = PersonManager.Instance.GetProvider(ctx);
                                    if (provider != null && token != null)
                                    {
                                        string first = token["firstname"].ToString();
                                        string last = token["lastname"].ToString();
                                        CompoundIdentity id = JsonUtils.ToId(token["id"]);
                                        if (first != null && last != null && id != null)
                                        {
                                            Person p = new Person(id, first, last);
                                            result &= provider.Delete(p);
                                        }

                                        if (result == true)
                                            RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok));
                                        else
                                            RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
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

        private static void Get(UserSecurityContext user, HttpContext context, CancellationToken cancel)
        {
            try
            {
                PersonProvider provider = PersonManager.Instance.GetProvider(user);
                if (provider != null)
                {
                    IEnumerable<Person> persons = provider.Get();
                    JArray jpersons = new JArray();
                    jpersons = Jsonifier.ToJson(persons);

                    if (jpersons != null)
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, jpersons.ToString());
                    else
                        RestUtils.Push(context.Response, JsonOpStatus.Ok, "[]");
                    return;
                }

                RestUtils.Push(context.Response, JsonOpStatus.Failed);
            }
            catch
            {
                RestUtils.Push(context.Response, JsonOpStatus.Failed);
                return;
            }
        }
    }
}
