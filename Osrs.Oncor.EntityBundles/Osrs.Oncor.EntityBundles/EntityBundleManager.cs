using Osrs.Data.Postgres;
using Osrs.Reflection;
using Osrs.Runtime;
using Osrs.Runtime.Configuration;
using Osrs.Runtime.Logging;
using Osrs.Security;
using System;

namespace Osrs.Oncor.EntityBundles
{
    public sealed class EntityBundleManager : ModuleBase
    {
        private static readonly LocalSystemUser user = new LocalSystemUser(new Guid("{99126DBA-9AF8-4602-BC20-B38CAB08283D}"), NameReflectionUtils.GetName(typeof(EntityBundleManager)), UserState.Active);
        private LogProviderBase logger;

        public EntityBundleProvider GetProvider()
        {
            if (this.State == RunState.Running)
                return new EntityBundleProvider(new UserSecurityContext(null));
            return null;
        }

        public EntityBundleProvider GetProvider(UserSecurityContext context)
        {
            if (this.State == RunState.Running)
                return new EntityBundleProvider(context);
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
                    this.logger = LogManager.Instance.GetProvider(typeof(EntityBundleManager));
                    Log(meth, LogLevel.Info, "Called");

                    ConfigurationProviderBase config = ConfigurationManager.Instance.GetProvider();
                    if (config != null)
                    {
                        ConfigurationParameter param = config.Get(typeof(EntityBundleProvider), "connectionString");
                        if (param != null)
                        {
                            string tName = param.Value as string;
                            if (!string.IsNullOrEmpty(tName))
                            {
                                if (NpgSqlCommandUtils.TestConnection(tName))
                                {
                                    Db.ConnectionString = tName;
                                    Log(meth, LogLevel.Info, "Succeeded");
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

        private EntityBundleManager()
        {
            SingletonHelper<EntityBundleManager> help = new SingletonHelper<EntityBundleManager>();
            help.Construct(this);
        }

        private static EntityBundleManager instance = new EntityBundleManager();
        public static EntityBundleManager Instance
        {
            get { return instance; }
        }
    }
}
