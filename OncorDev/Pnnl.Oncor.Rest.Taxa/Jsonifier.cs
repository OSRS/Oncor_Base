using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Net.Http;
using Osrs.WellKnown.Taxonomy;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.Taxa
{
	internal static class Jsonifier
	{
		public static JObject ToJson(Taxonomy taxonomy)
		{
			if (taxonomy != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(taxonomy.Identity));
				o.Add(JsonUtils.Name, taxonomy.Name);
				o.Add(JsonUtils.Description, taxonomy.Description);
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<Taxonomy> taxonomies)
		{
			if (taxonomies != null)
			{
				JArray o = new JArray();
				foreach (Taxonomy tax in taxonomies)
				{
					if (tax != null)
						o.Add(ToJson(tax));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(TaxaDomain domain)
		{
			if (domain != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(domain.Identity));
				o.Add(JsonUtils.Name, domain.Name);
				o.Add(JsonUtils.Description, domain.Description);
				if (domain.TaxonomyId != null)
					o.Add("taxonomyid", JsonUtils.ToJson(domain.TaxonomyId));
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<TaxaDomain> domains)
		{
			if (domains != null)
			{
				JArray o = new JArray();
				foreach (TaxaDomain dom in domains)
				{
					if (dom != null)
						o.Add(ToJson(dom));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(TaxaUnitType unittype)
		{
			if (unittype != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(unittype.Identity));
				o.Add(JsonUtils.Name, unittype.Name);
				o.Add(JsonUtils.Description, unittype.Description);
				if (unittype.TaxonomyId != null)
					o.Add("taxonomyid", JsonUtils.ToJson(unittype.TaxonomyId));
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<TaxaUnitType> unittypes)
		{
			if (unittypes != null)
			{
				JArray o = new JArray();
				foreach (TaxaUnitType unit in unittypes)
				{
					if (unit != null)
						o.Add(ToJson(unit));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(TaxaUnit unit)
		{
			if (unit != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(unit.Identity));
				o.Add(JsonUtils.Name, unit.Name);
				o.Add(JsonUtils.Description, unit.Description);
				if (unit.ParentId != null)
					o.Add(JsonUtils.ParentId, JsonUtils.ToJson(unit.ParentId));
				o.Add("domainid", JsonUtils.ToJson(unit.TaxaDomainId));
				o.Add("taxonomyid", JsonUtils.ToJson(unit.TaxonomyId));
				o.Add("unittypeid", JsonUtils.ToJson(unit.TaxaUnitTypeId));
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<TaxaUnit> units)
		{
			if (units != null)
			{
				JArray o = new JArray();
				foreach (TaxaUnit unit in units)
				{
					if (unit != null)
						o.Add(ToJson(unit));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(TaxaCommonName common)
		{
			if (common != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(common.Identity));
				o.Add(JsonUtils.Name, common.Name);
				o.Add(JsonUtils.Description, common.Description);
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<TaxaCommonName> commons)
		{
			if (commons != null)
			{
				JArray o = new JArray();
				foreach (TaxaCommonName name in commons)
				{
					if (name != null)
						o.Add(ToJson(name));
				}
				return o;
			}
			return null;
		}
	}
}
