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

namespace Osrs.Oncor.WellKnown.WaterQuality
{
    public sealed class WaterQualityDeployment : INamedEntity<CompoundIdentity>, IDescribable, IEquatable<WaterQualityDeployment>
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

        private CompoundIdentity sensorId;
        public CompoundIdentity SensorId
        {
            get { return this.sensorId; }
            set
            {
                if (!value.IsNullOrEmpty())
                    this.sensorId = value;
            }
        }

        public bool IsPrivate
        {
            get;
            set;
        }

        private string name;
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                if (!string.IsNullOrEmpty(value))
                    this.name = value;
            }
        }

        string INamed.Name
        {
            get
            {
                return this.name;
            }
        }

        public string Description
        {
            get;
            set;
        }

        private DateRange range;
        public DateRange Range
        {
            get { return this.range; }
            set
            {
                if (value != null && value.IsValid && value.StartDate.HasValue && value.EndDate.HasValue) //note that range allows null start/end and we do not here
                    this.range = value;
            }
        }

        public WaterQualityDeployment(CompoundIdentity id, string name, CompoundIdentity sampleEventId, CompoundIdentity siteId, CompoundIdentity sensorId, DateRange range, string description, bool isPrivate)
        {
            MethodContract.NotNullOrEmpty(id, nameof(id));
            MethodContract.NotNullOrEmpty(name, nameof(name));
            MethodContract.NotNullOrEmpty(sampleEventId, nameof(sampleEventId));
            MethodContract.NotNullOrEmpty(siteId, nameof(siteId));
            MethodContract.NotNullOrEmpty(sensorId, nameof(sensorId));
            MethodContract.Assert(range != null && range.IsValid && range.StartDate.HasValue && range.EndDate.HasValue, nameof(range));

            this.Identity = id;
            this.name = name;
            this.sampleEventId = sampleEventId;
            this.siteId = siteId;
            this.sensorId = sensorId;
            this.range = range;
            this.Description = description;
            this.IsPrivate = isPrivate;
        }

        public bool Equals(IIdentifiableEntity<CompoundIdentity> other)
        {
            return this.Equals(other as WaterQualityDeployment);
        }

        public bool Equals(WaterQualityDeployment other)
        {
            if (other != null)
                return this.Identity.Equals(other.Identity);
            return false;
        }
    }
}
