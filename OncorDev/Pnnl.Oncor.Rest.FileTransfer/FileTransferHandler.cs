using Newtonsoft.Json.Linq;
using Osrs.Net.Http;
using Osrs.Net.Http.Handlers;
using Osrs.Oncor.FileStore;
using Osrs.Security;
using Osrs.Security.Sessions;
using Osrs.Threading;
using System;
using System.IO;

namespace Pnnl.Oncor.Rest.FileTransfer
{
    public sealed class FileTransferHandler : HttpHandlerBase, IServiceHandler
    {
        private static IFileStoreProvider prov;
        private const string Upload = "/upload";
        private const string Download = "/download/";

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
                return "files";
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
                        if (meth.StartsWith(Upload, StringComparison.OrdinalIgnoreCase))
                        {
                            string err = null;
                            try
                            {
                                string fName = MetaInfo.GetFileName(context);   //TODO -- add origin filename handling
                                if (fName != null && MetaInfo.SupportedUploadType(fName))  //this might be a bogus way to go
                                {
                                    if (prov == null)
                                        prov = FileStoreManager.Instance.GetProvider();

                                    if (prov != null)
                                    {
                                        Stream input = context.Request.Body;
                                        if (input != null)
                                        {
                                            FilestoreFile fil = prov.MakeTemp();
                                            if (fil != null)
                                            {
                                                input.CopyTo(fil);
                                                fil.Flush();
                                                fil.Close();
                                                fil.FileName = fName;
                                                prov.Update(fil);
                                                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, "\"" + fil.FileId.ToString() + "\""));
                                                return;
                                            }
                                            else
                                                err = "failed to make";
                                        }
                                        else
                                            err = "no file received";
                                    }
                                    else
                                        err = "no provider";
                                }
                                else
                                    err = "unsupported file extension";  //this might be a bogus way to go
                            }
                            catch
                            { err = "unknown error"; }

                            if (err==null)
                                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                            else
                                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed, "\""+err+"\""));
                            return;
                        }
                        if (meth.StartsWith(Download, StringComparison.OrdinalIgnoreCase))
                        {
                            string err = null;
                            try
                            {

                                //this should be a Guid string
                                string filename = Uri.UnescapeDataString(meth.Substring(Download.Length));
                                Guid fileId;

                                if (Guid.TryParse(filename, out fileId))
                                {
                                    if (prov == null)
                                        prov = FileStoreManager.Instance.GetProvider();

                                    if (prov != null)
                                    {
                                        FilestoreFile fil = prov.Get(fileId);
                                        if (fil != null)
                                        {
                                            if (fil.Length > -1)
                                            {
                                                //send the bytes of a file in the response body
                                                context.Response.Headers.Add("Content-Disposition", "attachment ; filename=\"" + fil.FileName + "\"");
                                                context.Response.StatusCode = HttpStatusCodes.Status200OK;
                                                context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                                                fil.CopyTo(context.Response.Body);
                                                return;
                                            }
                                            else
                                                err = "no stream";
                                        }
                                        else
                                            err = "no such file";
                                    }
                                    else
                                        err = "no provider";
                                }
                                else
                                    err = "fileid is not legal";
                            }
                            catch
                            { err = "unknown error"; }

                            if (err == null)
                                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                            else
                                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed, "\"" + err + "\""));
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
