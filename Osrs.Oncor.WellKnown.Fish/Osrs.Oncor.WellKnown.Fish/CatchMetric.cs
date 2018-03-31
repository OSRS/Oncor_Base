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
    public sealed class CatchMetric : IIdentifiableEntity<Guid>, IDescribable, IEquatable<CatchMetric>
    {
        //CatchEffortId, Value, MetricType(String), Description

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


        public string Description
        {
            get;
            set;
        }

        public float Value
        {
            get;
            set;
        }

        private string metricType;
        public string MetricType
        {
            get { return this.metricType; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    this.metricType = value;
            }
        }

        public CatchMetric(Guid id, CompoundIdentity catchEffortId, float value, string metricType, string description)
        {
            MethodContract.Assert(!Guid.Empty.Equals(id), nameof(id));
            MethodContract.NotNullOrEmpty(catchEffortId, nameof(catchEffortId));
            MethodContract.NotNullOrEmpty(metricType, nameof(metricType));

            this.Identity = id;
            this.Description = description;
            this.catchEffortId = catchEffortId;
            this.Value = value;
            this.metricType = metricType;
        }

        public bool Equals(IIdentifiableEntity<Guid> other)
        {
            return this.Equals(other as CatchMetric);
        }

        public bool Equals(CatchMetric other)
        {
            if (other != null)
                return this.Identity.Equals(other.Identity);
            return false;
        }
    }
}
