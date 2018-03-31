using Osrs.Data;
using System;
using System.Collections.Generic;

namespace Osrs.Oncor.EntityBundles
{
    public enum BundleDataType
    {
        Site,
        TaxaUnit,
        Instrument,
        PlotType
    }

    public sealed class EntityBundle
    {
        internal bool elementsDirty = false;

        public Guid Id
        {
            get;
            private set;
        }

        public CompoundIdentity PrincipalOrgId
        {
            get;
            private set;
        }

        private string name;
        public string Name
        {
            get { return this.name; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    this.name = value;
            }
        }

        public BundleDataType DataType
        {
            get;
            private set;
        }

        internal readonly Dictionary<CompoundIdentity, BundleElement> elements = new Dictionary<CompoundIdentity, BundleElement>();
        public IEnumerable<BundleElement> Elements
        {
            get
            {
                return this.elements.Values;
            }
        }

        public BundleElement Add(CompoundIdentity entityId, string localKey, string displayName)
        {
            if (entityId!=null && BundleElement.MatchesType(entityId, this.DataType) && !string.IsNullOrEmpty(displayName))
            {
                if (!(this.Contains(entityId) || this.Contains(localKey)))
                {
                    this.elementsDirty = true;
                    BundleElement tmp = new BundleElement(this.Id, entityId, localKey, displayName);
                    this.elements.Add(entityId, tmp);
                    return tmp;
                }
            }
            return null;
        }

        public BundleElement Get(string localKey)
        {
            if (!string.IsNullOrEmpty(localKey))
            {
                foreach (BundleElement cur in this.elements.Values)
                {
                    if (cur.LocalKey.Equals(localKey, StringComparison.OrdinalIgnoreCase))
                        return cur;
                }
            }
            return null;
        }

        public BundleElement Get(CompoundIdentity entityId)
        {
            if (this.elements.ContainsKey(entityId))
                return this.elements[entityId];
            return null;
        }

        public bool Contains(string localKey)
        {
            if (!string.IsNullOrEmpty(localKey))
            {
                foreach(BundleElement cur in this.elements.Values)
                {
                    if (cur.LocalKey.Equals(localKey, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            return false;
        }

        public bool Contains(CompoundIdentity entityId)
        {
            return this.elements.ContainsKey(entityId);
        }


        internal EntityBundle(Guid bundleId, string name, CompoundIdentity principalOrgId, BundleDataType dataType)
        {
            this.Id = bundleId;
            this.name = name;
            this.PrincipalOrgId = principalOrgId;
            this.DataType = dataType;
        }
    }
}
