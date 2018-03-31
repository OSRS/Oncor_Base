using ExcelDETs.DETs;
using Osrs.Data;
using Osrs.Oncor.DetFactories;
using Osrs.Oncor.DetFactories.DETs;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;
using Osrs.Oncor.EntityBundles;
using Osrs.Oncor.FileStore;
using Osrs.Oncor.WellKnown.WaterQuality;
using Osrs.Oncor.WellKnown.WaterQuality.Module;
using Osrs.Security;
using Pnnl.Oncor.DetProcessing;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.DetProcessor.Wq
{
    //TODO -- add user affiliation checks to all items
    public sealed class WqDetProcessor : DetProcessorBase
    {
        private Osrs.Oncor.WellKnown.WaterQuality.DateRange GetRange(DateTime? start, DateTime? end)
        {
            return Osrs.Oncor.WellKnown.WaterQuality.DateRange.Create(ToUtc(start), ToUtc(end));
        }

        public FilestoreFile Create(SampleEventMap map, EntityBundle sites, EntityBundle instruments)
        {
            if (this.CanModify(map))
            {
                Guid id = Guid.NewGuid();
                WaterQualityDET det = new WaterQualityDET();
                det.Id = id;
                det.Owner = "originator:" + sites.PrincipalOrgId.Identity.ToString() + ":" + instruments.PrincipalOrgId.Identity.ToString();

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

                    foreach (BundleElement cur in instruments.Elements)
                    {
                        InstrumentDTO tmp = new InstrumentDTO();
                        tmp.Key = cur.LocalKey;
                        tmp.Name = cur.DisplayName;
                        det.Instruments.Add(tmp);
                        ct++;
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
                                    ExcelWaterQualityDET excel = new ExcelWaterQualityDET(det);
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
                        WaterQualityDET curdet = new WaterQualityDET();
                        ExcelWaterQualityDET curexcel = new ExcelWaterQualityDET(curdet);
                        curexcel.Load(tempFileUpload);
                        curdet.Validate();
                        ValidationIssues curIssues = curdet.ValidationIssues;
                        if (curIssues.Count < 1)
                        {
                            WaterQualityDET olddet = new WaterQualityDET();
                            ExcelWaterQualityDET excel = new ExcelWaterQualityDET(olddet);
                            excel.Load(oldFile);
                            olddet.Validate();
                            ValidationIssues issues = olddet.ValidationIssues;
                            if (issues.Count < 1) //should be this is the old file
                            {
                                UpdateStatus status = null;
                                if (olddet.Deployments.Count < 1) //old file has no data - bare det
                                {
                                    if (curdet.Deployments.Count < 1) //nothing to do really, new file has no data either
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

                                    SampleEventMapItem item = map.Get(KnownDetType.WaterQuality);
                                    if (item != null)
                                    {
                                        olddet = null; //release old reference
                                        excel = null; //release old reference

                                        //ok, no data in old file, but data in new file -- we have work to do -- handle first-time data insertion
                                        IWQDeploymentProvider depl = WaterQualityManager.Instance.GetDeploymentProvider(this.Context);
                                        IWQMeasurementProvider meas = WaterQualityManager.Instance.GetMeasurementProvider(this.Context);
                                        if (depl != null && meas != null)
                                        {
                                            if (depl.CanCreate() && meas.CanCreate())
                                            {
                                                List<EntityBundle> bundles = this.GetBundles(map, KnownDetType.WaterQuality);
                                                if (bundles != null && bundles.Count == 2)
                                                {
                                                    status = this.InitialLoad(map.SampleEventId, bundles[0], bundles[1], curdet, depl, meas, item.IsPrivate);
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
                                    IWQDeploymentProvider depl = WaterQualityManager.Instance.GetDeploymentProvider(this.Context);
                                    IWQMeasurementProvider meas = WaterQualityManager.Instance.GetMeasurementProvider(this.Context);
                                    if (depl != null && meas != null)
                                    {
                                        if (depl.CanCreate() && meas.CanCreate())
                                        {
                                            SampleEventMapItem item = map.Get(KnownDetType.WaterQuality);
                                            List<EntityBundle> bundles = this.GetBundles(map, KnownDetType.WaterQuality);
                                            if (item!=null && bundles != null && bundles.Count == 2)
                                            { 
                                                status = this.DeltaLoad(map.SampleEventId, bundles[0], bundles[1], curdet, olddet, depl, meas, map);
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

        private UpdateStatus InitialLoad(CompoundIdentity seId, EntityBundle sites, EntityBundle instruments, WaterQualityDET curdet, IWQDeploymentProvider depl, IWQMeasurementProvider meas, bool isPrivate)
        {
            UpdateStatus stat=null;
            if (sites == null || instruments == null || seId == null || seId.IsEmpty)
            {
                stat = new UpdateStatus(UpdateIssue.DataIssue);
                stat.Add(new IssueNotice("BundlesOrEvent", "Null value"));
            }
            else
            {
                //TODO -- may need to do case insensitive ops or not -- verify how excel part is implemented

                Dictionary<string, CompoundIdentity> deployIds = new Dictionary<string, CompoundIdentity>();
                BundleElement elem;
                foreach (DeploymentDTO cur in curdet.Deployments.Values)
                {
                    if (cur != null && !deployIds.ContainsKey(cur.DeployCode))
                    {
                        elem = sites.Get(cur.SiteId);
                        if (elem != null)
                        {
                            CompoundIdentity siteId = elem.EntityId;
                            elem = instruments.Get(cur.InstrumentId);
                            if (elem != null)
                            {
                                CompoundIdentity sensorId = elem.EntityId;
                                WaterQualityDeployment dep = depl.Create(cur.DeployCode, seId, siteId, sensorId, this.GetRange(cur.StartDate, cur.EndDate), cur.Comments, isPrivate);
                                if (dep != null)
                                {
                                    deployIds[dep.Name] = dep.Identity;
                                }
                                else
                                    stat = Add("Create", "Unable to create deployment "+cur.DeployCode, UpdateIssue.DataIssue, stat);
                            }
                            else
                                stat = Add("InstCode", "Empty or invalid Instrument Code on deployment "+cur.DeployCode, UpdateIssue.DataIssue, stat);
                        }
                        else
                            stat = Add("SiteCode", "Empty or invalid Site Code on Deployment "+cur.DeployCode, UpdateIssue.DataIssue, stat);
                    }
                    else
                        stat = Add("DeployCode", "A deployment is missing a deployment code", UpdateIssue.DataIssue, stat);
                }

                Dictionary<CompoundIdentity, WaterQualityMeasurementsDTO> items = new Dictionary<CompoundIdentity, WaterQualityMeasurementsDTO>();
                foreach (MeasurementDTO cur in curdet.Measurements.Values)
                {
                    if (cur!=null && deployIds.ContainsKey(cur.DeployCode) && cur.MeasureDateTime!=null)
                    {
                        CompoundIdentity depId = deployIds[cur.DeployCode];
                        if (depId != null)
                        {
                            if (!items.ContainsKey(depId))
                                items[depId] = WaterQualityMeasurementsDTO.Create(depId);

                            WaterQualityMeasurementsDTO elems = items[depId];
                            elems.Add(new WaterQualityMeasurementDTO(cur.MeasureDateTime.Value, cur.SurfaceElevation, cur.Temperature, cur.pH, cur.DO, cur.Conductivity, cur.Salinity, cur.Velocity));
                        }
                    }
                    else
                        stat = Add("Create", "Unable to create Measurement with key "+cur.DeployCode+":"+cur.MeasureDateTime, UpdateIssue.DataIssue, stat);
                }

                foreach(WaterQualityMeasurementsDTO cur in items.Values)
                {
                    if (cur!=null)
                        meas.Create(cur);
                }

                if (stat == null)
                {
                    stat = new UpdateStatus(UpdateIssue.AllOk);
                    stat.Add(new IssueNotice("NoIssues", "No issues"));
                }
            }
            return stat;
        }

        private UpdateStatus DeltaLoad(CompoundIdentity seId, EntityBundle sites, EntityBundle instruments, WaterQualityDET curdet, WaterQualityDET olddet, IWQDeploymentProvider depl, IWQMeasurementProvider meas, SampleEventMap map)
        {
            UpdateStatus stat = new UpdateStatus(UpdateIssue.SystemIssue); //Not implemented
            if (curdet.Deployments.Count < 1) //ok, old file had data, this file has none
            {
                //what do we want to do? -- could just do a delete all
                stat = new UpdateStatus(UpdateIssue.DataIssue);
                stat.Add(new IssueNotice("NoData", "Delete then create"));
                SampleEventMapItem tmp = map.Get(KnownDetType.WaterQuality); //fetch it back
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
                SampleEventMapItem tmp = map.Get(KnownDetType.WaterQuality); //fetch it back
                if (this.DeleteData(map))
                {
                    if (this.InitialLoad(seId, sites, instruments, curdet, depl, meas, tmp.IsPrivate) != null)
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
            if (map!=null)
            {
                SampleEventMapItem id = map.Get(KnownDetType.WaterQuality);
                if (id!=null)
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
            IWQDeploymentProvider wqProv = WaterQualityManager.Instance.GetDeploymentProvider(this.Context);
            if (wqProv != null && wqProv.CanDelete())
            {
                IEnumerable<WaterQualityDeployment> deploys = wqProv.GetForSampleEvent(map.SampleEventId);
                foreach (WaterQualityDeployment cur in deploys)
                {
                    wqProv.Delete(cur);
                }
                return true;
            }
            return false;
        }

        public override bool Delete(SampleEventMap map, bool fileOnly)
        {
            if (map!=null)
            {
                SampleEventMapItem id = map.Get(KnownDetType.WaterQuality);
                if (id!=null)
                {
                    IFileStoreProvider p = FileStore;
                    if (p != null)
                    {
                        IWQDeploymentProvider wqProv = WaterQualityManager.Instance.GetDeploymentProvider(this.Context);
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

        public WqDetProcessor(UserSecurityContext ctx) : base(ctx)
        { }
    }
}
