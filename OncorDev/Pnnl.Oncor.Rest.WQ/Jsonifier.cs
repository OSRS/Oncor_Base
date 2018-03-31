using Newtonsoft.Json.Linq;
using Osrs.Oncor.WellKnown.WaterQuality;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.WQ
{
	internal static class Jsonifier
	{

		public static JObject ToJson(WaterQualityDeployment deployment)
		{
			if (deployment != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(deployment.Identity));
				o.Add(JsonUtils.Name, deployment.Name);
				o.Add("sampleeventid", JsonUtils.ToJson(deployment.SampleEventId));
				o.Add("siteid", JsonUtils.ToJson(deployment.SiteId));
				o.Add("sensorid", JsonUtils.ToJson(deployment.SensorId));
				o.Add("startdate", deployment.Range.StartDate);
				o.Add("enddate", deployment.Range.EndDate);
				if (!string.IsNullOrEmpty(deployment.Description))
					o.Add(JsonUtils.Description, deployment.Description);
				o.Add("isprivate", deployment.IsPrivate);
				return o;
			}
			return null;
		}

		public static JObject ToJson(WaterQualityMeasurement measurement)
		{
			if (measurement != null)
			{
				JObject o = new JObject();
				o.Add("deploymentid", JsonUtils.ToJson(measurement.DeploymentId));
				o.Add("sampledate", measurement.SampleDate);
				o.Add("surfaceelevation", measurement.SurfaceElevation);
				if (measurement.Temperature != null)
					o.Add("temperature", measurement.Temperature);
				if (measurement.pH != null)
					o.Add("ph", measurement.pH);
				if (measurement.DissolvedOxygen != null)
					o.Add("dissolvedoxygen", measurement.DissolvedOxygen);
				if (measurement.Conductivity != null)
					o.Add("conductivity", measurement.Conductivity);
				if (measurement.Salinity != null)
					o.Add("salinity", measurement.Salinity);
				if (measurement.Velocity != null)
					o.Add("velocity", measurement.Velocity);
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<WaterQualityDeployment> deployments)
		{
			if (deployments != null)
			{
				JArray o = new JArray();
				foreach (WaterQualityDeployment dep in deployments)
				{
					if (dep != null)
						o.Add(ToJson(dep));
				}
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<WaterQualityMeasurement> measurements)
		{
			if (measurements != null)
			{
				JArray o = new JArray();
				foreach (WaterQualityMeasurement meas in measurements)
				{
					if (meas != null)
						o.Add(ToJson(meas));
				}
				return o;
			}
			return null;
		}
	}
}
