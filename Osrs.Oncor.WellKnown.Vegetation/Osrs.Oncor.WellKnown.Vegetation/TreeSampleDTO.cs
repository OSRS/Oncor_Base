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
    public sealed class TreeSamplesDTO : IEnumerable<TreeSampleDTO>
    {
        public CompoundIdentity DeploymentId
        {
            get;
        }

        private readonly Dictionary<Guid, TreeSampleDTO> readings = new Dictionary<Guid, TreeSampleDTO>();

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

        public TreeSampleDTO this[Guid key]
        {
            get
            {
                if (this.readings.ContainsKey(key))
                    return this.readings[key];
                return null;
            }
        }

        public static TreeSamplesDTO Create(CompoundIdentity deploymentId)
        {
            if (!deploymentId.IsNullOrEmpty())
                return new TreeSamplesDTO(deploymentId);
            return null;
        }
        private TreeSamplesDTO(CompoundIdentity deploymentId)
        {
            this.DeploymentId = deploymentId;
        }

        public void Add(TreeSampleDTO value)
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

        public bool TryGetValue(Guid key, out TreeSampleDTO value)
        {
            return this.readings.TryGetValue(key, out value);
        }

        public void Clear()
        {
            this.readings.Clear();
        }

        public IEnumerator<TreeSampleDTO> GetEnumerator()
        {
            return this.readings.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public sealed class TreeSampleDTO
    {
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
        }

        public string Description
        {
            get;
        }

        public TreeSampleDTO(Guid vegSampleId, CompoundIdentity taxaUnitId, float dbh, string description)
        {
            MethodContract.NotNullOrEmpty(taxaUnitId, nameof(taxaUnitId));
            MethodContract.Assert(!Guid.Empty.Equals(vegSampleId), nameof(vegSampleId));

            this.VegSampleId = vegSampleId;
            this.TaxaUnitId = taxaUnitId;
            this.DiameterBreastHigh = dbh;
            this.Description = description;
        }

        public TreeSampleDTO(Guid vegSampleId, VegTreeSampleDTO innerItem)
        {
            MethodContract.NotNull(innerItem, nameof(innerItem));
            MethodContract.Assert(!Guid.Empty.Equals(vegSampleId), nameof(vegSampleId));

            this.VegSampleId = vegSampleId;
            this.TaxaUnitId = innerItem.TaxaUnitId;
            this.DiameterBreastHigh = innerItem.DiameterBreastHigh;
            this.Description = innerItem.Description;
        }
    }
}
