using Newtonsoft.Json.Linq;
using Osrs.Security.Authorization;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.Security
{
    internal static class Jsonifier
    {
        public static JArray ToJson(IEnumerable<Permission> item)
        {
            if (item != null)
            {
                JArray o = new JArray();
                foreach (Permission cur in item)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

        public static JValue ToJson(Permission item)
        {
            if (item != null)
            {
                return new JValue(item.Name);
            }
            return null;
        }
    }
}
