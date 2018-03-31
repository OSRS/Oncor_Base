using Osrs.Security;
using Pnnl.Oncor.DetProcessing;
using System;
using System.Collections.Generic;
using Osrs.Oncor.FileStore;
using Osrs.Oncor.WellKnown.Vegetation.Module;
using Osrs.Oncor.WellKnown.Vegetation;
using Osrs.Oncor.DetFactories.DETs;
using ExcelDETs.DETs;
using Osrs.Oncor.DetFactories;
using Osrs.Oncor.EntityBundles;
using Osrs.Data;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;
using Osrs.Numerics.Spatial.Geometry;
using Osrs.Numerics;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;

namespace Pnnl.Oncor.DetProcessor.Veg
{
    public sealed class VegDetProcessor : DetProcessorBase
    {
        public FilestoreFile Create(SampleEventMap map, EntityBundle sites, EntityBundle plotTypes, EntityBundle shrubSpecies, EntityBundle treeSpecies, EntityBundle herbSpecies, EntityBundle nonLiving, bool isPrivate)
        {
            if (this.CanModify(map))
            {
                Guid id = Guid.NewGuid();
                VegDET det = new VegDET();
                det.Id = id;
                det.Owner = "originator:" + sites.PrincipalOrgId.Identity.ToString() + ":" + plotTypes.PrincipalOrgId.Identity.ToString();

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

                    foreach (BundleElement cur in plotTypes.Elements)
                    {
                        PlotTypeDTO tmp = new PlotTypeDTO();
                        tmp.Key = cur.LocalKey;
                        tmp.Name = cur.DisplayName;
                        det.PlotTypes.Add(tmp);
                        ct++;
                    }

                    if (ct > 0)
                    {
                        ct = 0;

                        foreach (BundleElement cur in shrubSpecies.Elements)
                        {
                            SpeciesDTO tmp = new SpeciesDTO();
                            tmp.Key = cur.LocalKey;
                            tmp.Name = cur.DisplayName;
                            det.ShrubSpecies.Add(tmp);
                            ct++;
                        }

                        if (treeSpecies != null)
                        {
                            foreach (BundleElement cur in treeSpecies.Elements)
                            {
                                SpeciesDTO tmp = new SpeciesDTO();
                                tmp.Key = cur.LocalKey;
                                tmp.Name = cur.DisplayName;
                                det.TreeSpecies.Add(tmp);
                                ct++;
                            }
                        }

                        if (herbSpecies != null)
                        {
                            foreach (BundleElement cur in herbSpecies.Elements)
                            {
                                SpeciesDTO tmp = new SpeciesDTO();
                                tmp.Key = cur.LocalKey;
                                tmp.Name = cur.DisplayName;
                                det.HerbSpecies.Add(tmp);
                                ct++;
                            }
                        }

                        if (nonLiving != null)
                        {
                            foreach (BundleElement cur in nonLiving.Elements)
                            {
                                SpeciesDTO tmp = new SpeciesDTO();
                                tmp.Key = cur.LocalKey;
                                tmp.Name = cur.DisplayName;
                                det.NonLiving.Add(tmp);
                            }
                        }

                        if (ct > 0) //have to have at least one of herb, tree, shrub to POSSIBLY be valid
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
                                        ExcelVegDET excel = new ExcelVegDET(det);
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

            return null;
        }

        private UpdateStatus InitialLoad(CompoundIdentity seId, EntityBundle sites, EntityBundle plotTypes, EntityBundle shrubSpecies, EntityBundle treeSpecies, EntityBundle herbSpecies, EntityBundle nonLiving, VegDET curdet, IVegSurveyProvider depl, IVegSampleProvider meas, bool isPrivate)
        {
            UpdateStatus stat = null;
            if (sites == null || plotTypes == null || seId == null || seId.IsEmpty)
            {
                stat = new UpdateStatus(UpdateIssue.DataIssue);
                stat.Add(new IssueNotice("BundlesOrEvent", "Null value"));
            }
            else
            {
                //TODO -- may need to do case insensitive ops or not -- verify how excel part is implemented

                Dictionary<string, CompoundIdentity> deployIds = new Dictionary<string, CompoundIdentity>();
                BundleElement elem;
                foreach (VegSurveyDTO cur in curdet.Surveys.Values)
                {
                    if (cur != null && !deployIds.ContainsKey(cur.SurveyId))
                    {
                        elem = sites.Get(cur.SiteId);
                        if (elem != null)
                        {
                            CompoundIdentity siteId = elem.EntityId;
                            if (cur.PlotTypeId != null)
                                elem = plotTypes.Get(cur.PlotTypeId);
                            else
                                elem = null;

                            if (elem != null || cur.PlotTypeId == null)
                            {
                                CompoundIdentity plotTypeId;
                                if (elem != null)
                                    plotTypeId = elem.EntityId;
                                else
                                    plotTypeId = null;

                                Point2<double> tmpPoint = null;
                                if (cur.AdHocLat.HasValue && cur.AdHocLon.HasValue)
                                {
                                    if (!(cur.AdHocLat.Value.IsInfiniteOrNaN() && cur.AdHocLon.Value.IsInfiniteOrNaN()))
                                        tmpPoint = GeometryFactory2Double.Instance.ConstructPoint(cur.AdHocLat.Value, cur.AdHocLon.Value);
                                }

                                float area = float.NaN;
                                if (cur.Area.HasValue)
                                    area = (float)cur.Area.Value;

                                float minElev = float.NaN;
                                float maxElev = float.NaN;

                                if (cur.MinElevation.HasValue)
                                    minElev = (float)cur.MinElevation.Value;
                                else if (cur.MaxElevation.HasValue)
                                    minElev = (float)cur.MaxElevation.Value; //lets min/max be the same when only one is provided
                                if (cur.MaxElevation.HasValue)
                                    maxElev = (float)cur.MaxElevation.Value;
                                else
                                    maxElev = minElev; //lets min/max be the same when only one is provided


                                VegSurvey dep = depl.Create(seId, siteId, plotTypeId, tmpPoint, area, minElev, maxElev, cur.Comments, isPrivate);
                                if (dep != null)
                                {
                                    deployIds[cur.SurveyId] = dep.Identity;
                                }
                                else
                                    stat = Add("Create", "Deployment", UpdateIssue.DataIssue, stat);
                            }
                            else
                                stat = Add("PlotType", "Empty or missing", UpdateIssue.DataIssue, stat);
                        }
                        else
                            stat = Add("SiteCode", "Empty or missing", UpdateIssue.DataIssue, stat);
                    }
                    else
                        stat = Add("DeployCode", "Empty or missing", UpdateIssue.DataIssue, stat);
                }

                if (stat == null)
                {
                    stat = new UpdateStatus(UpdateIssue.DataIssue);
                    IEnumerable<VegSamplesDTO> samps = ToSamples(curdet, deployIds, sites, plotTypes, shrubSpecies, treeSpecies, herbSpecies, nonLiving);
                    if (samps != null)
                    {
                        foreach (VegSamplesDTO cur in samps)
                        {
                            if (meas.Create(cur) == null)
                                stat.Add(new IssueNotice("InsertBatch", "Failed inserting a batch of measurements"));
                        }
                        if (stat.Count < 1)
                            stat = null;
                    }
                    else
                        stat.Add(new IssueNotice("CreateSamples", "Failed to generate sample batches from file"));
                }

                if (stat == null)
                {
                    stat = new UpdateStatus(UpdateIssue.AllOk);
                    stat.Add(new IssueNotice("NoIssues", "No issues"));
                }
            }
            return stat;
        }

        private IEnumerable<VegSamplesDTO> ToSamples(VegDET det, Dictionary<string, CompoundIdentity> deployIds, EntityBundle sites, EntityBundle plotTypes, EntityBundle shrubSpecies, EntityBundle treeSpecies, EntityBundle herbSpecies, EntityBundle nonLiving)
        {
            Dictionary<string, VegSamplesDTO> samples = new Dictionary<string, VegSamplesDTO>();
            foreach(KeyValuePair<string, CompoundIdentity> cur in deployIds)
            {
                samples.Add(cur.Key, VegSamplesDTO.Create(cur.Value));
            }

            Dictionary<Tuple<CompoundIdentity, Point2<double>>, Dictionary<DateTime, VegSampleDTO>> elements = new Dictionary<Tuple<CompoundIdentity, Point2<double>>, Dictionary<DateTime, VegSampleDTO>>();
            foreach (VegElevationDTO cur in det.Elevations.Values)
            {
                if (cur.MeasureDateTime.HasValue)
                {
                    Dictionary<DateTime, VegSampleDTO> element = null;
                    Tuple<CompoundIdentity, Point2<double>> loc = GetLocation(cur, sites);
                    if (!elements.ContainsKey(loc))
                    {
                        element = new Dictionary<DateTime, VegSampleDTO>();
                        elements[loc] = element;
                    }
                    else
                        element = elements[loc];

                    float min = cur.MinElevation.HasValue ? (float)cur.MinElevation.Value : float.NaN;
                    float max = cur.MaxElevation.HasValue ? (float)cur.MaxElevation.Value : float.NaN;

                    element[cur.MeasureDateTime.Value] = new VegSampleDTO(loc.Item1, cur.MeasureDateTime.Value, loc.Item2, min, max);
                }
            }

            foreach(VegHerbDTO cur in det.Herbs.Values)
            {
                if (cur.MeasureDateTime.HasValue)
                {
                    VegSampleDTO it = GetSample(cur, sites, elements);
                    if (it != null)
                    {
                        float pct = cur.PctCover.HasValue ? (float)cur.PctCover.Value : float.NaN;
                        it.Add(new VegHerbSampleDTO(GetTaxa(cur.HerbSpeciesId, herbSpecies, nonLiving), pct, cur.Comments));
                        if (!samples[cur.SurveyId].ContainsKey(it.When))
                            samples[cur.SurveyId].Add(it);
                    }
                }
            }

            foreach (VegShrubDTO cur in det.Shrubs.Values)
            {
                if (cur.MeasureDateTime.HasValue)
                {
                    VegSampleDTO it = GetSample(cur, sites, elements);
                    if (it != null)
                    {
                        uint pct = cur.Count.HasValue ? cur.Count.Value : 0;
                        it.Add(new VegShrubSampleDTO(GetTaxa(cur.ShrubSpeciesId, shrubSpecies, nonLiving), cur.SizeClass, pct, cur.Comments));
                        if (!samples[cur.SurveyId].ContainsKey(it.When))
                            samples[cur.SurveyId].Add(it);
                    }
                }
            }

            foreach (VegTreeDTO cur in det.Trees.Values)
            {
                if (cur.MeasureDateTime.HasValue)
                {
                    VegSampleDTO it = GetSample(cur, sites, elements);
                    if (it != null)
                    {
                        float pct = cur.DBH.HasValue ? (float)cur.DBH.Value : float.NaN;
                        it.Add(new VegTreeSampleDTO(GetTaxa(cur.TreeSpeciesId, treeSpecies, nonLiving), pct, cur.Comments));
                        if (!samples[cur.SurveyId].ContainsKey(it.When))
                            samples[cur.SurveyId].Add(it);
                    }
                }
            }

            return samples.Values;
        }

        private VegSampleDTO GetSample(VegDataDTO item, EntityBundle sites, Dictionary<Tuple<CompoundIdentity, Point2<double>>, Dictionary<DateTime, VegSampleDTO>> elements)
        {
            Dictionary<DateTime, VegSampleDTO> element = null;
            Tuple<CompoundIdentity, Point2<double>> loc = GetLocation(item, sites);
            if (!elements.ContainsKey(loc))
            {
                element = new Dictionary<DateTime, VegSampleDTO>();
                elements[loc] = element;
            }
            else
                element = elements[loc];
            if (element.ContainsKey(item.MeasureDateTime.Value))
                return element[item.MeasureDateTime.Value];

            VegSampleDTO res = new VegSampleDTO(loc.Item1, item.MeasureDateTime.Value, loc.Item2, float.NaN, float.NaN);
            element[item.MeasureDateTime.Value] = res;
            return res;
        }

        private Tuple<CompoundIdentity, Point2<double>> GetLocation(VegDataDTO item, EntityBundle sites)
        {
            CompoundIdentity id = null;
            if (!string.IsNullOrEmpty(item.SiteId) && sites.Contains(item.SiteId))
                id = sites.Get(item.SiteId).EntityId;
            Point2<double> loc = null;
            if (item.AdHocLat.HasValue && item.AdHocLon.HasValue && !MathUtils.IsInfiniteOrNaN(item.AdHocLat.Value) && !MathUtils.IsInfiniteOrNaN(item.AdHocLon.Value))
            {
                loc = GeometryFactory2Double.Instance.ConstructPoint(item.AdHocLon.Value, item.AdHocLat.Value);
            }
            return new Tuple<CompoundIdentity, Point2<double>>(id, loc);
        }

        private CompoundIdentity GetTaxa(string taxaId, EntityBundle taxaUnits, EntityBundle nonLiving)
        {
            if (taxaUnits.Contains(taxaId))
                return taxaUnits.Get(taxaId).EntityId;
            if (nonLiving != null && nonLiving.Contains(taxaId))
                return nonLiving.Get(taxaId).EntityId;
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
                        VegDET curdet = new VegDET();
                        ExcelVegDET curexcel = new ExcelVegDET(curdet);
                        curexcel.Load(tempFileUpload);
                        ValidationIssues curIssues = curdet.ValidationIssues;
                        if (curIssues.Count < 1)
                        {
                            VegDET olddet = new VegDET();
                            ExcelVegDET excel = new ExcelVegDET(olddet);
                            excel.Load(oldFile);
                            ValidationIssues issues = olddet.ValidationIssues;
                            if (issues.Count < 1) //should be this is the old file
                            {
                                UpdateStatus status = null;
                                if (olddet.Surveys.Count < 1) //old file has no data - bare det
                                {
                                    if (curdet.Surveys.Count < 1) //nothing to do really, new file has no data either
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

                                    SampleEventMapItem item = map.Get(KnownDetType.Veg);
                                    if (item != null)
                                    {
                                        olddet = null; //release old reference
                                        excel = null; //release old reference

                                        //ok, no data in old file, but data in new file -- we have work to do -- handle first-time data insertion
                                        IVegSurveyProvider depl = VegetationManager.Instance.GetSurveyProvider(this.Context);
                                        IVegSampleProvider meas = VegetationManager.Instance.GetSampleProvider(this.Context);
                                        if (depl != null && meas != null)
                                        {
                                            if (depl.CanCreate() && meas.CanCreate())
                                            {
                                                List<EntityBundle> bundles = this.GetBundles(map, KnownDetType.Veg);
                                                if (bundles != null && bundles.Count >= 5)
                                                {
                                                    if (bundles.Count>5)
                                                        status = this.InitialLoad(map.SampleEventId, bundles[0], bundles[1], bundles[2], bundles[3], bundles[4], bundles[5], curdet, depl, meas, item.IsPrivate);
                                                    else
                                                        status = this.InitialLoad(map.SampleEventId, bundles[0], bundles[1], bundles[2], bundles[3], bundles[4], null, curdet, depl, meas, item.IsPrivate);
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
                                    IVegSurveyProvider depl = VegetationManager.Instance.GetSurveyProvider(this.Context);
                                    IVegSampleProvider meas = VegetationManager.Instance.GetSampleProvider(this.Context);
                                    if (depl != null && meas != null)
                                    {
                                        if (depl.CanCreate() && meas.CanCreate())
                                        {
                                            SampleEventMapItem item = map.Get(KnownDetType.Veg);
                                            List<EntityBundle> bundles = this.GetBundles(map, KnownDetType.Veg);
                                            if (bundles != null && bundles.Count >= 5)
                                            {
                                                if (bundles.Count > 5)
                                                    status = this.DeltaLoad(map.SampleEventId, bundles[0], bundles[1], bundles[2], bundles[3], bundles[4], bundles[5], curdet, olddet, depl, meas, map);
                                                else
                                                    status = this.DeltaLoad(map.SampleEventId, bundles[0], bundles[1], bundles[2], bundles[3], bundles[4], null, curdet, olddet, depl, meas, map);
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

        private UpdateStatus DeltaLoad(CompoundIdentity seId, EntityBundle sites, EntityBundle plotTypes, EntityBundle shrubSpecies, EntityBundle treeSpecies, EntityBundle herbSpecies, EntityBundle nonLiving, VegDET curdet, VegDET olddet, IVegSurveyProvider depl, IVegSampleProvider meas, SampleEventMap map)
        {
            UpdateStatus stat = null;
            if (curdet.Surveys.Count < 1) //ok, old file had data, this file has none
            {
                //what do we want to do? -- could just do a delete all
                stat = new UpdateStatus(UpdateIssue.DataIssue);
                stat.Add(new IssueNotice("NoData", "Delete then create"));
                SampleEventMapItem tmp = map.Get(KnownDetType.Veg); //fetch it back
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
                SampleEventMapItem tmp = map.Get(KnownDetType.Veg); //fetch it back
                if (this.DeleteData(map))
                {
                    if (this.InitialLoad(seId, sites, plotTypes, shrubSpecies, treeSpecies, herbSpecies, nonLiving, curdet, depl, meas, tmp.IsPrivate) != null)
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
                SampleEventMapItem id = map.Get(KnownDetType.Veg);
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
            IVegSurveyProvider srvProv = VegetationManager.Instance.GetSurveyProvider(this.Context);
            if (srvProv != null && srvProv.CanDelete())
            {
                IEnumerable<VegSurvey> deploys = srvProv.GetForSampleEvent(map.SampleEventId);
                foreach (VegSurvey cur in deploys)
                {
                    srvProv.Delete(cur);
                }
                return true;
            }
            return false;
        }

        public override bool Delete(SampleEventMap map, bool fileOnly)
        {
            if (map != null)
            {
                SampleEventMapItem id = map.Get(KnownDetType.Veg);
                if (id != null)
                {
                    IFileStoreProvider p = FileStore;
                    if (p != null)
                    {
                        IVegSurveyProvider srvProv = VegetationManager.Instance.GetSurveyProvider(this.Context);
                        if (srvProv != null && srvProv.CanDelete())
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

        public VegDetProcessor(UserSecurityContext ctx) : base(ctx)
        { }
    }
}
