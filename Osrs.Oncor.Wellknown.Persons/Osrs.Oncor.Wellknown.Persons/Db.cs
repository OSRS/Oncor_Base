using Npgsql;
using Osrs.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osrs.Oncor.Wellknown.Persons
{
    internal static class Db
    {
        internal static string ConnectionString;
        internal static readonly Guid DataStoreIdentity = new Guid("{CD720192-DC64-4CFF-AC5A-FB7154FBCD4B}");

		internal const string SelectById = " WHERE \"Id\"=:id";
		internal const string CountPerson = "SELECT COUNT(*) FROM oncor.\"Persons\"";
		internal const string SelectPerson = "SELECT \"Id\", \"FirstName\", \"LastName\" FROM oncor.\"Persons\"";
		internal const string InsertPerson = "INSERT INTO oncor.\"Persons\"(\"Id\", \"FirstName\", \"LastName\") VALUES (:id, :fn, :ln)";
		internal const string UpdatePerson = "UPDATE oncor.\"Persons\" SET \"Id\"=:id, \"FirstName\"=:fn, \"LastName\"=:ln";
		internal const string DeletePerson = "DELETE FROM oncor.\"Persons\" WHERE \"Id\"=:id";

		internal const string SelectPersonConatactInfoById = "SELECT \"PersonId\", \"Name\", \"EmailAddress\" FROM oncor.\"PersonContactInfo\" WHERE \"PersonId\"=:pid";
		internal const string InsertPersonContactInfo = "INSERT INTO oncor.\"PersonContactInfo\"(\"PersonId\", \"Name\", \"EmailAddress\") VALUES (:pid, :name, :email)";
		internal const string DeletePersonContactInfoByPerson = "DELETE FROM oncor.\"PersonContactInfo\" WHERE \"PersonId\"=:pid";

		internal static NpgsqlConnection GetCon(string conString)
		{
			try
			{
				NpgsqlConnectionStringBuilder sb = new NpgsqlConnectionStringBuilder(conString);
				if (sb.Timeout == 15) //default
					sb.Timeout = 60;
				if (sb.CommandTimeout == 30) //default
					sb.CommandTimeout = 60;
				sb.Pooling = false;
				NpgsqlConnection conn = new NpgsqlConnection(sb.ToString());
				return conn;
			}
			catch
			{ }
			return null;
		}

		internal static NpgsqlCommand GetCmd(NpgsqlConnection con)
		{
			if (con == null)
				return null;
			try
			{
				NpgsqlCommand cmd = new NpgsqlCommand();
				if (cmd != null)
				{
					cmd.Connection = con;
					return cmd;
				}
			}
			catch
			{ }
			return null;
		}

		internal static NpgsqlCommand GetCmd(string conString)
		{
			try
			{
				NpgsqlCommand cmd = new NpgsqlCommand();
				cmd.Connection = GetCon(conString);
				return cmd;
			}
			catch
			{ }
			return null;
		}

		internal static int ExecuteNonQuery(NpgsqlCommand cmd)
		{
			int res = int.MinValue;
			try
			{
				if (cmd.Connection.State != ConnectionState.Open)
					cmd.Connection.Open();
				res = cmd.ExecuteNonQuery();
			}
			catch (Exception e)
			{ }

			try
			{
				if (cmd.Connection.State == ConnectionState.Open)
					cmd.Connection.Close();
			}
			catch
			{ }

			return res;
		}

		internal static NpgsqlDataReader ExecuteReader(NpgsqlCommand cmd)
		{
			try
			{
				if (cmd.Connection.State != ConnectionState.Open)
					cmd.Connection.Open();
				return cmd.ExecuteReader();
			}
			catch
			{ }
			return null;
		}

		internal static void Close(NpgsqlConnection con)
		{
			try
			{
				if (con != null && con.State == ConnectionState.Open)
					con.Close();
			}
			catch
			{ }
		}

		internal static void Close(NpgsqlCommand cmd)
		{
			if (cmd != null && cmd.Connection != null)
				Close(cmd.Connection);
		}

		internal static bool Exists(NpgsqlCommand cmd)
		{
			NpgsqlDataReader rdr = null;
			try
			{
				if (cmd.Connection.State != ConnectionState.Open)
					cmd.Connection.Open();
				rdr = ExecuteReader(cmd);
				rdr.Read();

				try
				{
					long ct = (long)(rdr[0]);
					if (cmd.Connection.State == System.Data.ConnectionState.Open)
						cmd.Connection.Close();

					return ct > 0L;
				}
				catch
				{ }
			}
			catch
			{
				Close(cmd);
			}
			finally
			{
				cmd.Dispose();
			}
			return false;
		}
	}

	internal sealed class PersonBuilder : IBuilder<Person>
	{
		internal static readonly PersonBuilder Instance = new PersonBuilder();
		public Person Build(DbDataReader reader)
		{
			return new Person(new CompoundIdentity(Db.DataStoreIdentity, DbReaderUtils.GetGuid(reader, 0)), DbReaderUtils.GetString(reader, 1),
				DbReaderUtils.GetString(reader, 2));
		}
	}
}
