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
using System;
using System.Collections.Generic;

namespace Osrs.Oncor.WellKnown.WaterQuality
{
    public interface IWQDeploymentProvider
    {
        bool CanGet();
        bool CanUpdate();
        bool CanUpdate(WaterQualityDeployment item);
        bool CanDelete();
        bool CanDelete(WaterQualityDeployment item);
        bool CanCreate();

        IEnumerable<WaterQualityDeployment> Get();

        WaterQualityDeployment Get(CompoundIdentity id);

        IEnumerable<WaterQualityDeployment> Get(string name);

        IEnumerable<WaterQualityDeployment> Get(string name, StringComparison comparisonOption);

        IEnumerable<WaterQualityDeployment> Get(string name, DateTime start, DateTime end);

        IEnumerable<WaterQualityDeployment> Get(string name, StringComparison comparisonOption, DateTime start, DateTime end);

        IEnumerable<WaterQualityDeployment> Get(DateTime start, DateTime end);

        IEnumerable<WaterQualityDeployment> Get(IEnumerable<CompoundIdentity> sampleEventIds, IEnumerable<CompoundIdentity> siteIds, DateTime start, DateTime end);

        IEnumerable<WaterQualityDeployment> Get(IEnumerable<CompoundIdentity> sampleEventIds, IEnumerable<CompoundIdentity> siteIds);

        IEnumerable<WaterQualityDeployment> GetForSampleEvent(CompoundIdentity sampleEventId);

        IEnumerable<WaterQualityDeployment> GetForSampleEvent(CompoundIdentity sampleEventId, DateTime start, DateTime end);

        IEnumerable<WaterQualityDeployment> GetForSite(CompoundIdentity siteId);

        IEnumerable<WaterQualityDeployment> GetForSite(CompoundIdentity siteId, DateTime start, DateTime end);

        IEnumerable<WaterQualityDeployment> Get(IEnumerable<CompoundIdentity> ids);

        IEnumerable<WaterQualityDeployment> Get(IEnumerable<CompoundIdentity> ids, DateTime start, DateTime end);

        IEnumerable<WaterQualityDeployment> GetForSampleEvent(IEnumerable<CompoundIdentity> sampleEventIds);

        IEnumerable<WaterQualityDeployment> GetForSampleEvent(IEnumerable<CompoundIdentity> sampleEventIds, DateTime start, DateTime end);

        IEnumerable<WaterQualityDeployment> GetForSite(IEnumerable<CompoundIdentity> siteIds);

        IEnumerable<WaterQualityDeployment> GetForSite(IEnumerable<CompoundIdentity> siteIds, DateTime start, DateTime end);

        bool Update(WaterQualityDeployment item);
        bool Delete(WaterQualityDeployment item);

        WaterQualityDeployment Create(string name, CompoundIdentity sampleEventId, CompoundIdentity siteId, CompoundIdentity sensorId, DateRange range, bool isPrivate);

        WaterQualityDeployment Create(string name, CompoundIdentity sampleEventId, CompoundIdentity siteId, CompoundIdentity sensorId, DateRange range, string description, bool isPrivate);
    }
}
