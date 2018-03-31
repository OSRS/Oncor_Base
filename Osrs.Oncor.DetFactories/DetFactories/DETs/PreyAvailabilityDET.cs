using System;
using System.Collections.Generic;
using Osrs.Oncor.DetFactories.DTOs;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;

namespace Osrs.Oncor.DetFactories.DETs
{
    public class PreyAvailabilityDET : IDet, IValidatable
    {
        public DTOMap<PreySurveyDTO> PreySurveys { get; set; }
        public DTOMap<PreyDTO> Prey { get; set; }
        public DTOMap<SiteDTO> Sites { get; set; }
        public DTOMap<SpeciesDTO> Species { get; set; }
        public DTOMap<LifeStageDTO> LifeStages { get; set; }
        public Guid Id { get; set; }
        public string Owner { get; set; }

        public PreyAvailabilityDET()
        {
            InitializeCollections();
            ValidationIssues = new ValidationIssues();
        }

        private void InitializeCollections()
        {
            PreySurveys = new DTOMap<PreySurveyDTO>();
            Prey = new DTOMap<PreyDTO>();
            Sites = new DTOMap<SiteDTO>();
            Species = new DTOMap<SpeciesDTO>();
            LifeStages = new DTOMap<LifeStageDTO>();
        }

        public void Validate()
        {
            foreach (PreySurveyDTO dto in PreySurveys.Values)
            {
                CheckReferentialIntegrity("prey availability", "site", dto.SiteId, Sites.Keys, ValidationIssues);
                CheckReferentialIntegrity("prey availability", "instrument", dto.SiteId, Sites.Keys, ValidationIssues);
            }
            foreach (PreyDTO dto in Prey.Values)
            {
               // ValidateDto(dto, ValidationIssues);
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

        private void ValidateDto(BaseDTO dto, ValidationIssues issues)
        {
            dto.Validate();
            issues.Merge(dto.ValidationIssues);
        }
        public ValidationIssues ValidationIssues { get; }
    }
}
