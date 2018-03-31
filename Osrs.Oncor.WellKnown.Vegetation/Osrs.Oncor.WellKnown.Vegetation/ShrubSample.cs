//Copyright 2017 Open Science, Engineering, Research and Development Information Systems Open, LLC. (OSRS Open)
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//       http://www.apache.org/licenses/LICENSE-2.0
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Osrs.Data;
using Osrs.Runtime;
using System;

namespace Osrs.Oncor.WellKnown.Vegetation
{
    public sealed class ShrubSample : IIdentifiableEntity<Guid>, IDescribable, IEquatable<ShrubSample>
    {
        public Guid Identity
        {
            get;
        }

        public Guid VegSampleId
        {
            get;
        }

        public CompoundIdentity TaxaUnitId
        {
            get;
        }

        public string SizeClass
        {
            get;
            set;
        }

        public uint Count
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public ShrubSample(Guid id, Guid vegSampleId, CompoundIdentity taxaUnitId, string sizeClass, uint count, string description)
        {
            MethodContract.Assert(!Guid.Empty.Equals(id), nameof(id));
            MethodContract.NotNullOrEmpty(taxaUnitId, nameof(taxaUnitId));
            MethodContract.Assert(!Guid.Empty.Equals(vegSampleId), nameof(vegSampleId));

            this.Identity = id;
            this.VegSampleId = vegSampleId;
            this.TaxaUnitId = taxaUnitId;
            this.SizeClass = sizeClass;
            this.Count = count;
            this.Description = description;
        }

        public bool Equals(IIdentifiableEntity<Guid> other)
        {
            return this.Equals(other as ShrubSample);
        }

        public bool Equals(ShrubSample other)
        {
            if (other != null)
                return this.Identity.Equals(other.Identity);
            return false;
        }
    }
}
