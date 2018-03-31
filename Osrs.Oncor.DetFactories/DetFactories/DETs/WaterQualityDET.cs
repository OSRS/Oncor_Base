using System;
using System.Collections.Generic;
using Osrs.Oncor.DetFactories.DTOs;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;

namespace Osrs.Oncor.DetFactories.DETs
{
    public class WaterQualityDET : IDet, IValidatable
    {
        public DTOMap<DeploymentDTO> Deployments { get; set; }
        public DTOMap<MeasurementDTO> Measurements { get; set; }
        public DTOMap<SiteDTO> Sites { get; set; }
        public DTOMap<InstrumentDTO> Instruments { get; set; }
        public Guid Id { get; set; }
        public string Owner { get; set; }

        public WaterQualityDET()
        {
            InitializeCollections();
            ValidationIssues = new ValidationIssues();
        }

        public void Validate()
        {
            Dictionary<string, List<Pair>> items = new Dictionary<string, List<Pair>>();
            foreach (DeploymentDTO dto in Deployments.Values)
            {
                CheckReferentialIntegrity("deployment", "site", dto.SiteId, Sites.Keys, ValidationIssues);
                CheckReferentialIntegrity("deployment", "instrument", dto.InstrumentId, Instruments.Keys, ValidationIssues);
                DeploymentDateOrder(dto.StartDate, dto.EndDate);
                InstrumentDeploymentOverlap(dto.StartDate, dto.EndDate, dto.InstrumentId, items);
            }
            items = null;
            foreach (MeasurementDTO dto in Measurements.Values)
            {
                CheckReferentialIntegrity("measurement", "deployment", dto.DeployCode, Deployments.Keys, ValidationIssues);
                if (Deployments.ContainsKey(dto.DeployCode))
                {
                    DeploymentDTO deployment = Deployments[dto.DeployCode];
                    MeasurementDuringDeployment(dto.MeasureDateTime, deployment.StartDate, deployment.EndDate);
                }
            }
        }

        private void InstrumentDeploymentOverlap(DateTime? startDate, DateTime? endDate, string instrumentCode, Dictionary<string, List<Pair>> items)
        {
            if (items.ContainsKey(instrumentCode))
            {
                List<Pair> tmp = items[instrumentCode];
                foreach(Pair item in tmp)
                {
                    if ((startDate >= item.min && startDate <= item.max) || (endDate <= item.max && endDate >= item.min)) //overlapping range
                    {
                        string message = string.Format("The instrument {0} deployment overlaps with another deployment {1} - {2}.", instrumentCode, startDate, endDate);
                        ValidationIssues.Add(ValidationIssue.Code.TemporalConsistencyCode, message);
                        if (startDate < item.min)
                            item.min = startDate;
                        if (endDate > item.max)
                            item.max = endDate;
                    }
                }
            }
            else
            {
                List<Pair> tmp = new List<Pair>();
                tmp.Add(new Pair(startDate, endDate));
                items.Add(instrumentCode, tmp);
            }
        }

        private void DeploymentDateOrder(DateTime? startDate, DateTime? endDate)
        {
            DateTime now = DateTime.Now;
            if (startDate >= endDate)
            {
                string message = string.Format("The deployment window {0} {1} ends before it starts.", startDate, endDate);
                ValidationIssues.Add(ValidationIssue.Code.TemporalConsistencyCode, message);
            }
            if (startDate >= now || endDate >= now)
            {
                string message = string.Format("The deployment window {0} {1} must be in the past.", startDate, endDate);
                ValidationIssues.Add(ValidationIssue.Code.TemporalConsistencyCode, message);
            }
        }

        private void MeasurementDuringDeployment(DateTime? measureDateTime, DateTime? deploymentStartDate, DateTime? deploymentEndDate)
        {
            if (measureDateTime < deploymentStartDate || measureDateTime > deploymentEndDate)
            {
                string message = string.Format("The measurement date and time {0} occurs outside the deployment window.", measureDateTime);
                ValidationIssues.Add(ValidationIssue.Code.TemporalConsistencyCode, message);
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
            Deployments = new DTOMap<DeploymentDTO>();
            Measurements = new DTOMap<MeasurementDTO>();
            Sites = new DTOMap<SiteDTO>();
            Instruments = new DTOMap<InstrumentDTO>();
        }
        public ValidationIssues ValidationIssues { get; }

        internal sealed class Pair
        {
            internal DateTime? min;
            internal DateTime? max;
            internal Pair(DateTime? min, DateTime? max)
            {
                this.min = min;
                this.max = max;
            }
        }
    }
}