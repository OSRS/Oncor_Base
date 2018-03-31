using Osrs.Data;
using Osrs.Oncor.WellKnown.WaterQuality;
using Osrs.Oncor.WellKnown.WaterQuality.Module;
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

            WaterQualityManager.Instance.Initialize();
            Console.WriteLine("WaterQuality state: " + WaterQualityManager.Instance.State);
            WaterQualityManager.Instance.Start();
            Console.WriteLine("WaterQuality state: " + WaterQualityManager.Instance.State);

            if (WaterQualityManager.Instance.State == Osrs.Runtime.RunState.Running)
            {
                IWQDeploymentProvider depProv = WaterQualityManager.Instance.GetDeploymentProvider(context);
                if (depProv != null)
                {
                }
            }

            Console.WriteLine("ALL COMPLETE - Enter to exit");
            Console.ReadLine();
        }

        static void CreateDeps(IWQDeploymentProvider prov)
        {
            Guid sysId = new Guid("{5914629d-dd2d-4f1f-a06f-1b199fe19b37}");
            Guid pnlId = new Guid("{f9e1d49f-0b91-41cc-a88f-a24afa1a669e}");
            Guid aceId = new Guid("{6ca626ef-aab6-4746-8f18-7d97097055df}");

            CompoundIdentity pnnlId = new CompoundIdentity(sysId, pnlId);
            CompoundIdentity usaceId = new CompoundIdentity(sysId, aceId);
        }

        static void RegisterPermissions(UserSecurityContext context)
        {
            IPermissionProvider perms = AuthorizationManager.Instance.GetPermissionProvider(context);
            Permission p;

            p = WQModuleUtils.WQCreatePermission;
            if (!perms.Exists(p.Id))
            {
                Console.Write("Registering Permission: Create " + p.Name + " ");
                perms.RegisterPermission(p);
                Console.WriteLine(perms.Exists(p.Id));
            }

            p = WQModuleUtils.WQGetPermission;
            if (!perms.Exists(p.Id))
            {
                Console.Write("Registering Permission: Create " + p.Name + " ");
                perms.RegisterPermission(p);
                Console.WriteLine(perms.Exists(p.Id));
            }

            p = WQModuleUtils.WQDeletePermission;
            if (!perms.Exists(p.Id))
            {
                Console.Write("Registering Permission: Create " + p.Name + " ");
                perms.RegisterPermission(p);
                Console.WriteLine(perms.Exists(p.Id));
            }

            p = WQModuleUtils.WQUpdatePermission;
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

            p = WQModuleUtils.WQCreatePermission;
            Console.WriteLine("Granting Permission: " + p.Name);
            perms.AddToRole(r, p);

            p = WQModuleUtils.WQGetPermission;
            Console.WriteLine("Granting Permission: " + p.Name);
            perms.AddToRole(r, p);

            p = WQModuleUtils.WQDeletePermission;
            Console.WriteLine("Granting Permission: " + p.Name);
            perms.AddToRole(r, p);

            p = WQModuleUtils.WQUpdatePermission;
            Console.WriteLine("Granting Permission: " + p.Name);
            perms.AddToRole(r, p);
        }
    }
}
