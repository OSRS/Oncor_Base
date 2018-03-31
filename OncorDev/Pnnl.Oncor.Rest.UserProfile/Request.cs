using Newtonsoft.Json.Linq;
using Npgsql;
using Osrs;
using Osrs.Data;
using Osrs.Data.Postgres;
using Osrs.Net.Http;
using Osrs.Net.Http.Handlers;
using Osrs.Runtime.Configuration;
using Osrs.Security;
using Osrs.Security.Sessions;
using Osrs.Threading;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Pnnl.Oncor.Rest.UserProfile
{
	public sealed class Request : HttpHandlerBase, IServiceHandler
	{

		private SessionProviderBase sessionProvider;
		private SessionProviderBase SessionProvider
		{
			get
			{
				if (sessionProvider == null)
					sessionProvider = SessionManager.Instance.GetProvider();
				return sessionProvider;
			}
		}

		ConfigurationProviderBase configProvider;
		ConfigurationProviderBase ConfigProvider
		{
			get
			{
				if (configProvider == null)
					configProvider = ConfigurationManager.Instance.GetProvider();
				return configProvider;
			}
		}
		

		private ConfigurationParameter connectionString;
		private ConfigurationParameter ConnectionString
		{
			get
			{
				if (connectionString == null)
					connectionString = ConfigProvider.Get(typeof(Request),"connectionString");
				return connectionString;
			}
		}

		public string BaseUrl
		{
			get
			{
				return "user";
			}
		}

		public override void Handle(HttpContext context, CancellationToken cancel)
		{
			if (context != null)
			{
				UserIdentityBase user = Security.Session.GetUser(context);
				if (user != null)
				{
					UserSecurityContext ctx = new UserSecurityContext(user);
					string localUrl = RestUtils.LocalUrl(this, context.Request);
					string meth = RestUtils.StripLocal(this.BaseUrl, localUrl);
					meth = meth.Substring(1);

					if (!string.IsNullOrEmpty(meth))
					{
						if (context.Request.Method == "POST")
						{
							if (meth.Equals("request", StringComparison.OrdinalIgnoreCase))
							{
								try
								{
									HashSet<CompoundIdentity> affilsRead = null;
									HashSet<CompoundIdentity> affilsWrite = null;
									HashSet<string> rolesRead = null;
									HashSet<string> rolesWrite = null;
									string reason = null;
									JToken token = JsonUtils.GetDataPayload(context.Request);
									if (token != null)
									{
										if (token["affils"] != null)
										{
											JToken affils = token["affils"];
											if (affils["read"] != null)
												affilsRead = JsonUtils.ToIds(affils["read"]);
											if (affils["write"] != null)
												affilsWrite = JsonUtils.ToIds(affils["write"]);
										}
										if (token["roles"] != null)
										{
											JToken roles = token["roles"];
											if (roles["read"] != null)
												rolesRead = ToStrings(roles["read"] as JArray);
											if (roles["write"] != null)
												rolesWrite = ToStrings(roles["write"] as JArray);
										}
										if (token["reason"] != null)
										{
											reason = token["reason"].ToString();
										}
									}
									Guid currentUser = ctx.User.Uid;
									if (currentUser != null)
									{
										if (affilsRead != null)
										{
											foreach (CompoundIdentity id in affilsRead)
											{
												if (!id.IsNullOrEmpty())
													CreateAffil(currentUser, id, false, reason);
											}
										}
										if (affilsWrite != null)
										{
											foreach (CompoundIdentity id in affilsWrite)
											{
												if (!id.IsNullOrEmpty())
													CreateAffil(currentUser, id, true, reason);
											}
										}
										if (rolesRead != null)
										{
											foreach (string role in rolesRead)
											{
												if (!string.IsNullOrEmpty(role))
													CreateRole(currentUser, role, reason);
											}
										}
										if (rolesWrite != null)
										{
											foreach (string role in rolesWrite)
											{
												if (!string.IsNullOrEmpty(role))
													CreateRole(currentUser, role, reason);
											}
										}
									}
									RestUtils.Push(context.Response, JsonOpStatus.Ok);
								}
								catch
								{
									RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
									return;
								}
							}
						}
					}
				}
			}
			context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
		}

		internal const string InsertRequestAffil = "INSERT INTO oncor.\"RequestAffiliation\"(\"UserId\", \"OrgSystemId\", \"OrgId\", \"WriteAccess\", \"Reason\") VALUES (:uid, :osid, :oid, :rw, :reas)";
		internal const string InsertRequestRole = "INSERT INTO oncor.\"RequestRole\"(\"UserId\", \"RoleName\", \"Reason\") VALUES (:uid, :rn, :reas)";

		private void CreateAffil(Guid userId, CompoundIdentity affil, bool writeAccess, string reason)
		{
			NpgsqlCommand cmd = GetCmd(this.ConnectionString.Value.ToString());
			cmd.CommandText = InsertRequestAffil;
			AddParam(cmd, "uid", userId);
			AddParam(cmd, "osid", affil.DataStoreIdentity);
			AddParam(cmd, "oid", affil.Identity);
			AddParam(cmd, "rw", writeAccess);
			AddParam(cmd, "reas", reason);
			ExecuteNonQuery(cmd);
		}

		private void CreateRole(Guid userId, string roleName, string reason)
		{
			NpgsqlCommand cmd = GetCmd(this.ConnectionString.Value.ToString());
			cmd.CommandText = InsertRequestRole;
			AddParam(cmd, "uid", userId);
			AddParam(cmd, "rn", roleName);
			AddParam(cmd, "reas", reason);
			ExecuteNonQuery(cmd);
		}

		private static HashSet<string> ToStrings(JArray dataPayload)
		{
			if (dataPayload != null)
			{
				try
				{
					HashSet<string> strings = new HashSet<string>();
					string item;
					foreach (JToken cur in dataPayload)
					{
						item = cur.ToString();
						if (!string.IsNullOrEmpty(item))
							strings.Add(item);
					}
					return strings;
				}
				catch
				{ }
			}
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
			catch
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

		internal static void AddParam(NpgsqlCommand cmd, string name, Guid val)
		{
			if (Guid.Empty.Equals(val))
				cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(name, NpgsqlTypes.NpgsqlDbType.Uuid));
			else
				cmd.Parameters.AddWithValue(name, val);
		}

		internal static void AddParam(NpgsqlCommand cmd, string name, bool? val)
		{
			if (val.HasValue)
				cmd.Parameters.AddWithValue(name, val);
			else
				cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(name, NpgsqlTypes.NpgsqlDbType.Boolean));
		}

		internal static void AddParam(NpgsqlCommand cmd, string name, string val)
		{
			if (string.IsNullOrEmpty(val))
				cmd.Parameters.Add(NpgSqlCommandUtils.GetNullInParam(name, NpgsqlTypes.NpgsqlDbType.Varchar));
			else
				cmd.Parameters.AddWithValue(name, val);
		}
	}
}
