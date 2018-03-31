using System;
using System.Collections.Generic;
using Osrs.Oncor.DetFactories.DTOs;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;

namespace Osrs.Oncor.DetFactories.DETs
{
    public class CrossSectionDET : IDet, IValidatable
    {
        public DTOMap<CrossSectionSurveyDTO> Surveys { get; set; }
        public DTOMap<CrossSectionElevationDTO> Elevations { get; set; }
        public DTOMap<InstrumentDTO> Instruments { get; set; }
        public DTOMap<SiteDTO> Sites { get; set; }
        public Guid Id { get; set; }
        public string Owner { get; set; }

        public CrossSectionDET()
        {
            InitializeCollections();
            ValidationIssues = new ValidationIssues();
        }
        public void Validate()
        {
            foreach (CrossSectionSurveyDTO dto in Surveys.Values)
            {
                CheckReferentialIntegrity("cross section", "site", dto.SiteId, Sites.Keys, ValidationIssues);
                CheckReferentialIntegrity("cross section", "instrument", dto.InstrumentId, Instruments.Keys, ValidationIssues);
            }
            foreach (CrossSectionElevationDTO dto in Elevations.Values)
            {
                CheckReferentialIntegrity("cross section", "site", dto.SurveyId, Surveys.Keys, ValidationIssues);
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
            Surveys = new DTOMap<CrossSectionSurveyDTO>();
            Elevations = new DTOMap<CrossSectionElevationDTO>();
            Instruments = new DTOMap<InstrumentDTO>();
            Sites = new DTOMap<SiteDTO>();
        }

        private void ValidateDto(BaseDTO dto, ValidationIssues issues)
        {
            dto.Validate();
            issues.Merge(dto.ValidationIssues);
        }
        public ValidationIssues ValidationIssues { get; }
    }
}
