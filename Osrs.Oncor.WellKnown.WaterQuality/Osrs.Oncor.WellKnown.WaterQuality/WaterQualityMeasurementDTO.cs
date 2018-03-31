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
using System.Collections.Generic;
using System.Collections;

namespace Osrs.Oncor.WellKnown.WaterQuality
{
    public sealed class WaterQualityMeasurementsDTO : IEnumerable<WaterQualityMeasurementDTO>
    {
        public CompoundIdentity DeploymentId
        {
            get;
        }

        private readonly Dictionary<DateTime, WaterQualityMeasurementDTO> readings = new Dictionary<DateTime, WaterQualityMeasurementDTO>();

        public ICollection<DateTime> Keys
        {
            get
            {
                return this.readings.Keys;
            }
        }

        public int Count
        {
            get
            {
                return this.readings.Count;
            }
        }

        public WaterQualityMeasurementDTO this[DateTime key]
        {
            get
            {
                if (this.readings.ContainsKey(key))
                    return this.readings[key];
                return null;
            }
        }

        public static WaterQualityMeasurementsDTO Create(CompoundIdentity deploymentId)
        {
            if (!deploymentId.IsNullOrEmpty())
                return new WaterQualityMeasurementsDTO(deploymentId);
            return null;
        }
        private WaterQualityMeasurementsDTO(CompoundIdentity deploymentId)
        {
            this.DeploymentId = deploymentId;
        }

        public void Add(WaterQualityMeasurementDTO value)
        {
            this.readings.Add(value.SampleDate, value);
        }

        public bool ContainsKey(DateTime key)
        {
            return this.readings.ContainsKey(key);
        }

        public bool Remove(DateTime key)
        {
            return this.readings.Remove(key);
        }

        public bool TryGetValue(DateTime key, out WaterQualityMeasurementDTO value)
        {
            return this.readings.TryGetValue(key, out value);
        }

        public void Clear()
        {
            this.readings.Clear();
        }

        public IEnumerator<WaterQualityMeasurementDTO> GetEnumerator()
        {
            return this.readings.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public sealed class WaterQualityMeasurementDTO
    {
        public DateTime SampleDate
        {
            get;
        }

        public double? Temperature
        {
            get;
        }

        public double? SurfaceElevation
        {
            get;
        }

        public double? pH
        {
            get;
        }

        public double? DissolvedOxygen
        {
            get;
        }

        public double? Conductivity
        {
            get;
        }

        public double? Salinity
        {
            get;
        }

        public double? Velocity
        {
            get;
        }

        public WaterQualityMeasurementDTO(DateTime sampleDate, double? surfaceElevation, double? temperature, double? ph, double? dissolvedOxygen, double? conductivity, double? salinity, double? velocity)
        {
            MethodContract.Assert(sampleDate < DateTime.UtcNow && sampleDate > WQUtils.GlobalMinDate, nameof(sampleDate));
            MethodContract.Assert(surfaceElevation.HasValue || temperature.HasValue || ph.HasValue || dissolvedOxygen.HasValue || conductivity.HasValue || salinity.HasValue || velocity.HasValue, "values");

            this.SampleDate = WQUtils.FixDate(sampleDate);
            this.SurfaceElevation = surfaceElevation;
            this.Temperature = temperature;
            this.pH = ph;
            this.DissolvedOxygen = dissolvedOxygen;
            this.Conductivity = conductivity;
            this.Salinity = salinity;
            this.Velocity = velocity;
        }
    }
}
