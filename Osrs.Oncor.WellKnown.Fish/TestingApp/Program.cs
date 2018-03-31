using Osrs.Oncor.WellKnown.Fish;
using Osrs.Oncor.WellKnown.Fish.Module;
using Osrs.Runtime.Configuration;
using Osrs.Runtime.Logging;
using Osrs.Security;
using Osrs.Security.Authorization;
using System;

namespace TestingApp
{
    class Program
    {
        static void Main(string[] args)
        {
            StockEstimates testing = new StockEstimates();
            testing["goo"] = 0.5F;
            testing["poo"] = 0F;
            testing["moo"] = 1F;
            foreach(StockEstimate cur in testing)
            {
                Console.WriteLine(cur.Stock + " " + cur.Probability);
            }


            ConfigurationManager.Instance.Bootstrap();
            ConfigurationManager.Instance.Initialize();
            ConfigurationManager.Instance.Start();
            Console.WriteLine("Config state: " + ConfigurationManager.Instance.State);

            LogManager.Instance.Bootstrap();
            LogManager.Instance.Initialize();
            LogManager.Instance.Start();
            Console.WriteLine("Log state: " + LogManager.Instance.State);

            AuthorizationManager.Instance.Bootstrap();
            AuthorizationManager.Instance.Initialize();
            AuthorizationManager.Instance.Start();
            Console.WriteLine("Auth state: " + AuthorizationManager.Instance.State);

            LocalSystemUser usr = new LocalSystemUser(SecurityUtils.AdminIdentity, "Admin", UserState.Active);
            UserSecurityContext context = new UserSecurityContext(usr);

            if (AuthorizationManager.Instance.State == Osrs.Runtime.RunState.Running)
            {
                //RegisterPermissions(context);
                //Grant(context);
            }

            FishManager.Instance.Initialize();
            Console.WriteLine("FishManager state: " + FishManager.Instance.State);
            FishManager.Instance.Start();
            Console.WriteLine("FishManager state: " + FishManager.Instance.State);

            if (FishManager.Instance.State == Osrs.Runtime.RunState.Running)
            {
                ICatchEffortProvider depProv = FishManager.Instance.GetCatchEffortProvider(context);
                if (depProv != null)
                {
                    IFishProvider fProv = FishManager.Instance.GetFishProvider(context);
                    if (fProv!=null)
                    {
                        depProv.Get();
                        Console.WriteLine("furray");
                    }
                }
            }

            Console.WriteLine("ALL COMPLETE - Enter to exit");
            Console.ReadLine();
        }

        static void RegisterPermissions(UserSecurityContext context)
        {
            IPermissionProvider perms = AuthorizationManager.Instance.GetPermissionProvider(context);
            Permission p;

            p = FishModuleUtils.FishCreatePermission;
            if (!perms.Exists(p.Id))
            {
                Console.Write("Registering Permission: Create " + p.Name + " ");
                perms.RegisterPermission(p);
                Console.WriteLine(perms.Exists(p.Id));
            }

            p = FishModuleUtils.FishGetPermission;
            if (!perms.Exists(p.Id))
            {
                Console.Write("Registering Permission: Create " + p.Name + " ");
                perms.RegisterPermission(p);
                Console.WriteLine(perms.Exists(p.Id));
            }

            p = FishModuleUtils.FishDeletePermission;
            if (!perms.Exists(p.Id))
            {
                Console.Write("Registering Permission: Create " + p.Name + " ");
                perms.RegisterPermission(p);
                Console.WriteLine(perms.Exists(p.Id));
            }

            p = FishModuleUtils.FishUpdatePermission;
            if (!perms.Exists(p.Id))
            {
                Console.Write("Registering Permission: Create " + p.Name + " ");
                perms.RegisterPermission(p);
                Console.WriteLine(perms.Exists(p.Id));
            }
        }

        static void Grant(UserSecurityContext context)
        {
            IRoleProvider perms = AuthorizationManager.Instance.GetRoleProvider(context);
            Permission p;
            Role r = perms.Get(SecurityUtils.AdminRole);

            p = FishModuleUtils.FishCreatePermission;
            Console.WriteLine("Granting Permission: " + p.Name);
            perms.AddToRole(r, p);

            p = FishModuleUtils.FishGetPermission;
            Console.WriteLine("Granting Permission: " + p.Name);
            perms.AddToRole(r, p);

            p = FishModuleUtils.FishDeletePermission;
            Console.WriteLine("Granting Permission: " + p.Name);
            perms.AddToRole(r, p);

            p = FishModuleUtils.FishUpdatePermission;
            Console.WriteLine("Granting Permission: " + p.Name);
            perms.AddToRole(r, p);
        }
    }
}
