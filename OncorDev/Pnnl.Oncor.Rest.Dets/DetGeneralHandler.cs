using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Net.Http;
using Osrs.Numerics;
using Osrs.Oncor.EntityBundles;
using Osrs.Oncor.FileStore;
using Osrs.Oncor.UserAffiliationPermissionChecks;
using Osrs.Security;
using Osrs.Threading;
using Pnnl.Oncor.DetProcessing;
using Pnnl.Oncor.DetProcessor;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.Dets
{
    internal static class DetGeneralHandler
    {
        private static IFileStoreProvider fProvider;
        private static IFileStoreProvider FileProvider
        {
            get
            {
                if (fProvider==null)
                    fProvider = FileStoreManager.Instance.GetProvider();
                return fProvider;
            }
        }

        private static bool HasAffiliation(UserSecurityContext user, CompoundIdentity prOrgId)
        {
            if (user != null && prOrgId != null)
            {
                UserProvider up = UserAffilationSecurityManager.Instance.GetProvider(user);
                if (up != null)
                {
                    IEnumerable<CompoundIdentity> ids = up.UserAffiliations();
                    if (ids!=null)
                    {
                        foreach(CompoundIdentity cur in ids)
                        {
                            if (prOrgId.Equals(cur))
                                return true;
                        }
                    }
                }
            }
            return false;
        }

        private static KnownDetType ToType(string val)
        {
            if (val!=null)
            {
                if (val.Equals(KnownDets.Instance.WQ))
                    return KnownDetType.WaterQuality;
                else if (val.Equals(KnownDets.Instance.Veg))
                    return KnownDetType.Veg;
                else if (val.Equals(KnownDets.Instance.Fish))
                    return KnownDetType.Fish;
                else if (val.Equals(KnownDets.Instance.Photo))
                    return KnownDetType.PhotoPoint;
                else if (val.Equals(KnownDets.Instance.SedAcc))
                    return KnownDetType.SedimentAccretion;
            }
            return KnownDetType.Unknown;
        }

        private static JToken ToJson(UpdateStatus stat)
        {
            long cnt = 0;
            JObject o = new JObject();
            foreach (IssueNotice cur in stat.Notices)
            {
                if(cnt <= 100)
                {
                    o.Add(cnt + "-" + cur.Title, new JValue(cur.Message));
                }
                cnt++;
            }
            if (cnt > 100)
                o.Add("More issues detected", new JValue("A total of " + cnt + " issues were detected"));
            return o;
        }

        private static JToken ToJson(SampleEventMap map)
        {
            JObject o = new JObject();
            foreach(SampleEventMapItem s in map)
            {
                o.Add(s.DetType.ToString(), new JValue(s.DetId.ToString()));
            }
            return o;
        }

        private static void PushStat(UpdateStatus stat, HttpContext context)
        {
            if (stat!=null)
            {
                if (stat.HasIssue)
                {
                    RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed, ToJson(stat).ToString()));
                    return;
                }
                else
                {
                    RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, ToJson(stat).ToString()));
                    return;
                }
            }
            RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed, "\"No status returned\""));
        }

        //to support mutliple det types in a single request (which all share the same sample event), we have a collection of items as follows:
        //{id:<fieldtripid>, name:<sampleeventname - to be>, desc:<sampleeventdesc - to be>, princorgid: <id>, start:<date>, end:<date>, dets:[<det element>,...]}
        //each det element is an object as follows:
        //{type:<dettype>, privacy:<bool>, <<entity bundle ids specific to det>>}   (example is sites:{id:bundleid,include:[key list]})    optional "exclude" instead of include would allow removing elements from bundle in det
        //returns fileid of the file to download
        //      note that the name is required, desc is optional - name
        //for all types, the entity bundle format is:  {id:<entitybundleid>, keys:[<id>, <id>, ...]}
        //      note that keys are optional - if not provided it means "all items in bundle"
        //      note that the bundle should be of the type expected for the parameter - e.g. if the provided bundle is for "sites", that bundle should be of sites
        internal static void Create(UserSecurityContext user, JToken dat, HttpContext context, CancellationToken cancel)
        {
            if (dat !=null)
            {
                JObject payload = dat as JObject;
                if (payload!=null)
                {
                    //these are requried for any and all DETs to create
                    CompoundIdentity ftId = JsonUtils.ToId(payload[JsonUtils.Id]);
                    CompoundIdentity prOrgId = JsonUtils.ToId(payload[JsonUtils.OwnerId]);

                    if (HasAffiliation(user, prOrgId))
                    {

                        string name = JsonUtils.ToString(payload[JsonUtils.Name]);
                        string desc = JsonUtils.ToString(payload[JsonUtils.Description]);

                        ValueRange<DateTime> range = JsonUtils.ToRange(payload, JsonUtils.Start, JsonUtils.Finish);

                        if (ftId != null && prOrgId != null && !string.IsNullOrEmpty(name))
                        {
                            JToken dets = payload["dets"];
                            if (dets != null && dets is JArray)
                            {
                                JArray detItems = dets as JArray;
                                if (detItems != null)
                                {
                                    List<DetPayload> items = JsonDetExtractor.ExtractAll(detItems);
                                    if (items != null && items.Count>0) //we actually have things to create
                                    {
                                        GeneralDetProcessor prov = DetProcessorManager.Instance.GetProvider(user);
                                        if (prov != null)
                                        {
                                            CompoundIdentity seId = prov.CreateSampleEvent(ftId, prOrgId, name, desc, range);
                                            EntityBundleProvider eProv = EntityBundleManager.Instance.GetProvider(user);
                                            if (eProv != null)
                                            {
                                                if (seId != null)
                                                {
                                                    Dictionary<string, Guid> fileIds = new Dictionary<string, Guid>();
                                                    foreach (DetPayload cur in items)
                                                    {
                                                        if (cur.DetTypeName == KnownDets.Instance.WQ)
                                                        {
                                                            EntityBundle siteBundle = eProv.Get(cur.EntityBundles[JsonDetExtractor.Sites]);
                                                            EntityBundle instBundle = eProv.Get(cur.EntityBundles[JsonDetExtractor.Instruments]);
                                                            if (siteBundle!=null && instBundle!=null)
                                                            {
                                                                FilestoreFile fil = prov.CreateWQ(seId, siteBundle, instBundle, cur.IsPrivate);
                                                                if (fil != null)
                                                                {
                                                                    fileIds.Add(cur.DetTypeName, fil.FileId);
                                                                    fil.Dispose();
                                                                    fil = null;
                                                                }
                                                            }
                                                        }
                                                        else if (cur.DetTypeName == KnownDets.Instance.Fish)
                                                        {
                                                            EntityBundle siteBundle = eProv.Get(cur.EntityBundles[JsonDetExtractor.Sites]);
                                                            EntityBundle instBundle = eProv.Get(cur.EntityBundles[JsonDetExtractor.Nets]);
                                                            EntityBundle fishBundle = eProv.Get(cur.EntityBundles[JsonDetExtractor.Fish]);
                                                            EntityBundle macroBundle = eProv.Get(cur.EntityBundles[JsonDetExtractor.Macro]);

                                                            if (siteBundle != null && instBundle != null && fishBundle!=null && macroBundle!=null)
                                                            {
                                                                FilestoreFile fil = prov.CreateFish(seId, siteBundle, instBundle, fishBundle, macroBundle, cur.IsPrivate);
                                                                if (fil != null)
                                                                {
                                                                    fileIds.Add(cur.DetTypeName, fil.FileId);
                                                                    fil.Dispose();
                                                                    fil = null;
                                                                }
                                                            }
                                                        }
                                                        else if (cur.DetTypeName == KnownDets.Instance.Veg)
                                                        {
                                                            EntityBundle siteBundle = eProv.Get(cur.EntityBundles[JsonDetExtractor.Sites]);
                                                            EntityBundle treeBundle = eProv.Get(cur.EntityBundles[JsonDetExtractor.Tree]);
                                                            EntityBundle shrubBundle = eProv.Get(cur.EntityBundles[JsonDetExtractor.Shrub]);
                                                            EntityBundle herbBundle = eProv.Get(cur.EntityBundles[JsonDetExtractor.Herb]);
                                                            EntityBundle plotBundle = eProv.Get(cur.EntityBundles[JsonDetExtractor.Plots]);

                                                            EntityBundle nlBundle = null;
                                                            if (JsonDetExtractor.NonLiving != null)
                                                            {
                                                                 nlBundle = eProv.Get(cur.EntityBundles[JsonDetExtractor.NonLiving]);
                                                            }
                                                            

                                                            if (siteBundle != null && treeBundle != null && shrubBundle != null && herbBundle != null && plotBundle != null)
                                                            {
                                                                FilestoreFile fil = prov.CreateVeg(seId, siteBundle, plotBundle, shrubBundle, treeBundle, herbBundle, nlBundle, cur.IsPrivate);
                                                                if (fil != null)
                                                                {
                                                                    fileIds.Add(cur.DetTypeName, fil.FileId);
                                                                    fil.Dispose();
                                                                    fil = null;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    //NOTE - we may want to deal with partial successes in case we have multiple DETs and some work, some don't
                                                    //ok, now we do the response with a list of fileIds
                                                    if (fileIds.Count > 0)
                                                    {
                                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, JsonUtils.ToJson(fileIds).ToString()));
                                                        return;
                                                    }
                                                    else
                                                    {
                                                        RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed, new JValue("\"No items succeeded\"").ToString()));
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = HttpStatusCodes.Status401Unauthorized; //in this case, by way of affiliation
                        return;
                    }
                }
            }
            context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
        }

        //upload is done first, with the fileid returned, then we call this update which will process the uploaded file
        internal static void Update(UserSecurityContext user, JToken dat, HttpContext context, CancellationToken cancel)
        {
            if (dat != null)
            {
                JObject payload = dat as JObject;
                if (payload != null)
                {
                    Guid fileId = JsonUtils.ToGuid(dat[JsonUtils.Id]);
                    if (!Guid.Empty.Equals(fileId))
                    {
                        FilestoreFile tempFile = FileStoreManager.Instance.GetProvider().Get(fileId);
                        if (tempFile!=null)
                        {
                            GeneralDetProcessor prov = DetProcessorManager.Instance.GetProvider(user);
                            if (prov!=null)
                            {
                                UpdateStatus stat = prov.Update(tempFile);
                                PushStat(stat, context);
                                return;
                            }
                        }
                    }
                }
            }
            context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
        }

        //{sampleeventid:<id>, datatype:<det type>}
        //returns map of  [{type:<det type>, id:<detfileid>}, ...]
        //      note datatype is optional - if omitted, get list of all dets for that sample event
        internal static void Get(UserSecurityContext user, JToken dat, HttpContext context, CancellationToken cancel)
        {
            if (dat != null)
            {
                JObject payload = dat as JObject;
                if (payload != null)
                {
                    CompoundIdentity seId = JsonUtils.ToId(dat[JsonUtils.Id]);
                    if (seId!=null)
                    {
                        GeneralDetProcessor prov = DetProcessorManager.Instance.GetProvider(user);
                        if (prov != null)
                        {
                            SampleEventMap map = prov.GetMap(seId);
                            if (map!=null)
                            {
                                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok, ToJson(map).ToString()));
                                return;
                            }
                            else
                            {
                                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok)); //no dets in sample event
                                return;
                            }
                        }
                    }
                }
            }
            context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
        }

        //{eventid:<id>, cascade:<bool>, type:<dettype>}
        //      note that cascade is optional, if omitted, treated as false
        //      note that if cascade==true, that means delete both the file and all stored data in the system for that file -- THIS CANNOT BE UNDONE
        //      note that type is optional, if omitted, treated as all det types otherwise it's a single type such as wq
        internal static void Delete(UserSecurityContext user, JToken dat, HttpContext context, CancellationToken cancel)
        {
            if (dat != null)
            {
                JObject payload = dat as JObject;
                if (payload != null)
                {
                    CompoundIdentity seId = JsonUtils.ToId(dat["eventid"]);
                    if (seId!=null)
                    {
                        bool fileOnly = true;
                        JToken t = payload["cascade"];
                        if (t != null)
                        {
                            string tmp = t.ToString();
                            if (tmp != null)
                            {
                                tmp = tmp.Trim().ToLowerInvariant();
                                if (tmp.StartsWith("tr"))
                                    fileOnly = false;
                            }
                            else
                            {
                                context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
                                return;
                            }
                        }

                        KnownDetType type = KnownDetType.Unknown;
                        t = payload[JsonUtils.Type];
                        if (t!=null)
                        {
                            string tmp = KnownDets.Instance.Clean(t.ToString());
                            if (!KnownDets.Instance.IsValid(tmp))
                            {
                                context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
                                return;
                            }
                            type = ToType(tmp);
                        }

                        GeneralDetProcessor prov = DetProcessorManager.Instance.GetProvider(user);
                        if (prov != null)
                        {
                            bool res = false;
                            if (type != KnownDetType.Unknown)
                                res = prov.Delete(seId, fileOnly, type);
                            else
                                res = prov.Delete(seId, fileOnly);

                            if (res)
                                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Ok));
                            else
                                RestUtils.Push(context.Response, RestUtils.JsonOpStatus(JsonOpStatus.Failed));
                            return;
                        }
                    }
                }
            }
            context.Response.StatusCode = HttpStatusCodes.Status400BadRequest;
        }
    }
}
