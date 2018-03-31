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
using Osrs.Numerics.Spatial.Geometry;
using Osrs.Runtime;
using System;

namespace Osrs.Oncor.WellKnown.Fish
{
    public sealed class CatchEffort : IIdentifiableEntity<CompoundIdentity>, IDescribable, IEquatable<CatchEffort>
    {
        //Id, siteId, DateTime, PointLocation, Method(String), Strata(String), Description, Depth, pH, Temp, DO, Salinity, Velocity

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

        public DateTime SampleDate
        {
            get;
            set;
        }

        public Point2<Double> Location
        {
            get;
            set;
        }

        public string CatchMethod
        {
            get;
            set;
        }

        public string Strata
        {
            get;
            set;
        }

        public float Depth
        {
            get;
            set;
        }

        public float pH
        {
            get;
            set;
        }

        public float Temp
        {
            get;
            set;
        }

        public float DO
        {
            get;
            set;
        }

        public float Salinity
        {
            get;
            set;
        }

        public float Velocity
        {
            get;
            set;
        }

        public CatchEffort(CompoundIdentity id, CompoundIdentity sampleEventId, CompoundIdentity siteId, DateTime sampleDate, Point2<double> location, string catchMethod, string strata, float depth, float pH, float temp, float DO, float salinity, float velocity, string description, bool isPrivate)
        {
            MethodContract.NotNullOrEmpty(id, nameof(id));
            MethodContract.NotNullOrEmpty(sampleEventId, nameof(sampleEventId));
            MethodContract.NotNullOrEmpty(siteId, nameof(siteId));

            this.Identity = id;
            this.sampleEventId = sampleEventId;
            this.siteId = siteId;
            this.SampleDate = sampleDate;
            this.Location = location;
            this.CatchMethod = catchMethod;
            this.Strata = strata;
            this.Depth = depth;
            this.pH = pH;
            this.Temp = temp;
            this.DO = DO;
            this.Salinity = salinity;
            this.Velocity = velocity;
            this.Description = description;
            this.IsPrivate = isPrivate;
        }

        public bool Equals(IIdentifiableEntity<CompoundIdentity> other)
        {
            return this.Equals(other as CatchEffort);
        }

        public bool Equals(CatchEffort other)
        {
            if (other != null)
                return this.Identity.Equals(other.Identity);
            return false;
        }
    }
}
