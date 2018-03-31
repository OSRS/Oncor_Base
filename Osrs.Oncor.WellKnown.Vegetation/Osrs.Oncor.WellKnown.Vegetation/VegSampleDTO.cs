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
using Osrs.Numerics;
using Osrs.Numerics.Spatial.Geometry;
using Osrs.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Osrs.Oncor.WellKnown.Vegetation
{
    public sealed class VegSamplesDTO : IEnumerable<VegSampleDTO>
    {
        public CompoundIdentity VegSurveyId
        {
            get;
        }

        private readonly Dictionary<DateTime, VegSampleDTO> readings = new Dictionary<DateTime, VegSampleDTO>();

        public ICollection<DateTime> Keys
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

        public VegSampleDTO this[DateTime key]
        {
            get
            {
                if (this.readings.ContainsKey(key))
                    return this.readings[key];
                return null;
            }
        }

        public static VegSamplesDTO Create(CompoundIdentity deploymentId)
        {
            if (!deploymentId.IsNullOrEmpty())
                return new VegSamplesDTO(deploymentId);
            return null;
        }
        private VegSamplesDTO(CompoundIdentity vegSurveyId)
        {
            this.VegSurveyId = vegSurveyId;
        }

        public void Add(VegSampleDTO value)
        {
            this.readings.Add(value.When, value);
        }

        public bool ContainsKey(DateTime key)
        {
            return this.readings.ContainsKey(key);
        }

        public bool Remove(DateTime key)
        {
            return this.readings.Remove(key);
        }

        public bool TryGetValue(DateTime key, out VegSampleDTO value)
        {
            return this.readings.TryGetValue(key, out value);
        }

        public void Clear()
        {
            this.readings.Clear();
        }

        public IEnumerator<VegSampleDTO> GetEnumerator()
        {
            return this.readings.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public sealed class VegSampleDTO
    {
        public CompoundIdentity SiteId
        {
            get;
        }

        public DateTime When
        {
            get;
        }

        public Point2<double> Location
        {
            get;
        }

        public ValueRange<float> ElevationRange
        {
            get;
        }

        public bool HasSamples
        {
            get { return this.trees.Count > 0 || this.herbs.Count > 0 || this.shrubs.Count > 0; }
        }

        public IEnumerable<VegTreeSampleDTO> Trees
        {
            get { return this.trees; }
        }
        public bool HasTrees
        {
            get { return this.trees.Count > 0; }
        }

        private readonly List<VegTreeSampleDTO> trees = new List<VegTreeSampleDTO>();
        public void Add(VegTreeSampleDTO subItem)
        {
            if (subItem!=null)
            {
                this.trees.Add(subItem);
            }
        }

        public IEnumerable<VegHerbSampleDTO> Herbs
        {
            get { return this.herbs; }
        }
        public bool HasHerbs
        {
            get { return this.herbs.Count > 0; }
        }

        private readonly List<VegHerbSampleDTO> herbs = new List<VegHerbSampleDTO>();
        public void Add(VegHerbSampleDTO subItem)
        {
            if (subItem != null)
            {
                this.herbs.Add(subItem);
            }
        }

        public IEnumerable<VegShrubSampleDTO> Shrubs
        {
            get { return this.shrubs; }
        }
        public bool HasShrubs
        {
            get { return this.shrubs.Count > 0; }
        }

        private readonly List<VegShrubSampleDTO> shrubs = new List<VegShrubSampleDTO>();
        public void Add(VegShrubSampleDTO subItem)
        {
            if (subItem != null)
            {
                this.shrubs.Add(subItem);
            }
        }

        public VegSampleDTO(CompoundIdentity siteId, DateTime when, Point2<double> location, float minElev, float maxElev)
        {
            MethodContract.Assert(!siteId.IsNullOrEmpty() || location != null, "siteId | location");
            when = VegUtils.FixDate(when);
            MethodContract.Assert(DateTime.UtcNow >= when, nameof(when));

            this.SiteId = siteId;
            this.When = when;
            this.Location = location;
            this.ElevationRange = VegUtils.Create(minElev, maxElev);
        }
    }

    public sealed class VegTreeSampleDTO
    {
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
        public VegTreeSampleDTO(CompoundIdentity taxaUnitId, float dbh, string description)
        {
            MethodContract.NotNullOrEmpty(taxaUnitId, nameof(taxaUnitId));

            this.TaxaUnitId = taxaUnitId;
            this.DiameterBreastHigh = dbh;
            this.Description = description;
        }
    }

    public sealed class VegHerbSampleDTO
    {
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

        public VegHerbSampleDTO(CompoundIdentity taxaUnitId, float percentCover, string description)
        {
            MethodContract.NotNullOrEmpty(taxaUnitId, nameof(taxaUnitId));

            this.TaxaUnitId = taxaUnitId;
            this.PercentCover = percentCover;
            this.Description = description;
        }
    }

    public sealed class VegShrubSampleDTO
    {
        public CompoundIdentity TaxaUnitId
        {
            get;
        }

        public string SizeClass
        {
            get;
        }

        public uint Count
        {
            get;
        }

        public string Description
        {
            get;
        }

        public VegShrubSampleDTO(CompoundIdentity taxaUnitId, string sizeClass, uint count, string description)
        {
            MethodContract.NotNullOrEmpty(taxaUnitId, nameof(taxaUnitId));

            this.TaxaUnitId = taxaUnitId;
            this.SizeClass = sizeClass;
            this.Count = count;
            this.Description = description;
        }
    }
}
