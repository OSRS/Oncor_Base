using Osrs.Data;
using Osrs.Runtime.Configuration;
using Osrs.Runtime.Logging;
using Osrs.Security;
using Osrs.Security.Authorization;
using Osrs.Oncor.Wellknown.Persons;
using System;

namespace TestingApp
{
	class Program
	{
		static void Main(string[] args)
		{
			if (!Startup())
			{
				Exit();
				return; //quick exit
			}

			//setup our default user to use for all testing
			LocalSystemUser usr = new LocalSystemUser(SecurityUtils.AdminIdentity, "Admin", UserState.Active);
			UserSecurityContext context = new UserSecurityContext(usr);

			//Install(context); //this only needs to be run once, then commented out


			if (!StartLocal())
			{
				Exit();
				return; //quick exit
			}

			TestPersons(context);

			Console.ReadKey();
		}

		static void TestPersons(UserSecurityContext context)
		{
			PersonProvider prov = PersonManager.Instance.GetProvider(context);

			Console.WriteLine("Testing Persons");

			Person dev = prov.Create("Grant", "Fuji");
			if (dev != null)
				Console.WriteLine("Create Person: " + dev.FirstName + " " + dev.LastName);
			else
				Console.WriteLine("Failed to create Person");

			dev.LastName = "Fujimoto";
			bool updated = prov.Update(dev);
			if (updated)
				Console.WriteLine("Updated Person");
			else
				Console.WriteLine("Failed to update Person");

			var retrievedPerson = prov.Get(dev.Identity);
			if (retrievedPerson != null)
				Console.WriteLine("Get Person: " + retrievedPerson.FirstName + " " + retrievedPerson.LastName);
			else
				Console.WriteLine("Failed to retrieve Person");

			bool deleted = prov.Delete(dev);
			if (deleted)
				Console.WriteLine("Deleted Person");
			else
				Console.WriteLine("Failed to Delete Person");

			Console.WriteLine("Person Test Complete!");

			return;
		}

		static void RegisterPermissions(UserSecurityContext context)
		{
			IPermissionProvider perms = AuthorizationManager.Instance.GetPermissionProvider(context);
			Permission p;

			if (!perms.Exists(PersonUtils.PersonCreatePermissionId))
			{
				p = new Permission(PermissionUtils.PermissionName(OperationType.Create, "Person"), PersonUtils.PersonCreatePermissionId);
				Console.Write("Registering Permission: Create " + p.Name + " ");
				perms.RegisterPermission(p);
				Console.WriteLine(perms.Exists(PersonUtils.PersonCreatePermissionId));
			}
			if (!perms.Exists(PersonUtils.PersonGetPermissionId))
			{
				p = new Permission(PermissionUtils.PermissionName(OperationType.Retrive, "Person"), PersonUtils.PersonGetPermissionId);
				Console.Write("Registering Permission: Get " + p.Name + " ");
				perms.RegisterPermission(p);
				Console.WriteLine(perms.Exists(PersonUtils.PersonGetPermissionId));
			}
			if (!perms.Exists(PersonUtils.PersonUpdatePermissionId))
			{
				p = new Permission(PermissionUtils.PermissionName(OperationType.Update, "Person"), PersonUtils.PersonUpdatePermissionId);
				Console.Write("Registering Permission: Update " + p.Name + " ");
				perms.RegisterPermission(p);
				Console.WriteLine(perms.Exists(PersonUtils.PersonUpdatePermissionId));
			}
			if (!perms.Exists(PersonUtils.PersonDeletePermissionId))
			{
				p = new Permission(PermissionUtils.PermissionName(OperationType.Delete, "Person"), PersonUtils.PersonDeletePermissionId);
				Console.Write("Registering Permission: Delete " + p.Name + " ");
				perms.RegisterPermission(p);
				Console.WriteLine(perms.Exists(PersonUtils.PersonDeletePermissionId));
			}
		}

		static void Grant(UserSecurityContext context)
		{
			IRoleProvider perms = AuthorizationManager.Instance.GetRoleProvider(context);
			Permission p;
			Role r = perms.Get(SecurityUtils.AdminRole);

			p = new Permission(PermissionUtils.PermissionName(OperationType.Create, "Person"), PersonUtils.PersonCreatePermissionId);
			Console.WriteLine("Granting Permission: " + p.Name);
			perms.AddToRole(r, p);

			p = new Permission(PermissionUtils.PermissionName(OperationType.Retrive, "Person"), PersonUtils.PersonGetPermissionId);
			Console.WriteLine("Granting Permission: " + p.Name);
			perms.AddToRole(r, p);

			p = new Permission(PermissionUtils.PermissionName(OperationType.Update, "Person"), PersonUtils.PersonUpdatePermissionId);
			Console.WriteLine("Granting Permission: " + p.Name);
			perms.AddToRole(r, p);

			p = new Permission(PermissionUtils.PermissionName(OperationType.Delete, "Person"), PersonUtils.PersonDeletePermissionId);
			Console.WriteLine("Granting Permission: " + p.Name);
			perms.AddToRole(r, p);
		}

		static bool StartLocal()
		{
			PersonManager.Instance.Initialize();
			Console.WriteLine("IM state: " + PersonManager.Instance.State);
			PersonManager.Instance.Start();
			Console.WriteLine("IM state: " + PersonManager.Instance.State);
			if (PersonManager.Instance.State == Osrs.Runtime.RunState.Running)
				return true;
			return false;
		}

		static void Install(UserSecurityContext context)
		{
			if (AuthorizationManager.Instance.State == Osrs.Runtime.RunState.Running)
			{
				RegisterPermissions(context);
				Grant(context);
			}
		}

		static bool Startup()
		{
			ConfigurationManager.Instance.Bootstrap();
			ConfigurationManager.Instance.Initialize();
			ConfigurationManager.Instance.Start();
			Console.WriteLine("Config state: " + ConfigurationManager.Instance.State);
			if (ConfigurationManager.Instance.State != Osrs.Runtime.RunState.Running)
				return false;

			LogManager.Instance.Bootstrap();
			LogManager.Instance.Initialize();
			LogManager.Instance.Start();
			Console.WriteLine("Log state: " + LogManager.Instance.State);
			if (LogManager.Instance.State != Osrs.Runtime.RunState.Running)
				return false;

			AuthorizationManager.Instance.Bootstrap();
			AuthorizationManager.Instance.Initialize();
			AuthorizationManager.Instance.Start();
			Console.WriteLine("Auth state: " + AuthorizationManager.Instance.State);
			if (AuthorizationManager.Instance.State != Osrs.Runtime.RunState.Running)
				return false;

			return true;
		}

		static void Exit()
		{
			Console.WriteLine("ALL DONE");
			Console.ReadLine(); //prevents console from closing
		}
	}
}
