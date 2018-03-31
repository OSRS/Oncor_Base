using Osrs.Net.Http.Handlers;
using System;
using System.Collections.Generic;
using System.Text;
using Osrs.Net.Http;
using Osrs.Threading;
using System.Text.RegularExpressions;
using Osrs.Security.Identity;
using Osrs.Security;
using Osrs.Security.Authentication.Providers;
using Osrs.Security.Authentication;
using Osrs.Security.Sessions;
using Osrs.Runtime.Configuration;
using Osrs.Net.Mail;
using Osrs.Security.Authorization;

namespace Pnnl.Oncor.Rest.Security
{
    public class Login : HttpHandlerBase, IServiceHandler
    {
        private readonly UserSecurityContext ctx = new UserSecurityContext(new LocalSystemUser(SecurityUtils.AdminIdentity, "Admin", UserState.Active)); //TODO -- change this to a system-level account
        private const string signin = "/signin";
        private const string signout = "/signout";
        private const string signup = "/signup";
        private const string reset = "/reset";
        private const string validate = "/validate";
        private const string update = "/update";
        private const string confirm = "/confirm";

        private const string type = "type";
        private const string user = "u";
        private const string pass = "p";
        private const string passChange = "newpass";

        private const string knownType = "upass";
        private const string token = "token";

        private string activateUrl = null;
        private string ActivateUrl
        {
            get
            {
                if (this.activateUrl == null)
                {
                    ConfigurationProviderBase prov = ConfigurationManager.Instance.GetProvider();
                    if (prov!=null)
                    {
                        ConfigurationParameter parm = prov.Get(typeof(Login), "activateUrl");
                        if (parm!=null)
                        {
                            this.activateUrl = parm.Value.ToString();
                        }
                    }
                }
                return this.activateUrl;
            }
        }

        private string resetUrl = null;
        private string ResetUrl
        {
            get
            {
                if (this.resetUrl == null)
                {
                    ConfigurationProviderBase prov = ConfigurationManager.Instance.GetProvider();
                    if (prov != null)
                    {
                        ConfigurationParameter parm = prov.Get(typeof(Login), "resetUrl");
                        if (parm != null)
                        {
                            this.resetUrl = parm.Value.ToString();
                        }
                    }
                }
                return this.resetUrl;
            }
        }

        private string emailSubject = null;
        private string EmailSubject
        {
            get
            {
                if (this.emailSubject == null)
                {
                    ConfigurationProviderBase prov = ConfigurationManager.Instance.GetProvider();
                    if (prov != null)
                    {
                        ConfigurationParameter parm = prov.Get(typeof(Login), "smtpMessageSubject");
                        if (parm != null)
                        {
                            this.emailSubject = parm.Value.ToString();
                        }
                    }
                }
                return this.emailSubject;
            }
        }

