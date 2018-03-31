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
    public sealed class TreeSample : IIdentifiableEntity<Guid>, IDescribable, IEquatable<TreeSample>
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

        public float DiameterBreastHigh
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public TreeSample(Guid id, Guid vegSampleId, CompoundIdentity taxaUnitId, float dbh, string description)
        {
            MethodContract.Assert(!Guid.Empty.Equals(id), nameof(id));
            MethodContract.NotNullOrEmpty(taxaUnitId, nameof(taxaUnitId));
            MethodContract.Assert(!Guid.Empty.Equals(vegSampleId), nameof(vegSampleId));

            this.Identity = id;
            this.VegSampleId = vegSampleId;
            this.TaxaUnitId = taxaUnitId;
            this.DiameterBreastHigh = dbh;
            this.Description = description;
        }

        public bool Equals(IIdentifiableEntity<Guid> other)
        {
            return this.Equals(other as TreeSample);
        }

        public bool Equals(TreeSample other)
        {
            if (other != null)
                return this.Identity.Equals(other.Identity);
            return false;
        }
    }
}
