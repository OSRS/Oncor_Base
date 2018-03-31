using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Oncor.EntityBundles;
using Osrs.Security;
using Osrs.WellKnown.SensorsAndInstruments;
using Osrs.WellKnown.Sites;
using Osrs.WellKnown.Taxonomy;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.EntityBundles
{
    internal static class Items
    {
        internal static JArray GetItems(JToken t)
        {
            if (t != null && t is JArray)
                return t as JArray;
            return null;
        }

        internal static Tuple<CompoundIdentity, string, string> GetItem(JToken item)
        {
            if (item!=null && item is JObject)
            {
                JObject o = item as JObject;
                if (o!=null && o[JsonUtils.Id] !=null && o[Jsonifier.Key]!=null && o[Jsonifier.Display] !=null)
                {
                    JToken idTok = o[JsonUtils.Id];
                    JToken keyTok = o[Jsonifier.Key];
                    JToken displayTok = o[Jsonifier.Display];

                    string key = keyTok.ToString();
                    if (!string.IsNullOrEmpty(key))
                    {
                        string display = displayTok.ToString();
                        if (!string.IsNullOrEmpty(display))
                        {
                            CompoundIdentity id = JsonUtils.ToId(idTok);
                            if (id != null && !id.IsEmpty)
                                return new Tuple<CompoundIdentity, string, string>(id, key, display);
                        }
                    }
                }
            }
            return null;
        }

        internal static bool VandV(IEnumerable<Tuple<CompoundIdentity, string, string>> items, BundleDataType type, UserSecurityContext ctx)
        {
            if (Validate(items))
                return Verify(items, type, ctx);
            return false;
        }

        internal static bool Validate(IEnumerable<Tuple<CompoundIdentity, string, string>> items)
        {
            HashSet<string> keys = new HashSet<string>();
            foreach(Tuple<CompoundIdentity, string, string> cur in items)
            {
                if (!keys.Add(cur.Item2))
                    return false;
            }
            return true; //all are unique keys
        }

        internal static bool Verify(IEnumerable<Tuple<CompoundIdentity, string, string>> items, BundleDataType type, UserSecurityContext ctx)
        {
            if (type == BundleDataType.TaxaUnit)
            {
                ITaxaUnitProvider tprov = TaxonomyManager.Instance.GetTaxaUnitProvider(ctx);
                if (tprov!=null)
                {
                    foreach(Tuple<CompoundIdentity, string, string> cur in items)
                    {
                        if (!tprov.Exists(cur.Item1))
                            return false;
                    }
                    return true;
                }
            }
            else if (type == BundleDataType.Site)
            {
                ISiteProvider sprov = SiteManager.Instance.GetSiteProvider(ctx);
                if (sprov != null)
                {
                    foreach (Tuple<CompoundIdentity, string, string> cur in items)
                    {
                        if (!sprov.Exists(cur.Item1))
                            return false;
                    }
                    return true;
                }
            }
            else if (type == BundleDataType.Instrument)
            {
                IInstrumentProvider iprov = InstrumentManager.Instance.GetInstrumentProvider(ctx);
                if (iprov != null)
                {
                    foreach (Tuple<CompoundIdentity, string, string> cur in items)
                    {
                        if (!iprov.Exists(cur.Item1))
                            return false;
                    }
                    return true;
                }
            }
            return false;
        }
    }
}
