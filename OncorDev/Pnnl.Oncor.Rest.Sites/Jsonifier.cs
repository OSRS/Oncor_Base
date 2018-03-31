using Newtonsoft.Json.Linq;
using Osrs.Numerics.Spatial.Geometry;
using Osrs.WellKnown.Sites;
using Osrs.Numerics;
using System.Collections.Generic;
using System;

namespace Pnnl.Oncor.Rest.Sites
{
    internal static class Jsonifier
    {
        private const string Dsid = "dsid";
        private const string Id = "id";

        public static JArray ToJson(IEnumerable<SiteAliasScheme> schemes)
        {
            if (schemes != null)
            {
                JArray o = new JArray();
                foreach (SiteAliasScheme cur in schemes)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

        public static JArray ToJson(IEnumerable<SiteAlias> aliases)
        {
            if (aliases != null) 
            {
                JArray o = new JArray();
                foreach (SiteAlias cur in aliases)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

        public static JArray ToJson(IEnumerable<Site> sites)
        {
            if (sites != null) 
            {
                JArray o = new JArray();
                foreach (Site cur in sites)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

        public static JObject ToJson(SiteAliasScheme scheme)
        {
            if (scheme != null)
            {
                JObject o = new JObject();
                o.Add(JsonUtils.Id, JsonUtils.ToJson(scheme.Identity));
                o.Add(JsonUtils.OwnerId, JsonUtils.ToJson(scheme.OwningOrganizationIdentity));
                o.Add(JsonUtils.Name, scheme.Name);
                o.Add(JsonUtils.Description, scheme.Description);
                return o;
            }
            return null;
        }

        public static JObject ToJson(SiteAlias alias)
        {
            if (alias != null) 
            {
                JObject o = new JObject();
                o.Add(JsonUtils.Id, JsonUtils.ToJson(alias.Identity));
                o.Add(JsonUtils.SchemeId, JsonUtils.ToJson(alias.AliasSchemeIdentity));
                o.Add(JsonUtils.Name, alias.Name);
                return o;
            }
            return null;
        }

        public static JObject ToJson(Site site)
        {
            if (site != null)
            {
                JObject o = new JObject();
                o.Add(JsonUtils.Id, JsonUtils.ToJson(site.Identity));
                o.Add(JsonUtils.Name, site.Name);
                o.Add(JsonUtils.Description, site.Description);
                o.Add(JsonUtils.OwnerId, JsonUtils.ToJson(site.OwningOrganizationIdentity));
                if (site.Location!=null)
                    o.Add(JsonUtils.Location, ToJson(site.Location));
                if (site.LocationMark!=null)
                    o.Add(JsonUtils.LocationMark, ToJson(site.LocationMark));
                return o;
            }
            return null;
        }

        public static JObject ToJson(IGeometry2<double> shape)
        {
            if (shape!=null)
                return GeoJsonUtils.ToGeoJson(shape);
            return null;
        }

        public static JObject ToJson(Point2<double> shape)
        {
            if (shape != null)
                return GeoJsonUtils.ToGeoJson(shape);
            return null;
        }

        public static bool IsNullOrEmpty(this JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null);
        }
    }
}
