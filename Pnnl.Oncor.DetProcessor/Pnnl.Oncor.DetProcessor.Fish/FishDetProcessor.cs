using Osrs.Security;
using Pnnl.Oncor.DetProcessing;
using System;
using System.Collections.Generic;
using Osrs.Oncor.FileStore;
using Osrs.Oncor.EntityBundles;
using Osrs.Oncor.WellKnown.Fish.Module;
using Osrs.Oncor.WellKnown.Fish;
using Osrs.Oncor.DetFactories.DETs;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;
using ExcelDETs.DETs;
using Osrs.Data;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;
using Osrs.Numerics.Spatial.Geometry;
using Osrs.Numerics;
using Osrs.Oncor.DetFactories;

namespace Pnnl.Oncor.DetProcessor.Fish
{
    public sealed class FishDetProcessor : DetProcessorBase
    {
        public FilestoreFile Create(SampleEventMap map, EntityBundle sites, EntityBundle nets, EntityBundle fishSpecies, EntityBundle macroSpecies, bool isPrivate)
        {
            if (this.CanModify(map))
            {
                Guid id = Guid.NewGuid();
                FishDET det = new FishDET();
                det.Id = id;
                det.Owner = "originator:" + sites.PrincipalOrgId.Identity.ToString() + ":" + nets.PrincipalOrgId.Identity.ToString();

                int ct = 0;
                foreach (BundleElement cur in sites.Elements)
                {
                    SiteDTO tmp = new SiteDTO();
                    tmp.Key = cur.LocalKey;
                    tmp.Name = cur.DisplayName;
                    det.Sites.Add(tmp);
                    ct++;
                }
                if (ct > 0) //has to be elements in the collection
                {
                    ct = 0;

                    foreach (BundleElement cur in nets.Elements)
                    {
                        NetDTO tmp = new NetDTO();
                        tmp.Key = cur.LocalKey;
                        tmp.Name = cur.DisplayName;
                        det.Nets.Add(tmp);
                        ct++;
                    }

                    if (ct > 0)
                    {
                        ct = 0;

                        foreach (BundleElement cur in fishSpecies.Elements)
                        {
                            FishSpeciesDTO tmp = new FishSpeciesDTO();
                            tmp.Key = cur.LocalKey;
                            tmp.Name = cur.DisplayName;
                            det.FishSpecies.Add(tmp);
                            ct++;
                        }

                        if (ct > 0)
                        {
                            if (macroSpecies!=null)
                            {
                                ct = 0;
                                foreach (BundleElement cur in macroSpecies.Elements)
                                {
                                    MacroSpeciesDTO tmp = new MacroSpeciesDTO();
                                    tmp.Key = cur.LocalKey;
                                    tmp.Name = cur.DisplayName;
                                    det.MacroSpecies.Add(tmp);
                                    ct++;
                                }
                            }

                            if (ct > 0)
                            {
                                det.Validate();
                                if (det.ValidationIssues.Count == 0)
                                {
                                    IFileStoreProvider prov = this.FileStore;

                                    if (prov != null)
                                    {
                                        FilestoreFile fil = prov.Make(id);
                                        if (fil != null)
                                        {
                                            ExcelFishDET excel = new ExcelFishDET(det);
                                            excel.Save(fil);
                                            fil.Flush();
                                            fil.Seek(0, System.IO.SeekOrigin.Begin);
                                            return fil;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public override UpdateStatus Update(FilestoreFile tempFileUpload, SampleEventMap map)
        {
            if (tempFileUpload != null)
            {
                FilestoreFile oldFile = this.Get(map);
                if (oldFile != null)
                {
                    if (this.CanModify(map))
                    {
                        FishDET curdet = new FishDET();
                        ExcelFishDET curexcel = new ExcelFishDET(curdet);
                        curexcel.Load(tempFileUpload);
                        ValidationIssues curIssues = curdet.ValidationIssues;
                        if (curIssues.Count < 1)
                        {
                            FishDET olddet = new FishDET();
                            ExcelFishDET excel = new ExcelFishDET(olddet);
                            excel.Load(oldFile);
                            ValidationIssues issues = olddet.ValidationIssues;
                            if (issues.Count < 1) //should be this is the old file
                            {
                                UpdateStatus status = null;
                                if (olddet.CatchEfforts.Count < 1) //old file has no data - bare det
                                {
                                    if (curdet.CatchEfforts.Count < 1) //nothing to do really, new file has no data either
                                    {
                                        olddet = null;
                                        excel = null;
                                        oldFile.Dispose();
                                        curdet = null;
                                        curexcel = null;
                                        FileStoreManager.Instance.GetProvider().Delete(tempFileUpload);

                                        status = new UpdateStatus(UpdateIssue.AllOk);
                                        status.Add(new IssueNotice("NoDataInEither", "nothing to do"));
                                        return status;
                                    }

                                    SampleEventMapItem item = map.Get(KnownDetType.Fish);
                                    if (item != null)
                                    {
                                        olddet = null; //release old reference
                                        excel = null; //release old reference

                                        //ok, no data in old file, but data in new file -- we have work to do -- handle first-time data insertion
                                        ICatchEffortProvider depl = FishManager.Instance.GetCatchEffortProvider(this.Context);
                                        ICatchHaulProvider meas = FishManager.Instance.GetCatchHaulProvider(this.Context);
                                        IFishProvider fi = FishManager.Instance.GetFishProvider(this.Context);
                                        if (depl != null && meas != null && fi != null)
                                        {
                                            if (depl.CanCreate())
                                            {
                                                List<EntityBundle> bundles = this.GetBundles(map, KnownDetType.Fish);
                                                if (bundles != null && bundles.Count >= 3)
                                                {
                                                    EntityBundle mac = null;
                                                    if (bundles.Count > 3)
                                                        mac = bundles[3];
                                                    status = this.InitialLoad(map.SampleEventId, bundles[0], bundles[1], bundles[2], mac, curdet, depl, meas, fi, item.IsPrivate);
                                                    if (status != null && status.Issue == UpdateIssue.AllOk) //success, so we overwrite the file
                                                    {
                                                        curdet = null;
                                                        curexcel = null;
                                                        olddet = null;
                                                        excel = null;
                                                        IFileStoreProvider ip = FileStoreManager.Instance.GetProvider();
                                                        if (ip.Replace(tempFileUpload, oldFile)) //overwrite the existing file with the new one
                                                        {
                                                            tempFileUpload.Dispose();
                                                            oldFile.Dispose();
                                                            //ip.Delete(tempFileUpload); //may still be references so delete can fail
                                                            return status; //we're done here
                                                        }
                                                        else
                                                        {
                                                            status = new UpdateStatus(UpdateIssue.DataIssue);
                                                            status.Add(new IssueNotice("File", "Failed to replace file"));
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    status = new UpdateStatus(UpdateIssue.DataIssue);
                                                    status.Add(new IssueNotice("Bundles", "Failed to get bundles"));
                                                }
                                            }
                                            else
                                            {
                                                status = new UpdateStatus(UpdateIssue.Security);
                                                status.Add(new IssueNotice("Permissions", "create denied"));
                                            }
                                        }
                                        else
                                        {
                                            status = new UpdateStatus(UpdateIssue.SystemIssue);
                                            status.Add(new IssueNotice("Failure", "Failed to get provider"));
                                        }
                                    }
                                    else
                                    {
                                        status = new UpdateStatus(UpdateIssue.NoExistingFile);
                                        status.Add(new IssueNotice("NoMapEntry", "Failed to find map reference"));
                                    }
                                    return status;
                                }
                                else //crap -- this is an update where existing file already had data
                                {
                                    ICatchEffortProvider depl = FishManager.Instance.GetCatchEffortProvider(this.Context);
                                    ICatchHaulProvider meas = FishManager.Instance.GetCatchHaulProvider(this.Context);
                                    IFishProvider fi = FishManager.Instance.GetFishProvider(this.Context);
                                    if (depl != null && meas != null && fi!=null)
                                    {
                                        if (depl.CanCreate())
                                        {
                                            SampleEventMapItem item = map.Get(KnownDetType.Fish);
                                            List<EntityBundle> bundles = this.GetBundles(map, KnownDetType.Fish);
                                            if (item!=null && bundles != null && bundles.Count >= 3)
                                            {
                                                EntityBundle mac = null;
                                                if (bundles.Count > 3)
                                                    mac = bundles[3];
                                                status = this.DeltaLoad(map.SampleEventId, bundles[0], bundles[1], bundles[2], mac, curdet, olddet, depl, meas, fi, map);
                                                if (status != null && status.Issue == UpdateIssue.AllOk) //success, so we overwrite the file
                                                {
                                                    curdet = null;
                                                    curexcel = null;
                                                    olddet = null;
                                                    excel = null;
                                                    //NOTE -- making new file the permanent file and removing old file, then updating map for new file
                                                    IFileStoreProvider ip = FileStoreManager.Instance.GetProvider();
                                                    if (ip.Replace(tempFileUpload, oldFile)) //overwrite the existing file with the new one
                                                    {
                                                        tempFileUpload.Dispose();
                                                        oldFile.Dispose();
                                                        //ip.Delete(tempFileUpload); //may still be references so delete can fail
                                                        return status; //we're done here
                                                    }
                                                    else
                                                    {
                                                        status = new UpdateStatus(UpdateIssue.DataIssue);
                                                        status.Add(new IssueNotice("File", "Failed to replace file"));
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                status = new UpdateStatus(UpdateIssue.DataIssue);
                                                status.Add(new IssueNotice("Bundles", "Failed to get bundles"));
                                            }
                                        }
                                        else
                                        {
                                            status = new UpdateStatus(UpdateIssue.Security);
                                            status.Add(new IssueNotice("Permissions", "create denied"));
                                        }
                                    }
                                    else
                                    {
                                        status = new UpdateStatus(UpdateIssue.SystemIssue);
                                        status.Add(new IssueNotice("Failure", "Failed to get provider"));
                                    }
                                    return status;
                                }
                            }
                            else
                            {
                                UpdateStatus stat = new UpdateStatus(UpdateIssue.NoExistingFile);
                                foreach (ValidationIssue cur in issues)
                                {
                                    stat.Add(new IssueNotice(cur.IssueCode.ToString(), cur.IssueMessage));
                                }
                                return stat;
                            }
                        }
                        else
                        {
                            UpdateStatus stat = new UpdateStatus(UpdateIssue.FileValidationIssues);
                            foreach (ValidationIssue cur in curIssues)
                            {
                                stat.Add(new IssueNotice(cur.IssueCode.ToString(), cur.IssueMessage));
                            }
                            return stat;
                        }
                    }
                    else
                    {
                        UpdateStatus status = new UpdateStatus(UpdateIssue.Security);
                        status.Add(new IssueNotice("Permissions", "create denied"));
                        return status;
                    }
                }
                return new UpdateStatus(UpdateIssue.NoExistingFile);
            }
            return new UpdateStatus(UpdateIssue.NoFilePosted);
        }

        private UpdateStatus InitialLoad(CompoundIdentity seId, EntityBundle sites, EntityBundle nets, EntityBundle fishSpecies, EntityBundle macroSpecies, FishDET curdet, ICatchEffortProvider depl, ICatchHaulProvider haul, IFishProvider fish, bool isPrivate)
        {
            UpdateStatus stat = null;
            if (sites == null || nets == null || fishSpecies == null || seId == null || seId.IsEmpty || macroSpecies == null)
            {
                stat = new UpdateStatus(UpdateIssue.DataIssue);
                stat.Add(new IssueNotice("BundlesOrEvent", "Null value"));
            }
            else
            {
                //TODO -- may need to do case insensitive ops or not -- verify how excel part is implemented

                Dictionary<string, CompoundIdentity> deployIds = new Dictionary<string, CompoundIdentity>();
                BundleElement elem;
                foreach (CatchEffortDTO cur in curdet.CatchEfforts.Values)
                {
                    if (cur != null && !deployIds.ContainsKey(cur.CatchId))
                    {
                        elem = sites.Get(cur.SiteId);
                        if (elem != null)
                        {
                            CompoundIdentity siteId = elem.EntityId;
                            Point2<double> tmpPoint = null;
                            if (cur.CatchX.HasValue && cur.CatchY.HasValue)
                            {
                                if (!(cur.CatchX.Value.IsInfiniteOrNaN() && cur.CatchY.Value.IsInfiniteOrNaN()))
                                    tmpPoint = GeometryFactory2Double.Instance.ConstructPoint(cur.CatchX.Value, cur.CatchY.Value);
                            }

                            double dep = cur.Depth.HasValue ? cur.Depth.Value : double.NaN;
                            double dox = cur.DO.HasValue ? cur.DO.Value : double.NaN;
                            double pH = cur.pH.HasValue ? cur.pH.Value : double.NaN;
                            double sal = cur.Salinity.HasValue ? cur.Salinity.Value : double.NaN;
                            double tmp = cur.Temp.HasValue ? cur.Temp.Value : double.NaN;
                            double vel = cur.Velocity.HasValue ? cur.Velocity.Value : double.NaN;

                            CatchEffort cat = depl.Create(seId, siteId, cur.DateTime, cur.Comments, isPrivate, cur.CatchMethod, cur.HabitatStrata, tmpPoint, (float)dep, (float)pH, (float)tmp, (float)dox, (float)sal, (float)vel);
                            if (cat != null)
                                deployIds[cur.CatchId] = cat.Identity;
                            else
                                stat = Add("Create", "Deployment", UpdateIssue.DataIssue, stat);
                        }
                        else
                            stat = Add("SiteCode", "Empty or missing", UpdateIssue.DataIssue, stat);
                    }
                    else
                        stat = Add("CatchId", "Empty or missing", UpdateIssue.DataIssue, stat);
                }

                foreach (CatchMetricDTO cur in curdet.CatchMetrics.Values)
                {
                    if (cur != null && deployIds.ContainsKey(cur.CatchId))
                    {
                        CompoundIdentity depId = deployIds[cur.CatchId];
                        if (depId!=null)
                        {
                            try
                            {
                                CatchMetric met = haul.CreateMetric(depId, (float)cur.Value, cur.MetricType, cur.Comments);
                                if (met==null)
                                    stat = Add("Create", "CatchMetric", UpdateIssue.DataIssue, stat);
                            }
                            catch
                            {
                                stat = Add("Create", "CatchMetric", UpdateIssue.DataIssue, stat);
                            }
                        }
                        else
                            stat = Add("CatchId", "Fail to lookup in CatchMetric", UpdateIssue.DataIssue, stat);
                    }
                    else
                        stat = Add("CatchId", "Fail to lookup in CatchMetric", UpdateIssue.DataIssue, stat);
                }

                foreach (NetHaulEventDTO cur in curdet.NetHaulEvents.Values)
                {
                    if (cur != null && deployIds.ContainsKey(cur.CatchId))
                    {
                        CompoundIdentity depId = deployIds[cur.CatchId];
                        if (depId != null)
                        {
                            elem = nets.Get(cur.NetId);
                            if (elem != null)
                            {
                                CompoundIdentity netId = elem.EntityId;
                                try
                                {
                                    double area = cur.AreaSampled.HasValue ? cur.AreaSampled.Value : double.NaN;
                                    double vol = cur.VolumeSampled.HasValue ? cur.VolumeSampled.Value : double.NaN;

                                    NetHaulEvent met = haul.CreateHaul(depId, netId, (float)area, (float)vol, cur.Comments);
                                    if (met == null)
                                        stat = Add("Create", "NetHaulEvent", UpdateIssue.DataIssue, stat);
                                }
                                catch
                                {
                                    stat = Add("Create", "NetHaulEvent", UpdateIssue.DataIssue, stat);
                                }
                            }
                            else
                                stat = Add("NetId", "Fail to lookup in NetHaulEvent", UpdateIssue.DataIssue, stat);
                        }
                        else
                            stat = Add("CatchId", "Fail to lookup in NetHaulEvent", UpdateIssue.DataIssue, stat);
                    }
                    else
                        stat = Add("CatchId", "Fail to lookup in NetHaulEvent", UpdateIssue.DataIssue, stat);
                }

                foreach (FishCountDTO cur in curdet.FishCounts.Values)
                {
                    if (cur != null && deployIds.ContainsKey(cur.CatchId))
                    {
                        CompoundIdentity depId = deployIds[cur.CatchId];
                        if (depId != null)
                        {
                            elem = fishSpecies.Get(cur.SpeciesId);
                            if (elem != null)
                            {
                                CompoundIdentity fishId = elem.EntityId;
                                try
                                {
                                    FishCount met = haul.CreateFishCount(depId, fishId, cur.Count.HasValue?cur.Count.Value:0, cur.Comments);
                                    if (met == null)
                                        stat = Add("Create", "FishCount", UpdateIssue.DataIssue, stat);
                                }
                                catch
                                {
                                    stat = Add("Create", "FishCount", UpdateIssue.DataIssue, stat);
                                }
                            }
                            else
                                stat = Add("TaxaId", "Fail to lookup in FishCount", UpdateIssue.DataIssue, stat);
                        }
                        else
                            stat = Add("CatchId", "Fail to lookup in FishCount", UpdateIssue.DataIssue, stat);
                    }
                    else
                        stat = Add("CatchId", "Fail to lookup in FishCount", UpdateIssue.DataIssue, stat);
                }

                //keep track of all fishids as we go for lookups
                Dictionary<string, Guid> fishIds = new Dictionary<string, Guid>();
                foreach (FishDTO cur in curdet.Fish.Values)
                {
                    if (cur != null && deployIds.ContainsKey(cur.CatchId))
                    {
                        CompoundIdentity depId = deployIds[cur.CatchId];
                        if (depId != null)
                        {
                            elem = fishSpecies.Get(cur.SpeciesId);
                            if (elem != null)
                            {
                                CompoundIdentity fishId = elem.EntityId;
                                try
                                {
                                    double std = cur.LengthStandard.HasValue ? cur.LengthStandard.Value : double.NaN;
                                    double frk = cur.LengthFork.HasValue ? cur.LengthFork.Value : double.NaN;
                                    double tot = cur.LengthTotal.HasValue ? cur.LengthTotal.Value : double.NaN;
                                    double wei = cur.Mass.HasValue ? cur.Mass.Value : double.NaN;

                                    Osrs.Oncor.WellKnown.Fish.Fish met = fish.CreateFish(depId, fishId, (float)std, (float)frk, (float)tot, (float)wei, cur.AdClipped, cur.CodedWireTag, cur.Comments);
                                    fishIds[cur.FishId] = met.Identity;
                                    if (met == null)
                                        stat = Add("Create", "Fish", UpdateIssue.DataIssue, stat);
                                }
                                catch
                                {
                                    stat = Add("Create", "Fish", UpdateIssue.DataIssue, stat);
                                }
                            }
                            else
                                stat = Add("TaxaId", "Fail to lookup in Fish", UpdateIssue.DataIssue, stat);
                        }
                        else
                            stat = Add("CatchId", "Fail to lookup in Fish", UpdateIssue.DataIssue, stat);
                    }
                    else
                        stat = Add("CatchId", "Fail to lookup in Fish", UpdateIssue.DataIssue, stat);
                }

                foreach(DietDTO cur in curdet.Diet.Values)
                {
                    if (cur != null && fishIds.ContainsKey(cur.FishId))
                    {
                        Guid depId = fishIds[cur.FishId];
                        if (!Guid.Empty.Equals(depId))
                        {
                            elem = macroSpecies.Get(cur.SpeciesId);
                            if (elem != null || string.IsNullOrEmpty(cur.SpeciesId)) //allow for empty gut
                            {
                                CompoundIdentity taxaId;
                                if (elem != null)
                                    taxaId = elem.EntityId;
                                else
                                    taxaId = new CompoundIdentity(Guid.Empty, Guid.Empty);
                                try
                                {
                                    double sam = cur.SampleMass.HasValue ? cur.SampleMass.Value : double.NaN;
                                    double ind = cur.IndividualMass.HasValue ? cur.IndividualMass.Value : double.NaN;
                                    uint? wh = null;
                                    if (cur.WholeAnimalsWeighed.HasValue)
                                        wh = (uint)cur.WholeAnimalsWeighed.Value;

                                    FishDiet met = null;
                                    if (taxaId.IsEmpty)
                                    {
                                        if (cur.Count==0) //the only way its legal
                                            met = fish.CreateFishDiet(depId, taxaId, cur.GutSampleId, cur.VialId, cur.LifeStage, cur.Count, (float)sam, (float)ind, wh, cur.Comments);
                                    }
                                    else
                                        met = fish.CreateFishDiet(depId, taxaId, cur.GutSampleId, cur.VialId, cur.LifeStage, cur.Count, (float)sam, (float)ind, wh, cur.Comments);

                                    if (met == null)
                                        stat = Add("Create", "FishDiet", UpdateIssue.DataIssue, stat);
                                }
                                catch
                                {
                                    stat = Add("Create", "FishDiet", UpdateIssue.DataIssue, stat);
                                }
                            }
                            else
                                stat = Add("TaxaId", "Fail to lookup in FishDiet", UpdateIssue.DataIssue, stat);
                        }
                        else
                            stat = Add("FishId", "Fail to lookup in FishDiet", UpdateIssue.DataIssue, stat);
                    }
                    else
                        stat = Add("FishId", "Fail to lookup in FishDiet", UpdateIssue.DataIssue, stat);
                }

                foreach (GeneticDTO cur in curdet.Genetics.Values)
                {
                    if (cur != null && fishIds.ContainsKey(cur.FishId))
                    {
                        Guid depId = fishIds[cur.FishId];
                        if (!Guid.Empty.Equals(depId))
                        {
                            try
                            {
                                StockEstimates est = new StockEstimates();
                                if (!string.IsNullOrEmpty(cur.BestStockEstimate) && cur.ProbabilityBest.HasValue)
                                    est[cur.BestStockEstimate] = (float)cur.ProbabilityBest.Value;
                                if (!string.IsNullOrEmpty(cur.SecondStockEstimate) && cur.ProbabilitySecondBest.HasValue)
                                    est[cur.SecondStockEstimate] = (float)cur.ProbabilitySecondBest.Value;
                                if (!string.IsNullOrEmpty(cur.ThirdStockEstimate) && cur.ProbabilityThirdBest.HasValue)
                                    est[cur.ThirdStockEstimate] = (float)cur.ProbabilityThirdBest.Value;

                                FishGenetics met = fish.CreateFishGenetics(depId, cur.GeneticSampleId, cur.LabSampleId, est, cur.Comments);
                                if (met == null)
                                    stat = Add("Create", "FishIdTag", UpdateIssue.DataIssue, stat);
                            }
                            catch
                            {
                                stat = Add("Create", "FishIdTag", UpdateIssue.DataIssue, stat);
                            }
                        }
                        else
                            stat = Add("FishId", "Fail to lookup in FishIdTag", UpdateIssue.DataIssue, stat);
                    }
                    else
                        stat = Add("FishId", "Fail to lookup in FishIdTag", UpdateIssue.DataIssue, stat);
                }

                foreach (IdTagDTO cur in curdet.IdTags.Values)
                {
                    if (cur != null && fishIds.ContainsKey(cur.FishId))
                    {
                        Guid depId = fishIds[cur.FishId];
                        if (!Guid.Empty.Equals(depId))
                        {
                            try
                            {
                                FishIdTag met = fish.CreateIdTag(depId, cur.TagCode, cur.TagType, cur.TagManufacturer, cur.Comments);
                                if (met == null)
                                    stat = Add("Create", "FishIdTag", UpdateIssue.DataIssue, stat);
                            }
                            catch
                            {
                                stat = Add("Create", "FishIdTag", UpdateIssue.DataIssue, stat);
                            }
                        }
                        else
                            stat = Add("FishId", "Fail to lookup in FishIdTag", UpdateIssue.DataIssue, stat);
                    }
                    else
                        stat = Add("FishId", "Fail to lookup in FishIdTag", UpdateIssue.DataIssue, stat);
                }

                if (stat == null)
                {
                    stat = new UpdateStatus(UpdateIssue.AllOk);
                    stat.Add(new IssueNotice("NoIssues", "No issues"));
                }
            }
            return stat;
        }

        private UpdateStatus DeltaLoad(CompoundIdentity seId, EntityBundle sites, EntityBundle nets, EntityBundle fishSpecies, EntityBundle macroSpecies, FishDET curdet, FishDET olddet, ICatchEffortProvider depl, ICatchHaulProvider haul, IFishProvider fish, SampleEventMap map)
        {
            UpdateStatus stat = null;
            if (curdet.CatchEfforts.Count < 1) //ok, old file had data, this file has none
            {
                //what do we want to do? -- could just do a delete all
                stat = new UpdateStatus(UpdateIssue.DataIssue);
                stat.Add(new IssueNotice("NoData", "Delete then create"));
                SampleEventMapItem tmp = map.Get(KnownDetType.Fish); //fetch it back
                if (this.Delete(map, false))
                {
                    stat = new UpdateStatus(UpdateIssue.AllOk);
                    stat.Add(new IssueNotice("NoIssues", "No issues"));
                }
                else
                    this.Add("Failure", "Delete", UpdateIssue.Security, stat);
            }
            else
            {
                //data in both files, this is a change/append situation
                stat = new UpdateStatus(UpdateIssue.DataIssue);
                stat.Add(new IssueNotice("NoData", "Delete then create"));
                SampleEventMapItem tmp = map.Get(KnownDetType.Fish); //fetch it back
                if (this.DeleteData(map))
                {
                    if (this.InitialLoad(seId, sites, nets, fishSpecies, macroSpecies, curdet, depl, haul, fish, tmp.IsPrivate) != null)
                    {
                        stat = new UpdateStatus(UpdateIssue.AllOk);
                        stat.Add(new IssueNotice("NoIssues", "No issues"));
                    }
                    else
                        this.Add("Failure", "Create", UpdateIssue.Security, stat);
                }
                else
                    this.Add("Failure", "Delete", UpdateIssue.Security, stat);
            }

            return stat;
        }

        public override FilestoreFile Get(SampleEventMap map)
        {
            if (map != null)
            {
                SampleEventMapItem id = map.Get(KnownDetType.Fish);
                if (id != null)
                {
                    IFileStoreProvider p = FileStore;
                    if (p != null)
                        return p.Get(id.DetId); //TODO add validation checking if we want
                }
            }
            return null;
        }

        private bool DeleteData(SampleEventMap map)
        {
            ICatchEffortProvider wqProv = FishManager.Instance.GetCatchEffortProvider(this.Context);
            if (wqProv != null && wqProv.CanDelete())
            {
                IEnumerable<CatchEffort> deploys = wqProv.GetForSampleEvent(map.SampleEventId);
                foreach (CatchEffort cur in deploys)
                {
                    wqProv.Delete(cur);
                }
                return true;
            }
            return false;
        }

        public override bool Delete(SampleEventMap map, bool fileOnly)
        {
            if (map != null)
            {
                SampleEventMapItem id = map.Get(KnownDetType.Fish);
                if (id != null)
                {
                    IFileStoreProvider p = FileStore;
                    if (p != null)
                    {
                        ICatchEffortProvider wqProv = FishManager.Instance.GetCatchEffortProvider(this.Context);
                        if (wqProv != null && wqProv.CanDelete())
                        {
                            if (p.Delete(id.DetId))
                            {
                                if (fileOnly)
                                    return true;
                                return DeleteData(map);
                            }
                        }
                    }
                }
            }

            return false;
        }

        public FishDetProcessor(UserSecurityContext ctx) : base(ctx)
        { }
    }
}
