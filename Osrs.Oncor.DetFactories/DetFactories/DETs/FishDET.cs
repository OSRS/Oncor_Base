using System;
using System.Collections.Generic;
using Osrs.Oncor.DetFactories.DTOs;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;

namespace Osrs.Oncor.DetFactories.DETs
{
    public class FishDET : IDet, IValidatable
    {
        public DTOMap<CatchEffortDTO> CatchEfforts { get; set; }
        public DTOMap<NetHaulEventDTO> NetHaulEvents { get; set; }
        public DTOMap<FishCountDTO> FishCounts { get; set; }
        public DTOMap<CatchMetricDTO> CatchMetrics { get; set; }
        public DTOMap<FishDTO> Fish { get; set; }
        public DTOMap<IdTagDTO> IdTags { get; set; }
        public DTOMap<GeneticDTO> Genetics { get; set; }
        public DTOMap<DietDTO> Diet { get; set; }
        public DTOMap<FishSpeciesDTO> FishSpecies { get; set; }
        public DTOMap<MacroSpeciesDTO> MacroSpecies { get; set; }
        public DTOMap<SiteDTO> Sites { get; set; }
        public DTOMap<NetDTO> Nets { get; set; }
        public Guid Id { get; set; }
        public string Owner { get; set; }

        public FishDET()
        {
            InitializeCollections();
            ValidationIssues = new ValidationIssues();
        }

        public void Validate()
        {
            foreach (CatchEffortDTO dto in CatchEfforts.Values)
            {
                CheckReferentialIntegrity("catch effort", "site", dto.SiteId, Sites.Keys, ValidationIssues);
            }
            foreach (NetHaulEventDTO dto in NetHaulEvents.Values)
            {
                CheckReferentialIntegrity("net haul event", "catch effort", dto.CatchId, CatchEfforts.Keys, ValidationIssues);
                CheckReferentialIntegrity("net haul event", "net", dto.NetId, Nets.Keys, ValidationIssues);
            }
            foreach (FishCountDTO dto in FishCounts.Values)
            {
                CheckReferentialIntegrity("fish count", "catch effort", dto.CatchId, CatchEfforts.Keys, ValidationIssues);
                CheckReferentialIntegrity("fish count", "fish species", dto.SpeciesId, FishSpecies.Keys, ValidationIssues);
            }
            foreach (CatchMetricDTO dto in CatchMetrics.Values)
            {
                CheckReferentialIntegrity("catch metric", "catch effort", dto.CatchId, CatchEfforts.Keys, ValidationIssues);
            }
            foreach (FishDTO dto in Fish.Values)
            {
                CheckReferentialIntegrity("fish", "catch effort", dto.CatchId, CatchEfforts.Keys, ValidationIssues);
                CheckReferentialIntegrity("fish", "fish species", dto.SpeciesId, FishSpecies.Keys, ValidationIssues);
            }
            foreach (IdTagDTO dto in IdTags.Values)
            {
                CheckReferentialIntegrity("id tag", "fish", dto.FishId, Fish.Keys, ValidationIssues);
            }
            foreach (GeneticDTO dto in Genetics.Values)
            {
                CheckReferentialIntegrity("genetics", "fish", dto.FishId, Fish.Keys, ValidationIssues);
            }
            foreach (DietDTO dto in Diet.Values)
            {
                CheckReferentialIntegrity("diet", "fish", dto.FishId, Fish.Keys, ValidationIssues);
            }
        }

        public void CheckReferentialIntegrity(string sourceName, string lookupName, string testId, IList<string> keys, ValidationIssues issues)
        {
            if (!keys.Contains(testId))
            {
                string message = string.Format("The {2} has an invalid {0} key with a value of {1}.", lookupName, testId, sourceName);
                ValidationIssues.Add(ValidationIssue.Code.InvalidForeignKeyCode, message);
            }
        }

        private void InitializeCollections()
        {
            CatchEfforts = new DTOMap<CatchEffortDTO>();
            NetHaulEvents = new DTOMap<NetHaulEventDTO>();
            FishCounts = new DTOMap<FishCountDTO>();
            CatchMetrics = new DTOMap<CatchMetricDTO>();
            Fish = new DTOMap<FishDTO>();
            IdTags = new DTOMap<IdTagDTO>();
            Genetics = new DTOMap<GeneticDTO>();
            Diet = new DTOMap<DietDTO>();
            FishSpecies = new DTOMap<FishSpeciesDTO>();
            MacroSpecies = new DTOMap<MacroSpeciesDTO>();
            Sites = new DTOMap<SiteDTO>();
            Nets = new DTOMap<NetDTO>();
        }
        public ValidationIssues ValidationIssues { get; }
    }
}
