using Osrs.Runtime;
using Osrs.Runtime.Hosting;
using Osrs.Runtime.Logging;

namespace Pnnl.Oncor.Host
{
    public sealed class OncorHostShim : IHostedService
    {
        private static OncorServer svr;

        public RunState State
        {
            get;
            private set;
        } = RunState.Created;

        public OncorHostShim()
        {
            svr = OncorServer.Instance;
        }

        public void Initialize()
        {
            if (this.State == RunState.Created || this.State == RunState.FailedInitializing)
            {
                this.State = RunState.Initializing;
                LogProviderBase pp = LogManager.Instance.GetProvider(typeof(OncorHostShim));
                pp.Log(0, "Init: "+ svr.State.ToString());
                if (svr.Init())
                    this.State = RunState.Initialized;
                else
                    this.State = RunState.FailedInitializing;
            }
        }

        public void Pause()
        {
            this.Stop();
            if (this.State == RunState.Stopped)
                this.State = RunState.Paused;
        }

        public void Resume()
        {
            if (this.State == RunState.Paused)
            {
                this.State = RunState.Stopped;
                this.Start();
            }
        }

        public void Start()
        {
            if (this.State == RunState.Stopped || this.State == RunState.Initialized)
            {
                LogProviderBase pp = LogManager.Instance.GetProvider(typeof(OncorHostShim));
                pp.Log(0, "Start: " + svr.State.ToString());
                if (svr.Start())
                    this.State = RunState.Running;
                else
                    this.State = RunState.FailedStarting;
            }
        }

        public void Stop()
        {
            if (this.State == RunState.Running)
            {
                if (svr.Stop())
                    this.State = RunState.Stopped;
                else
                    this.State = RunState.FailedStopping;
            }
        }

        public void Shutdown()
        {
            this.Stop();
            this.State = RunState.Shutdown;
        }
    }
}
