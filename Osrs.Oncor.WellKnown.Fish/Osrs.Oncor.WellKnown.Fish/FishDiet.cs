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
    public sealed class FishDiet : IIdentifiableEntity<Guid>, IEquatable<FishDiet>
    {
        //FishId, GutSampleId(String), VialId(String), TaxaId, LifeStage(String), Count, SampleMass, WholeAnimalsWeighed, IndividualMass, Description

        public Guid Identity
        {
            get;
        }

        private Guid fishId;
        public Guid FishId
        {
            get { return this.fishId; }
            set
            {
                if (!Guid.Empty.Equals(value))
                    this.fishId = value;
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

        public string VialId
        {
            get;
            set;
        }

        public string GutSampleId
        {
            get;
            set;
        }

        public string LifeStage
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public uint? Count
        {
            get;
            set;
        }

        public float SampleMass
        {
            get;
            set;
        }

        public float IndividualMass
        {
            get;
            set;
        }

        public uint? WholeAnimalsWeighed
        {
            get;
            set;
        }

        //TODO -- allow insert of record with NULL taxa iff -> (lifestage==null, count==0, indMass==NaN, whole==0)
        public FishDiet(Guid id, Guid fishId, CompoundIdentity taxaId, string vialId, string gutsampleid, string lifestage, uint? count, float sampleMass, float indMass, uint? wholeAnimalsWeighed, string description)
        {
            MethodContract.Assert(!Guid.Empty.Equals(id), nameof(id));
            MethodContract.Assert(!Guid.Empty.Equals(fishId), nameof(fishId));
            //MethodContract.NotNullOrEmpty(taxaId, nameof(taxaId));
            if (taxaId.IsNullOrEmpty())
            {
                if (count.HasValue && count > 0)
                    throw new ArgumentException("for null taxa must have 0 or null count");
            }

            this.Identity = id;
            this.fishId = fishId;
            this.taxaId = taxaId;
            this.VialId = vialId;
            this.GutSampleId = gutsampleid;
            this.LifeStage = lifestage;
            this.Count = count;
            this.SampleMass = sampleMass;
            this.IndividualMass = indMass;
            this.WholeAnimalsWeighed = wholeAnimalsWeighed;
            this.Description = description;
        }

        public bool EqualFish(FishDiet item)
        {
            if (item != null)
                return this.fishId.Equals(item.fishId);
            return false;
        }

        public bool Equals(IIdentifiableEntity<Guid> other)
        {
            return this.Equals(other as FishDiet);
        }

        public bool Equals(FishDiet other)
        {
            if (other != null)
                return this.Identity.Equals(other.Identity);
            return false;
        }
    }
}
