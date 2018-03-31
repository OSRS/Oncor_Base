using System;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;

namespace TestingApp
{
    public static class PhonyLookup
    {
        public static NetDTO CreatePhonyNet(int count)
        {
            NetDTO dto = new NetDTO();
            dto.Key = String.Format("NetID {0}", count);
            dto.Name = String.Format("Name {0}", count);
            //dto.Type = String.Format("Type {0}", count);
            //dto.Details = String.Format("Details {0}", count);
            //dto.Description = String.Format("Description {0}", count);
            return dto;
        }

        public static InstrumentDTO CreatePhonyInstrument(int count)
        {
            InstrumentDTO dto = new InstrumentDTO();
            dto.Key = String.Format("SiteID {0}", count);
            dto.Name = String.Format("Name {0}", count);
            return dto;
        }

        public static LifeStageDTO CreatePhonyLifeStage(int count)
        {
            LifeStageDTO dto = new LifeStageDTO();
            dto.Key = String.Format("SiteID {0}", count);
            dto.Name = String.Format("Name {0}", count);
            //dto.Description = String.Format("Description {0}", count);
            //dto.InternalId = String.Format("Internal ID {0}", count);
            return dto;
        }

        public static SiteDTO CreatePhonySite(int count)
        {
            SiteDTO dto = new SiteDTO();
            dto.Key = String.Format("SiteID {0}", count);
            dto.Name = String.Format("Name {0}", count);
            return dto;
        }

        public static InstrumentDTO CreatePhonySensor(int count)
        {
            InstrumentDTO dto = new InstrumentDTO();
            dto.Key = String.Format("SensorID {0}", count);
            dto.Name = String.Format("InstrumentName {0}", count);
            return dto;
        }

        public static FishSpeciesDTO CreatePhonyFishSpecies(int count)
        {
            FishSpeciesDTO dto = new FishSpeciesDTO();
            dto.Key = String.Format("Key {0}", count);
            dto.Name = String.Format("Name {0}", count);
            return dto;
        }

        public static MacroSpeciesDTO CreatePhonyMacroSpecies(int count)
        {
            MacroSpeciesDTO dto = new MacroSpeciesDTO();
            dto.Key = String.Format("Key {0}", count);
            dto.Name = String.Format("Name {0}", count);
            return dto;
        }

        public static SpeciesDTO CreatePhonySpecies(int count)
        {
            SpeciesDTO dto = new SpeciesDTO();
            dto.Key = String.Format("Key {0}", count);
            dto.Name = String.Format("Name {0}", count);
            return dto;
        }
    }
}
