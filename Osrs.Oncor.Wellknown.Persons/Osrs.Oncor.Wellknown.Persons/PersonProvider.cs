using Npgsql;
using Osrs.Data;
using Osrs.Data.Postgres;
using Osrs.Security;
using Osrs.Security.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osrs.Oncor.Wellknown.Persons
{
    public sealed class PersonProvider
    {
		private static Permission PersonCreatePermission
		{
			get
			{
				return new Permission(PermissionUtils.PermissionName(OperationType.Create, "Person"), PersonUtils.PersonCreatePermissionId);
			}
		}
		private static Permission PersonGetPermission
		{
			get
			{
				return new Permission(PermissionUtils.PermissionName(OperationType.Retrive, "Person"), PersonUtils.PersonGetPermissionId);
			}
		}
		private static Permission PersonUpdatePermission
		{
			get
			{
				return new Permission(PermissionUtils.PermissionName(OperationType.Update, "Person"), PersonUtils.PersonUpdatePermissionId);
			}
		}
		private static Permission PersonDeletePermission
		{
			get
			{
				return new Permission(PermissionUtils.PermissionName(OperationType.Delete, "Person"), PersonUtils.PersonDeletePermissionId);
			}
		}

		private UserSecurityContext Context
		{
			get;
		}

		private IRoleProvider prov;
		private IRoleProvider AuthProvider
		{
			get
			{
				if (prov == null)
					prov = AuthorizationManager.Instance.GetRoleProvider(this.Context);
				return prov;
			}
		}

		public bool CanCreate()
		{
			IRoleProvider perms = this.AuthProvider;
			if (perms != null)
			{
				if (this.Context != null && this.Context.User != null)
				{
					return perms.HasPermission(this.Context.User, PersonCreatePermission);
				}
			}
			return false;
		}

		public bool CanGet()
		{
			IRoleProvider perms = this.AuthProvider;
			if (perms != null)
			{
				if (this.Context != null && this.Context.User != null)
				{
					return perms.HasPermission(this.Context.User, PersonGetPermission);
				}
			}
			return false;
		}

		public bool CanUpdate()
		{
			IRoleProvider perms = this.AuthProvider;
			if (perms != null)
			{
				if (this.Context != null && this.Context.User != null)
				{
					return perms.HasPermission(this.Context.User, PersonUpdatePermission);
				}
			}
			return false;
		}

		public bool CanDelete()
		{
			IRoleProvider perms = this.AuthProvider;
			if (perms != null)
			{
				if (this.Context != null && this.Context.User != null)
				{
					return perms.HasPermission(this.Context.User, PersonDeletePermission);
				}
			}
			return false;
		}

		public IEnumerable<Person> Get()
		{
			if (this.CanGet())
			{
				List<Person> personsWithContacts = new List<Person>();
				IEnumerable<Person> persons =  new Enumerable<Person>(new EnumerableCommand<Person>(PersonBuilder.Instance, Db.SelectPerson, Db.ConnectionString));
				foreach(Person p in persons)
				{
					getContactInfoForPerson(p);
					personsWithContacts.Add(p);
				}
				return personsWithContacts;
			}
			return null;
		}

		public IEnumerable<Person> Get(string lastName)
		{
			if (!string.IsNullOrEmpty(lastName) && this.CanGet())
			{
				NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
				cmd.CommandText = Db.SelectPerson + " WHERE \"LastName\"=:ln";
				cmd.Parameters.AddWithValue("ln", lastName);
				NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
				List<Person> persons = new List<Person>();
				try
				{
					Person p;
					while (rdr.Read())
					{
						p = PersonBuilder.Instance.Build(rdr);
						if (p != null)
						{
							getContactInfoForPerson(p);
							persons.Add(p);
						}
					}
					if (cmd.Connection.State == System.Data.ConnectionState.Open)
						cmd.Connection.Close();
				}
				catch
				{ }
				finally
				{
					cmd.Dispose();
				}
				return persons;
			}
			return null;
		}

		public IEnumerable<Person> Get(string lastName, string firstName)
		{
			if (!string.IsNullOrEmpty(lastName) && this.CanGet())
			{
				NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
				cmd.CommandText = Db.SelectPerson + " WHERE \"LastName\"=:ln AND \"FirstName\"=:fn";
				cmd.Parameters.AddWithValue("ln", lastName);
				cmd.Parameters.AddWithValue("fn", firstName);
				NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
				List<Person> persons = new List<Person>();
				try
				{
					Person p;
					while (rdr.Read())
					{
						p = PersonBuilder.Instance.Build(rdr);
						if (p != null)
						{
							getContactInfoForPerson(p);
							persons.Add(p);
						}
					}
					if (cmd.Connection.State == System.Data.ConnectionState.Open)
						cmd.Connection.Close();
				}
				catch
				{ }
				finally
				{
					cmd.Dispose();
				}
				return persons;
			}
			return null;
		}

		public IEnumerable<Person> Get(EmailAddress contactAddress)
		{
			if (contactAddress != null && this.CanGet())
			{
				NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
				cmd.CommandText = Db.SelectPerson + " WHERE \"Id\" IN (SELECT \"PersonId\" FROM oncor.\"PersonContactInfo\" WHERE \"EmailAddress\"=:email)";
				cmd.Parameters.AddWithValue("email", contactAddress.AddressText);
				NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
				List<Person> persons = new List<Person>();
				try
				{
					Person p;
					while(rdr.Read())
					{
						p = PersonBuilder.Instance.Build(rdr);
						if (p != null)
						{
							getContactInfoForPerson(p);
							persons.Add(p);
						}
					}
					if (cmd.Connection.State == System.Data.ConnectionState.Open)
						cmd.Connection.Close();
				}
				catch
				{ }
				finally
				{
					cmd.Dispose();
				}
				return persons;
			}
			return null;
		}

		public Person Get(CompoundIdentity id)
		{
			if (!id.IsNullOrEmpty() && this.CanGet())
			{
				NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
				cmd.CommandText = Db.SelectPerson + Db.SelectById;
				cmd.Parameters.AddWithValue("id", id.Identity);
				NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
				Person p = null;
				if (rdr != null)
				{
					try
					{
						rdr.Read();
						p = PersonBuilder.Instance.Build(rdr);
						if (p != null)
							getContactInfoForPerson(p);
						if (cmd.Connection.State == System.Data.ConnectionState.Open)
							cmd.Connection.Close();
					}
					catch
					{ }
					finally
					{
						cmd.Dispose();
					}
				}
				return p;
			}
			return null;
		}

		public Person Create(string firstName, string lastName)
		{
			if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName) && this.CanCreate())
			{
				try
				{
					NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
					cmd.CommandText = Db.InsertPerson;
					Guid id = Guid.NewGuid();
					cmd.Parameters.AddWithValue("id", id);
					cmd.Parameters.AddWithValue("fn", firstName);
					cmd.Parameters.AddWithValue("ln", lastName);
					Db.ExecuteNonQuery(cmd);
					return new Person(new CompoundIdentity(Db.DataStoreIdentity, id), firstName, lastName);
				}
				catch
				{ }
			}
			return null;
		}

		public bool Update(Person p)
		{
			if (p != null && this.CanUpdate())
			{
				try
				{
					NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
					cmd.CommandText = Db.UpdatePerson;
					cmd.Parameters.AddWithValue("id", p.Identity.Identity);
					cmd.Parameters.AddWithValue("fn", p.FirstName);
					cmd.Parameters.AddWithValue("ln", p.LastName);
					Db.ExecuteNonQuery(cmd);
					updateContactInfoForPerson(p);
					return true;
				}
				catch
				{ }
			}
			return false;
		}

		public bool Delete(Person p)
		{
			if (p != null && this.CanDelete())
			{
				try
				{
					NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
					cmd.CommandText = Db.DeletePerson;
					cmd.Parameters.AddWithValue("id", p.Identity.Identity);
					Db.ExecuteNonQuery(cmd);
					deleteContactInfoForPerson(p);
					return true;
				}
				catch
				{ }
			}
			return false;
		}

		private bool getContactInfoForPerson(Person p)
		{
			if (p != null && this.CanGet())
			{
				NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
				cmd.CommandText = Db.SelectPersonConatactInfoById;
				cmd.Parameters.AddWithValue("pid", p.Identity.Identity);
				NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
				try
				{
					while (rdr.Read())
					{
						string name = DbReaderUtils.GetString(rdr, 1);
						string email = DbReaderUtils.GetString(rdr, 2);
						if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email))
							p.Contacts.Add(name, new EmailAddress(email));
					}
					if (cmd.Connection.State == System.Data.ConnectionState.Open)
						cmd.Connection.Close();
					return true;
				}
				catch
				{ }
				finally
				{
					cmd.Dispose();
				}
			}
			return false;
		}

		private bool updateContactInfoForPerson(Person p)
		{
			if (p != null && this.CanUpdate())
			{
				deleteContactInfoForPerson(p);

				Dictionary<string, EmailAddress> emails = p.Contacts.Get();
				if (emails.Count > 0)
				{
					StringBuilder entries = new StringBuilder();
					foreach(KeyValuePair<string, EmailAddress> email in emails)
					{
						entries.Append("('");
						entries.Append(p.Identity.Identity.ToString());
						entries.Append("', '");
						entries.Append(email.Key);
						entries.Append("', '");
						entries.Append(email.Value.AddressText);
						entries.Append("'),");
					}
					entries.Remove(entries.Length - 1, 1);

					NpgsqlCommand insCmd = Db.GetCmd(Db.ConnectionString);
					insCmd.CommandText = "INSERT INTO oncor.\"PersonContactInfo\"(\"PersonId\", \"Name\", \"EmailAddress\") VALUES " + entries.ToString();
					Db.ExecuteNonQuery(insCmd);
					return true;
				}
			}
			return false;
		}

		private bool deleteContactInfoForPerson(Person p)
		{
			if (p != null && this.CanDelete())
			{
				NpgsqlCommand delCmd = Db.GetCmd(Db.ConnectionString);
				delCmd.CommandText = Db.DeletePersonContactInfoByPerson;
				delCmd.Parameters.AddWithValue("pid", p.Identity.Identity);
				Db.ExecuteNonQuery(delCmd);
				return true;
			}
			return false;
		}

		internal PersonProvider(UserSecurityContext context)
        {
			this.Context = context;
		}
    }
}
