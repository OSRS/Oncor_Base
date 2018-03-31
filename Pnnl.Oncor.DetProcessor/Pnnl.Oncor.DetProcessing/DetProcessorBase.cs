using Osrs.Oncor.EntityBundles;
using Osrs.Oncor.Excel;
using Osrs.Oncor.FileStore;
using Osrs.Oncor.UserAffiliationPermissionChecks;
using Osrs.Security;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.DetProcessing
{
    public abstract class DetProcessorBase
    {
        private IFileStoreProvider prov;
        protected IFileStoreProvider FileStore
        {
            get
            {
                if (prov == null)
                    prov = FileStoreManager.Instance.GetProvider();
                return prov;
            }
        }

        protected static readonly DateTime MinDate = new DateTime(DateTime.MinValue.Ticks, DateTimeKind.Utc);
        protected static readonly DateTime MaxDate = new DateTime(DateTime.MaxValue.Ticks, DateTimeKind.Utc);

        protected DateTime? ToUtc(DateTime? item)
        {
            if (item!=null && item.Value.Kind != DateTimeKind.Utc)
            {
                if (item.Value.Kind == DateTimeKind.Unspecified)
                    return new DateTime(item.Value.Ticks, DateTimeKind.Utc); //assume utc
                return item.Value.ToUniversalTime(); //in local, so allow conversion
            }
            return item;
        }

        protected bool CanModify(SampleEventMap map)
        {
            UserProvider up = UserAffilationSecurityManager.Instance.GetProvider(this.ctx);
            if (up!=null)
                return up.HasAffiliationForSampleEvent(map.SampleEventId, true);
            return false;
        }

        protected bool DeleteFile(SampleEventMap map, KnownDetType type)
        {
            SampleEventMapItem id = map.Get(type);
            if (id != null)
            {
                IFileStoreProvider p = this.FileStore;
                if (p != null)
                {
                    if (p.Delete(id.DetId))
                    {
                        map.Remove(id.DetId);
                        return true;
                    }
                }
            }
            return false;
        }

        public static Guid GetFileId(FilestoreFile excelDet)
        {
            if (excelDet != null)
            {
                try
                {
                    XlCustomProperties props = XlWorkbook.LoadProperties(excelDet);
                    foreach (XlCustomProperty property in props)
                    {
                        if (property.Name == "oncorId")
                        {
                            string guidValue = property.Value;
                            if (!string.IsNullOrEmpty(guidValue))
                                return Guid.Parse(guidValue);
                        }
                    }
                }
                catch { }
            }
            return Guid.Empty;
        }

        protected Guid GetId(FilestoreFile excelDet)
        {
            return GetFileId(excelDet);
        }

        protected List<EntityBundle> GetBundles(SampleEventMap map, KnownDetType type)
        {
            if (map!=null)
            {
                if (map.Contains(type))
                {
                    List<Guid> bundleIds = map.GetBundles(type);
                    if (bundleIds != null && bundleIds.Count > 0)
                        return GetBundles(bundleIds);
                }
            }
            return null;
        }

        protected List<EntityBundle> GetBundles(List<Guid> bundleIds)
        {
            if (bundleIds != null && bundleIds.Count > 0)
            {
                EntityBundleProvider prov = EntityBundleManager.Instance.GetProvider(this.Context);
                if (prov != null)
                {
                    List<EntityBundle> tmp = new List<EntityBundle>();
                    foreach (Guid cur in bundleIds)
                    {
                        EntityBundle item = prov.Get(cur);
                        if (item != null)
                            tmp.Add(item);
                    }
                    if (tmp.Count == bundleIds.Count)
                        return tmp;
                }
            }
            return null;
        }

        public abstract UpdateStatus Update(FilestoreFile tempFileUpload, SampleEventMap map);

        public abstract FilestoreFile Get(SampleEventMap map);

        public abstract bool Delete(SampleEventMap map, bool fileOnly);

        protected UpdateStatus Add(string k, string v, UpdateIssue code, UpdateStatus exist)
        {
            if (exist == null)
                exist = new UpdateStatus(code);
            exist.Add(new IssueNotice(k, v));
            return exist;
        }

        protected string GetUserString()
        {
            return "OncorUser";
        }

        private readonly UserSecurityContext ctx;
        protected UserSecurityContext Context
        {
            get { return this.ctx; }
        }

        protected DetProcessorBase(UserSecurityContext ctx)
        {
            this.ctx = ctx;
        }
    }
}
