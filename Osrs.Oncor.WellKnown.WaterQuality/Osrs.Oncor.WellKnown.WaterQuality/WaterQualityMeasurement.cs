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
using Osrs.Runtime;
using System;

namespace Osrs.Oncor.WellKnown.WaterQuality
{
    public sealed class WaterQualityMeasurement
    {
        private readonly CompoundIdentity deploymentId;
        public CompoundIdentity DeploymentId
        {
            get { return this.deploymentId; }
        }

        private readonly DateTime sampleDate;
        public DateTime SampleDate
        {
            get { return this.sampleDate; }
        }

        private readonly double? temperature;
        public double? Temperature
        {
            get { return this.temperature; }
        }

        private readonly double? surfaceElevation;
        public double? SurfaceElevation
        {
            get { return this.surfaceElevation; }
        }

        private readonly double? ph;
        public double? pH
        {
            get { return this.ph; }
        }

        private readonly double? dissolvedOxygen;
        public double? DissolvedOxygen
        {
            get { return this.dissolvedOxygen; }
        }

        private readonly double? conductivity;
        public double? Conductivity
        {
            get { return this.conductivity; }
        }

        private readonly double? salinity;
        public double? Salinity
        {
            get { return this.salinity; }
        }

        private readonly double? velocity;
        public double? Velocity
        {
            get { return this.velocity; }
        }

        public WaterQualityMeasurement(CompoundIdentity deploymentId, DateTime sampleDate, double? surfaceElevation, double? temperature, double? ph, double? dissolvedOxygen, double? conductivity, double? salinity, double? velocity)
        {
            MethodContract.NotNullOrEmpty(deploymentId, nameof(deploymentId));
            MethodContract.Assert(sampleDate < DateTime.UtcNow && sampleDate > WQUtils.GlobalMinDate, nameof(sampleDate));
            MethodContract.Assert(surfaceElevation.HasValue || temperature.HasValue || ph.HasValue || dissolvedOxygen.HasValue || conductivity.HasValue || salinity.HasValue || velocity.HasValue, "values");

            this.deploymentId = deploymentId;
            this.sampleDate = sampleDate;
            this.surfaceElevation = surfaceElevation;
            this.temperature = temperature;
            this.ph = ph;
            this.dissolvedOxygen = dissolvedOxygen;
            this.conductivity = conductivity;
            this.salinity = salinity;
            this.velocity = velocity;
        }
    }
}
