//Copyright 2017 Open Science, Engineering, Research and Development Information Systems Open, LLC. (OSRS Open)
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//       http://www.apache.org/licenses/LICENSE-2.0
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Collections.Generic;
using Osrs.Data;
using Osrs.Security.Authorization;
using Osrs.Security;
using Osrs.Oncor.WellKnown.Fish.Module;
using Osrs.Data.Postgres;
using Npgsql;

namespace Osrs.Oncor.WellKnown.Fish.Providers.Pg
{
    public sealed class PgFishProvider : IFishProvider
    {
        private readonly FishBuilder fishBuilder;
        private readonly FishDietBuilder dietBuilder;
        private readonly FishGeneticsBuilder genBuilder;
        private readonly FishIdTagBuilder idBuilder;
        private PgCatchEffortProvider helperProvider = null;
        private Guid lastFishId = Guid.Empty;
        private bool lastEditFishPerm;

        public bool CanCreate()
        {
            IRoleProvider perms = this.AuthProvider;
            if (perms != null)
            {
                if (this.Context != null && this.Context.User != null)
                {
                    return perms.HasPermission(this.Context.User, FishModuleUtils.FishCreatePermission);
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
                    return perms.HasPermission(this.Context.User, FishModuleUtils.FishDeletePermission);
                }
            }
            return false;
        }

        public bool CanDelete(FishGenetics item)
        {
            if (item != null)
                return this.CanDelete(item.FishId);
            return false;
        }

        public bool CanDelete(FishIdTag item)
        {
            if (item != null)
                return this.CanDelete(item.FishId);
            return false;
        }

        public bool CanDelete(FishDiet item)
        {
            if (item != null)
                return this.CanDelete(item.FishId);
            return false;
        }

        public bool CanDelete(Fish item)
        {
            if (item != null)
            {
                if (this.helperProvider == null)
                    this.helperProvider = new PgCatchEffortProvider(this.Context);

                return this.helperProvider.CanDelete(item.CatchEffortId);
            }
            return false;
        }

        private bool CanDelete(Guid fishId)
        {
            if (!Guid.Empty.Equals(fishId) && this.CanDelete())
            {
                if (!fishId.Equals(lastFishId))
                {
                    this.lastFishId = fishId;
                    Fish f = this.GetFish(fishId);
                    if (f!=null)
                        this.lastEditFishPerm = this.CanDelete(f);
                    else
                        this.lastEditFishPerm = false;
                }

                return this.lastEditFishPerm; //same as last check
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
                    return perms.HasPermission(this.Context.User, FishModuleUtils.FishGetPermission);
                }
            }
            return false;
        }

