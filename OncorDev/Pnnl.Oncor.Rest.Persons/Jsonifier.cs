using Newtonsoft.Json.Linq;
using Osrs.Oncor.Wellknown.Persons;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.Persons
{
    internal static class Jsonifier
    {
        public static JArray ToJson(IEnumerable<Person> items)
        {
            if (items != null)
            {
                JArray o = new JArray();
                foreach (Person cur in items)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

        public static JObject ToJson(SimpleContactInfo item)
        {
            if (item != null)
            {
                JObject o = new JObject();
                Dictionary<string, EmailAddress> items = item.Get();
                foreach (KeyValuePair<string, EmailAddress> cur in items)
                {
                    o.Add(JsonUtils.Name, new JValue(cur.Key));
                    o.Add(JsonUtils.SchemeId, new JValue(cur.Value.ToString()));
                }
                return o;
            }
            return null;
        }

        public static JObject ToJson(Person p)
        {
            if (p != null)
            {
                JObject o = new JObject();
                o.Add(JsonUtils.Id, JsonUtils.ToJson(p.Identity));
                o.Add("firstname", p.FirstName);
                o.Add("lastname", p.LastName);
                o.Add("contacts", ToJson(p.Contacts));
                return o;
            }
            return null;
        }
    }
}
