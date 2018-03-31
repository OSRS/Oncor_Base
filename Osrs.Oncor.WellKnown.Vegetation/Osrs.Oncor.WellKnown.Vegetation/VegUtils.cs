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

using Osrs.Numerics;
using System;

namespace Osrs.Oncor.WellKnown.Vegetation
{
    public static class VegUtils
    {
        public static readonly DateTime GlobalMinDate = new DateTime(1800, 1, 1);

        public static readonly Guid CreatePermissionId = new Guid("{240F7448-6858-4465-9100-C698A19CF60F}");
        public static readonly Guid GetPermissionId = new Guid("{88B49E4E-43B5-4A77-8662-BAF69E77E345}");
        public static readonly Guid UpdatePermissionId = new Guid("{F0DBFB75-CA65-4555-908D-115BD76C608A}");
        public static readonly Guid DeletePermissionId = new Guid("{28C837CF-F29A-4055-99CC-A1121967B29F}");

        public static DateTime FixDate(DateTime when)
        {
            if (when.Kind == DateTimeKind.Local)
                when = when.ToUniversalTime();
            if (when.Kind == DateTimeKind.Unspecified)
                when = new DateTime(when.Ticks, DateTimeKind.Utc); //assume UTC

            if (when < GlobalMinDate)
                return new DateTime(GlobalMinDate.Ticks, DateTimeKind.Utc);
            DateTime now = DateTime.UtcNow;
            if (now.Kind == DateTimeKind.Unspecified)
                now = new DateTime(now.Ticks, DateTimeKind.Utc);
            if (when > now)
                return now;
            return when;
        }

        public static ValueRange<float> Create(float min, float max)
        {
            if (float.IsNaN(min) || float.IsInfinity(min))
            {
                if (float.IsPositiveInfinity(min))
                    return null; //max can't be > min and +INF isn't "real" for us

                min = float.NegativeInfinity;
            }

            if (float.IsNaN(max) || float.IsInfinity(max))
            {
                if (float.IsNegativeInfinity(min))
                    return null; //both were NaN / +-INF
                max = float.PositiveInfinity;
            }

            if (min > max)
                return new ValueRange<float>(max, min); //we'll be nice and reverse them properly

            return new ValueRange<float>(min, max);
        }

    }
}
