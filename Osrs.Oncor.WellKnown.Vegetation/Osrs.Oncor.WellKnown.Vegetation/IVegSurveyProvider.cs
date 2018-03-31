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
using System.Collections.Generic;

namespace Osrs.Oncor.WellKnown.Vegetation
{
    public interface IVegSurveyProvider
    {
        bool CanGet();
        bool CanUpdate();
        bool CanUpdate(VegSurvey item);
        bool CanUpdate(VegPlotType item);
        bool CanDelete();
        bool CanDelete(VegSurvey item);
        bool CanDelete(VegPlotType item);
        bool CanCreate();

        IEnumerable<VegPlotType> GetPlotType();

        VegPlotType GetPlotType(CompoundIdentity id);

        IEnumerable<VegPlotType> GetPlotType(IEnumerable<CompoundIdentity> ids);


        IEnumerable<VegSurvey> GetSurvey();

        VegSurvey GetSurvey(CompoundIdentity id);

        IEnumerable<VegSurvey> Get(IEnumerable<CompoundIdentity> sampleEventIds, IEnumerable<CompoundIdentity> siteIds);

        IEnumerable<VegSurvey> GetForSampleEvent(CompoundIdentity sampleEventId);

        IEnumerable<VegSurvey> GetForSite(CompoundIdentity siteId);

        IEnumerable<VegSurvey> Get(IEnumerable<CompoundIdentity> ids);

        IEnumerable<VegSurvey> GetForSampleEvent(IEnumerable<CompoundIdentity> sampleEventIds);

        IEnumerable<VegSurvey> GetForSite(IEnumerable<CompoundIdentity> siteIds);

        bool Update(VegSurvey item);
        bool Update(VegPlotType item);
        bool Delete(VegSurvey item);
        bool Delete(VegPlotType item);
        bool DeleteSurvey(CompoundIdentity id);
        bool DeletePlotType(CompoundIdentity id);

        //CompoundIdentity id, CompoundIdentity sampleEventId, CompoundIdentity siteId, CompoundIdentity plotTypeId, Point2<double> location, float area, float minElev, float maxElev, string description, bool isPrivate
        VegSurvey Create(CompoundIdentity sampleEventId, CompoundIdentity siteId, CompoundIdentity plotTypeId, Point2<double> location, float area, float minElev, float maxElev, bool isPrivate);

        VegSurvey Create(CompoundIdentity sampleEventId, CompoundIdentity siteId, CompoundIdentity plotTypeId, Point2<double> location, float area, float minElev, float maxElev, string description, bool isPrivate);

        VegPlotType Create(string name);

        VegPlotType Create(string name, string description);
    }
}
