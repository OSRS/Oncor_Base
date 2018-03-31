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

namespace Osrs.Oncor.WellKnown.Fish
{
    public sealed class Fish : IIdentifiableEntity<Guid>, IEquatable<Fish>
    {
        //Id, CatchEffortId, TaxaId, LengthStandard, LengthFork, LengthTotal, Weight, AdClipped, CWT
        public Guid Identity
        {
            get;
        }

        private CompoundIdentity catchEffortId;
        public CompoundIdentity CatchEffortId
        {
            get { return this.catchEffortId; }
            set
            {
                if (!value.IsNullOrEmpty())
                    this.catchEffortId = value;
            }
        }

        private CompoundIdentity taxaId;
        public CompoundIdentity TaxaId
        {
            get { return this.taxaId; }
            set
            {
                if (!value.IsNullOrEmpty())
                    this.taxaId = value;
            }
        }

        public float LengthStandard
        {
            get;
            set;
        }

        public float LengthFork
        {
            get;
            set;
        }

        public float LengthTotal
        {
            get;
            set;
        }

        public float Weight
        {
            get;
            set;
        }

        public bool? AdClipped
        {
            get;
            set;
        }

        public bool? CWT
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public Fish(Guid id, CompoundIdentity catchEffortId, CompoundIdentity taxaId, float lengthStandard, float lengthFork, float lengthTotal, float weight, bool? adClipped, bool? cwt, string description)
        {
            MethodContract.Assert(!Guid.Empty.Equals(id), nameof(id));
            MethodContract.NotNullOrEmpty(catchEffortId, nameof(catchEffortId));
            MethodContract.NotNullOrEmpty(taxaId, nameof(taxaId));

            this.Identity = id;
            this.catchEffortId = catchEffortId;
            this.taxaId = taxaId;
            this.LengthStandard = lengthStandard;
            this.LengthFork = lengthFork;
            this.LengthTotal = lengthTotal;
            this.Weight = weight;
            this.AdClipped = adClipped;
            this.CWT = cwt;
            this.Description = description;
        }

        public bool Equals(IIdentifiableEntity<Guid> other)
        {
            return this.Equals(other as Fish);
        }

        public bool Equals(Fish other)
        {
            if (other != null)
                return this.Identity.Equals(other.Identity);
            return false;
        }
    }
}
