using Newtonsoft.Json.Linq;
using Osrs.Data;
using Osrs.Net.Http;
using Osrs.WellKnown.SensorsAndInstruments;
using Osrs.WellKnown.SensorsAndInstruments.Archetypes;
using System;
using System.Collections.Generic;

namespace Pnnl.Oncor.Rest.Instruments
{
    internal static class Jsonifier
    {
        private const string Dsid = "dsid";
        private const string Id = "id";

        public static HashSet<CompoundIdentity> ParseIds(string jsonPayload)
        {
            try
            {
                JArray data = JToken.Parse(jsonPayload) as JArray;
                if (data != null)
                {
                    HashSet<CompoundIdentity> ids = new HashSet<CompoundIdentity>();
                    CompoundIdentity item;
                    foreach (JToken cur in data)
                    {
                        item = ToId(cur as JObject);
                        if (item != null)
                            ids.Add(item);
                    }
                    return ids;
                }
            }
            catch
            { }
            return null;
        }

        public static CompoundIdentity ToId(JObject ob)
        {
            if (ob != null)
            {
                if (ob[Dsid] != null && ob[Id] != null)
                {
                    JToken d = ob[Dsid];
                    JToken i = ob[Id];

                    Guid ds;
                    Guid id;

                    if (Guid.TryParse(d.ToString(), out ds) && Guid.TryParse(i.ToString(), out id))
                        return new CompoundIdentity(ds, id);
                }
            }
            return null;
        }

        public static JToken ParseBody(HttpRequest request)
        {
            try
            {
                return JToken.Parse(RestUtils.ReadBody(request));
            }
            catch
            { }
            return null;
        }

