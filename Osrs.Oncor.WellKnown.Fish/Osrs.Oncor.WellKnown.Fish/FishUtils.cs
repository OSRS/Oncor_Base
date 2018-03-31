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

using System;

namespace Osrs.Oncor.WellKnown.Fish
{
    public static class FishUtils
    {
        public static readonly DateTime GlobalMinDate = new DateTime(1800, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static readonly Guid FishCreatePermissionId = new Guid("{07962645-A981-46C0-B56C-18780CC408C0}");
        public static readonly Guid FishGetPermissionId = new Guid("{E986C7EA-C512-4D59-A835-75106FB4E402}");
        public static readonly Guid FishUpdatePermissionId = new Guid("{75B072C6-33C7-4E89-B82B-DD66A428726F}");
        public static readonly Guid FishDeletePermissionId = new Guid("{7CAE3493-8FA4-4F3B-887E-380AD891AC45}");

        internal static DateTime? FixDate(DateTime? when)
        {
            if (when.HasValue)
                return FixDate(when.Value);
            return null;
        }

        internal static DateTime FixDate(DateTime when)
        {
            if (when.Kind == DateTimeKind.Local)
                when = when.ToUniversalTime();
            if (when.Kind == DateTimeKind.Unspecified)
                when = new DateTime(when.Ticks, DateTimeKind.Utc); //assume UTC

            if (when < FishUtils.GlobalMinDate)
                return new DateTime(FishUtils.GlobalMinDate.Ticks, DateTimeKind.Utc);
            DateTime now = DateTime.UtcNow;
            if (now.Kind == DateTimeKind.Unspecified)
                now = new DateTime(now.Ticks, DateTimeKind.Utc);
            if (when > now)
                return now;
            return when;
        }
    }
}
