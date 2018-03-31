using Osrs.Data.Postgres;
using Osrs.Reflection;
using Osrs.Runtime;
using Osrs.Runtime.Configuration;
using Osrs.Runtime.Logging;
using Osrs.Security;
using System;

namespace Osrs.Oncor.Wellknown.Persons
{
    public sealed class PersonManager : ModuleBase
    {
        private static readonly LocalSystemUser user = new LocalSystemUser(new Guid("{2CA93C06-7C49-4872-81DA-3D280F2FCAE6}"), NameReflectionUtils.GetName(typeof(PersonManager)), UserState.Active);
        private LogProviderBase logger;

        public PersonProvider GetProvider()
        {
            if (this.State == RunState.Running)
                return new PersonProvider(new UserSecurityContext(null));
            return null;
        }
        public PersonProvider GetProvider(UserSecurityContext context)
        {
            if (this.State == RunState.Running)
                return new PersonProvider(context);
            return null;
        }

        protected override void InitializeImpl()
        {
            lock (instance)
            {
                if (RuntimeUtils.Initializable(this.State))
                {
                    string meth = "Initialize";
                    this.logger = LogManager.Instance.GetProvider(typeof(PersonManager));
                    Log(meth, LogLevel.Info, "Called");

                    ConfigurationProviderBase config = ConfigurationManager.Instance.GetProvider();
                    if (config != null)
                    {
                        ConfigurationParameter param = config.Get(typeof(PersonManager), "connectionString");
                        if (param != null)
                        {
                            string tName = param.Value as string;
                            if (!string.IsNullOrEmpty(tName))
                            {
                                if (NpgSqlCommandUtils.TestConnection(tName))
                                {
                                    Db.ConnectionString = tName;
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

        private PersonManager()
        {
            SingletonHelper<PersonManager> help = new SingletonHelper<PersonManager>();
            help.Construct(this);
        }

        private static PersonManager instance = new PersonManager();
        public static PersonManager Instance
        {
            get { return instance; }
        }
    }
}
