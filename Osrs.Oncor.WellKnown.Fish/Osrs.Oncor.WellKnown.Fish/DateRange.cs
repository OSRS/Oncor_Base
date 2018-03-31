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
    //TODO - refactor to data core
    public sealed class DateRange
    {
        public DateTime? StartDate
        {
            get;
            private set;
        }

        public DateTime? EndDate
        {
            get;
            private set;
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
            DateRange tmp = new DateRange(FishUtils.FixDate(start), FishUtils.FixDate(end));
            if (tmp.IsValid)
                return tmp;
            return null;
        }

        private DateRange(DateTime? start, DateTime? end)
        {
            this.StartDate = start;
            this.EndDate = end;
        }
    }
}
