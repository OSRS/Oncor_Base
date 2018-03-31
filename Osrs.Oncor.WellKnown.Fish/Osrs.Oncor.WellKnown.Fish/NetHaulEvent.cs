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
    public sealed class NetHaulEvent : IIdentifiableEntity<Guid>, IDescribable, IEquatable<NetHaulEvent>
    {
        //CatchEffortId, NetId, AreaSampled, VolumeSampled
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

        private CompoundIdentity netId;
        public CompoundIdentity NetId
        {
            get { return this.netId; }
            set
            {
                if (!value.IsNullOrEmpty())
                    this.netId = value;
            }
        }

        public float AreaSampled
        {
            get;
            set;
        }

        public float VolumeSampled
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public NetHaulEvent(Guid id, CompoundIdentity catchEffortId, CompoundIdentity netId, float areaSampled, float volumeSampled, string description)
        {
            MethodContract.Assert(!Guid.Empty.Equals(id), nameof(id));
            MethodContract.NotNullOrEmpty(catchEffortId, nameof(catchEffortId));
            MethodContract.NotNullOrEmpty(netId, nameof(netId));

            this.Identity = id;
            this.catchEffortId = catchEffortId;
            this.netId = netId;
            this.AreaSampled = areaSampled;
            this.VolumeSampled = volumeSampled;
            this.Description = description;
        }

        public bool Equals(IIdentifiableEntity<Guid> other)
        {
            return this.Equals(other as NetHaulEvent);
        }

        public bool Equals(NetHaulEvent other)
        {
            if (other != null)
                return this.Identity.Equals(other.Identity);
            return false;
        }
    }
}
