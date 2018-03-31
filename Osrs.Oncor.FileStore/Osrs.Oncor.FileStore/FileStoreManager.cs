using Osrs.Data.Postgres;
using Osrs.Oncor.FileStore.Providers.Pg;
using Osrs.Runtime;
using Osrs.Runtime.Configuration;
using Osrs.Runtime.Logging;
using System.IO;

namespace Osrs.Oncor.FileStore
{
    public sealed class FileStoreManager : ModuleBase
    {
        internal static string basePath;
        private LogProviderBase logger;

        public IFileStoreProvider GetProvider()
        {
            if (this.State == RunState.Running)
                return new PgFileStoreProvider();
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
                    this.logger = LogManager.Instance.GetProvider(typeof(FileStoreManager));
                    Log(meth, LogLevel.Info, "Called");

                    ConfigurationProviderBase config = ConfigurationManager.Instance.GetProvider();
                    if (config != null)
                    {
                        ConfigurationParameter param = config.Get(typeof(FileStoreManager), "connectionString");
                        if (param != null)
                        {
                            string tName = param.Value as string;
                            if (!string.IsNullOrEmpty(tName))
                            {
                                if (NpgSqlCommandUtils.TestConnection(tName))
                                {
                                    Db.ConnectionString = tName;
                                    param = config.Get(typeof(FileStoreManager), "basePath");
                                    if (param != null)
                                    {
                                        tName = param.Value as string;
                                        if (!string.IsNullOrEmpty(tName))
                                        {
                                            if (Directory.Exists(tName))
                                            {
                                                FileStoreManager.basePath = tName;
                                                Log(meth, LogLevel.Info, "Succeeded");
                                                this.State = RunState.Initialized;
                                                return;
                                            }
                                            else
                                                Log(meth, LogLevel.Error, "BasePath directory does not exist: "+tName);
                                        }
                                        else
                                            Log(meth, LogLevel.Error, "Failed to get basePath param value");
                                    }
                                    else
                                        Log(meth, LogLevel.Error, "Failed to get basePath param");
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

        private FileStoreManager()
        {
            SingletonHelper<FileStoreManager> help = new SingletonHelper<FileStoreManager>();
            help.Construct(this);
        }

        private static FileStoreManager instance = new FileStoreManager();
        public static FileStoreManager Instance
        {
            get { return instance; }
        }
    }
}
