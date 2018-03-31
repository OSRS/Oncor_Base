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
using System.Collections;
using System.Collections.Generic;

namespace Osrs.Oncor.WellKnown.Vegetation
{
    public sealed class HerbSamplesDTO : IEnumerable<HerbSampleDTO>
    {
        public CompoundIdentity DeploymentId
        {
            get;
        }

        private readonly Dictionary<Guid, HerbSampleDTO> readings = new Dictionary<Guid, HerbSampleDTO>();

        public ICollection<Guid> Keys
        {
            get
            {
                return this.readings.Keys;
            }
        }

        public int Count
        {
            get
            {
                return this.readings.Count;
            }
        }

        public HerbSampleDTO this[Guid key]
        {
            get
            {
                if (this.readings.ContainsKey(key))
                    return this.readings[key];
                return null;
            }
        }

        public static HerbSamplesDTO Create(CompoundIdentity deploymentId)
        {
            if (!deploymentId.IsNullOrEmpty())
                return new HerbSamplesDTO(deploymentId);
            return null;
        }
        private HerbSamplesDTO(CompoundIdentity deploymentId)
        {
            this.DeploymentId = deploymentId;
        }

        public void Add(HerbSampleDTO value)
        {
            this.readings.Add(value.VegSampleId, value);
        }

        public bool ContainsKey(Guid key)
        {
            return this.readings.ContainsKey(key);
        }

        public bool Remove(Guid key)
        {
            return this.readings.Remove(key);
        }

        public bool TryGetValue(Guid key, out HerbSampleDTO value)
        {
            return this.readings.TryGetValue(key, out value);
        }

        public void Clear()
        {
            this.readings.Clear();
        }

        public IEnumerator<HerbSampleDTO> GetEnumerator()
        {
            return this.readings.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public sealed class HerbSampleDTO
    {
        public Guid VegSampleId
        {
            get;
        }

        public CompoundIdentity TaxaUnitId
        {
            get;
        }

        public float PercentCover
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public HerbSampleDTO(Guid vegSampleId, CompoundIdentity taxaUnitId, float percentCover, string description)
        {
            MethodContract.Assert(!Guid.Empty.Equals(vegSampleId), nameof(vegSampleId));
            MethodContract.NotNullOrEmpty(taxaUnitId, nameof(taxaUnitId));

            this.VegSampleId = vegSampleId;
            this.TaxaUnitId = taxaUnitId;
            this.PercentCover = percentCover;
            this.Description = description;
        }

        public HerbSampleDTO(Guid vegSampleId, VegHerbSampleDTO innerItem)
        {
            MethodContract.Assert(!Guid.Empty.Equals(vegSampleId), nameof(vegSampleId));
            MethodContract.NotNull(innerItem, nameof(innerItem));

            this.VegSampleId = vegSampleId;
            this.TaxaUnitId = innerItem.TaxaUnitId;
            this.PercentCover = innerItem.PercentCover;
            this.Description = innerItem.Description;
        }
    }
}
