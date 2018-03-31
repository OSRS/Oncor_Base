using Npgsql;
using Osrs.Data;
using Osrs.Data.Postgres;
using Osrs.Security;
using Osrs.Security.Authorization;
using System;
using System.Collections.Generic;
using System.Text;

namespace Osrs.Oncor.EntityBundles
{
    //Security for Entity Bundles
    //  CRUD permissions will exist and be enforced at the table level (not record level)
    //  Update/Delete permission will ensure user affiliation with owner org
    //intent is to allow all users to use any bundle, but not be able to modify ones they don't have affiliation with
    public sealed class EntityBundleProvider
    {
        public bool CanGet()
        {
            IRoleProvider perms = this.AuthProvider;
            if (perms != null)
            {
                if (this.Context != null && this.Context.User != null)
                {
                    return perms.HasPermission(this.Context.User, EntityBundleUtils.EntityBundleGetPermission);
                }
            }
            return false;
        }

        public bool CanGet(EntityBundle bundle)
        {
            if (bundle!=null && this.CanGet())
            {
                return true; //TODO -- implement this
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
                    return perms.HasPermission(this.Context.User, EntityBundleUtils.EntityBundleUpdatePermission);
                }
            }
            return false;
        }

        public bool CanUpdate(EntityBundle bundle)
        {
            if (bundle != null && this.CanUpdate())
            {
                return true; //TODO -- implement this
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
                    return perms.HasPermission(this.Context.User, EntityBundleUtils.EntityBundleDeletePermission);
                }
            }
            return false;
        }

        public bool CanDelete(EntityBundle bundle)
        {
            if (bundle != null && this.CanDelete())
            {
                return true; //TODO -- implement this
            }
            return false;
        }

        public bool CanCreate()
        {
            IRoleProvider perms = this.AuthProvider;
            if (perms != null)
            {
                if (this.Context != null && this.Context.User != null)
                {
                    return perms.HasPermission(this.Context.User, EntityBundleUtils.EntityBundleCreatePermission);
                }
            }
            return false;
        }

        public bool CanCreate(EntityBundle bundle)
        {
            if (bundle != null && this.CanCreate())
            {
                return true; //TODO -- implement this
            }
            return false;
        }


        public bool Exists(Guid id)
        {
            return this.Get(id) != null;
        }

        public bool Exists(string name)
        {
            return this.Get(name) != null;
        }

        public bool Exists(string name, StringComparison comparisonOption)
        {
            return this.Get(name, comparisonOption) != null;
        }

        public bool ExistsForOrg(CompoundIdentity principalOrgId)
        {
            IEnumerable<EntityBundle> items = this.GetForOrg(principalOrgId);
            if (items!=null)
            {
                foreach (EntityBundle cur in items)
                    return true; //if we're here, we got at least 1
            }
            return false;
        }

        public IEnumerable<EntityBundle> Get()
        {
            if (this.CanGet())
                return new Enumerable<EntityBundle>(new EnumerableCommand<EntityBundle>(EntityBundleBuilder.Instance, Db.SelectBundle, Db.ConnectionString));
            return null;
        }

        public IEnumerable<EntityBundle> Get(string name)
        {
            return this.Get(name, StringComparison.OrdinalIgnoreCase);
        }

        public IEnumerable<EntityBundle> Get(string name, StringComparison comparisonOption)
        {
            if (!string.IsNullOrEmpty(name) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                string where;
                if (comparisonOption == StringComparison.CurrentCultureIgnoreCase || comparisonOption == StringComparison.OrdinalIgnoreCase)
                    where = " WHERE lower(\"Name\")=lower(:name)";
                else
                    where = " WHERE \"Name\"=:name";
                cmd.CommandText = Db.SelectBundle + where;
                cmd.Parameters.AddWithValue("name", name);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                List<EntityBundle> items = new List<EntityBundle>();
                try
                {
                    EntityBundle o;
                    while (rdr.Read())
                    {
                        o = EntityBundleBuilder.Instance.Build(rdr);
                        if (o != null)
                            items.Add(o);
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

                return items;
            }
            return null;
        }

        public EntityBundle Get(Guid id)
        {
            if (!Guid.Empty.Equals(id) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectBundle + Db.SelectById;
                cmd.Parameters.AddWithValue("id", id);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                EntityBundle o = null;
                if (rdr != null)
                {
                    try
                    {
                        if (rdr.Read())
                            o = EntityBundleBuilder.Instance.Build(rdr);

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
                return o;
            }
            return null;
        }

        public IEnumerable<EntityBundle> Get(IEnumerable<Guid> ids)
        {
            if (ids!=null && this.CanGet())
            {
                HashSet<Guid> uIds = new HashSet<Guid>();
                foreach(Guid cur in ids)
                {
                    if (!Guid.Empty.Equals(cur))
                        uIds.Add(cur);
                }

                if (uIds.Count>0)
                {
                    StringBuilder where = new StringBuilder(" WHERE \"Id\" IN (");
                    foreach (Guid cur in uIds)
                    {
                            where.Append('\'');
                            where.Append(cur.ToString());
                            where.Append("',");
                    }
                    where[where.Length - 1] = ')';


                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectBundle + where.ToString();
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    List<EntityBundle> items = new List<EntityBundle>();
                    try
                    {
                        EntityBundle o;
                        while (rdr.Read())
                        {
                            o = EntityBundleBuilder.Instance.Build(rdr);
                            if (o != null)
                                items.Add(o);
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

                    return items;
                }
            }
            return null;
        }

        public IEnumerable<EntityBundle> GetForOrg(CompoundIdentity principalOrgId)
        {
            if (principalOrgId != null && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectBundle + Db.SelectByPrinId;
                cmd.Parameters.AddWithValue("osid", principalOrgId.DataStoreIdentity);
                cmd.Parameters.AddWithValue("oid", principalOrgId.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                List<EntityBundle> items = new List<EntityBundle>();
                try
                {
                    EntityBundle o;
                    while (rdr.Read())
                    {
                        o = EntityBundleBuilder.Instance.Build(rdr);
                        if (o != null)
                            items.Add(o);
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
                return items;
            }
            return null;
        }

        public bool Update(EntityBundle item)
        {
            if (item != null && this.CanUpdate(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.UpdateBundle;
                    cmd.Parameters.AddWithValue("id", item.Id);
                    cmd.Parameters.AddWithValue("name", item.Name);
                    cmd.Parameters.AddWithValue("osid", item.PrincipalOrgId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("oid", item.PrincipalOrgId.Identity);

                    Db.ExecuteNonQuery(cmd);

                    return this.UpdateElements(item);
                }
                catch
                { }
            }
            return false;
        }

        private bool UpdateElements(EntityBundle item)
        {
            if (item.elementsDirty)
            {
                if (this.DeleteElements(item.Id))
                {
                    try
                    {
                        NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                        cmd.CommandText = Db.InsertElement;

                        foreach (BundleElement cur in item.Elements)
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("id", cur.BundleId);
                            cmd.Parameters.AddWithValue("esid", cur.EntityId.DataStoreIdentity);
                            cmd.Parameters.AddWithValue("eid", cur.EntityId.Identity);
                            cmd.Parameters.AddWithValue("name", cur.LocalKey);
                            cmd.Parameters.AddWithValue("val", cur.DisplayName);

                            Db.ExecuteNonQuery(cmd);
                        }

                        item.elementsDirty = false;
                        return true;
                    }
                    catch
                    { }
                }
            }
            return false;
        }

        private bool DeleteElements(Guid id)
        {
            try
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.DeleteElement;
                cmd.Parameters.AddWithValue("id", id);
                Db.ExecuteNonQuery(cmd);

                return true;
            }
            catch
            { }
            return false;
        }

        public bool Delete(EntityBundle item)
        {
            if (item != null && this.CanDelete(item))
                return this.Delete(item.Id);
            return false;
        }

        public bool Delete(Guid id)
        {
            if (!Guid.Empty.Equals(id) && this.CanDelete())
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.DeleteBundle + Db.SelectById;
                    cmd.Parameters.AddWithValue("id", id);
                    Db.ExecuteNonQuery(cmd);

                    this.DeleteElements(id);
                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public EntityBundle Create(string name, CompoundIdentity principalOrgId, BundleDataType dataType)
        {
            if (!string.IsNullOrEmpty(name) && !principalOrgId.IsNullOrEmpty() && this.CanCreate())
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertBundle;
                    Guid id = Guid.NewGuid();
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("name", name);
                    cmd.Parameters.AddWithValue("osid", principalOrgId.DataStoreIdentity);
                    cmd.Parameters.AddWithValue("oid", principalOrgId.Identity);
                    cmd.Parameters.AddWithValue("dt", (int)dataType);

                    Db.ExecuteNonQuery(cmd);

                    return new EntityBundle(id, name, principalOrgId, dataType);
                }
                catch
                { }
            }
            return null;
        }

        private readonly UserSecurityContext context;
        private UserSecurityContext Context
        {
            get { return this.context; }
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

        internal EntityBundleProvider(UserSecurityContext context)
        {
            this.context = context;
        }
    }
}
