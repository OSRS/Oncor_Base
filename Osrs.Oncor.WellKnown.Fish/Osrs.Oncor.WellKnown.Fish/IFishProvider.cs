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

namespace Osrs.Oncor.WellKnown.Fish
{
    //TODO -- Add insert methods
    //NOTE: due to DET model, there is no need for "update" just delete/create - although the data types support mutation
    //Provides IO for: Fish (root), FishDiet, FishGenetics, FishIdTag
    public interface IFishProvider
    {
        bool CanGet();
        bool CanDelete();
        bool CanDelete(Fish item);
        bool CanDelete(FishDiet item);
        bool CanDelete(FishGenetics item);
        bool CanDelete(FishIdTag item);
        bool CanCreate();

        Fish GetFish(Guid fishId);
        IEnumerable<Fish> GetFish(IEnumerable<Guid> fishId);

        IEnumerable<Fish> GetFish();
        IEnumerable<Fish> GetFish(CompoundIdentity catchEffortId);
        IEnumerable<Fish> GetFish(IEnumerable<CompoundIdentity> catchEffortId);
        IEnumerable<Fish> GetFishByTaxa(CompoundIdentity taxaId);
        IEnumerable<Fish> GetFishByTaxa(IEnumerable<CompoundIdentity> taxaId);
        IEnumerable<Fish> GetFish(CompoundIdentity catchEffortId, CompoundIdentity taxaId);
        IEnumerable<Fish> GetFish(CompoundIdentity catchEffortId, IEnumerable<CompoundIdentity> taxaId);
        IEnumerable<Fish> GetFish(IEnumerable<CompoundIdentity> catchEffortId, CompoundIdentity taxaId);
        IEnumerable<Fish> GetFish(IEnumerable<CompoundIdentity> catchEffortId, IEnumerable<CompoundIdentity> taxaId);

        IEnumerable<FishDiet> GetFishDiet();
        IEnumerable<FishDiet> GetFishDiet(Guid fishId);
        IEnumerable<FishDiet> GetFishDiet(CompoundIdentity taxaId);
        IEnumerable<FishDiet> GetFishDiet(IEnumerable<CompoundIdentity> taxaId);
        IEnumerable<FishDiet> GetFishDiet(IEnumerable<Guid> fishId);

        IEnumerable<FishGenetics> GetFishGenetics();
        IEnumerable<FishGenetics> GetFishGenetics(Guid fishId);
        IEnumerable<FishGenetics> GetFishGenetics(IEnumerable<Guid> fishId);

        IEnumerable<FishIdTag> GetFishIdTag();
        IEnumerable<FishIdTag> GetFishIdTag(Guid fishId);
        IEnumerable<FishIdTag> GetFishIdTag(IEnumerable<Guid> fishId);
        IEnumerable<FishIdTag> GetFishIdTag(string tagCode, string tagType);


        bool Delete(Fish item);
        bool Delete(FishDiet item);
        bool Delete(FishGenetics item);
        bool Delete(FishIdTag item);

        Fish CreateFish(CompoundIdentity catchEffortId, CompoundIdentity taxaId, float standard, float fork, float total, float weight, bool? adClipped, bool? cwt);
        Fish CreateFish(CompoundIdentity catchEffortId, CompoundIdentity taxaId, float standard, float fork, float total, float weight, bool? adClipped, bool? cwt, string description);

        FishDiet CreateFishDiet(Guid fishId, CompoundIdentity taxaId, string gutsampleId, uint? count, float sampleMass, float individualMass, uint? wholeAnimalsWeighed);
        FishDiet CreateFishDiet(Guid fishId, CompoundIdentity taxaId, string gutsampleId, uint? count, float sampleMass, float individualMass, uint? wholeAnimalsWeighed, string description);
        FishDiet CreateFishDiet(Guid fishId, CompoundIdentity taxaId, string gutsampleId, string vialId, uint? count, float sampleMass, float individualMass, uint? wholeAnimalsWeighed);
        FishDiet CreateFishDiet(Guid fishId, CompoundIdentity taxaId, string gutsampleId, string vialId, uint? count, float sampleMass, float individualMass, uint? wholeAnimalsWeighed, string description);
        FishDiet CreateFishDiet(Guid fishId, CompoundIdentity taxaId, string gutsampleId, string vialId, string lifeStage, uint? count, float sampleMass, float individualMass, uint? wholeAnimalsWeighed);
        FishDiet CreateFishDiet(Guid fishId, CompoundIdentity taxaId, string gutsampleId, string vialId, string lifeStage, uint? count, float sampleMass, float individualMass, uint? wholeAnimalsWeighed, string description);

        FishGenetics CreateFishGenetics(Guid fishId, StockEstimates estimates);
        FishGenetics CreateFishGenetics(Guid fishId, StockEstimates estimates, string description);
        FishGenetics CreateFishGenetics(Guid fishId, string geneticSampleId, string labSampleId, StockEstimates estimates);
        FishGenetics CreateFishGenetics(Guid fishId, string geneticSampleId, string labSampleId, StockEstimates estimates, string description);

        FishIdTag CreateIdTag(Guid fishId, string tagCode, string tagType);
        FishIdTag CreateIdTag(Guid fishId, string tagCode, string tagType, string tagManufacturer);
        FishIdTag CreateIdTag(Guid fishId, string tagCode, string tagType, string tagManufacturer, string description);
    }
}
