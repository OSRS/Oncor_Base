using Osrs.Runtime;
using Osrs.Runtime.Logging;
using Osrs.Security;

namespace Pnnl.Oncor.DetProcessor
{
    public sealed class DetProcessorManager : ModuleBase
    {
        private LogProviderBase logger;

        public GeneralDetProcessor GetProvider(UserSecurityContext ctx)
        {
            if (this.State == RunState.Running && ctx !=null)
            {
                return new GeneralDetProcessor(ctx);
            }
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
                    this.logger = LogManager.Instance.GetProvider(typeof(DetProcessorManager));
                    Log(meth, LogLevel.Info, "Called");
                    if (DetRegistry.Instance.Init())
                    {
                        this.State = RunState.Initialized;
                        return;
                    }

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

        private DetProcessorManager()
        {
            SingletonHelper<DetProcessorManager> help = new SingletonHelper<DetProcessorManager>();
            help.Construct(this);
        }

        private static DetProcessorManager instance = new DetProcessorManager();
        public static DetProcessorManager Instance
        {
            get { return instance; }
        }
    }
}
