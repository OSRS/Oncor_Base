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
    public interface IWQMeasurementProvider
    {
        bool CanGet();
        bool CanDelete();
        bool CanDelete(WaterQualityMeasurement item);
        bool CanCreate();

        IEnumerable<WaterQualityMeasurement> Get(); //all measurements, no ordering

        IEnumerable<WaterQualityMeasurement> Get(CompoundIdentity deploymentId);

        IEnumerable<WaterQualityMeasurement> Get(DateTime start, DateTime end);

        IEnumerable<WaterQualityMeasurement> Get(CompoundIdentity deploymentId, DateTime start, DateTime end);

        bool Delete(WaterQualityMeasurement item);

        WaterQualityMeasurement Create(CompoundIdentity deploymentId, DateTime sampleDate, double? surfaceElevation, double? temperature, double? ph, double? dissolvedOxygen, double? conductivity, double? salinity, double? velocity);

        WaterQualityMeasurement Create(WaterQualityDeployment deployment, DateTime sampleDate, double? surfaceElevation, double? temperature, double? ph, double? dissolvedOxygen, double? conductivity, double? salinity, double? velocity);

        WaterQualityMeasurement Create(CompoundIdentity deploymentId, WaterQualityMeasurementDTO item);
        WaterQualityMeasurement Create(WaterQualityDeployment deployment, WaterQualityMeasurementDTO item);
        IEnumerable<WaterQualityMeasurement> Create(WaterQualityMeasurementsDTO items);
    }
}