        public static JArray ToJson(IEnumerable<Instrument> instruments)
        {
            if (instruments != null)
            {
                JArray o = new JArray();
                foreach (Instrument cur in instruments)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

		public static JArray ToJson(IEnumerable<InstrumentFamily> instrumentFams)
		{
			if (instrumentFams != null)
			{
				JArray o = new JArray();
				foreach (InstrumentFamily fam in instrumentFams)
				{
					if (fam != null)
						o.Add(ToJson(fam));
				}
				return o;
			}
			return null;
		}

		public static JArray ToJson(IEnumerable<InstrumentType> instrumentTypes)
		{
			if (instrumentTypes != null)
			{
				JArray o = new JArray();
				foreach (InstrumentType type in instrumentTypes)
				{
					if (type != null)
						o.Add(ToJson(type));
				}
				return o;
			}
			return null;
		}

        public static JArray ToJson(IEnumerable<CompoundIdentity> cids)
        {
            if (cids != null)
            {
                JArray o = new JArray();
                foreach (CompoundIdentity cur in cids)
                {
                    if (cur != null)
                        o.Add(ToJson(cur));
                }
                return o;
            }
            return null;
        }

		public static JArray ToJson(IEnumerable<Tuple<CompoundIdentity, CompoundIdentity>> archetypes)
		{
			if (archetypes != null)
			{
				JArray o = new JArray();
				foreach(Tuple<CompoundIdentity, CompoundIdentity> arch in archetypes)
				{
					if (arch != null)
						o.Add(ToJson(arch));
				}
				return o;
			}
			return null;
		}

		public static JObject ToJson(Tuple<CompoundIdentity, CompoundIdentity> archetype)
		{
			if(archetype != null && archetype.Item1 != null && archetype.Item2 != null)
			{
				JObject o = new JObject();
				o.Add("typeid", JsonUtils.ToJson(archetype.Item1));
				o.Add("archid", JsonUtils.ToJson(archetype.Item2));
				return o;
			}
			return null;
		}

        public static JObject ToJson(Instrument instrument)
        {
            if (instrument != null)
            {
                JObject o = new JObject();
                o.Add(JsonUtils.Id, JsonUtils.ToJson(instrument.Identity));
                o.Add(JsonUtils.Name, instrument.Name);
                o.Add(JsonUtils.OwnerId, JsonUtils.ToJson(instrument.OwningOrganizationIdentity));
                o.Add("typeid", JsonUtils.ToJson(instrument.InstrumentTypeIdentity));
                o.Add(JsonUtils.Description, instrument.Description);
                if (instrument.ManufacturerId != null)
                    o.Add("manf", JsonUtils.ToJson(instrument.ManufacturerId));
                if (instrument.SerialNumber != null)
                    o.Add("serial", instrument.SerialNumber);
                return o;
            }
            return null;
        }

		public static JObject ToJson(SimpleTrapDredge std)
		{
			if (std != null)
			{
				JObject o = new JObject();
				o.Add("openarea", std.OpenArea);
				return o;
			}
			return null;
		}

		public static JObject ToJson(Instrument instrument, SimpleTrapDredge std)
		{
			if (std != null && instrument != null)
			{
				JObject o = ToJson(instrument);
				o.Add("archid", JsonUtils.ToJson(std.Identity));
				o.Add("archdata", ToJson(std));
				return o;
			}
			return null;
		}

		public static JObject ToJson(StandardMeshNet smn)
		{
			if (smn != null)
			{
				JObject o = new JObject();
				o.Add("length", smn.Length);
				o.Add("depth", smn.Depth);
				o.Add("meshsize", smn.MeshSize);
				return o;
			}
			return null;
		}

		public static JObject ToJson(Instrument instrument, StandardMeshNet smn)
		{
			if (smn != null && instrument != null)
			{
				JObject o = ToJson(instrument);
				o.Add("archid", JsonUtils.ToJson(smn.Identity));
				o.Add("archdata", ToJson(smn));
				return o;
			}
			return null;
		}

		public static JObject ToJson(StandardPlanktonNet spn)
		{
			if (spn != null)
			{
				JObject o = new JObject();
				o.Add("openarea", spn.OpenArea);
				o.Add("meshsize", spn.MeshSize);
				o.Add("codsize", spn.CodSize);
				return o;
			}
			return null;
		}

		public static JObject ToJson(Instrument instrument, StandardPlanktonNet spn)
		{
			if (spn != null && instrument != null)
			{
				JObject o = ToJson(instrument);
				o.Add("archid", JsonUtils.ToJson(spn.Identity));
				o.Add("archdata", ToJson(spn));
				return o;
			}
			return null;
		}

		public static JObject ToJson(WingedBagNet wbn)
		{
			if (wbn != null)
			{
				JObject o = new JObject();
				o.Add("length", wbn.Length);
				o.Add("depth", wbn.Depth);
				o.Add("meshsizewings", wbn.MeshSizeWings);
				o.Add("meshsizebag", wbn.MeshSizeBag);
				return o;
			}
			return null;
		}

		public static JObject ToJson(Instrument instrument, WingedBagNet wbn)
		{
			if (wbn != null && instrument != null)
			{
				JObject o = ToJson(instrument);
				o.Add("archid", JsonUtils.ToJson(wbn.Identity));
				o.Add("archdata", ToJson(wbn));
				return o;
			}
			return null;
		}

		public static JObject ToJson(InstrumentFamily instrumentFam)
		{
			if (instrumentFam != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(instrumentFam.Identity));
				o.Add(JsonUtils.Name, instrumentFam.Name);
				o.Add(JsonUtils.Description, instrumentFam.Description);
				o.Add(JsonUtils.ParentId, JsonUtils.ToJson(instrumentFam.ParentId));
				return o;
			}
			return null;
		}

		public static JObject ToJson(InstrumentType instrumentType)
		{
			if (instrumentType != null)
			{
				JObject o = new JObject();
				o.Add(JsonUtils.Id, JsonUtils.ToJson(instrumentType.Identity));
				o.Add(JsonUtils.Name, instrumentType.Name);
				o.Add("familyid", JsonUtils.ToJson(instrumentType.FamilyId));
				o.Add(JsonUtils.Description, instrumentType.Description);
				o.Add(JsonUtils.ParentId, JsonUtils.ToJson(instrumentType.ParentId));
				return o;
			}
			return null;
		}

        public static JObject ToJson(CompoundIdentity cid)
        {
            if (cid != null)
            {
                JObject o = new JObject();
                o.Add(JsonUtils.Id, JsonUtils.ToJson(cid));
                return o;
            }
            return null;
        }
    }
}
