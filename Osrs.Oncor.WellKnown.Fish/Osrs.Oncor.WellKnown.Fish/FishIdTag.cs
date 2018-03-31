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
    //NOTE - we'll need to lookup all fish records for a fish with the same id tag - the unique "fishId" is for a fish catch, not a unique fish- the same fish may have multiple fish records, one per catch.
    public sealed class FishIdTag : IEquatable<FishIdTag>
    {
        //FishId, TagCode, TagType(String), TagManufacturer
        public Guid Identity
        {
            get;
        }

        public Guid FishId
        {
            get;
        }

        public string TagCode
        {
            get;
        }

        public string TagType
        {
            get;
        }

        public string TagManufacturer
        {
            get;
        }

        public string Description
        {
            get;
            set;
        }

        public FishIdTag(Guid id, Guid fishId, string tagCode, string tagType, string tagManuf, string description)
        {
            MethodContract.Assert(!Guid.Empty.Equals(id), nameof(id));
            MethodContract.Assert(!Guid.Empty.Equals(fishId), nameof(fishId));
            MethodContract.NotNullOrEmpty(tagCode, nameof(tagCode));
            MethodContract.NotNullOrEmpty(tagType, nameof(tagType));
            this.Identity = id;
            this.FishId = fishId;
            this.TagCode = tagCode.Trim();
            this.TagType = tagType.Trim();
            if (tagManuf != null)
                tagManuf = tagManuf.Trim();
            this.TagManufacturer = tagManuf;
            this.Description = description;
        }

        public bool SameTag(FishIdTag other)
        {
            if (other == null)
                return false;
            return this.TagCode.ToLowerInvariant().Equals(other.TagCode.ToLowerInvariant()) && this.TagType.ToLowerInvariant().Equals(other.TagType.ToLowerInvariant());
        }

        public bool Equals(FishIdTag other)
        {
            if (other != null)
                return this.FishId.Equals(other.FishId);
            return false;
        }
    }
}
