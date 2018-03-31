using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.Dets
{
    internal static class JsonDetExtractor
    {
        internal static string Sites = "sites";
        internal static string Instruments = "instruments";
        internal static string Nets = "nets";
        internal static string Fish = "fishTaxa";
        internal static string Macro = "macroTaxa";
        internal static string Privacy = "privacy";
        internal static string Tree = "treeTaxa";
        internal static string Shrub = "shrubTaxa";
        internal static string Herb = "herbTaxa";
        internal static string NonLiving = "nonlivingTaxa";
        internal static string Plots = "plotTypes";

        internal static List<DetPayload> ExtractAll(JToken elements)
        {
            if (elements != null)
                return ExtractAll(elements as JArray);
            return null;
        }

        //TODO -- augment this for each known DET type - this will parse the provided listings and entity bundles
        //TODO -- add include/exclude handling within each bundle -- includes are "only these" excludes are "all but these" -- there can be includes or excludes or neither but not both
        internal static List<DetPayload> ExtractAll(JArray elements) 
        {
            if (elements == null)
                return null;

            List<DetPayload> payloads = new List<DetPayload>();
            HashSet<string> visited = new HashSet<string>();
            foreach (JToken cur in elements)
            {
                if (cur == null || !(cur is JObject))
                    return null; //short circuit exit

                string detTypeName = KnownDets.Instance.Clean(JsonUtils.ToString(cur[JsonUtils.Type]));
                if (string.IsNullOrEmpty(detTypeName) || !KnownDets.Instance.IsValid(detTypeName))
                    return null; //short circuit exit
                bool privacy = false;
                if (cur[Privacy] !=null)
                {
                    JToken pr = cur[Privacy];
                    if (pr.Type == JTokenType.Boolean)
                        privacy = (bool)pr;
                }

                if (detTypeName == KnownDets.Instance.WQ) //we need sites and instruments
                {
                    JObject siteToken = cur[Sites] as JObject;
                    JObject instToken = cur[Instruments] as JObject;

                    if (siteToken == null || instToken == null)
                        return null; //short circuit exit

                    Guid siteId = JsonUtils.ToGuid(siteToken[JsonUtils.Id]);
                    Guid instId = JsonUtils.ToGuid(instToken[JsonUtils.Id]);

                    if (Guid.Empty.Equals(siteId) || Guid.Empty.Equals(instId))
                        return null; //short circuit exit

                    if (visited.Contains(detTypeName))
                        return null; //short circuit exit - only 1 allowed of each type

                    DetPayload p = new DetPayload(detTypeName, privacy);
                    p.EntityBundles.Add(Sites, siteId);
                    p.EntityBundles.Add(Instruments, instId);
                    payloads.Add(p);
                    visited.Add(detTypeName);
                }
                else if (detTypeName == KnownDets.Instance.Fish)
                {
                    JObject siteToken = cur[Sites] as JObject;
                    JObject instToken = cur[Nets] as JObject;
                    JObject fishToken = cur[Fish] as JObject;
                    JObject macroToken = cur[Macro] as JObject;

                    if (siteToken == null || instToken == null || fishToken==null || macroToken==null) //may allow for null macro at some point
                        return null; //short circuit exit

                    Guid siteId = JsonUtils.ToGuid(siteToken[JsonUtils.Id]);
                    Guid instId = JsonUtils.ToGuid(instToken[JsonUtils.Id]);
                    Guid fishId = JsonUtils.ToGuid(fishToken[JsonUtils.Id]);
                    Guid macroId = JsonUtils.ToGuid(macroToken[JsonUtils.Id]);

                    if (Guid.Empty.Equals(siteId) || Guid.Empty.Equals(instId) || Guid.Empty.Equals(fishId) || Guid.Empty.Equals(macroId))
                        return null; //short circuit exit

                    if (visited.Contains(detTypeName))
                        return null; //short circuit exit - only 1 allowed of each type

                    DetPayload p = new DetPayload(detTypeName, privacy);
                    p.EntityBundles.Add(Sites, siteId);
                    p.EntityBundles.Add(Nets, instId);
                    p.EntityBundles.Add(Fish, fishId);
                    p.EntityBundles.Add(Macro, macroId);
                    payloads.Add(p);
                    visited.Add(detTypeName);
                }
                else if (detTypeName == KnownDets.Instance.Veg)
                {
                    JObject siteToken = cur[Sites] as JObject;
                    JObject treeToken = cur[Tree] as JObject;
                    JObject shrubToken = cur[Shrub] as JObject;
                    JObject herbToken = cur[Herb] as JObject;
                    JObject plotToken = cur[Plots] as JObject;
                    JObject nlToken = cur[NonLiving] != null ? cur[NonLiving] as JObject : null;

                    if (siteToken == null || treeToken == null || shrubToken == null || herbToken == null || plotToken == null) // || nlToken == null
                        return null; //short circuit exit

                    Guid siteId = JsonUtils.ToGuid(siteToken[JsonUtils.Id]);
                    Guid treeId = JsonUtils.ToGuid(treeToken[JsonUtils.Id]);
                    Guid shrubId = JsonUtils.ToGuid(shrubToken[JsonUtils.Id]);
                    Guid herbId = JsonUtils.ToGuid(herbToken[JsonUtils.Id]);
                    Guid plotId = JsonUtils.ToGuid(plotToken[JsonUtils.Id]);

                    Guid nlId;
                    if (nlToken != null)
                        nlId = JsonUtils.ToGuid(nlToken[JsonUtils.Id]);

                    if (Guid.Empty.Equals(siteId) || Guid.Empty.Equals(treeId) || Guid.Empty.Equals(shrubId) || Guid.Empty.Equals(herbId) || Guid.Empty.Equals(plotId))  // || Guid.Empty.Equals(nlId)
                        return null; //short circuit exit

                    if (visited.Contains(detTypeName))
                        return null; //short circuit exit - only 1 allowed of each type

                    DetPayload p = new DetPayload(detTypeName, privacy);
                    p.EntityBundles.Add(Sites, siteId);
                    p.EntityBundles.Add(Tree, treeId);
                    p.EntityBundles.Add(Shrub, shrubId);
                    p.EntityBundles.Add(Herb, herbId);
                    p.EntityBundles.Add(Plots, plotId);
                    p.EntityBundles.Add(NonLiving, nlId);

                    payloads.Add(p);
                    visited.Add(detTypeName);
                }

            }

            if (elements.Count == payloads.Count)
                return payloads;
            return null;
        }
    }

    internal sealed class DetPayload
    {
        internal readonly string DetTypeName;
        internal readonly bool IsPrivate;
        internal readonly Dictionary<string, Guid> EntityBundles = new Dictionary<string, Guid>();
        internal readonly Dictionary<string, HashSet<string>> BundleIncludes = new Dictionary<string, HashSet<string>>();
        internal readonly Dictionary<string, HashSet<string>> BundleExcludes = new Dictionary<string, HashSet<string>>();

        public bool FullBundle(string bundle)
        {
            return !(this.HasIncludes(bundle) || this.HasExcludes(bundle));
        }

        public bool HasIncludes(string bundle)
        {
            return this.BundleIncludes.ContainsKey(bundle) && this.BundleIncludes[bundle].Count > 0;
        }

        public bool HasExcludes(string bundle)
        {
            return this.BundleExcludes.ContainsKey(bundle) && this.BundleExcludes[bundle].Count > 0;
        }

        internal DetPayload(string detTypeName, bool isPrivate)
        {
            this.DetTypeName = detTypeName;
            this.IsPrivate = isPrivate;
        }
    }
}
