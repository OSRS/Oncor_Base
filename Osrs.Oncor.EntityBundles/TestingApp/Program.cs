using Osrs.Oncor.EntityBundles;
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

            EntityBundleManager.Instance.Initialize();
            Console.WriteLine("EntityBundles state: " + EntityBundleManager.Instance.State);
            EntityBundleManager.Instance.Start();
            Console.WriteLine("EntityBundles state: " + EntityBundleManager.Instance.State);

            if (EntityBundleManager.Instance.State == Osrs.Runtime.RunState.Running)
            {
                //do testing
            }

            Console.WriteLine("ALL COMPLETE - Enter to exit");
            Console.ReadLine();
        }

        static void RegisterPermissions(UserSecurityContext context)
        {
            IPermissionProvider perms = AuthorizationManager.Instance.GetPermissionProvider(context);
            Permission p;

            if (!perms.Exists(EntityBundleUtils.EntityBundleCreatePermissionId))
            {
                p = EntityBundleUtils.EntityBundleCreatePermission;
                Console.Write("Registering Permission: Create " + p.Name + " ");
                perms.RegisterPermission(p);
                Console.WriteLine(perms.Exists(EntityBundleUtils.EntityBundleCreatePermissionId));
            }

            if (!perms.Exists(EntityBundleUtils.EntityBundleDeletePermissionId))
            {
                p = EntityBundleUtils.EntityBundleDeletePermission;
                Console.Write("Registering Permission: Delete " + p.Name + " ");
                perms.RegisterPermission(p);
                Console.WriteLine(perms.Exists(EntityBundleUtils.EntityBundleDeletePermissionId));
            }

            if (!perms.Exists(EntityBundleUtils.EntityBundleGetPermissionId))
            {
                p = EntityBundleUtils.EntityBundleGetPermission;
                Console.Write("Registering Permission: Get " + p.Name + " ");
                perms.RegisterPermission(p);
                Console.WriteLine(perms.Exists(EntityBundleUtils.EntityBundleGetPermissionId));
            }

            if (!perms.Exists(EntityBundleUtils.EntityBundleUpdatePermissionId))
            {
                p = EntityBundleUtils.EntityBundleUpdatePermission;
                Console.Write("Registering Permission: Update " + p.Name + " ");
                perms.RegisterPermission(p);
                Console.WriteLine(perms.Exists(EntityBundleUtils.EntityBundleUpdatePermissionId));
            }
        }

        static void Grant(UserSecurityContext context)
        {
            IRoleProvider perms = AuthorizationManager.Instance.GetRoleProvider(context);
            Permission p;
            Role r = perms.Get(SecurityUtils.AdminRole);

            p = EntityBundleUtils.EntityBundleCreatePermission;
            Console.WriteLine("Granting Permission: " + p.Name);
            perms.AddToRole(r, p);

            p = EntityBundleUtils.EntityBundleDeletePermission;
            Console.WriteLine("Granting Permission: " + p.Name);
            perms.AddToRole(r, p);

            p = EntityBundleUtils.EntityBundleGetPermission;
            Console.WriteLine("Granting Permission: " + p.Name);
            perms.AddToRole(r, p);

            p = EntityBundleUtils.EntityBundleUpdatePermission;
            Console.WriteLine("Granting Permission: " + p.Name);
            perms.AddToRole(r, p);
        }
    }
}
