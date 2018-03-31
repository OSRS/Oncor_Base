using Osrs.Oncor.FileStore;

namespace Pnnl.Oncor.Host
{
    public sealed class OncorServer
    {
        public Osrs.Runtime.RunState State
        {
            get
            {
                if (HttpModules.Server != null)
                    return HttpModules.Server.State;
                return Osrs.Runtime.RunState.Unknown;
            }
        }

        public bool Stop()
        {
            if (HttpModules.Server != null)
            {
                if (scavengeTimer != null)
                {
                    scavengeTimer.Change(System.Threading.Timeout.Infinite, 60000); //just to be safe here
                    scavengeTimer.Dispose();
                    scavengeTimer = null;
                }
                HttpModules.Server.Stop();
                return HttpModules.Server.State == Osrs.Runtime.RunState.Stopped;
            }
            return true;
        }

        public bool Start()
        {
            if (Init())
            {
                if (HttpModules.Server != null)
                {
                    HttpModules.Server.Start();
                    if (HttpModules.Server.State == Osrs.Runtime.RunState.Running)
                        Scavenge(); //sets up the timer
                    return HttpModules.Server.State == Osrs.Runtime.RunState.Running;
                }
            }
            return false;
        }

        internal bool Init()
        {
            if (this.State == Osrs.Runtime.RunState.Unknown || this.State == Osrs.Runtime.RunState.Created || this.State == Osrs.Runtime.RunState.FailedInitializing)
            {
                if (RuntimeModules.InitConfigAndLog())
                {
                    if (RuntimeModules.InitSecurity())
                    {
                        if (RuntimeModules.Application())
                        {
                            return HttpModules.Initialize();
                        }
                    }
                }
                return false;
            }
            return true; //we're already initialized
        }

        private System.Threading.Timer scavengeTimer = null;
        private IFileStoreProvider prov = null;
        private void Scavenge()
        {
            prov = FileStoreManager.Instance.GetProvider();
            if (prov != null)
            {
                scavengeTimer = new System.Threading.Timer(this.DoScavenge, null, 0, 60000); //1 minute
            }
        }

        private void DoScavenge(object notUsed)
        {
            if (this.State == Osrs.Runtime.RunState.Running)
            {
                try
                {
                    prov.DeleteExpired();
                }
                catch { }
            }
            else if (scavengeTimer!=null)
            {
                try
                {
                    scavengeTimer.Change(System.Threading.Timeout.Infinite, 60000); //just to be safe here
                    scavengeTimer.Dispose();
                }
                catch { }
            }
        }

        public static OncorServer Instance
        {
            get;
        } = new OncorServer();

        private OncorServer()
        { }
    }
}
