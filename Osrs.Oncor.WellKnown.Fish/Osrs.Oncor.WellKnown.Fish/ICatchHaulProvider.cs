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
using System.Collections.Generic;

namespace Osrs.Oncor.WellKnown.Fish
{
    //NOTE: due to DET model, there is no need for "update" just delete/create - although the data types support mutation
    //Provides IO for:  CatchMetric, NetHaulEvent, FishCount
    public interface ICatchHaulProvider
    {
        bool CanGet();
        bool CanDelete();
        bool CanDelete(CatchMetric item);
        bool CanDelete(NetHaulEvent item);
        bool CanDelete(FishCount item);
        bool CanCreate();

        IEnumerable<CatchMetric> GetMetrics();
        IEnumerable<NetHaulEvent> GetHauls();
        IEnumerable<FishCount> GetFishCounts();

        IEnumerable<CatchMetric> GetMetrics(CompoundIdentity catchEffortId);
        IEnumerable<CatchMetric> GetMetrics(string metricType);
        IEnumerable<CatchMetric> GetMetrics(CompoundIdentity catchEffortId, string metricType);

        IEnumerable<NetHaulEvent> GetHauls(CompoundIdentity catchEffortId);

        IEnumerable<FishCount> GetFishCounts(CompoundIdentity catchEffortId);
        IEnumerable<FishCount> GetFishCountsByTaxa(CompoundIdentity taxaId);
        IEnumerable<FishCount> GetFishCounts(CompoundIdentity catchEffortId, CompoundIdentity taxaId);
        IEnumerable<FishCount> GetFishCounts(CompoundIdentity catchEffortId, IEnumerable<CompoundIdentity> taxaId);

        bool Delete(CatchMetric item);
        bool Delete(NetHaulEvent item);
        bool Delete(FishCount item);

        CatchMetric CreateMetric(CompoundIdentity catchEffortId, float value, string metricType);

        CatchMetric CreateMetric(CompoundIdentity catchEffortId, float value, string metricType, string description);

        NetHaulEvent CreateHaul(CompoundIdentity catchEffortId, CompoundIdentity netId, float areaSampled, float volumeSampled);

        NetHaulEvent CreateHaul(CompoundIdentity catchEffortId, CompoundIdentity netId, float areaSampled, float volumeSampled, string description);

        FishCount CreateFishCount(CompoundIdentity catchEffortId, CompoundIdentity taxaId, uint count);

        FishCount CreateFishCount(CompoundIdentity catchEffortId, CompoundIdentity taxaId, uint count, string description);
    }
}
