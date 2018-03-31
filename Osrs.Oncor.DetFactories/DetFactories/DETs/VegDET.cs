using Osrs.Oncor.DetFactories.DTOs;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;
using System;
using System.Collections.Generic;

namespace Osrs.Oncor.DetFactories.DETs
{
    public class VegDET : IDet, IValidatable
    {
        //DATA TABS
        public DTOMap<VegSurveyDTO> Surveys { get; set; }
        public DTOMap<VegElevationDTO> Elevations { get; set; }
        public DTOMap<VegTreeDTO> Trees { get; set; }
        public DTOMap<VegHerbDTO> Herbs { get; set; }
        public DTOMap<VegShrubDTO> Shrubs { get; set; }

        //LOOKUPS
        public DTOMap<SpeciesDTO> TreeSpecies { get; set; }
        public DTOMap<SpeciesDTO> HerbSpecies { get; set; }
        public DTOMap<SpeciesDTO> ShrubSpecies { get; set; }
        public DTOMap<SpeciesDTO> NonLiving { get; set; }
        public DTOMap<SiteDTO> Sites { get; set; }
        public DTOMap<PlotTypeDTO> PlotTypes { get; set; }

        public Guid Id { get; set; }
        public string Owner { get; set; }

        public VegDET()
        {
            InitializeCollections();
            ValidationIssues = new ValidationIssues();
        }

        public void Validate()
        {
            CheckCollision("shrubspecies", "nonliving", NonLiving.Keys, ShrubSpecies.Keys, ValidationIssues);
            CheckCollision("herbspecies", "nonliving", NonLiving.Keys, HerbSpecies.Keys, ValidationIssues);
            CheckCollision("treespecies", "nonliving", NonLiving.Keys, TreeSpecies.Keys, ValidationIssues);

            foreach (VegSurveyDTO dto in Surveys.Values)
            {
                CheckReferentialIntegrity("survey", "site", dto.SiteId, Sites.Keys, ValidationIssues);
                if (!string.IsNullOrEmpty(dto.PlotTypeId))
                    CheckReferentialIntegrity("survey", "plottype", dto.PlotTypeId, PlotTypes.Keys, ValidationIssues);
            }

            foreach (VegElevationDTO dto in Elevations.Values)
            {
                CheckReferentialIntegrity("elevation", "survey", dto.SurveyId, Surveys.Keys, ValidationIssues);
                if (!string.IsNullOrEmpty(dto.SiteId))
                    CheckReferentialIntegrity("elevation", "site", dto.SiteId, Sites.Keys, ValidationIssues);
            }

            foreach (VegTreeDTO dto in Trees.Values)
            {
                CheckReferentialIntegrity("tree", "survey", dto.SurveyId, Surveys.Keys, ValidationIssues);
                if (!string.IsNullOrEmpty(dto.SiteId))
                    CheckReferentialIntegrity("tree", "site", dto.SiteId, Sites.Keys, ValidationIssues);
                CheckReferentialIntegrity("tree", "species", dto.TreeSpeciesId, NonLiving.Keys, TreeSpecies.Keys, ValidationIssues);
            }

            foreach (VegHerbDTO dto in Herbs.Values)
            {
                CheckReferentialIntegrity("herb", "survey", dto.SurveyId, Surveys.Keys, ValidationIssues);
                if (!string.IsNullOrEmpty(dto.SiteId))
                    CheckReferentialIntegrity("herb", "site", dto.SiteId, Sites.Keys, ValidationIssues);
                CheckReferentialIntegrity("herb", "species", dto.HerbSpeciesId, NonLiving.Keys, HerbSpecies.Keys, ValidationIssues);
            }

            foreach (VegShrubDTO dto in Shrubs.Values)
            {
                CheckReferentialIntegrity("shrub", "survey", dto.SurveyId, Surveys.Keys, ValidationIssues);
                if (!string.IsNullOrEmpty(dto.SiteId))
                    CheckReferentialIntegrity("shrub", "site", dto.SiteId, Sites.Keys, ValidationIssues);
                CheckReferentialIntegrity("shrub", "species", dto.ShrubSpeciesId, NonLiving.Keys, ShrubSpecies.Keys, ValidationIssues);
            }
        }

        public void CheckCollision(string sourceName, string lookupName, IList<string> keys, IList<string> altKeys, ValidationIssues issues)
        {
            foreach(String testId in altKeys)
            {
                if (keys.Contains(testId))
                {
                    string message = string.Format("The {2} has a duplicate {0} key with a value of {1}.", lookupName, testId, sourceName);
                    ValidationIssues.Add(ValidationIssue.Code.NonUniqueKeyCode, message);
                }
            }
        }

        public void CheckReferentialIntegrity(string sourceName, string lookupName, string testId, IList<string> keys, IList<string> altKeys, ValidationIssues issues)
        {
            if (!keys.Contains(testId) && !altKeys.Contains(testId))
            {
                string message = string.Format("The {2} has an invalid {0} key with a value of {1}.", lookupName, testId, sourceName);
                ValidationIssues.Add(ValidationIssue.Code.InvalidForeignKeyCode, message);
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
            TreeSpecies = new DTOMap<SpeciesDTO>();
            HerbSpecies = new DTOMap<SpeciesDTO>();
            ShrubSpecies = new DTOMap<SpeciesDTO>();
            NonLiving = new DTOMap<SpeciesDTO>();
            Sites = new DTOMap<SiteDTO>();
            PlotTypes = new DTOMap<PlotTypeDTO>();

            Surveys = new DTOMap<VegSurveyDTO>();
            Elevations = new DTOMap<VegElevationDTO>();
            Trees = new DTOMap<VegTreeDTO>();
            Herbs = new DTOMap<VegHerbDTO>();
            Shrubs = new DTOMap<VegShrubDTO>();
        }
        public ValidationIssues ValidationIssues { get; }
    }
}
