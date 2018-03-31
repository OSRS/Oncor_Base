using System;
using System.Collections.Generic;
using Osrs.Oncor.DetFactories.DTOs;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;

namespace Osrs.Oncor.DetFactories.DETs
{
    public class SedimentAccretionDET : IDet, IValidatable
    {
        public DTOMap<SedimentAccretionSurvey> Surveys { get; set; }
        public DTOMap<SedimentAccretionElevation> Elevations { get; set; }
        public DTOMap<SiteDTO> Sites { get; set; }
        public Guid Id { get; set; }
        public string Owner { get; set; }

        public SedimentAccretionDET()
        {
            InitializeCollections();
            ValidationIssues = new ValidationIssues();
        }
        public void Validate()
        {
            foreach (SedimentAccretionSurvey dto in Surveys.Values)
            {
                CheckReferentialIntegrity("sediment accretion", "site", dto.SiteId, Sites.Keys, ValidationIssues);
            }
            foreach (SedimentAccretionElevation dto in Elevations.Values)
            {
                CheckReferentialIntegrity("sediment accretion", "survey", dto.SurveyId, Surveys.Keys, ValidationIssues);
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
            Surveys = new DTOMap<SedimentAccretionSurvey>();
            Elevations = new DTOMap<SedimentAccretionElevation>();
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
