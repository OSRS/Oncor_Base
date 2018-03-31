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

namespace Osrs.Oncor.WellKnown.Vegetation
{
    public interface IVegSampleProvider
    {
        bool CanGet();
        bool CanUpdate();
        bool CanUpdate(VegSample item);
        bool CanUpdate(HerbSample item);
        bool CanUpdate(TreeSample item);
        bool CanUpdate(ShrubSample item);
        bool CanDelete();
        bool CanDelete(VegSample item);
        bool CanDelete(HerbSample item);
        bool CanDelete(TreeSample item);
        bool CanDelete(ShrubSample item);
        bool CanCreate();

        IEnumerable<VegSample> Get();

        VegSample Get(Guid id);

        IEnumerable<VegSample> Get(IEnumerable<Guid> ids);

        IEnumerable<VegSample> Get(IEnumerable<Guid> ids, IEnumerable<CompoundIdentity> siteIds);

        IEnumerable<VegSample> GetForSurvey(CompoundIdentity vegSurveyId);

        IEnumerable<VegSample> GetForSite(CompoundIdentity siteId);

        IEnumerable<VegSample> GetForSurvey(IEnumerable<CompoundIdentity> vegSurveyIds);

        IEnumerable<VegSample> GetForSite(IEnumerable<CompoundIdentity> siteIds);

        bool Update(VegSample item);
        bool Delete(VegSample item);

        bool DeleteSample(Guid id);

        VegSample Create(CompoundIdentity vegSurveyId, CompoundIdentity siteId, DateTime when);

        VegSample Create(CompoundIdentity vegSurveyId, CompoundIdentity siteId, DateTime when, float minElev, float maxElev);

        VegSample Create(CompoundIdentity vegSurveyId, DateTime when, Point2<double> location);

        VegSample Create(CompoundIdentity vegSurveyId, DateTime when, Point2<double> location, float minElev, float maxElev);

        VegSample Create(CompoundIdentity vegSurveyId, CompoundIdentity siteId, DateTime when, Point2<double> location);

        VegSample Create(CompoundIdentity vegSurveyId, CompoundIdentity siteId, DateTime when, Point2<double> location, float minElev, float maxElev);

        IEnumerable<VegSample> Create(VegSamplesDTO items);
        VegSample Create(CompoundIdentity vegSurveyId, VegSampleDTO item);

        HerbSample CreateHerb(CompoundIdentity vegSampleId, CompoundIdentity taxaUnitId, float percentCover);
        HerbSample CreateHerb(CompoundIdentity vegSampleId, CompoundIdentity taxaUnitId, float percentCover, string description);
        ShrubSample CreateShrub(CompoundIdentity vegSampleId, CompoundIdentity taxaUnitId, string sizeClass, uint count);
        ShrubSample CreateShrub(CompoundIdentity vegSampleId, CompoundIdentity taxaUnitId, string sizeClass, uint count, string description);
        TreeSample CreateTree(CompoundIdentity vegSampleId, CompoundIdentity taxaUnitId, float dbh);
        TreeSample CreateTree(CompoundIdentity vegSampleId, CompoundIdentity taxaUnitId, float dbh, string description);


        HerbSample GetHerb(Guid id);
        IEnumerable<HerbSample> GetHerb();

        IEnumerable<HerbSample> GetHerb(IEnumerable<Guid> ids);
        IEnumerable<HerbSample> GetHerbForVegSample(IEnumerable<Guid> vegSampleIds);

        IEnumerable<HerbSample> GetHerbForTaxa(CompoundIdentity taxaUnitId);
        IEnumerable<HerbSample> GetHerbForTaxa(IEnumerable<CompoundIdentity> taxaUnitIds);
        IEnumerable<HerbSample> GetHerbForTaxa(IEnumerable<Guid> ids, CompoundIdentity taxaUnitId);
        IEnumerable<HerbSample> GetHerbForTaxa(IEnumerable<Guid> ids, IEnumerable<CompoundIdentity> taxaUnitIds);


        bool UpdateHerb(HerbSample item);
        bool DeleteHerb(HerbSample item);

        bool DeleteHerb(Guid id);


        ShrubSample GetShrub(Guid id);
        IEnumerable<ShrubSample> GetShrub();

        IEnumerable<ShrubSample> GetShrub(IEnumerable<Guid> ids);
        IEnumerable<ShrubSample> GetShrubForVegSample(IEnumerable<Guid> vegSampleIds);

        IEnumerable<ShrubSample> GetShrubForTaxa(CompoundIdentity taxaUnitId);
        IEnumerable<ShrubSample> GetShrubForTaxa(IEnumerable<CompoundIdentity> taxaUnitIds);
        IEnumerable<ShrubSample> GetShrubForTaxa(IEnumerable<Guid> ids, CompoundIdentity taxaUnitId);
        IEnumerable<ShrubSample> GetShrubForTaxa(IEnumerable<Guid> ids, IEnumerable<CompoundIdentity> taxaUnitIds);

        bool UpdateShrub(ShrubSample item);
        bool DeleteShrub(ShrubSample item);

        bool DeleteShrub(Guid id);


        TreeSample GetTree(Guid id);
        IEnumerable<TreeSample> GetTree();
        IEnumerable<TreeSample> GetTree(IEnumerable<Guid> ids);
        IEnumerable<TreeSample> GetTreeForVegSample(IEnumerable<Guid> vegSampleIds);

        IEnumerable<TreeSample> GetTreeForTaxa(CompoundIdentity taxaUnitId);
        IEnumerable<TreeSample> GetTreeForTaxa(IEnumerable<CompoundIdentity> taxaUnitIds);
        IEnumerable<TreeSample> GetTreeForTaxa(IEnumerable<Guid> ids, CompoundIdentity taxaUnitId);
        IEnumerable<TreeSample> GetTreeForTaxa(IEnumerable<Guid> ids, IEnumerable<CompoundIdentity> taxaUnitIds);

        bool UpdateTree(TreeSample item);
        bool DeleteTree(TreeSample item);

        bool DeleteTree(Guid id);

    }
}
