using Osrs.Data;
using Osrs.Runtime;
using System;

namespace Osrs.Oncor.EntityBundles
{
    public sealed class BundleElement
    {
        //NOTE -- these allow us to ensure we only get matching ids for the datatype expected from this system (this is currently a cheat for simplicity)
        private static readonly Guid siteDomainId = new Guid("{D927CA09-85F4-40E3-B5A8-41E83BF63D2D}"); //TODO -- this should be moved to config or such like a dynamic lookup in the api
        private static readonly Guid taxaDomainId = new Guid("{E578CA70-6CEC-4961-BB43-14FD45F455BD}"); //TODO -- this should be moved to config or such like a dynamic lookup in the api
        private static readonly Guid instrumentDomainId = new Guid("{5F297502-B620-42BF-80BC-A4AF5A597267}"); //TODO -- this should be moved to config or such like a dynamic lookup in the api
        private static readonly Guid plotTypeDomainId = new Guid("{A38A6254-8AB1-4D12-A576-DD058813F856}");

        internal static bool MatchesType(CompoundIdentity entityId, BundleDataType type)
        {
            if (type == BundleDataType.Site)
                return entityId.DataStoreIdentity.Equals(siteDomainId);
            else if (type == BundleDataType.TaxaUnit)
                return entityId.DataStoreIdentity.Equals(taxaDomainId);
            else if (type == BundleDataType.Instrument)
                return entityId.DataStoreIdentity.Equals(instrumentDomainId);
            return entityId.DataStoreIdentity.Equals(plotTypeDomainId);
        }

        public Guid BundleId
        {
            get;
            private set;
        }

        public CompoundIdentity EntityId
        {
            get;
            private set;
        }

        public String LocalKey
        {
            get;
            private set;
        }

        public String DisplayName
        {
            get;
            private set;
        }

        internal BundleElement(Guid bundleId, CompoundIdentity entityId, String localKey, string displayName)
        {
            if (!Guid.Empty.Equals(bundleId) && !entityId.IsNullOrEmpty() && !string.IsNullOrEmpty(localKey) && !string.IsNullOrEmpty(displayName))
            {
                this.BundleId = bundleId;
                this.EntityId = entityId;
                this.LocalKey = localKey;
                this.DisplayName = displayName;
            }
            else
                throw new InstantiationException();
        }
    }
}