        public bool Delete(FishIdTag item)
        {
            if (item != null && this.CanDelete(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.DeleteFishIdTag + Db.WhereId;
                    cmd.Parameters.AddWithValue("id", item.Identity);
                    Db.ExecuteNonQuery(cmd);

                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public bool Delete(FishDiet item)
        {
            if (item != null && this.CanDelete(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.DeleteFishDiet + Db.WhereId;
                    cmd.Parameters.AddWithValue("id", item.Identity);
                    Db.ExecuteNonQuery(cmd);

                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public bool Delete(FishGenetics item)
        {
            if (item != null && this.CanDelete(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.DeleteFishGenetics + Db.WhereId;
                    cmd.Parameters.AddWithValue("id", item.Identity);
                    Db.ExecuteNonQuery(cmd);

                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public bool Delete(Fish item)
        {
            if (item != null && this.CanDelete(item))
            {
                try
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.Parameters.AddWithValue("fid", item.Identity); //this is the fishid on all sub-items
                    
                    //--items sub to fish do a where fishid in (select)
                    cmd.CommandText = Db.DeleteFishDiet + Db.WhereFishId;
                    Db.ExecuteNonQuery(cmd);

                    cmd.CommandText = Db.DeleteFishGenetics + Db.WhereFishId;
                    Db.ExecuteNonQuery(cmd);

                    cmd.CommandText = Db.DeleteFishIdTag + Db.WhereFishId;
                    Db.ExecuteNonQuery(cmd);
                    //--end all sub-items, now it's ok to remove the fish itself

                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("id", item.Identity);
                    cmd.CommandText = Db.DeleteFishIndividual + Db.WhereId;
                    Db.ExecuteNonQuery(cmd);
                    return true;
                }
                catch
                { }
            }
            return false;
        }

        public IEnumerable<Fish> GetFish()
        {
            if (this.CanGet())
                return new Enumerable<Fish>(new EnumerableCommand<Fish>(this.fishBuilder, Db.SelectFishIndividual, Db.ConnectionString));
            return null;
        }

        public Fish GetFish(Guid fishId)
        {
            if (!Guid.Empty.Equals(fishId) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishIndividual + Db.Where + Db.WhereId;
                cmd.Parameters.AddWithValue("id", fishId);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                Fish o = null;
                if (rdr != null)
                {
                    try
                    {
                        if (rdr.Read())
                        {
                            o = this.fishBuilder.Build(rdr);
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
                }
                return o;
            }
            return null;
        }

        public IEnumerable<Fish> GetFish(IEnumerable<Guid> fishId)
        {
            if (fishId!=null && this.CanGet())
            {
                string wh = Db.GetWhere(fishId, "\"Id\"");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectFishIndividual + Db.Where + wh;
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    Fish o = null;
                    List<Fish> items = new List<Fish>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.fishBuilder.Build(rdr);
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
                    }
                    return items;
                }
            }
            return null;
        }

        public IEnumerable<Fish> GetFish(IEnumerable<CompoundIdentity> catchEffortId)
        {
            if (catchEffortId != null && this.CanGet())
            {
                string wh = Db.GetWhere(catchEffortId, "\"CatchEffortId\"");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectFishIndividual + Db.Where + wh;
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    Fish o = null;
                    List<Fish> items = new List<Fish>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.fishBuilder.Build(rdr);
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
                    }
                    return items;
                }
            }
            return null;
        }

        public IEnumerable<Fish> GetFish(CompoundIdentity catchEffortId)
        {
            if (!catchEffortId.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(catchEffortId.DataStoreIdentity) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishIndividual + Db.Where + Db.WhereEffort;
                cmd.Parameters.AddWithValue("ceid", catchEffortId.Identity);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                Fish o = null;
                List<Fish> permissions = new List<Fish>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.fishBuilder.Build(rdr);
                            if (o != null)
                                permissions.Add(o);
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
                }
                return permissions;
            }
            return null;
        }

        public IEnumerable<Fish> GetFish(IEnumerable<CompoundIdentity> catchEffortId, CompoundIdentity taxaId)
        {
            if (catchEffortId != null && this.CanGet())
            {
                string wh = Db.GetWhere(catchEffortId, "\"CatchEffortId\"");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectFishIndividual + Db.Where + Db.WhereTaxa + " AND " + wh;
                    Db.AddParam(cmd, "taxasid", "taxaid", taxaId);
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    Fish o = null;
                    List<Fish> items = new List<Fish>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.fishBuilder.Build(rdr);
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
                    }
                    return items;
                }
            }
            return null;
        }

        public IEnumerable<Fish> GetFish(IEnumerable<CompoundIdentity> catchEffortId, IEnumerable<CompoundIdentity> taxaId)
        {
            if (catchEffortId != null && this.CanGet())
            {
                string wh = Db.GetWhere(catchEffortId, "\"CatchEffortId\"");
                if (wh != null && wh.Length > 0)
                {
                    string whB = Db.GetWhere(taxaId, "taxasid", "taxaid");
                    if (whB != null && whB.Length > 0)
                    {
                        NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                        cmd.CommandText = Db.SelectFishIndividual + Db.Where + wh + " AND " + whB;
                        NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                        Fish o = null;
                        List<Fish> items = new List<Fish>();
                        if (rdr != null)
                        {
                            try
                            {
                                while (rdr.Read())
                                {
                                    o = this.fishBuilder.Build(rdr);
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
                        }
                        return items;
                    }
                }
            }
            return null;
        }

        public IEnumerable<Fish> GetFish(CompoundIdentity catchEffortId, IEnumerable<CompoundIdentity> taxaId)
        {
            if (!catchEffortId.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(catchEffortId.DataStoreIdentity) && this.CanGet())
            {
                string wh = Db.GetWhere(taxaId, "taxasid", "taxaid");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectFishIndividual + Db.Where + Db.WhereEffort + " AND " + wh;
                    cmd.Parameters.AddWithValue("ceid", catchEffortId.Identity);
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    Fish o = null;
                    List<Fish> permissions = new List<Fish>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.fishBuilder.Build(rdr);
                                if (o != null)
                                    permissions.Add(o);
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
                    }
                    return permissions;
                }
            }
            return null;
        }

        public IEnumerable<Fish> GetFish(CompoundIdentity catchEffortId, CompoundIdentity taxaId)
        {
            if (!catchEffortId.IsNullOrEmpty() && Db.DataStoreIdentity.Equals(catchEffortId.DataStoreIdentity) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishIndividual + Db.Where + Db.WhereEffort + " AND " + Db.WhereTaxa;
                cmd.Parameters.AddWithValue("ceid", catchEffortId.Identity);
                Db.AddParam(cmd, "taxasid", "taxaid", taxaId);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                Fish o = null;
                List<Fish> permissions = new List<Fish>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.fishBuilder.Build(rdr);
                            if (o != null)
                                permissions.Add(o);
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
                }
                return permissions;
            }
            return null;
        }

        public IEnumerable<Fish> GetFishByTaxa(IEnumerable<CompoundIdentity> taxaId)
        {
            if (this.CanGet())
            {
                string wh = Db.GetWhere(taxaId, "taxasid", "taxaid");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectFishIndividual + Db.Where + wh;
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    Fish o = null;
                    List<Fish> permissions = new List<Fish>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.fishBuilder.Build(rdr);
                                if (o != null)
                                    permissions.Add(o);
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
                    }
                    return permissions;
                }
            }
            return null;
        }

