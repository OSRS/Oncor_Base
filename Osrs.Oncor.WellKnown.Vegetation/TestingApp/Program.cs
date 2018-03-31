using Osrs.Numerics;
using Osrs.Oncor.WellKnown.Vegetation;
using Osrs.Oncor.WellKnown.Vegetation.Module;
using Osrs.Runtime.Configuration;
using Osrs.Runtime.Logging;
using Osrs.Security;
using Osrs.Security.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            VegetationManager.Instance.Initialize();
            Console.WriteLine("VegetationManager state: " + VegetationManager.Instance.State);
            VegetationManager.Instance.Start();
            Console.WriteLine("VegetationManager state: " + VegetationManager.Instance.State);

            if (VegetationManager.Instance.State == Osrs.Runtime.RunState.Running)
            {
                IVegSurveyProvider depProv = VegetationManager.Instance.GetSurveyProvider(context);
                if (depProv != null)
                {
                    IVegSampleProvider fProv = VegetationManager.Instance.GetSampleProvider(context);
                    if (fProv != null)
                    {
                        depProv.GetSurvey();
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

            p = VegModuleUtils.CreatePermission;
            if (!perms.Exists(p.Id))
            {
                Console.Write("Registering Permission: Create " + p.Name + " ");
                perms.RegisterPermission(p);
                Console.WriteLine(perms.Exists(p.Id));
            }

            p = VegModuleUtils.GetPermission;
            if (!perms.Exists(p.Id))
            {
                Console.Write("Registering Permission: Create " + p.Name + " ");
                perms.RegisterPermission(p);
                Console.WriteLine(perms.Exists(p.Id));
            }

            p = VegModuleUtils.DeletePermission;
            if (!perms.Exists(p.Id))
            {
                Console.Write("Registering Permission: Create " + p.Name + " ");
                perms.RegisterPermission(p);
                Console.WriteLine(perms.Exists(p.Id));
            }

            p = VegModuleUtils.UpdatePermission;
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

            p = VegModuleUtils.CreatePermission;
            Console.WriteLine("Granting Permission: " + p.Name);
            perms.AddToRole(r, p);

            p = VegModuleUtils.GetPermission;
            Console.WriteLine("Granting Permission: " + p.Name);
            perms.AddToRole(r, p);

            p = VegModuleUtils.DeletePermission;
            Console.WriteLine("Granting Permission: " + p.Name);
            perms.AddToRole(r, p);

            p = VegModuleUtils.UpdatePermission;
            Console.WriteLine("Granting Permission: " + p.Name);
            perms.AddToRole(r, p);
        }
    }
}
