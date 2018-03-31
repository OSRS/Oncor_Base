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

namespace Osrs.Oncor.WellKnown.WaterQuality
{
    public static class WQUtils
    {
        public static readonly DateTime GlobalMinDate = new DateTime(1800, 1, 1);

        public static readonly Guid WQCreatePermissionId = new Guid("{919C482C-3438-42EB-8125-C52DE6872076}");
        public static readonly Guid WQGetPermissionId = new Guid("{494AEA89-DA42-49E5-BEDA-D2C6E704A301}");
        public static readonly Guid WQUpdatePermissionId = new Guid("{403B636E-F8D8-46D3-89D0-079C0F1E8FFE}");
        public static readonly Guid WQDeletePermissionId = new Guid("{532BCB7F-6106-4866-8709-5B60410859FA}");

        public static DateTime FixDate(DateTime when)
        {
            if (when.Kind == DateTimeKind.Local)
                when = when.ToUniversalTime();
            if (when.Kind == DateTimeKind.Unspecified)
                when = new DateTime(when.Ticks, DateTimeKind.Utc); //assume UTC

            if (when < WQUtils.GlobalMinDate)
                return new DateTime(WQUtils.GlobalMinDate.Ticks, DateTimeKind.Utc);
            DateTime now = DateTime.UtcNow;
            if (now.Kind == DateTimeKind.Unspecified)
                now = new DateTime(now.Ticks, DateTimeKind.Utc);
            if (when > now)
                return now;
            return when;
        }
    }
}
