using Newtonsoft.Json.Linq;
using Osrs.Oncor.EntityBundles;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.EntityBundles
{
    internal static class Jsonifier
    {
        public const string Type = "type";
        public const string Items = "items";
        public const string Key = "key";
        public const string Display = "display";

        public static JArray ToJson(IEnumerable<EntityBundle> bundles)
        {
            if (bundles != null)
            {
                JArray o = new JArray();
                foreach (EntityBundle cur in bundles)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

        public static JObject ToJson(EntityBundle item)
        {
            if (item != null)
            {
                JObject o = new JObject();
                o.Add(JsonUtils.Id, new JValue(item.Id).ToString());
                o.Add(JsonUtils.OwnerId, JsonUtils.ToJson(item.PrincipalOrgId));
                o.Add(JsonUtils.Name, new JValue(item.Name));
                o.Add(Jsonifier.Type, new JValue(item.DataType.ToString()));
                JArray tmp = ToJson(item.Elements);
                if (tmp != null)
                    o.Add(Jsonifier.Items, tmp);

                return o;
            }
            return null;
        }

        public static JArray ToJson(IEnumerable<BundleElement> items)
        {
            if (items!=null)
            {
                JArray tmp = new JArray();
                foreach(BundleElement cur in items)
                {
                    JObject o = ToJson(cur);
                    if (o!=null)
                        tmp.Add(o);
                }
                if (tmp.Count > 0)
                    return tmp;
            }
            return null;
        }

        public static JObject ToJson(BundleElement cur)
        {
            if (cur != null)
            {
                JObject o = new JObject();
                o.Add(Jsonifier.Key, cur.LocalKey);
                o.Add(JsonUtils.Id, JsonUtils.ToJson(cur.EntityId));
                o.Add(Jsonifier.Display, cur.DisplayName);
                return o;
            }
            return null;
        }
    }
}