        public IEnumerable<Fish> GetFishByTaxa(CompoundIdentity taxaId)
        {
            if (this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishIndividual + Db.Where + Db.WhereTaxa;
                Db.AddParam(cmd, "taxasid", "taxaid", taxaId);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                Fish o = null;
                List<Fish> permissions = new List<Fish>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.fishBuilder.Build(rdr);
                            if (o != null)
                                permissions.Add(o);
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
                }
                return permissions;
            }
            return null;
        }

        public IEnumerable<FishDiet> GetFishDiet()
        {
            if (this.CanGet())
                return new Enumerable<FishDiet>(new EnumerableCommand<FishDiet>(this.dietBuilder, Db.SelectFishDiet, Db.ConnectionString));
            return null;
        }

        public IEnumerable<FishDiet> GetFishDiet(IEnumerable<Guid> fishId)
        {
            if (fishId != null && this.CanGet())
            {
                string wh = Db.GetWhere(fishId, "\"FishId\"");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectFishDiet + Db.Where + wh;
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    FishDiet o = null;
                    List<FishDiet> items = new List<FishDiet>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.dietBuilder.Build(rdr);
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
                    }
                    return items;
                }
            }
            return null;
        }

        public IEnumerable<FishDiet> GetFishDiet(IEnumerable<CompoundIdentity> taxaId)
        {
            if (this.CanGet())
            {
                string wh = Db.GetWhere(taxaId, "taxasid", "taxaid");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectFishDiet + Db.Where + wh;
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    FishDiet o = null;
                    List<FishDiet> permissions = new List<FishDiet>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.dietBuilder.Build(rdr);
                                if (o != null)
                                    permissions.Add(o);
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
                    }
                    return permissions;
                }
            }
            return null;
        }

        public IEnumerable<FishDiet> GetFishDiet(CompoundIdentity taxaId)
        {
            if (this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishDiet + Db.Where + Db.WhereTaxa;
                Db.AddParam(cmd, "taxasid", "taxaid", taxaId);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                FishDiet o = null;
                List<FishDiet> items = new List<FishDiet>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.dietBuilder.Build(rdr);
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
                }
                return items;
            }
            return null;
        }

        public IEnumerable<FishDiet> GetFishDiet(Guid fishId)
        {
            if (!Guid.Empty.Equals(fishId) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishDiet + Db.Where + Db.WhereFishId;
                cmd.Parameters.AddWithValue("fid", fishId);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                FishDiet o = null;
                List<FishDiet> items = new List<FishDiet>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.dietBuilder.Build(rdr);
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
                }
                return items;
            }
            return null;
        }

        public IEnumerable<FishGenetics> GetFishGenetics()
        {
            if (this.CanGet())
                return new Enumerable<FishGenetics>(new EnumerableCommand<FishGenetics>(this.genBuilder, Db.SelectFishGenetics, Db.ConnectionString));
            return null;
        }

        public IEnumerable<FishGenetics> GetFishGenetics(IEnumerable<Guid> fishId)
        {
            if (fishId != null && this.CanGet())
            {
                string wh = Db.GetWhere(fishId, "\"FishId\"");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectFishGenetics + Db.Where + wh;
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    FishGenetics o = null;
                    List<FishGenetics> items = new List<FishGenetics>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.genBuilder.Build(rdr);
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
                    }
                    return items;
                }
            }
            return null;
        }

        public IEnumerable<FishGenetics> GetFishGenetics(Guid fishId)
        {
            if (!Guid.Empty.Equals(fishId) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishGenetics + Db.Where + Db.WhereFishId;
                cmd.Parameters.AddWithValue("fid", fishId);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                FishGenetics o = null;
                List<FishGenetics> items = new List<FishGenetics>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.genBuilder.Build(rdr);
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
                }
                return items;
            }
            return null;
        }

        public IEnumerable<FishIdTag> GetFishIdTag()
        {
            if (this.CanGet())
                return new Enumerable<FishIdTag>(new EnumerableCommand<FishIdTag>(this.idBuilder, Db.SelectFishIdTag, Db.ConnectionString));
            return null;
        }

        public IEnumerable<FishIdTag> GetFishIdTag(IEnumerable<Guid> fishId)
        {
            if (fishId != null && this.CanGet())
            {
                string wh = Db.GetWhere(fishId, "\"FishId\"");
                if (wh != null && wh.Length > 0)
                {
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.SelectFishIdTag + Db.Where + wh;
                    NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                    FishIdTag o = null;
                    List<FishIdTag> items = new List<FishIdTag>();
                    if (rdr != null)
                    {
                        try
                        {
                            while (rdr.Read())
                            {
                                o = this.idBuilder.Build(rdr);
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
                    }
                    return items;
                }
            }
            return null;
        }

        public IEnumerable<FishIdTag> GetFishIdTag(Guid fishId)
        {
            if (!Guid.Empty.Equals(fishId) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishIdTag + Db.Where + Db.WhereFishId;
                cmd.Parameters.AddWithValue("fid", fishId);
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                FishIdTag o = null;
                List<FishIdTag> items = new List<FishIdTag>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.idBuilder.Build(rdr);
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
                }
                return items;
            }
            return null;
        }

        public IEnumerable<FishIdTag> GetFishIdTag(string tagCode, string tagType)
        {
            if (!string.IsNullOrEmpty(tagCode) && !string.IsNullOrEmpty(tagType) && this.CanGet())
            {
                NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                cmd.CommandText = Db.SelectFishIdTag + Db.Where + "lower(\"TagCode\")=:tc AND lower(\"TagType\")=:tt";
                cmd.Parameters.AddWithValue("tc", tagCode.ToLowerInvariant());
                cmd.Parameters.AddWithValue("tt", tagType.ToLowerInvariant());
                NpgsqlDataReader rdr = Db.ExecuteReader(cmd);
                FishIdTag o = null;
                List<FishIdTag> items = new List<FishIdTag>();
                if (rdr != null)
                {
                    try
                    {
                        while (rdr.Read())
                        {
                            o = this.idBuilder.Build(rdr);
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
                }
                return items;
            }
            return null;
        }

        public Fish CreateFish(CompoundIdentity catchEffortId, CompoundIdentity taxaId, float standard, float fork, float total, float weight, bool? adClipped, bool? cwt)
        {
            return CreateFish(catchEffortId, taxaId, standard, fork, total, weight, adClipped, cwt, null);
        }

        public Fish CreateFish(CompoundIdentity catchEffortId, CompoundIdentity taxaId, float standard, float fork, float total, float weight, bool? adClipped, bool? cwt, string description)
        {
            if (!catchEffortId.IsNullOrEmpty() && !taxaId.IsNullOrEmpty() && catchEffortId.DataStoreIdentity.Equals(Db.DataStoreIdentity) && this.CanCreate())
            {
                try 
                {
                    Guid id = Guid.NewGuid();
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertFishIndividual;
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("ceid", catchEffortId.Identity);
                    Db.AddParam(cmd, "tsid", "tid", taxaId);
                    Db.AddParam(cmd, "std", standard);
                    Db.AddParam(cmd, "fork", fork);
                    Db.AddParam(cmd, "tot", total);
                    Db.AddParam(cmd, "wei", weight);
                    Db.AddParam(cmd, "ad", adClipped);
                    Db.AddParam(cmd, "cwt", cwt);
                    Db.AddParam(cmd, "d", description);
                    Db.ExecuteNonQuery(cmd);

                    return new Fish(id, catchEffortId, taxaId, standard, fork, total, weight, adClipped, cwt, description);
                }
                catch
                { }
            }
            return null;
        }

        public FishDiet CreateFishDiet(Guid fishId, CompoundIdentity taxaId, string gutsampleId, uint? count, float sampleMass, float individualMass, uint? wholeAnimalsWeighed)
        {
            return CreateFishDiet(fishId, taxaId, gutsampleId, null, null, count, sampleMass, individualMass, wholeAnimalsWeighed, null);
        }

        public FishDiet CreateFishDiet(Guid fishId, CompoundIdentity taxaId, string gutsampleId, uint? count, float sampleMass, float individualMass, uint? wholeAnimalsWeighed, string description)
        {
            return CreateFishDiet(fishId, taxaId, gutsampleId, null, null, count, sampleMass, individualMass, wholeAnimalsWeighed, description);
        }

        public FishDiet CreateFishDiet(Guid fishId, CompoundIdentity taxaId, string gutsampleId, string vialId, uint? count, float sampleMass, float individualMass, uint? wholeAnimalsWeighed)
        {
            return CreateFishDiet(fishId, taxaId, gutsampleId, vialId, null, count, sampleMass, individualMass, wholeAnimalsWeighed, null);
        }

        public FishDiet CreateFishDiet(Guid fishId, CompoundIdentity taxaId, string gutsampleId, string vialId, uint? count, float sampleMass, float individualMass, uint? wholeAnimalsWeighed, string description)
        {
            return CreateFishDiet(fishId, taxaId, gutsampleId, vialId, null, count, sampleMass, individualMass, wholeAnimalsWeighed, description);
        }

        public FishDiet CreateFishDiet(Guid fishId, CompoundIdentity taxaId, string gutsampleId, string vialId, string lifeStage, uint? count, float sampleMass, float individualMass, uint? wholeAnimalsWeighed)
        {
            return CreateFishDiet(fishId, taxaId, gutsampleId, vialId, lifeStage, count, sampleMass, individualMass, wholeAnimalsWeighed, null);
        }

        public FishDiet CreateFishDiet(Guid fishId, CompoundIdentity taxaId, string gutsampleId, string vialId, string lifeStage, uint? count, float sampleMass, float individualMass, uint? wholeAnimalsWeighed, string description)
        {
            if (!Guid.Empty.Equals(fishId) && this.CanCreate())
            {
                try
                {
                    Guid id = Guid.NewGuid();
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertFishDiet;
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("fid", fishId);
                    Db.AddParam(cmd, "tsid", "tid", taxaId);
                    Db.AddParam(cmd, "vial", vialId);
                    Db.AddParam(cmd, "gut", gutsampleId);
                    Db.AddParam(cmd, "life", lifeStage);
                    if (taxaId.IsNullOrEmpty())
                        count = null;
                    Db.AddParam(cmd, "ct", count);
                    Db.AddParam(cmd, "whole", wholeAnimalsWeighed);
                    Db.AddParam(cmd, "ind", individualMass);
                    Db.AddParam(cmd, "sam", sampleMass);
                    Db.AddParam(cmd, "d", description);
                    Db.ExecuteNonQuery(cmd);

                    return new FishDiet(id, fishId, taxaId, vialId, gutsampleId, lifeStage, count, sampleMass, individualMass, wholeAnimalsWeighed, description);
                }
                catch
                { }
            }
            return null;
        }

        public FishGenetics CreateFishGenetics(Guid fishId, StockEstimates estimates)
        {
            return CreateFishGenetics(fishId, null, null, estimates, null);
        }

        public FishGenetics CreateFishGenetics(Guid fishId, StockEstimates estimates, string description)
        {
            return CreateFishGenetics(fishId, null, null, estimates, description);
        }

        public FishGenetics CreateFishGenetics(Guid fishId, string geneticSampleId, string labSampleId, StockEstimates estimates)
        {
            return CreateFishGenetics(fishId, geneticSampleId, labSampleId, estimates, null);
        }

        public FishGenetics CreateFishGenetics(Guid fishId, string geneticSampleId, string labSampleId, StockEstimates estimates, string description)
        {
            if (!Guid.Empty.Equals(fishId) && estimates!=null && this.CanCreate())
            {
                try
                {
                    Guid id = Guid.NewGuid();
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertFishGenetics;
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("fid", fishId);
                    Db.AddParam(cmd, "gen", geneticSampleId);
                    Db.AddParam(cmd, "lab", labSampleId);
                    cmd.Parameters.AddWithValue("stock", FishGeneticsBuilder.Encode(estimates)); //stringifies the estimates
                    Db.AddParam(cmd, "d", description);
                    Db.ExecuteNonQuery(cmd);

                    return new FishGenetics(id, fishId, geneticSampleId, labSampleId, estimates, description);
                }
                catch
                { }
            }
            return null;
        }

        public FishIdTag CreateIdTag(Guid fishId, string tagCode, string tagType)
        {
            return CreateIdTag(fishId, tagCode, tagType, null, null);
        }

        public FishIdTag CreateIdTag(Guid fishId, string tagCode, string tagType, string tagManufacturer)
        {
            return CreateIdTag(fishId, tagCode, tagType, tagManufacturer, null);
        }

        public FishIdTag CreateIdTag(Guid fishId, string tagCode, string tagType, string tagManufacturer, string description)
        {
            if (!Guid.Empty.Equals(fishId) && !string.IsNullOrEmpty(tagCode) && !string.IsNullOrEmpty(tagType) && this.CanCreate())
            {
                try
                {
                    Guid id = Guid.NewGuid();
                    NpgsqlCommand cmd = Db.GetCmd(Db.ConnectionString);
                    cmd.CommandText = Db.InsertFishIdTag;
                    cmd.Parameters.AddWithValue("id", id);
                    cmd.Parameters.AddWithValue("fid", fishId);
                    Db.AddParam(cmd, "tag", tagCode);
                    Db.AddParam(cmd, "typ", tagType);
                    Db.AddParam(cmd, "man", tagManufacturer);
                    Db.AddParam(cmd, "d", description);
                    Db.ExecuteNonQuery(cmd);

                    return new FishIdTag(id, fishId, tagCode, tagType, tagManufacturer, description);
                }
                catch
                { }
            }
            return null;
        }

        private UserSecurityContext Context
        {
            get;
            set;
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

        internal PgFishProvider(UserSecurityContext context)
        {
            this.Context = context;
            this.fishBuilder = new FishBuilder(context);
            this.dietBuilder = new FishDietBuilder(context, this);
            this.genBuilder = new FishGeneticsBuilder(context, this);
            this.idBuilder = new FishIdTagBuilder(context, this);
        }
    }
}
