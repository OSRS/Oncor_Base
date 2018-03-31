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
using Osrs.Numerics.Spatial.Geometry;
using System;
using System.Collections.Generic;

namespace Osrs.Oncor.WellKnown.Fish
{
    public interface ICatchEffortProvider
    {
        bool CanGet();
        bool CanUpdate();
        bool CanUpdate(CatchEffort item);
        bool CanDelete();
        bool CanDelete(CatchEffort item);
        bool CanCreate();

        IEnumerable<CatchEffort> Get();

        CatchEffort Get(CompoundIdentity id);

        IEnumerable<CatchEffort> Get(DateTime start, DateTime end);

        IEnumerable<CatchEffort> Get(IEnumerable<CompoundIdentity> sampleEventIds, IEnumerable<CompoundIdentity> siteIds, DateTime start, DateTime end);

        IEnumerable<CatchEffort> Get(IEnumerable<CompoundIdentity> sampleEventIds, IEnumerable<CompoundIdentity> siteIds);

        IEnumerable<CatchEffort> GetForSampleEvent(CompoundIdentity sampleEventId);

        IEnumerable<CatchEffort> GetForSampleEvent(CompoundIdentity sampleEventId, DateTime start, DateTime end);

        IEnumerable<CatchEffort> GetForSite(CompoundIdentity siteId);

        IEnumerable<CatchEffort> GetForSite(CompoundIdentity siteId, DateTime start, DateTime end);

        IEnumerable<CatchEffort> Get(IEnumerable<CompoundIdentity> ids);

        IEnumerable<CatchEffort> Get(IEnumerable<CompoundIdentity> ids, DateTime start, DateTime end);

        IEnumerable<CatchEffort> GetForSampleEvent(IEnumerable<CompoundIdentity> sampleEventIds);

        IEnumerable<CatchEffort> GetForSampleEvent(IEnumerable<CompoundIdentity> sampleEventIds, DateTime start, DateTime end);

        IEnumerable<CatchEffort> GetForSite(IEnumerable<CompoundIdentity> siteIds);

        IEnumerable<CatchEffort> GetForSite(IEnumerable<CompoundIdentity> siteIds, DateTime start, DateTime end);

        bool Update(CatchEffort item);
        bool Delete(CatchEffort item);

        //Id, siteId, DateTime, PointLocation, Method(String), Strata(String), Description, Depth, pH, Temp, DO, Salinity, Velocity
        CatchEffort Create(CompoundIdentity sampleEventId, CompoundIdentity siteId, DateTime when, bool isPrivate, string method, string strata, Point2<double> location, float depth, float pH, float temp, float DO, float salinity, float vel);

        CatchEffort Create(CompoundIdentity sampleEventId, CompoundIdentity siteId, DateTime when, string description, bool isPrivate, string method, string strata, Point2<double> location, float depth, float pH, float temp, float DO, float salinity, float vel);
    }
}
