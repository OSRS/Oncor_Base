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
    public sealed class DateRange
    {
        public DateTime? StartDate
        {
            get;
        }

        public DateTime? EndDate
        {
            get;
        }

        public bool IsValid
        {
            get
            {
                if (StartDate.HasValue && EndDate.HasValue)
                {
                    return StartDate <= EndDate;
                }
                return true; //one or both are null
            }
        }

        public static DateRange Create(DateTime? start, DateTime? end)
        {
            DateRange tmp = new DateRange(start, end);
            if (tmp.IsValid)
                return tmp;
            return null;
        }

        private DateRange(DateTime? start, DateTime? end) {
            if (start.HasValue)
                this.StartDate = WQUtils.FixDate(start.Value);
            if (end.HasValue)
                this.EndDate = WQUtils.FixDate(end.Value);
        }
    }
}
