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

namespace Osrs.Oncor.WellKnown.Vegetation
{
    public sealed class VegSurvey : IIdentifiableEntity<CompoundIdentity>, IDescribable, IEquatable<VegSurvey>
    {
        public CompoundIdentity Identity
        {
            get;
        }

        private CompoundIdentity sampleEventId;
        public CompoundIdentity SampleEventId
        {
            get { return this.sampleEventId; }
            set
            {
                if (!value.IsNullOrEmpty())
                    this.sampleEventId = value;
            }
        }

        private CompoundIdentity siteId;
        public CompoundIdentity SiteId
        {
            get { return this.siteId; }
            set
            {
                if (!value.IsNullOrEmpty())
                    this.siteId = value;
            }
        }

        public bool IsPrivate
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public Point2<Double> Location
        {
            get;
            set;
        }

        public float Area
        {
            get;
            set;
        }

        public ValueRange<float> ElevationRange
        {
            get;
            set;
        }

        private CompoundIdentity plotTypeId;
        public CompoundIdentity PlotTypeId
        {
            get { return this.plotTypeId; }
            set { if (value != null) this.plotTypeId = value; }
        }

        public VegSurvey(CompoundIdentity id, CompoundIdentity sampleEventId, CompoundIdentity siteId, CompoundIdentity plotTypeId, Point2<double> location, float area, float minElev, float maxElev, string description, bool isPrivate)
        {
            MethodContract.NotNullOrEmpty(id, nameof(id));
            MethodContract.NotNullOrEmpty(sampleEventId, nameof(sampleEventId));
            MethodContract.NotNullOrEmpty(siteId, nameof(siteId));
            MethodContract.NotNullOrEmpty(plotTypeId, nameof(plotTypeId));

            this.Identity = id;
            this.sampleEventId = sampleEventId;
            this.siteId = siteId;
            this.plotTypeId = plotTypeId;
            this.Location = location;
            this.Area = area;
            this.ElevationRange = VegUtils.Create(minElev, maxElev);
            this.Description = description;
            this.IsPrivate = isPrivate;
        }

        public bool Equals(IIdentifiableEntity<CompoundIdentity> other)
        {
            return this.Equals(other as VegSurvey);
        }

        public bool Equals(VegSurvey other)
        {
            if (other != null)
                return this.Identity.Equals(other.Identity);
            return false;
        }
    }
}
