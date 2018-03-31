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
    public sealed class VegSample : IIdentifiableEntity<Guid>, IEquatable<VegSample>
    {
        public Guid Identity
        {
            get;
        }

        public CompoundIdentity VegSurveyId
        {
            get;
        }

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
            set;
        }

        public ValueRange<float> ElevationRange
        {
            get;
            set;
        }

        public VegSample(Guid id, CompoundIdentity vegSurveyId, CompoundIdentity siteId, DateTime when, Point2<double> location, float minElevation, float maxElevation)
        {
            MethodContract.Assert(!Guid.Empty.Equals(id), nameof(id));
            MethodContract.NotNullOrEmpty(vegSurveyId, nameof(vegSurveyId));
            MethodContract.Assert(!siteId.IsNullOrEmpty() || location != null, "siteId | location");
            MethodContract.Assert(DateTime.UtcNow > when.ToUniversalTime(), nameof(when));

            this.Identity = id;
            this.VegSurveyId = vegSurveyId;
            this.SiteId = siteId;
            this.When = when;
            this.Location = location;
            this.ElevationRange = VegUtils.Create(minElevation, maxElevation);
        }

        public bool Equals(IIdentifiableEntity<Guid> other)
        {
            return this.Equals(other as VegSample);
        }

        public bool Equals(VegSample other)
        {
            if (other != null)
                return this.Identity.Equals(other.Identity);
            return false;
        }
    }
}