        private SmtpClientConfig emailConfig = null;
        private SmtpClientConfig EmailConfig
        {
            get
            {
                if (emailConfig==null)
                {
                    ConfigurationProviderBase prov = ConfigurationManager.Instance.GetProvider();
                    if (prov != null)
                    {
                        ConfigurationParameter parm = prov.Get(typeof(Login), "smtpPort");
                        if (parm != null)
                        {
                            int port;
                            if (int.TryParse(parm.Value.ToString(), out port))
                            {
                                parm = prov.Get(typeof(Login), "smtpServer");
                                if (parm != null)
                                {
                                    string server = parm.Value.ToString();

                                    parm = prov.Get(typeof(Login), "smtpEmail");
                                    if (parm != null)
                                    {
                                        string user = parm.Value.ToString();

                                        parm = prov.Get(typeof(Login), "smtpPassword");
                                        if (parm!=null)
                                        {
                                            string pass = parm.Value.ToString();
                                            this.emailConfig = new SmtpClientConfig(port, server, user, pass);
                                            this.emailConfig.HtmlFormat = false;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                return emailConfig;
            }
        }


        public string BaseUrl
        {
            get
            {
                return "login";
            }
        }

        public override void Handle(HttpContext context, CancellationToken cancel)
        {
            if (context != null)
            {
                string localUrl = RestUtils.LocalUrl(this, context.Request);
                string meth = RestUtils.StripLocal(this.BaseUrl, localUrl);

                if (!string.IsNullOrEmpty(meth))
                {
                    if (meth.StartsWith(signin, StringComparison.OrdinalIgnoreCase))
                    {
                        Signin(context, cancel);
                        return;
                    }
                    else if (meth.StartsWith(signout, StringComparison.OrdinalIgnoreCase))
                    {
                        Signout(context, cancel);
                        return;
                    }
                    else if (meth.StartsWith(signup, StringComparison.OrdinalIgnoreCase))
                    {
                        SignUp(context, cancel);
                        return;
                    }
                    else if (meth.StartsWith(reset, StringComparison.OrdinalIgnoreCase))
                    {
                        ResetAcct(context, cancel);
                        return;
                    }
                    else if (meth.StartsWith(validate, StringComparison.OrdinalIgnoreCase))
                    {
                        ValidateAcct(context, cancel);
                        return;
                    }
                    else if (meth.StartsWith(update, StringComparison.OrdinalIgnoreCase))
                    {
                        ChangePass(context, cancel);
                        return;
                    }
                    else if (meth.StartsWith(confirm, StringComparison.OrdinalIgnoreCase))
                    {
                        Confirm(meth, context, cancel);
                        return;
                    }

                }
                context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
            }
        }


        private bool SendEmail(string email, string url, bool reset) //if reset is false, this is a new account
        {
            try
            {
                string message;
                string subject = this.EmailSubject;
                if (reset) //send a reset email
                {
                    if (subject != null)
                        subject = subject + " Account Reset";
                    else
                        subject = "Account Reset";
                    message = "Click on the provided link to reset your account: " + url + " if the link is not clickable, paste it into your browser url bar and select go.";
                }
                else //send a new account email
                {
                    if (subject != null)
                        subject = subject + " Account Validation";
                    else
                        subject = "Account Validation";
                    message = "This is to verify your new account, if you did not create an account ignore this message.\n Click on the provided link to verify your account: " + url + " if the link is not clickable, paste it into your browser url bar and select go.";
                }

                SmtpClient client = new SmtpClient(this.EmailConfig);
                return client.Send(email, subject, message);
            }
            catch
            { }

            return false;
        }

        private void Confirm(string path, HttpContext context, CancellationToken cancel) //this is where we come in from the REST request for the clicked link page on new account
        {
            //this is the only pure "GET" method which requires the token in the url for a one-time confirmation of a new account
            //techinically, this can go away if we change the interaction mechanism to use RESET instead
            if (context.Request.Method == HttpVerbs.GET.ToString())
            {
                string gText = path.Substring(1);
                int sep = gText.IndexOf('/');
                if (sep>0 && sep<gText.Length-1) //has to have a / somewhere in the remaining url... should be just a GUID after that slash
                {
                    gText = gText.Substring(sep + 1);
                    if (!string.IsNullOrEmpty(gText)) //better have content after the slash
                    {
                        Guid id;
                        if (Guid.TryParse(gText, out id)) //is the guid parsable from the remaining string of the url
                        {
                            if(Authenticator.Instance.Activate(id))
                            {
                                RestUtils.Push(context.Response, JsonOpStatus.Ok);
                                return;
                            }

                            RestUtils.Push(context.Response, JsonOpStatus.Failed, "activateFailed");
                            return;
                        }
                    }
                }
                RestUtils.Push(context.Response, JsonOpStatus.Failed, "badUrlToken");
                return;
            }
            else
            {
                RestUtils.Push(context.Response, JsonOpStatus.Failed, "badRequest");
                return;
            }
        }

        private void SignUp(HttpContext context, CancellationToken cancel)
        {
            string url = this.ActivateUrl;
            if (!string.IsNullOrEmpty(url))
            {
                IQueryCollection qry = context.Request.Query;
                string ty = qry[type];
                if (knownType.Equals(ty))
                {
                    string u = qry[user];
                    if (!string.IsNullOrEmpty(u))
                    {
                        string p = qry[pass];
                        if (!string.IsNullOrEmpty(p))
                        {
                            if (ValidUserEmail(u))
                            {
                                IIdentityProvider idProv = IdentityManager.Instance.GetProvider(ctx);
                                if (!idProv.Exists(u))
                                {
                                    IAuthenticationProvider authProv = AuthenticationManager.Instance.GetProvider(ctx);
                                    UserIdentityBase user = idProv.CreateUser(u);
                                    user.UserState = UserState.Pending;
                                    idProv.Update(user);
                                    UserPasswordCredential cred = new UserPasswordCredential(u, p);
                                    if (authProv.AddCredential(user, cred))
                                    {
                                        Guid token = Authenticator.Instance.Reset(u, false); //create a reset token
                                        //notice we create a url with the token at the end, this COULD map to the REST api directly - but is expected instead not to
                                        //we instead expect this to be a simple page that makes the rest request and "looks pretty" to confirm and perhaps send the user then back to the signin page.
                                        if (url.EndsWith("?"))
                                            url = url + token.ToString();
                                        else
                                            url = url + "?" + token.ToString();

                                        if (SendEmail(u, url, false))
                                        {
                                            RestUtils.Push(context.Response, JsonOpStatus.Ok);
                                            return;
                                        }
                                        else
                                        {
                                            idProv.Delete(user.Uid);
                                            authProv.DeleteCredential(user, cred);
                                            RestUtils.Push(context.Response, JsonOpStatus.Failed, "\"Couldn't send email\"");
                                            return;
                                        }
                                    }
                                    else
                                    {
                                        idProv.Delete(user.Uid);
                                        authProv.DeleteCredential(user, cred);
                                        RestUtils.Push(context.Response, JsonOpStatus.Failed, "\"Couldn't set credential\"");
                                        return;
                                    }
                                }
                                else
                                {
                                    RestUtils.Push(context.Response, JsonOpStatus.Failed, "\"UserExists\"");
                                    return;
                                }
                            }
                            else
                            {
                                RestUtils.Push(context.Response, JsonOpStatus.Failed, "\"InvalidEmail\"");
                                return;
                            }
                        }
                    }
                }
            }
            RestUtils.Push(context.Response, JsonOpStatus.Failed);
        }

        private void Signin(HttpContext context, CancellationToken cancel)
        {
            IQueryCollection qry = context.Request.Query;
            string ty = qry[type];
            if (knownType.Equals(ty))
            {
                string u = qry[user];
                if (!string.IsNullOrEmpty(u))
                {
                    string p = qry[pass];
                    if (!string.IsNullOrEmpty(p))
                    {
                        LoginToken token = Authenticator.Instance.Authenticate(u, p);
                        if (token != null)
                        {
                            RestUtils.Push(context.Response, JsonOpStatus.Ok, "\"" + token.ToJson() + "\"");
                            return;
                        }
                    }
                }
            }
            RestUtils.Push(context.Response, JsonOpStatus.Failed);
        }

        /// <summary>
        /// this is where you come back from the login with just a token
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancel"></param>
        private void ValidateAcct(HttpContext context, CancellationToken cancel)
        {
            Guid sid = Session.Get(context);
            if (!Guid.Empty.Equals(sid))
            {
                IQueryCollection qry = context.Request.Query;
                string ty = qry[token];
                if (!string.IsNullOrEmpty(ty))
                {
                    LoginToken tok = LoginToken.TryParse(ty);
                    if (tok!=null)
                    {
                        Guid uid = Authenticator.Instance.Authenticate(tok);
                        if (!Guid.Empty.Equals(uid))
                        {
                            if (Session.Bind(uid, sid))
                            {
                                RestUtils.Push(context.Response, JsonOpStatus.Ok);
                                return;
                            }
                        }
                    }
                }
            }
            RestUtils.Push(context.Response, JsonOpStatus.Failed);
        }

        private void ResetAcct(HttpContext context, CancellationToken cancel)
        {
            string url = this.ResetUrl;
            if (!string.IsNullOrEmpty(url))
            {
                Guid sid = Session.Get(context);
                if (!Guid.Empty.Equals(sid))
                {
                    IQueryCollection qry = context.Request.Query;
                    string ty = qry[type];
                    if (knownType.Equals(ty))
                    {
                        string u = qry[user];
                        if (!string.IsNullOrEmpty(u))
                        {
                            Guid token = Authenticator.Instance.Reset(u, true);
                            if (!Guid.Empty.Equals(token))
                            {
                                if (url.EndsWith("?"))
                                    url = url + token.ToString();
                                else
                                    url = url + "?" + token.ToString();
                                if (SendEmail(u, url, false))
                                {
                                    RestUtils.Push(context.Response, JsonOpStatus.Ok, "\"" + token.ToString() + "\"");
                                    return;
                                }
                            }
                            else
                            {
                                RestUtils.Push(context.Response, JsonOpStatus.Failed, "unknown email address");
                                return;
                            }
                        }
                        else
                        {
                            RestUtils.Push(context.Response, JsonOpStatus.Failed, "email address not valid");
                            return;
                        }
                    }
                }
            }
            RestUtils.Push(context.Response, JsonOpStatus.Failed);
        }

        private void ChangePass(HttpContext context, CancellationToken cancel)
        {
            Guid sid = Session.Get(context);
            if (!Guid.Empty.Equals(sid))
            {
                IQueryCollection qry = context.Request.Query;
                string ty = qry[type];
                if (knownType.Equals(ty))
                {
                    string u = qry[user];
                    if (!string.IsNullOrEmpty(u))
                    {
                        string p = qry[passChange];
                        if (!(string.IsNullOrEmpty(p)))
                        {
                            string oldPass = qry[pass];
                            string tok = qry[token];

                            if (!string.IsNullOrEmpty(oldPass)) //standard change
                            {
                                if (Authenticator.Instance.ChangePass(sid, u, oldPass, p))
                                {
                                    RestUtils.Push(context.Response, JsonOpStatus.Ok);
                                    return;
                                }
                            }
                            else if (!string.IsNullOrEmpty(tok)) //token based reset change
                            {
                                Guid token;
                                if (Guid.TryParse(tok, out token))
                                {
                                    if (Authenticator.Instance.ChangePass(sid, u, token, p))
                                    {
                                        RestUtils.Push(context.Response, JsonOpStatus.Ok);
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            RestUtils.Push(context.Response, JsonOpStatus.Failed);
        }

        private void Signout(HttpContext context, CancellationToken cancel)
        {
            Guid sessId = Session.Get(context);
            if (!Guid.Empty.Equals(sessId))
            {
                Session.Prov.Expire(sessId);
                RestUtils.Push(context.Response, JsonOpStatus.Ok);
                return;
            }
            RestUtils.Push(context.Response, JsonOpStatus.Failed, "\"nosession\"");
        }

        private static bool ValidUserEmail(string strIn)
        {
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }

        }
    }

    public class Authenticator
    {
        private readonly UserSecurityContext ctx = new UserSecurityContext(new LocalSystemUser(SecurityUtils.AdminIdentity, "Admin", UserState.Active));
        private readonly int lifeTime = 60000; //1 minute
        private readonly Dictionary<Guid, LoginToken> activeTokens = new Dictionary<Guid, LoginToken>(); //stored tokens here are key:token, value:<token, userid>

        private readonly int resetLife = 14400000; //4 hours
        private readonly Dictionary<Guid, LoginToken> resetTokens = new Dictionary<Guid, LoginToken>(); //stored tokens here are key:token, value:<token, userid>
        public Guid Reset(string user, bool deleteCred)
        {
            IIdentityProvider idProv = IdentityManager.Instance.GetProvider(ctx);
            IEnumerable<UserIdentityBase> users = idProv.Get(user, UserType.Person);
            if (users!=null)
            {
                foreach(UserIdentityBase u in users)
                {
                    if (u.UserState != UserState.Dead && u.UserState != UserState.Unknown)
                    {
                        if (deleteCred)
                        {
                            IAuthenticationProvider authProv = AuthenticationManager.Instance.GetProvider(ctx);
                            authProv.DeleteCredential(u, new UserPasswordCredential(user, user)); //TODO -- make sure this can work to kill a current credential
                        }
                        Guid tok = Guid.NewGuid();
                        resetTokens.Add(tok, new LoginToken(tok, u.Uid));
                        return tok;
                    }
                }
            }
            return Guid.Empty;
        }

        public bool ChangePass(Guid sid, string user, Guid token, string newPass)
        {
            if (!Guid.Empty.Equals(sid))
            {
                if (resetTokens.ContainsKey(token))
                {
                    ModuleRuntimeSession sess = Session.Prov.Get(sid);
                    if (sess != null)
                    {
                        IIdentityProvider idProv = IdentityManager.Instance.GetProvider(ctx);
                        UserIdentityBase userId = idProv.Get(resetTokens[token].Token);
                        if (userId != null && userId.UserState != UserState.Dead)
                        {
                            IAuthenticationProvider authProv = AuthenticationManager.Instance.GetProvider(ctx);
                            if (authProv.AddCredential(userId, new UserPasswordCredential(user, newPass)))
                            {
                                resetTokens.Remove(token);
                                sess.SetUserBinding(userId.Uid, sess.Binding);
                                Session.Prov.Update(sess);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        public bool ChangePass(Guid sid, string user, string oldPass, string newPass)
        {
            if (!Guid.Empty.Equals(sid))
            {
                ModuleRuntimeSession sess = Session.Prov.Get(sid);
                if (sess!=null && !Guid.Empty.Equals(sess.UserId)) //user must be logged in with a session to change password
                {
                    IIdentityProvider idProv = IdentityManager.Instance.GetProvider(ctx);
                    IUserIdentity userId = idProv.Get(sess.UserId);
                    if (userId!=null)
                    {
                        IAuthenticationProvider authProv = AuthenticationManager.Instance.GetProvider(ctx);
                        return authProv.ReplaceCredential(userId, new UserPasswordCredential(user, oldPass), new UserPasswordCredential(user, newPass));
                    }
                }
            }
            return false;
        }

        public bool Activate(Guid token)
        {
            if (!Guid.Empty.Equals(token))
            {
                if (resetTokens.ContainsKey(token))
                {
                    IIdentityProvider idProv = IdentityManager.Instance.GetProvider(ctx);
                    UserIdentityBase userId = idProv.Get(resetTokens[token].Token);
                    if (userId != null && userId.UserState != UserState.Dead)
                    {
                        resetTokens.Remove(token);
                        userId.UserState = UserState.Active;
                        if (idProv.Update(userId))
                        {
                            IRoleProvider roles = AuthorizationManager.Instance.GetRoleProvider(ctx);
                            if (roles != null)
                            {
                                Role r = roles.Get(new Guid("a55c4810-de06-466e-8f30-4f02b138186f"));
                                if (r != null)
                                    return roles.AddToRole(r, userId);
                            }
                        }
                    }
                }
            }
            return false;
        }

        public LoginToken Authenticate(string user, string pass)
        {
            IAuthenticationProvider authProv = AuthenticationManager.Instance.GetProvider(ctx);
            IUserIdentity uId = authProv.Authenticate(new UserPasswordCredential(user, pass));
            if (uId!=null && !Guid.Empty.Equals(uId.Uid))
            {
                LoginToken token = new LoginToken(AuthenticationProviders.Instance.Local, Guid.NewGuid());
                activeTokens.Add(token.Token, new LoginToken(token.Token, uId.Uid));
                return token;
            }

            return null;
        }

        public Guid Authenticate(LoginToken token)
        {
            if (token!=null)
            {
                Scavenge();
                if (activeTokens.ContainsKey(token.Token))
                {
                    Guid id = activeTokens[token.Token].Token;
                    activeTokens.Remove(token.Token);
                    return id;
                }
            }
            return Guid.Empty;
        }

        private void Scavenge()
        {
            DateTime mark = DateTime.Now.AddMilliseconds(lifeTime);
            HashSet<Guid> oldTokens = new HashSet<Guid>();
            foreach(LoginToken cur in activeTokens.Values)
            {
                if (mark < cur.Received)
                    oldTokens.Add(cur.AuthenticationSource);
            }

            foreach (Guid id in oldTokens)
            {
                activeTokens.Remove(id);
            }

            oldTokens.Clear();
            mark = DateTime.Now.AddMilliseconds(resetLife);
            foreach (LoginToken cur in resetTokens.Values)
            {
                if (mark < cur.Received)
                    oldTokens.Add(cur.AuthenticationSource);
            }

            foreach (Guid id in oldTokens)
            {
                resetTokens.Remove(id);
            }
        }

        private static Authenticator instance = new Authenticator();
        public static Authenticator Instance
        {
            get { return instance; }
        }
    }

    public class LoginToken
    {
        public string ToJson()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(this.AuthenticationSource.ToString());
            sb.Append(":");
            sb.Append(this.Token.ToString());
            return sb.ToString();
        }

        public Guid AuthenticationSource
        {
            get;
        }

        public Guid Token
        {
            get;
        }

        public DateTime Received
        {
            get;
        }

        internal LoginToken(Guid authenticationSource, Guid token)
        {
            this.AuthenticationSource = authenticationSource;
            this.Token = token;
            this.Received = DateTime.Now;
        }

        public static LoginToken TryParse(string tokenString)
        {
            if (!string.IsNullOrEmpty(tokenString))
            {
                string[] parts = tokenString.Split(':');
                if (parts!=null && parts.Length==2)
                {
                    Guid auth;
                    Guid token;

                    if (Guid.TryParse(parts[0], out auth))
                    {
                        if (Guid.TryParse(parts[1], out token))
                        {
                            return new LoginToken(auth, token);
                        }
                    }
                }
            }

            return null;
        }
    }

    /// <summary>
    /// registry of authentication providers - default is local
    /// </summary>
    public class AuthenticationProviders
    {
        private readonly Guid local = new Guid("{424F8773-9874-4319-B8C2-D30BDDFEBC45}");
        public Guid Local
        {
            get { return local; }
        }

        private readonly Dictionary<Guid, string> providers = new Dictionary<Guid, string>();
        public Dictionary<Guid, string> Providers
        {
            get { return this.providers; }
        }

        private AuthenticationProviders()
        {
            this.providers.Add(local, string.Empty);
        }

        public Guid ValidateToken(LoginToken token)
        {
            if (token != null)
            {
                if (this.providers.ContainsKey(token.AuthenticationSource))
                {
                    if (token.AuthenticationSource.Equals(local)) //for now, this is all we're supporting
                    {
                        return Authenticator.Instance.Authenticate(token);
                    }
                }
            }
            return Guid.Empty;
        }

        private static AuthenticationProviders instance = new AuthenticationProviders();
        public static AuthenticationProviders Instance
        {
            get { return instance; }
        }
    }
}
