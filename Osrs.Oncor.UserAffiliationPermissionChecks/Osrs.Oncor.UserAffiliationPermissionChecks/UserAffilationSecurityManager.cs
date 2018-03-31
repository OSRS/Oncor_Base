using Osrs.Data.Postgres;
using Osrs.Reflection;
using Osrs.Runtime;
using Osrs.Runtime.Configuration;
using Osrs.Runtime.Logging;
using Osrs.Security;
using System;

namespace Osrs.Oncor.UserAffiliationPermissionChecks
{
    public sealed class UserAffilationSecurityManager : ModuleBase
    {
        private static readonly LocalSystemUser user = new LocalSystemUser(new Guid("{87998D26-02FA-4EB4-95D2-B85198324168}"), NameReflectionUtils.GetName(typeof(UserAffilationSecurityManager)), UserState.Active);
        private LogProviderBase logger;
        private bool initialized = false;

        public UserProvider GetProvider(UserSecurityContext ctx)
        {
            if (this.initialized && this.State == RunState.Running)
                return new UserProvider(ctx);
            return null;
        }

        protected override void InitializeImpl()
        {
            lock (instance)
            {
                if (RuntimeUtils.Initializable(this.State))
                {
                    string meth = "Initialize";
                    this.State = RunState.Initializing;
                    this.logger = LogManager.Instance.GetProvider(typeof(UserAffilationSecurityManager));
                    Log(meth, LogLevel.Info, "Called");

                    ConfigurationProviderBase config = ConfigurationManager.Instance.GetProvider();
                    if (config != null)
                    {
                        ConfigurationParameter param = config.Get(NameReflectionUtils.GetType(TypeNameReference.Parse("Osrs.WellKnown.FieldActivities.Providers.PgFieldActivityProviderFactory, Osrs.WellKnown.FieldActivities.Providers.Postgres")), "connectionString");
                        if (param != null)
                        {
                            string tName = param.Value as string;
                            if (!string.IsNullOrEmpty(tName))
                            {
                                if (NpgSqlCommandUtils.TestConnection(tName))
                                {
                                    Db.ConnectionString = tName;
                                    this.initialized = true;
                                    this.State = RunState.Initialized;
                                    return;
                                }
                            }
                            else
                                Log(meth, LogLevel.Error, "Failed to get connectionString param value");
                        }
                        else
                            Log(meth, LogLevel.Error, "Failed to get connectionString param");
                    }
                    else
                        Log(meth, LogLevel.Error, "Failed to get ConfigurationProvider");


                    this.State = RunState.FailedInitializing;
                }
            }
        }

        protected override void StartImpl()
        {
            lock (instance)
            {
                if (RuntimeUtils.Startable(this.State))
                {
                    string meth = "Start";
                    this.State = RunState.Starting;
                    Log(meth, LogLevel.Info, "Called");
                    this.State = RunState.Running;
                }
            }
        }

        protected override void StopImpl()
        {
            lock (instance)
            {
                if (RuntimeUtils.Stoppable(this.State))
                {
                    string meth = "Stop";
                    this.State = RunState.Stopping;
                    Log(meth, LogLevel.Info, "Called");
                    this.State = RunState.Stopped;
                }
            }
        }

        private void Log(string method, LogLevel level, string message)
        {
            if (this.logger != null)
                this.logger.Log(method, LogLevel.Info, message);
        }

        private UserAffilationSecurityManager()
        {
            SingletonHelper<UserAffilationSecurityManager> help = new SingletonHelper<UserAffilationSecurityManager>();
            help.Construct(this);
        }

        private static UserAffilationSecurityManager instance = new UserAffilationSecurityManager();
        public static UserAffilationSecurityManager Instance
        {
            get { return instance; }
        }
    }
}
