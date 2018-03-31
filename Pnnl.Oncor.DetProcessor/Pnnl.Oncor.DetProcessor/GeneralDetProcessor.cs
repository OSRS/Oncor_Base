using Osrs;
using Osrs.Data;
using Osrs.Numerics;
using Osrs.Oncor.EntityBundles;
using Osrs.Oncor.FileStore;
using Osrs.Security;
using Osrs.WellKnown.FieldActivities;
using Pnnl.Oncor.DetProcessing;
using Pnnl.Oncor.DetProcessor.Fish;
using Pnnl.Oncor.DetProcessor.Veg;
using Pnnl.Oncor.DetProcessor.Wq;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.DetProcessor
{
    public sealed class GeneralDetProcessor
    {
        private readonly UserSecurityContext ctx;

        public CompoundIdentity CreateSampleEvent(CompoundIdentity fieldTripId, CompoundIdentity princOrgId, string name, string desc, ValueRange<DateTime> range)
        {
            if (fieldTripId!=null && !fieldTripId.IsEmpty && princOrgId!=null && !princOrgId.IsEmpty && !string.IsNullOrEmpty(name))
            {
                ISampleEventProvider prov = FieldActivityManager.Instance.GetSampleEventProvider(this.ctx);
                if (prov!=null)
                {
                    SamplingEvent evt = prov.Create(name, fieldTripId, princOrgId, range, desc);
                    if (evt != null)
                        return evt.Identity;
                }
            }
            return null;
        }

        public SamplingEvent GetSampleEvent(CompoundIdentity sampleEventId)
        {
            if (sampleEventId != null)
            {
                ISampleEventProvider prov = FieldActivityManager.Instance.GetSampleEventProvider(this.ctx);
                if (prov != null)
                    return prov.Get(sampleEventId);
            }
            return null;
        }

        public SampleEventMap GetMap(CompoundIdentity sampleEventId)
        {
            return DetRegistry.Instance.Get(sampleEventId);
        }

        public FilestoreFile CreateWQ(CompoundIdentity sampleEventId, EntityBundle sites, EntityBundle instruments, bool isPrivate)
        {
            if (!sampleEventId.IsNullOrEmpty() && sites!=null && instruments!=null && sites.DataType == BundleDataType.Site && instruments.DataType == BundleDataType.Instrument)
            {
                SampleEventMap map=DetRegistry.Instance.Get(sampleEventId);
                if (map == null)
                    map = DetRegistry.Instance.Create(sampleEventId);
                else if (map.Contains(KnownDetType.WaterQuality))
                    return null; //can't have more than 1

                WqDetProcessor wq = new WqDetProcessor(this.ctx);
                FilestoreFile fil = wq.Create(map, sites, instruments); //note the permission is checked in there
                if (fil!=null)
                {
                    map.Add(fil.FileId, KnownDetType.WaterQuality, isPrivate);
                    List<Guid> bundles = map.Get(KnownDetType.WaterQuality).BundleIds;
                    bundles.Add(sites.Id);
                    bundles.Add(instruments.Id);
                    DetRegistry.Instance.Update(map);
                    SamplingEvent e = this.GetSampleEvent(map.SampleEventId);
                    if (e!=null)
                    {
                        string tmp = e.Name.Trim().Replace(' ', '_');
                        if (tmp.Length > 25)
                            tmp = tmp.Substring(0, 24);
                        fil.FileName = tmp + "_WQ.xlsx";
                        FileStoreManager.Instance.GetProvider().Update(fil);
                    }
                }
                return fil;
            }
            return null;
        }

        public FilestoreFile CreateFish(CompoundIdentity sampleEventId, EntityBundle sites, EntityBundle nets, EntityBundle fishSpecies, EntityBundle macroSpecies, bool isPrivate)
        {
            if (!sampleEventId.IsNullOrEmpty() && sites != null && nets != null && fishSpecies != null && 
                sites.DataType == BundleDataType.Site && nets.DataType == BundleDataType.Instrument && fishSpecies.DataType == BundleDataType.TaxaUnit)
            {
                if (macroSpecies!=null)
                {
                    if (macroSpecies.DataType != BundleDataType.TaxaUnit)
                        return null; //if we have macroSpecies, they better be taxaunits
                }

                SampleEventMap map = DetRegistry.Instance.Get(sampleEventId);
                if (map == null)
                    map = DetRegistry.Instance.Create(sampleEventId);
                else if (map.Contains(KnownDetType.Fish))
                    return null; //can't have more than 1

                FishDetProcessor wq = new FishDetProcessor(this.ctx);
                FilestoreFile fil = wq.Create(map, sites, nets, fishSpecies, macroSpecies, isPrivate); //note the permission is checked in there
                if (fil != null)
                {
                    map.Add(fil.FileId, KnownDetType.Fish, isPrivate);
                    List<Guid> bundles = map.Get(KnownDetType.Fish).BundleIds;
                    bundles.Add(sites.Id);
                    bundles.Add(nets.Id);
                    bundles.Add(fishSpecies.Id);
                    if (macroSpecies != null)
                        bundles.Add(macroSpecies.Id);
                    DetRegistry.Instance.Update(map);
                    SamplingEvent e = this.GetSampleEvent(map.SampleEventId);
                    if (e != null)
                    {
                        string tmp = e.Name.Trim().Replace(' ', '_');
                        if (tmp.Length > 25)
                            tmp = tmp.Substring(0, 24);
                        fil.FileName = tmp + "_Fish.xlsx";
                        FileStoreManager.Instance.GetProvider().Update(fil);
                    }
                }
                return fil;
            }
            return null;
        }

        public FilestoreFile CreateVeg(CompoundIdentity sampleEventId, EntityBundle sites, EntityBundle plotTypes, EntityBundle shrubSpecies, EntityBundle treeSpecies, EntityBundle herbSpecies, EntityBundle nonLiving, bool isPrivate)
        {
            if (!sampleEventId.IsNullOrEmpty() && sites != null && plotTypes != null && shrubSpecies != null && treeSpecies != null && herbSpecies != null &&
                            sites.DataType == BundleDataType.Site && plotTypes.DataType == BundleDataType.PlotType && 
                            shrubSpecies.DataType == BundleDataType.TaxaUnit && treeSpecies.DataType == BundleDataType.TaxaUnit && herbSpecies.DataType == BundleDataType.TaxaUnit)
            {
                if (nonLiving != null)
                {
                    if (nonLiving.DataType != BundleDataType.TaxaUnit)
                        return null; //if we have nonLiving, they better be taxaunits
                }

                SampleEventMap map = DetRegistry.Instance.Get(sampleEventId);
                if (map == null)
                    map = DetRegistry.Instance.Create(sampleEventId);
                else if (map.Contains(KnownDetType.Veg))
                    return null; //can't have more than 1

                VegDetProcessor wq = new VegDetProcessor(this.ctx);
                FilestoreFile fil = wq.Create(map, sites, plotTypes, shrubSpecies, treeSpecies, herbSpecies, nonLiving, isPrivate); //note the permission is checked in there
                if (fil != null)
                {
                    map.Add(fil.FileId, KnownDetType.Veg, isPrivate);
                    List<Guid> bundles = map.Get(KnownDetType.Veg).BundleIds;
                    bundles.Add(sites.Id);
                    bundles.Add(plotTypes.Id);
                    bundles.Add(shrubSpecies.Id);
                    bundles.Add(treeSpecies.Id);
                    bundles.Add(herbSpecies.Id);
                    if (nonLiving != null)
                        bundles.Add(nonLiving.Id);
                    DetRegistry.Instance.Update(map);
                    SamplingEvent e = this.GetSampleEvent(map.SampleEventId);
                    if (e != null)
                    {
                        string tmp = e.Name.Trim().Replace(' ', '_');
                        if (tmp.Length > 25)
                            tmp = tmp.Substring(0, 24);
                        fil.FileName = tmp + "_Veg.xlsx";
                        FileStoreManager.Instance.GetProvider().Update(fil);
                    }
                }
                return fil;
            }
            return null;
        }

        public UpdateStatus Update(FilestoreFile tempFileUpload)
        {
            if (tempFileUpload!=null)
            {
                Guid fileId = DetProcessorBase.GetFileId(tempFileUpload);
                if (!Guid.Empty.Equals(fileId))
                {
                    SampleEventMap map = DetRegistry.Instance.Get(fileId);
                    if (map!=null)
                    {
                        SampleEventMapItem typ = map.Get(fileId); //this is to lookup the DET type this file was for
                        if (typ!=null && typ.DetType == KnownDetType.WaterQuality)
                        {
                            WqDetProcessor wq = new WqDetProcessor(this.ctx);
                            UpdateStatus stat = wq.Update(tempFileUpload, map);
                            if (stat != null && stat.Issue == UpdateIssue.AllOk)
                                DetRegistry.Instance.Update(map);
                            return stat;
                        }
                        else if (typ!=null && typ.DetType == KnownDetType.Fish)
                        {
                            FishDetProcessor f = new FishDetProcessor(this.ctx);
                            UpdateStatus stat = f.Update(tempFileUpload, map);
                            if (stat != null && stat.Issue == UpdateIssue.AllOk)
                                DetRegistry.Instance.Update(map);
                            return stat;
                        }
                        else if (typ != null && typ.DetType == KnownDetType.Veg)
                        {
                            VegDetProcessor v = new VegDetProcessor(this.ctx);
                            UpdateStatus stat = v.Update(tempFileUpload, map);
                            if (stat != null && stat.Issue == UpdateIssue.AllOk)
                                DetRegistry.Instance.Update(map);
                            return stat;
                        }
                    }
                }
            }
            return null;
        }

        public FilestoreFile Get(Guid fileId)
        {
            if (!Guid.Empty.Equals(fileId))
            {
                return FileStoreManager.Instance.GetProvider().Get(fileId);
            }
            return null;
        }

        public FilestoreFile Get(CompoundIdentity sampleEventId, KnownDetType type)
        {
            SampleEventMap map = DetRegistry.Instance.Get(sampleEventId);
            if (map != null)
            {
                SampleEventMapItem it = map.Get(type);
                if (it!=null)
                    return Get(it.DetId);
            }

            return null;
        }

        public bool Delete(CompoundIdentity sampleEventId, bool fileOnly, KnownDetType type)
        {
            if (type == KnownDetType.WaterQuality)  //For each known type, we need to add a block here
            {
                WqDetProcessor p = new WqDetProcessor(this.ctx);

                SampleEventMap map = DetRegistry.Instance.Get(sampleEventId);
                if (map.Contains(type))
                {
                    SampleEventMapItem it = map.Get(type);
                    if (it != null)
                    {
                        if (p.Delete(map, fileOnly)) //note the permission is checked in there
                        {
                            map.Remove(it.DetId);
                            if (!map.IsEmpty)
                                return DetRegistry.Instance.Update(map);
                            else
                                return DetRegistry.Instance.Delete(map.SampleEventId);
                        }
                    }
                }
            }
            else if (type == KnownDetType.Fish)
            {
                FishDetProcessor f = new FishDetProcessor(this.ctx);

                SampleEventMap map = DetRegistry.Instance.Get(sampleEventId);
                if (map.Contains(type))
                {
                    SampleEventMapItem it = map.Get(type);
                    if (it != null)
                    {
                        if (f.Delete(map, fileOnly)) //note the permission is checked in there
                        {
                            map.Remove(it.DetId);
                            if (!map.IsEmpty)
                                return DetRegistry.Instance.Update(map);
                            else
                                return DetRegistry.Instance.Delete(map.SampleEventId);
                        }
                    }
                }
            }
            else if (type == KnownDetType.Veg)
            {
                VegDetProcessor v = new VegDetProcessor(this.ctx);
                SampleEventMap map = DetRegistry.Instance.Get(sampleEventId);
                if (map.Contains(type))
                {
                    SampleEventMapItem it = map.Get(type);
                    if (it != null)
                    {
                        if (v.Delete(map, fileOnly)) //note the permission is checked in there
                        {
                            map.Remove(it.DetId);
                            if (!map.IsEmpty)
                                return DetRegistry.Instance.Update(map);
                            else
                                return DetRegistry.Instance.Delete(map.SampleEventId);
                        }
                    }
                }
            }

            return false;
        }

        public bool Delete(CompoundIdentity sampleEventId, bool fileOnly)
        {
            bool res = true;

            res = res && Delete(sampleEventId, fileOnly, KnownDetType.WaterQuality); //For each known type, we need to add a call here
            res = res && Delete(sampleEventId, fileOnly, KnownDetType.Fish);
            res = res && Delete(sampleEventId, fileOnly, KnownDetType.Veg);

            return res;
        }

        internal GeneralDetProcessor(UserSecurityContext ctx)
        {
            this.ctx = ctx;
        }
    }
}
