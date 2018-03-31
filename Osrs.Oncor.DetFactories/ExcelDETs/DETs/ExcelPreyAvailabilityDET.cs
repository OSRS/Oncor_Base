using System.Collections.Generic;
using System.IO;
using Osrs.Oncor.DetFactories;
using Osrs.Oncor.DetFactories.DETs;
using Osrs.Oncor.DetFactories.DTOs.Lookup_DTOs;
using Osrs.Oncor.DetFactories.DTOs.New_Data_DTOs;
using Osrs.Oncor.Excel;

namespace ExcelDETs.DETs
{
    public sealed class ExcelPreyAvailabilityDET
    {
        private readonly PreyAvailabilityDET generalDet;
        private ExcelBaseDet myDet;

        public ExcelPreyAvailabilityDET(PreyAvailabilityDET det)
        {
            generalDet = det;
            myDet = new ExcelBaseDet(det.Id, det.Owner);
        }

        private DataTab PreySurveysDataTab()
        {
            return new DataTab("DET_PreySurveys", XlColor.White, PreySurveyDTO.GetSchema(), generalDet.PreySurveys.Values);
        }

        private DataTab PreyDataTab()
        {
            return new DataTab("DET_Prey", XlColor.White, PreyDTO.GetSchema(), generalDet.Prey.Values);
        }

        private DataTab SitesDataTab()
        {
            return new DataTab("LIST_Sites", XlColor.Orange, SiteDTO.GetSchema(), generalDet.Sites.Values);
        }

        private DataTab SpeciesDataTab()
        {
            return new DataTab("LIST_Species", XlColor.Orange, SpeciesDTO.GetSchema(), generalDet.Species.Values);
        }

        //private DataTab LifeStagesDataTab()
        //{
        //    return new DataTab("LIST_LifeStages", XlColor.Orange, LifeStageDTO.GetSchema(), generalDet.LifeStages.Values);
        //}

        private List<DataTab> DataTabList()
        {
            List<DataTab> list = new List<DataTab>();
            list.Add(PreySurveysDataTab());
            list.Add(PreyDataTab());
            list.Add(SitesDataTab());
            list.Add(SpeciesDataTab());
            //list.Add(LifeStagesDataTab());
            return list;
        }

        private void LoadRow(string sheetName, Dictionary<string, string> values)
        {
            ValidationIssues issues = generalDet.ValidationIssues;
            if (sheetName == "DET_PreySurveys")
            {
                PreySurveyDTO newDto = new PreySurveyDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.PreySurveys.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The prey survey with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "DET_Prey")
            {
                PreyDTO newDto = new PreyDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.Prey.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The prey with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "LIST_Sites")
            {
                SiteDTO newDto = new SiteDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.Sites.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The site with code {0} is not unique.", newDto.LookupKey));
                }
            }
            else if (sheetName == "LIST_Species")
            {
                SpeciesDTO newDto = new SpeciesDTO(values);
                newDto.Validate();
                issues.Merge(newDto.ValidationIssues);
                bool success = generalDet.Species.Add(newDto);
                if (!success)
                {
                    issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The species with code {0} is not unique.", newDto.LookupKey));
                }
            }
            //else if (sheetName == "LIST_LifeStages")
            //{
            //    LifeStageDTO newDto = new LifeStageDTO(values);
            //    newDto.Validate();
            //    issues.Merge(newDto.ValidationIssues);
            //    bool success = generalDet.LifeStages.Add(newDto);
            //    if (!success)
            //    {
            //        issues.Add(ValidationIssue.Code.NonUniqueKeyCode, string.Format("The life stage with code {0} is not unique.", newDto.LookupKey));
            //    }
            //}
        }

        private void CheckSheetCount()
        {
            myDet.CheckOneSheet("DET_PreySurveys", generalDet.ValidationIssues);
            myDet.CheckOneSheet("DET_Prey", generalDet.ValidationIssues);
            myDet.CheckOneSheet("LIST_Sites", generalDet.ValidationIssues);
            myDet.CheckOneSheet("LIST_Species", generalDet.ValidationIssues);
            //myDet.CheckOneSheet("LIST_LifeStages", generalDet.ValidationIssues);
        }

        private void CheckHeaders(XlWorksheet worksheet)
        {
            if (worksheet != null)
            {
                Schema s = null;
                List<string> h;
                //if (worksheet.Name == "DET_Deployments")
                //{
                //    s = DeploymentDTO.GetSchema();
                //}
                //else
                //    return;

                //h = new List<string>();
                //foreach (SchemaEntry c in s)
                //{
                //    h.Add(c.ColumnName);
                //}
                //if (!ExcelBaseDet.HasHeaders(ExcelBaseDet.Headers(worksheet), h))
                //{
                //    generalDet.ValidationIssues.Add(ValidationIssue.Code.MissingFieldHeader, "Missing column header in " + worksheet.Name);
                //}
            }
        }

        public void Save(Stream stream)
        {
            myDet.Save(stream, DataTabList());
        }

        public void Save(string filename)
        {
            myDet.Save(filename, DataTabList());
        }

        public IDet Load(Stream stream)
        {
            ExcelPreyAvailabilityDET newDet = new ExcelPreyAvailabilityDET(generalDet);
            newDet.myDet.OpenWorkbook(stream, newDet.LoadRow, newDet.CheckSheetCount, newDet.CheckHeaders);
            generalDet.Id = newDet.myDet.Id;
            generalDet.Owner = newDet.myDet.Owner;
            return generalDet;
        }

        public IDet Load(string filename)
        {
            ExcelPreyAvailabilityDET newDet = new ExcelPreyAvailabilityDET(generalDet);
            newDet.myDet.OpenWorkbook(filename, newDet.LoadRow, newDet.CheckSheetCount, newDet.CheckHeaders);
            generalDet.Id = newDet.myDet.Id;
            generalDet.Owner = newDet.myDet.Owner;
            return generalDet;
        }
    }
}
