using Osrs.Data;
using System;
using System.Collections.Generic;
using System.Collections;

namespace Pnnl.Oncor.DetProcessing
{
    public sealed class SampleEventMapItem
    {
        public KnownDetType DetType
        {
            get;
        }

        public Guid DetId
        {
            get;
        }

        public bool IsPrivate
        {
            get;
        }

        public readonly List<Guid> BundleIds = new List<Guid>();

        public SampleEventMapItem(KnownDetType typ, Guid detId, bool isPrivate)
        {
            this.DetType = typ;
            this.DetId = detId;
            this.IsPrivate = isPrivate;
        }
    }

    public sealed class SampleEventMap : IEnumerable<SampleEventMapItem>
    {
        public CompoundIdentity SampleEventId
        {
            get;
            private set;
        }

        private readonly Dictionary<KnownDetType, SampleEventMapItem> items = new Dictionary<KnownDetType, SampleEventMapItem>();

        public int Count
        {
            get { return this.items.Count; }
        }

        public bool IsEmpty
        {
            get { return this.items.Count < 1; }
        }

        public SampleEventMapItem Get(KnownDetType type)
        {
            if (this.items.ContainsKey(type))
                return this.items[type];
            return null;
        }

        public SampleEventMapItem Get(Guid id)
        {
            foreach (KeyValuePair<KnownDetType, SampleEventMapItem> cur in this.items)
            {
                if (id == cur.Value.DetId)
                    return cur.Value;
            }
            return null;
        }

        public List<Guid> GetBundles(KnownDetType type)
        {
            if (this.items.ContainsKey(type))
                return this.items[type].BundleIds;
            return null;
        }

        public bool Add(Guid id, KnownDetType type)
        {
            return Add(id, type, false);
        }

        public bool Add(Guid id, KnownDetType type, bool isPrivate)
        {
            if (!Guid.Empty.Equals(id) && type != KnownDetType.Unknown && !this.items.ContainsKey(type))
            {
                this.items[type] = new SampleEventMapItem(type, id, isPrivate);
                return true;
            }
            return false;
        }

        public void Remove(Guid id)
        {
            SampleEventMapItem t = this.Get(id);
            if (t != null)
                this.items.Remove(t.DetType);
        }

        public void Remove(KnownDetType type)
        {
            if (type!= KnownDetType.Unknown)
                this.items.Remove(type);
        }

        public bool Contains(Guid id)
        {
            foreach (SampleEventMapItem cur in this.items.Values)
            {
                if (id.Equals(cur.DetId))
                    return true;
            }
            return false;
        }

        public bool Contains(KnownDetType type)
        {
            return this.items.ContainsKey(type);
        }

        public static SampleEventMap Create(CompoundIdentity sampleEventId)
        {
            if (sampleEventId != null && !sampleEventId.IsEmpty)
                return new SampleEventMap(sampleEventId);
            return null;
        }

        public IEnumerator<SampleEventMapItem> GetEnumerator()
        {
            return this.items.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private SampleEventMap(CompoundIdentity sampleEventId)
        {
            this.SampleEventId = sampleEventId;
        }
    }
}
